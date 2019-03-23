using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    public static class ProcessUtils
    {
        public static bool Is64Bit (this Process process) {
            if (!Environment.Is64BitOperatingSystem)
                return false;
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

            bool isWow64;
            if (!IsWow64Process(process.Handle, out isWow64)) {
                throw new Exception("error getting process info");
            }
            return !isWow64;
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process ([In] IntPtr process, [Out] out bool wow64Process);
    }
}
