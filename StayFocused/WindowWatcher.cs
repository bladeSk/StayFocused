using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace StayFocused
{
    class WindowWatcher: IDisposable
    {
        public delegate void ProcessStartedCallback (Process process);

        ProcessStartedCallback onProcessStarted;
        HashSet<int> activeProcesses = new HashSet<int>();
        IntPtr hookPtr;
        Timer pruneTimer;
        WinEventDelegate winDelegate; // store the delegate to prevent garbage collection when using the callback in a native function

        public WindowWatcher (ProcessStartedCallback onProcessStarted) {
            this.onProcessStarted = onProcessStarted;

            ProcessCurrentProcesses();

            winDelegate = new WinEventDelegate(OnWinEvent);

            hookPtr = SetWinEventHook(EVENT_OBJECT_CREATE, EVENT_OBJECT_HIDE, IntPtr.Zero, winDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            pruneTimer = new Timer();
            pruneTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e) {
                PruneDeadPids();
            });
            pruneTimer.Interval = 30000;
            pruneTimer.Enabled = true;
        }

        void ProcessCurrentProcesses () {
            foreach (var proc in Process.GetProcesses()) {
                if (activeProcesses.Contains(proc.Id)) return;

                activeProcesses.Add(proc.Id);
                onProcessStarted(proc);
            }
        }

        void OnWinEvent (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            if (idObject != 0) return; // not a window

            if (eventType == EVENT_OBJECT_CREATE) {
                uint uintPid;
                GetWindowThreadProcessId(hwnd, out uintPid);
                int pid = (int)uintPid;

                if (activeProcesses.Contains(pid)) return;
                activeProcesses.Add(pid);

                onProcessStarted(Process.GetProcessById(pid));
            }
        }

        void PruneDeadPids () {
            var currentProcesses = new HashSet<int>();
            foreach (var proc in Process.GetProcesses()) {
                currentProcesses.Add(proc.Id);
            }

            foreach (var pid in activeProcesses) {
                if (!currentProcesses.Contains(pid)) {
                    Form1.Log("Process exited", "(" + pid + ")");
                }
            }

            activeProcesses = new HashSet<int>(activeProcesses.Where((pid) => currentProcesses.Contains(pid)));
        }

        #region IDisposable pattern implementation
        private bool disposeCalled = false; // To detect redundant calls

        protected virtual void Dispose (bool freeManaged) {
            if (!disposeCalled) {
                if (freeManaged) {
                    // dispose managed state (managed objects).
                    pruneTimer.Stop();
                }

                // free unmanaged resources (unmanaged objects)
                UnhookWinEvent(hookPtr);

                disposeCalled = true;
            }
        }

        ~WindowWatcher() {
           Dispose(false);
        }

        public void Dispose () {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region native functions
        delegate bool EnumWindowsProc (IntPtr hWnd, int lParam);
        delegate void WinEventDelegate (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows (IntPtr hDesktop, EnumWindowsProc lpfn, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook (uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent (IntPtr hWinEventHook);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId (IntPtr hWnd, out uint processId);

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;
        const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;
        const uint EVENT_OBJECT_CREATE = 0x8000;
        const uint EVENT_OBJECT_DESTROY = 0x8001;
        const uint EVENT_OBJECT_SHOW = 0x8002;
        const uint EVENT_OBJECT_HIDE = 0x8003;
        const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        const int GA_PARENT = 1;
        const int GA_ROOT = 2;
        const int GA_ROOTOWNER = 3;
        #endregion
    }
}
