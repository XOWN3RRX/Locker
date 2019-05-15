using Gma.System.MouseKeyHook;
using Locker_v1.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Locker_v1
{
    public partial class MainForm : Form
    {
        private IKeyboardMouseEvents m_Events;
        private bool isEnabled = false;
        private Action actiune;
        private PassRecorder pass;
        private Combination combination;
        private Dictionary<Combination, Action> assignment;
        private bool err = false;
        bool lastMouse = false;
        MouseState mouseState;

        public MainForm()
        {
            InitializeComponent();
            pass = new PassRecorder();
            pass.Password = Settings.Default.Password;
            pass.OnValidPassword += Pass_OnValidPassword;
            LoadAction();
        }

        private void Pass_OnValidPassword(object sender, EventArgs e)
        {
            if (isEnabled)
            {
                isEnabled = false;
                StartHookKeyboard(isEnabled);
                StartHookMouse(isEnabled);
                notifyIcon1.Icon = Icon.FromHandle(Resources.k_silver.GetHicon());

                if (Settings.Default.TaskMgr)
                {
                    ToggleTaskManager(false);
                }

                textBox1.Text = textBox1.Text.Replace(pass.Password, "");
                textBox1.Select(0, 0);

                notifyIcon1.ShowBalloonTip(2000, "Locker", "Locker stopped...", ToolTipIcon.Info);
            }
        }

        private void LoadAction()
        {
            actiune = delegate ()
            {
                if (!string.IsNullOrWhiteSpace(pass.Password))
                {
                    if (m_Events != null)
                    {
                        if (isEnabled == false)
                        {

                            isEnabled = true;
                            StartHookKeyboard(isEnabled);
                            StartHookMouse(isEnabled);

                            if (Settings.Default.TaskMgr)
                            {
                                ToggleTaskManager(true);
                            }

                            notifyIcon1.ShowBalloonTip(2000, "Locker", "Locker started...", ToolTipIcon.Info);

                            if (Settings.Default.AutoClear)
                            {
                                textBox1.Clear();
                            }

                            notifyIcon1.Icon = Icon.FromHandle(Resources.k_red.GetHicon());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Start listening first.");
                    }
                }
                else
                {
                    MessageBox.Show("Password don't exists!");
                }
            };
        }

        private void newShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Unsubscribe();
            Shortcut shortcut = new Shortcut();
            shortcut.ShowDialog();
            pass.Password = Settings.Default.Password;
            //GlobalSubscribe();
        }

        private void currentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.Combination))
            {
                MessageBox.Show(Settings.Default.Combination);
            }
            else
            {
                MessageBox.Show("Undefined, please set new shortcut.");
            }
        }

        private void stoppedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Text == "Stopped")
            {
                GlobalSubscribe();
                stoppedToolStripMenuItem.Text = "Started";
                stoppedToolStripMenuItem1.Text = "Started";
                stoppedToolStripMenuItem.Checked = true;
                stoppedToolStripMenuItem1.Checked = true;
            }
            else
            {
                Unsubscribe();
                stoppedToolStripMenuItem.Text = "Stopped";
                stoppedToolStripMenuItem1.Text = "Stopped";
                stoppedToolStripMenuItem.Checked = false;
                stoppedToolStripMenuItem1.Checked = false;
            }

            Settings.Default.HookStatus = (sender as ToolStripMenuItem).Checked;
            Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Settings.Default.HookStatus)
            {
                stoppedToolStripMenuItem.Text = "Started";
                stoppedToolStripMenuItem.Checked = true;
                stoppedToolStripMenuItem1.Text = "Started";
                stoppedToolStripMenuItem1.Checked = true;
                GlobalSubscribe();
            }

            notifyIcon1.Icon = Icon.FromHandle(Resources.k_silver.GetHicon());

            TaskManagerStatus();
            if (err)
            {
                taskManagerToolStripMenuItem.Checked = false;
            }
            else
            {
                if (Settings.Default.TaskMgr)
                {
                    taskManagerToolStripMenuItem.Checked = true;
                }
            }

            if (Settings.Default.AutoClear)
            {
                autoClearToolStripMenuItem.Checked = true;
            }
        }

        private void GlobalSubscribe()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            LoadAction();
            isEnabled = false;

            if (!string.IsNullOrWhiteSpace(Settings.Default.Combination))
            {
                try
                {
                    combination = Combination.FromString(Settings.Default.Combination);
                    assignment = new Dictionary<Combination, Action> { { combination, actiune } };
                    m_Events.OnCombination(assignment);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid Combination, please change.");
                    Unsubscribe();
                }
            }
        }

        private void Unsubscribe()
        {
            if (m_Events == null)
            {
                return;
            }

            stoppedToolStripMenuItem.Text = "Stopped";
            stoppedToolStripMenuItem.Checked = false;
            stoppedToolStripMenuItem1.Text = "Stopped";
            stoppedToolStripMenuItem1.Checked = false;

            m_Events.Dispose();
            m_Events = null;
        }

        private void StartHookKeyboard(bool enabled)
        {
            if (enabled)
            {
                m_Events.KeyDown += OnKeyDown;
            }
            else
            {
                m_Events.KeyDown -= OnKeyDown;
            }
        }

        private void StartHookMouse(bool enabled)
        {
            if (enabled)
            {
                m_Events.MouseUpExt += HookManager_Supress;
                m_Events.MouseDownExt += HookManager_Supress;
                m_Events.MouseWheelExt += M_Events_MouseWheelExt;
            }
            else
            {
                m_Events.MouseUpExt -= HookManager_Supress;
                m_Events.MouseDownExt -= HookManager_Supress;
                m_Events.MouseWheelExt -= M_Events_MouseWheelExt;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(mouseState == MouseState.State2)
            {
                mouseState = MouseState.State1;
            }

            Log(string.Format("{0}", (char)e.KeyValue));

            if (isEnabled)
            {
                pass.Add((char)e.KeyValue);
            }

            e.Handled = true;
        }

        private void Log(string text)
        {
            if (IsDisposed)
            {
                return;
            }

            if(mouseState == MouseState.State1)
            {
                mouseState = MouseState.State0;
                textBox1.AppendText(Environment.NewLine);
            }

            if(mouseState == MouseState.State2)
            {
                textBox1.AppendText(Environment.NewLine);
            }

            textBox1.AppendText(text);
            textBox1.ScrollToCaret();
        }

        private void HookManager_Supress(object sender, MouseEventExtArgs e)
        {
            mouseState = MouseState.State2;
            Log(string.Format("{0}\tX : {1}\tY : {2}", e.Button, e.X, e.Y));
            e.Handled = true;
        }

        private void M_Events_MouseWheelExt(object sender, MouseEventExtArgs e)
        {
            e.Handled = true;
        }

        private void passwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasswordForm passwordForm = new PasswordForm();
            passwordForm.ShowDialog();

            if (!string.IsNullOrWhiteSpace(Settings.Default.Password))
            {
                pass.Password = Settings.Default.Password;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.Focus();
            this.WindowState = FormWindowState.Normal;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.notifyIcon1.Visible = true;
            }
            else
            {
                this.notifyIcon1.Visible = false;
            }
        }

        private void taskManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TaskManagerStatus();
            if (err)
            {
                taskManagerToolStripMenuItem.Checked = false;
            }
            else
            {
                taskManagerToolStripMenuItem.Checked = true;
            }

            Settings.Default.TaskMgr = taskManagerToolStripMenuItem.Checked;
            Settings.Default.Save();
        }

        private void ToggleTaskManager(bool enabled)
        {
            try
            {
                RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");

                if (enabled)
                {
                    if (objRegistryKey.GetValue("DisableTaskMgr") == null)
                    {
                        objRegistryKey.SetValue("DisableTaskMgr", "1");
                    }
                }
                else
                {
                    objRegistryKey.DeleteValue("DisableTaskMgr");
                }

                objRegistryKey.Close();
            }
            catch { }
        }

        private bool TaskManagerStatus()
        {
            try
            {
                err = false;
                RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");

                if (objRegistryKey.GetValue("DisableTaskMgr") == null)
                {
                    objRegistryKey.Close();
                    return false;
                }
                else
                {
                    objRegistryKey.Close();
                    return true;
                }
            }
            catch
            {
                err = true;
                return false;
            }
        }

        private void opemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Dispose();
            Process.GetCurrentProcess().Kill();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void autoClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender as ToolStripMenuItem).Checked)
            {
                (sender as ToolStripMenuItem).Checked = true;
            }
            else
            {
                (sender as ToolStripMenuItem).Checked = false;
            }

            Settings.Default.AutoClear = (sender as ToolStripMenuItem).Checked;
            Settings.Default.Save();
        }
    }
}
