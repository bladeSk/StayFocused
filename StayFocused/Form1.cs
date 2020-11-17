//#define TESTMODE

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace StayFocused
{
    public partial class Form1 : Form
    {
        WindowWatcher winWatcher;
        uint ownPid;

        Config config;
        
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
            ownPid = (uint)Process.GetCurrentProcess().Id;
            config = new Config();
            winWatcher = new WindowWatcher(OnWindowCreated);
        }

        public static void Log (params object[] entries) {
            if (instance.textBoxLog.InvokeRequired) {
                instance.BeginInvoke(logDelegateInstance, new object[] { entries });
                return;
            }

            if (entries.Length == 1) {
                instance.textBoxLog.AppendText(entries[0].ToString() + Environment.NewLine);
            } else {
                instance.textBoxLog.AppendText(String.Join(" ", entries) + Environment.NewLine);
            }
        }

        void OnWindowCreated (Process proc) {
            if (proc.Id == ownPid) return;

            try {
                string exeName = proc.MainModule.ModuleName;
                string exeNameLower = exeName.ToLowerInvariant();

                if (config.mode == Modes.Whitelist && !config.whitelist.Contains(exeNameLower)) return;
                if (config.mode == Modes.Blacklist && config.blacklist.Contains(exeNameLower)) return;

                Log("Hooking", exeName, "(" + proc.Id + ")");
#if (!TESTMODE)
                Injector.InjectDLL(proc);
#endif
            } catch (Win32Exception) {
                //Console.WriteLine("can't access " + proc.Id);
            } catch (InvalidOperationException) {
                //Console.WriteLine("the process has exited " + proc.Id);
            }
        }

        void UnloadHooks () {
            foreach (var proc in Process.GetProcesses()) {
                if (proc.Id == ownPid) continue;

                try {
                    string exeName = proc.MainModule.ModuleName;
                    string exeNameLower = exeName.ToLowerInvariant();

                    if (config.mode == Modes.Whitelist && !config.whitelist.Contains(exeNameLower)) return;
                    if (config.mode == Modes.Blacklist && config.blacklist.Contains(exeNameLower)) return;

                    Log("Unloading from", exeName, "(" + proc.Id + ")");
#if (!TESTMODE)
                    Injector.UnloadDLL(proc);
#endif
                } catch (Win32Exception) {
                    //Console.WriteLine("can't access " + proc.Id);
                } catch (InvalidOperationException) {
                    //Console.WriteLine("the process has exited " + proc.Id);
                }
            }
        }

        private void Form1_FormClosed (object sender, FormClosedEventArgs e) {
            winWatcher.Dispose();
        }

        private void buttonExit_Click (object sender, EventArgs e) {
            OnExitClicked(null, null);
        }

        private void buttonGotoConfig_Click (object sender, EventArgs e) {
            string args = "/select, \"" + config.GetConfigPath() + "\"";

            Process.Start("explorer.exe", args);
        }

        private void linkHomepage_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("https://github.com/bladeSk/StayFocused");
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
            ComponentResourceManager resources = new ComponentResourceManager(GetType());

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
