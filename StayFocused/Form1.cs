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
                instance.textBoxLog.AppendText(string.Join(" ", entries) + Environment.NewLine);
            }
        }

        void OnWindowCreated (Process proc) {
            if (IsOnBlacklist(proc))
                AddHook(proc);
        }

        void UnloadHooks () {
            foreach (var proc in Process.GetProcesses()) {
                if (proc.Id == ownPid) continue;

                RemoveHook(proc);
            }
        }

        void RemoveHook(Process proc)
        {
            try
            {
                string exeName = proc.MainModule.ModuleName;

                Log("Unloading from", exeName, "(" + proc.Id + ")");
#if (!TESTMODE)
                Injector.UnloadDLL(proc);
#endif
            }
            catch (Exception e)
            {
                //Log("Exception while unloading hook: " + e.Message);
            }
        }

        private void Form1_FormClosed (object sender, FormClosedEventArgs e) {
            winWatcher.Dispose();
        }

        private void buttonExit_Click (object sender, EventArgs e) {
            OnExitClicked(null, null);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            BlackList(newBlacklistedProgram.Text);

            newBlacklistedProgram.Text = "";
        }

        private void blacklistedPrograms_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                int location = blacklistedPrograms.IndexFromPoint(e.Location);

                if (location >= 0)
                {
                    blacklistedPrograms.SelectedIndex = location;
                    blacklistContextMenu.Show(PointToScreen(e.Location));
                }
            }
        }

        private void blacklistedPrograms_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string procName = blacklistedPrograms.Items[e.Index].ToString();

            Process[] processes = Process.GetProcessesByName(procName);

            if (e.NewValue == CheckState.Checked)
            {
                foreach (Process p in processes)
                    AddHook(p);
            }
            else if(e.NewValue == CheckState.Unchecked)
            {
                foreach (Process p in processes)
                    RemoveHook(p);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = blacklistedPrograms.SelectedIndex;

            if (blacklistedPrograms.GetItemCheckState(index) == CheckState.Checked)
            {
                UnBlackList(index);
            }

            blacklistedPrograms.Items.RemoveAt(index);
        }

        private void UnBlackList(int index)
        {
            DisableUI();

            string name = blacklistedPrograms.Items[index].ToString();

            Process[] processes = Process.GetProcessesByName(name);

            foreach (Process p in processes)
                RemoveHook(p);

            EnableUI();
        }

        private void BlackList(string name)
        {
            DisableUI();

            // this triggers an ItemCheckEvent, so we can let that take care of everything
            blacklistedPrograms.Items.Add(name, true);

            EnableUI();
        }

        private void DisableUI()
        {
            newBlacklistedProgram.Enabled = false;
            addButton.Enabled = false;
            blacklistedPrograms.Enabled = false;
        }

        private void EnableUI()
        {

            newBlacklistedProgram.Enabled = true;
            addButton.Enabled = true;
            blacklistedPrograms.Enabled = true;
        }

        private bool AddHook(Process proc)
        {
            try
            {
                string exeName = proc.MainModule.ModuleName;

                Log("Hooking", exeName, "(" + proc.Id + ")");

#if (!TESTMODE)
                Injector.InjectDLL(proc);
#endif

                return true;
            }
            catch (Exception e)
            {
                Log("Failed to hook " + proc.ProcessName + ": " + e.Message);
                return false;
            }

        }

        private bool IsOnBlacklist(Process proc)
        {
            // LINQ isn't readily-available on a CheckedItemCollection, so we do it the old-fashioned way:

            foreach(var item in blacklistedPrograms.CheckedItems)
            {
                try
                {
                    if (item.ToString() == proc.MainModule.ModuleName)
                        return true;
                }
                catch(Exception e)
                {
                    Log("Could not check an application: " + e.Message);
                }
            }

            return false;
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
