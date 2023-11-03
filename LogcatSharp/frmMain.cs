using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LogcatSharp {
    public partial class frmMain : Form {
        string adbPath = string.Empty;

        public frmMain() {
            InitializeComponent();
        }

        Process adb = null;

        private void frmMain_Load(object sender, EventArgs e) {
            this.toolStripComboBoxFilterType.Text = "Simple";

            this.adbPath = Settings.Instance.GetPath();

            if (string.IsNullOrEmpty(adbPath) || !System.IO.File.Exists(adbPath)) {
                FrmAdbSetting setting = new FrmAdbSetting();
                if (setting.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    this.adbPath = setting.AdbPath;
                    Settings.Instance.SavePath(setting.AdbPath);
                    return;
                }
                else {
                    MessageBox.Show(this,
                        "Could not find adb.exe.  Please install the SDK and Emulator and try again!  Alternatively you can set the path manually adb-path.txt",
                        "Failed to find adb.exe", MessageBoxButtons.OK);
                    this.Close();
                    Application.Exit();
                }


                FrmAdbFinder faf = new FrmAdbFinder();
                if (faf.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    this.adbPath = faf.AdbPath;
                    Settings.Instance.SavePath(faf.AdbPath);
                }
                else {
                    MessageBox.Show(this,
                        "Could not find adb.exe.  Please install the SDK and Emulator and try again!  Alternatively you can set the path manually adb-path.txt",
                        "Failed to find adb.exe", MessageBoxButtons.OK);

                    this.Close();
                    Application.Exit();
                }
            }
        }

        void clear() {
            if (adb == null || adb.HasExited) {
                doClear();
            }
        }

        private void doClear() {
            try {
                var adb = new Process();
                adb.StartInfo.UseShellExecute = false;
                adb.StartInfo.FileName = Settings.Instance.GetPath();
                adb.StartInfo.Arguments = "logcat -c";
                adb.StartInfo.CreateNoWindow = true;
                adb.Start();
                adb.WaitForExit(1500);
            }
            catch {
            }
        }

        void stop() {
            this.toolStripButtonStop.Enabled = false;

            if (adb != null && !adb.HasExited) {
                adb.Kill();
            }

            this.toolStripButtonStart.Enabled = true;
        }

        void start() {
            this.toolStripButtonStart.Enabled = false;

            if (adb != null && !adb.HasExited)
                return;

            var startInfoArguments = "logcat -v threadtime";
            adb = new Process();
            adb.StartInfo.UseShellExecute = false;
            adb.StartInfo.FileName = Settings.Instance.GetPath();
            adb.StartInfo.Arguments = startInfoArguments;
            adb.StartInfo.CreateNoWindow = true;
            
            adb.StartInfo.RedirectStandardOutput = true;
            adb.StartInfo.RedirectStandardError = true;
            adb.EnableRaisingEvents = true;

            var adbOnErrorDataReceived = new DataReceivedEventHandler(adb_ErrorDataReceived);
            var adbOnOutputDataReceived = new DataReceivedEventHandler(adb_OutputDataReceived);
            
            adb.ErrorDataReceived += adbOnErrorDataReceived;
            adb.OutputDataReceived += adbOnOutputDataReceived;
            this.Closing += (sender, args) => {
                adb.ErrorDataReceived -= adbOnErrorDataReceived;
                adb.OutputDataReceived -= adbOnOutputDataReceived;
                adb.CancelErrorRead();
                adb.CancelOutputRead();
                adb.Close();
            };


            try {
                var started = adb.Start();
            }
            catch (Exception ex) {
                this.toolStripButtonStart.Enabled = true;
                this.toolStripButtonStop.Enabled = false;

                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            adb.BeginErrorReadLine();
            adb.BeginOutputReadLine();
            

            this.toolStripButtonStop.Enabled = true;
        }

        void adb_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (this.Disposing)
                return;

            if (this.InvokeRequired) {
                if (this.Disposing)
                    return;
                this.Invoke(new Action<DataReceivedEventArgs>(NewMethod), e);
                //this.Invoke(new Action<object, DataReceivedEventArgs>(adb_OutputDataReceived), sender, e);
            }
            else
                NewMethod(e);
        }

        private void NewMethod(DataReceivedEventArgs e) {
            textAdb.AppendText(filterData(e.Data)); //System.InvalidOperationException: 线程间操作无效: 从不是创建控件“textAdb”的线程访问它。
        }

        void adb_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (this.Disposing)
                return;

            if (this.InvokeRequired) {
                this.Invoke(new Action<DataReceivedEventArgs>(NewMethod), e);
                //this.Invoke(new Action<object, DataReceivedEventArgs>(adb_ErrorDataReceived), sender, e);
            }
            else
                NewMethod(e);
        }

        private void toolStripButton1_Click(object sender, EventArgs e) {
            stop();
            //clear();
            doClear();
            start();
        }

        string filterData(string data) {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;

            if (!string.IsNullOrEmpty(this.toolStripTextBoxFilter.Text)) {
                var pattern = !this.toolStripComboBoxFilterType.Text.Equals("Regex")
                    ? this.toolStripTextBoxFilter.Text.Replace("*", ".*")
                    : this.toolStripTextBoxFilter.Text;
                System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(pattern,
                    System.Text.RegularExpressions.RegexOptions.Singleline |
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (rx.IsMatch(data))
                    return data.Trim() + Environment.NewLine;
                else
                    return string.Empty;
            }

            return data.Trim() + Environment.NewLine;
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e) {
            start();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e) {
            stop();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }
    }
}