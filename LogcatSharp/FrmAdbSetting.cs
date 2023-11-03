using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogcatSharp {
    public partial class FrmAdbSetting : Form {
        public FrmAdbSetting() {
            InitializeComponent();
        }

        public string AdbPath {
            get { return this.textBox1.Text; }
            //private set;
        }

        private void button1_Click(object sender, EventArgs e) {
            DialogResult dialogResult = this.openFileDialog1.ShowDialog();
            if (DialogResult.OK == dialogResult) {
                this.textBox1.Text = this.openFileDialog1.FileName;
            }
        }

        private void btnOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }
    }
}