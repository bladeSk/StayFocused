//#define TESTMODE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace StayFocused
{
    public partial class Form1 : Form
    {
        WindowWatcher winWatcher;
        uint ownPid;
        HashSet<string> ignoredExes;
        static Form1 instance;

        delegate void LogCallback (params object[] entries);
        static LogCallback logDelegateInstance;

        public Form1 () {
            InitializeComponent();

            Shown += OnInit;
            InitTrayIcon();
            instance = this;
            logDelegateInstance = new LogCallback(Log);
        }

        void OnInit (object sender, EventArgs e) {
            ignoredExes = new HashSet<string> {
                "explorer.exe",
                "chrome.exe",
                "foobar2000.exe",
                "8's hotkeys2.1.exe",
                "7+ taskbar tweaker.ex2",
            };

            ownPid = (uint)Process.GetCurrentProcess().Id;

            winWatcher = new WindowWatcher(OnWindowCreated);
        }

        public static void Log (params object[] entries) {
            if (instance.textBoxLog.InvokeRequired) {
                instance.BeginInvoke(logDelegateInstance, new object[] { entries });
                return;
            }

            if (entries.Length == 1) {
                instance.textBoxLog.AppendText(entries[0].ToString() + "\n");
            } else {
                instance.textBoxLog.AppendText(String.Join(" ", entries) + "\n");
            }
        }

        void OnWindowCreated (Process proc) {
            if (proc.Id == ownPid) return;

            try {
                string exeName = proc.MainModule.ModuleName;

                if (ignoredExes.Contains(exeName.ToLowerInvariant())) return;

                Log("Hooking", exeName, "(" + proc.Id + ")");
#if (!TESTMODE)
                Injector.InjectDLL(proc);
#endif
            } catch (Win32Exception) {
                //Console.WriteLine("can't access " + proc.Id);
            }
        }

        void UnloadHooks () {
            foreach (var proc in Process.GetProcesses()) {
                if (proc.Id == ownPid) continue;

                try {
                    string exeName = proc.MainModule.ModuleName;

                    if (ignoredExes.Contains(exeName.ToLowerInvariant())) continue;

                    Log("Unloading from", exeName, "(" + proc.Id + ")");
#if (!TESTMODE)
                    Injector.UnloadDLL(proc);
#endif
                } catch (Win32Exception) {
                    //Console.WriteLine("can't access " + proc.Id);
                }
            }
        }

        private void Form1_FormClosed (object sender, FormClosedEventArgs e) {
            winWatcher.Dispose();
        }

        private void buttonExit_Click (object sender, EventArgs e) {
            OnExitClicked(null, null);
        }

        protected override void Dispose (bool isDisposing) {
            if (trayIcon != null) {
                trayIcon.Dispose();
                trayIcon = null;
            }

            if (isDisposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(isDisposing);
        }

#region tray icon
        NotifyIcon trayIcon;
        bool minimizeOnClose = true;

        void InitTrayIcon () {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(GetType());

            var trayMenu = new ContextMenu();
            var openItem = new MenuItem("Open", OnOpenClicked);
            openItem.DefaultItem = true;
            trayMenu.MenuItems.Add(openItem);
            trayMenu.MenuItems.Add("Exit", OnExitClicked);

            trayIcon = new NotifyIcon();
            trayIcon.Text = Text;
            trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            trayIcon.Click += OnOpenClicked;

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            FormClosing += OnFormClosing;
            KeyPress += OnKeyPress;
        }

        void OnFormClosing (object sender, FormClosingEventArgs e) {
            if (minimizeOnClose && e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                Hide();
            }
        }

        void OnKeyPress (object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 27) {
                OnExitClicked(null, null);
            }
        }

        void OnOpenClicked (Object o, EventArgs evtArgs) {
            if (evtArgs != null && evtArgs.GetType() == typeof(MouseEventArgs) && (evtArgs as MouseEventArgs).Button != MouseButtons.Left) return;

            if (Visible) {
                Hide();
            } else {
                Show();
                Activate();
                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.ScrollToCaret();
            }
        }

        void OnExitClicked (Object o, EventArgs evtArgs) {
            if (!Visible) {
                Show();
                Activate();
            }
            trayIcon.Visible = false;
            UnloadHooks();
            minimizeOnClose = false;
            Close();
            Dispose();
        }
#endregion
    }
}
