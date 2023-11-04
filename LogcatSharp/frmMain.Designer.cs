namespace LogcatSharp
{
	partial class frmMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonClear = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripComboBoxFilterType = new System.Windows.Forms.ToolStripComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panelLeft = new System.Windows.Forms.Panel();
			this.textAdb = new System.Windows.Forms.TextBox();
			this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panelRight = new System.Windows.Forms.Panel();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.toolStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panelLeft.SuspendLayout();
			this.panelRight.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.toolStripButtonStart, this.toolStripButtonStop, this.toolStripSeparator2, this.toolStripButtonClear, this.toolStripSeparator1, this.toolStripLabel1, this.toolStripTextBoxFilter, this.toolStripComboBoxFilterType });
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.toolStrip1.Size = new System.Drawing.Size(865, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.TabStop = true;
			this.toolStrip1.Text = "miniToolStrip";
			// 
			// toolStripButtonStart
			// 
			this.toolStripButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonStart.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStart.Image")));
			this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonStart.Name = "toolStripButtonStart";
			this.toolStripButtonStart.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonStart.Text = "Start";
			this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonStart_Click);
			// 
			// toolStripButtonStop
			// 
			this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonStop.Enabled = false;
			this.toolStripButtonStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStop.Image")));
			this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonStop.Name = "toolStripButtonStop";
			this.toolStripButtonStop.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonStop.Text = "Stop";
			this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonClear
			// 
			this.toolStripButtonClear.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClear.Image")));
			this.toolStripButtonClear.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonClear.Name = "toolStripButtonClear";
			this.toolStripButtonClear.Size = new System.Drawing.Size(58, 22);
			this.toolStripButtonClear.Text = "Clear";
			this.toolStripButtonClear.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(39, 22);
			this.toolStripLabel1.Text = "Filter:";
			// 
			// toolStripTextBoxFilter
			// 
			this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
			this.toolStripTextBoxFilter.Size = new System.Drawing.Size(100, 25);
			// 
			// toolStripComboBoxFilterType
			// 
			this.toolStripComboBoxFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxFilterType.Items.AddRange(new object[] { "Simple", "Regex", "Simple", "Regex" });
			this.toolStripComboBoxFilterType.Name = "toolStripComboBoxFilterType";
			this.toolStripComboBoxFilterType.Size = new System.Drawing.Size(75, 25);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.panelLeft);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.panelRight);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(865, 412);
			this.panel1.TabIndex = 5;
			// 
			// panelLeft
			// 
			this.panelLeft.Controls.Add(this.textAdb);
			this.panelLeft.Controls.Add(this.vScrollBar1);
			this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLeft.Location = new System.Drawing.Point(0, 0);
			this.panelLeft.Name = "panelLeft";
			this.panelLeft.Size = new System.Drawing.Size(639, 412);
			this.panelLeft.TabIndex = 9;
			// 
			// textAdb
			// 
			this.textAdb.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textAdb.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textAdb.Location = new System.Drawing.Point(0, 0);
			this.textAdb.Multiline = true;
			this.textAdb.Name = "textAdb";
			this.textAdb.ReadOnly = true;
			this.textAdb.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textAdb.Size = new System.Drawing.Size(622, 412);
			this.textAdb.TabIndex = 5;
			this.textAdb.WordWrap = false;
			// 
			// vScrollBar1
			// 
			this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
			this.vScrollBar1.Location = new System.Drawing.Point(622, 0);
			this.vScrollBar1.Name = "vScrollBar1";
			this.vScrollBar1.Size = new System.Drawing.Size(17, 412);
			this.vScrollBar1.TabIndex = 6;
			this.vScrollBar1.ValueChanged += new System.EventHandler(this.vScrollBar1_ValueChanged);
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new System.Drawing.Point(639, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(26, 412);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// panelRight
			// 
			this.panelRight.Controls.Add(this.textBox1);
			this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelRight.Enabled = false;
			this.panelRight.Location = new System.Drawing.Point(665, 0);
			this.panelRight.Name = "panelRight";
			this.panelRight.Size = new System.Drawing.Size(200, 412);
			this.panelRight.TabIndex = 6;
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox1.Size = new System.Drawing.Size(200, 412);
			this.textBox1.TabIndex = 8;
			this.textBox1.WordWrap = false;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(865, 412);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.panel1);
			this.Location = new System.Drawing.Point(15, 15);
			this.Name = "frmMain";
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panelLeft.ResumeLayout(false);
			this.panelLeft.PerformLayout();
			this.panelRight.ResumeLayout(false);
			this.panelRight.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.Windows.Forms.Panel panel1;

		#endregion
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButtonClear;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFilterType;
		private System.Windows.Forms.ToolStripButton toolStripButtonStart;
		private System.Windows.Forms.ToolStripButton toolStripButtonStop;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.TextBox textAdb;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
    }
}

