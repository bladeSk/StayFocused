using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace StayFocused
{
    public class Injector
    {
        const string dllName64 = "PreventFocusStealing64.dll";
        const string dllName32 = "PreventFocusStealing32.dll";
        static string dllPath64;
        static string dllPath32;
        static string helper32ExePath;
        static IntPtr loadLibrary64;
        static IntPtr loadLibrary32;
        static IntPtr freeLibrary64;
        static IntPtr freeLibrary32;

        static Injector() {
            dllPath64 = Path.Combine(Directory.GetCurrentDirectory(), "helpers", dllName64);
            dllPath32 = Path.Combine(Directory.GetCurrentDirectory(), "helpers", dllName32);

            loadLibrary64 = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            freeLibrary64 = GetProcAddress(GetModuleHandle("kernel32.dll"), "FreeLibrary");

            helper32ExePath = Path.Combine(Directory.GetCurrentDirectory(), "helpers", "32BitHelper.exe");
            loadLibrary32 = (IntPtr)RunProcess(helper32ExePath, "dll_handle kernel32.dll LoadLibraryA");
            freeLibrary32 = (IntPtr)RunProcess(helper32ExePath, "dll_handle kernel32.dll FreeLibrary");
        }

        static int RunProcess(string exeName, string @params) {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = exeName;
            startInfo.Arguments = @params;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            var proc = Process.Start(startInfo);
            proc.WaitForExit();
            return proc.ExitCode;
        }

        static IntPtr Get32BitDLLAddress (int pid, string dllName) {
            return (IntPtr)RunProcess(helper32ExePath, "dll_address " + pid + " " + dllName);
        }

        public static void InjectDLL (Process proc) {
            IntPtr procHandle = OpenProcess(
                PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false,
                proc.Id
            );

            IntPtr loadLibraryAddr;
            string dllPath;

            if (proc.Is64Bit()) {
                loadLibraryAddr = loadLibrary64;
                dllPath = dllPath64;
            } else {
                loadLibraryAddr = loadLibrary32;
                dllPath = dllPath32;
            }

            int allocSize = ((dllPath.Length + 1) * Marshal.SizeOf(typeof(char)));
            IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)allocSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            UIntPtr bytesWritten;
            WriteProcessMemory(
                procHandle,
                allocMemAddress,
                Encoding.Default.GetBytes(dllPath),
                (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                out bytesWritten
            );

            IntPtr threadHandle = CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
            WaitForSingleObject(threadHandle, 5000);

            CloseHandle(threadHandle);
            VirtualFreeEx(procHandle, allocMemAddress, allocSize, MEM_RELEASE);
            CloseHandle(procHandle);
        }

        public static void UnloadDLL (Process proc) {
            IntPtr procHandle = OpenProcess(
                PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false,
                proc.Id
            );

            if (proc.Is64Bit()) {
                foreach (var module in proc.Modules) {
                    if (((ProcessModule)module).ModuleName == dllName64) {
                        var dllHandle = ((ProcessModule)module).BaseAddress;

                        IntPtr threadHandle = CreateRemoteThread(procHandle, IntPtr.Zero, 0, freeLibrary64, dllHandle, 0, IntPtr.Zero);
                        WaitForSingleObject(threadHandle, 5000);

                        CloseHandle(threadHandle);
                    }
                }
            } else {
                var dllHandle = Get32BitDLLAddress(proc.Id, dllName32);

                IntPtr threadHandle = CreateRemoteThread(procHandle, IntPtr.Zero, 0, freeLibrary32, dllHandle, 0, IntPtr.Zero);
                WaitForSingleObject(threadHandle, 5000);

                CloseHandle(threadHandle);
            }

            CloseHandle(procHandle);
        }

        #region native functions
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess (int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle (string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress (IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx (IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle (IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory (IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread (IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject (IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx (IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType);

        // privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint MEM_RELEASE = 0x00008000;
        const uint PAGE_READWRITE = 4;
        #endregion
    }
}
