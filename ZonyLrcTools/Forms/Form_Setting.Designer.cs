namespace ZonyLrcTools.Forms
{
    partial class Form_Setting
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
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBox_DownloadThreadNum = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_IsCheckUpdate = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBox_IsReplaceLyricFile = new System.Windows.Forms.CheckBox();
            this.textBox_ExtensionsName = new System.Windows.Forms.TextBox();
            this.comboBox_EncodingPages = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textBox_PluginOptions = new System.Windows.Forms.TextBox();
            this.button_SaveAndExit = new System.Windows.Forms.Button();
            this.button_selectProxiesFile = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 11);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(239, 126);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button_selectProxiesFile);
            this.tabPage1.Controls.Add(this.textBox_DownloadThreadNum);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.textBox2);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.checkBox_IsCheckUpdate);
            this.tabPage1.Controls.Add(this.checkBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(231, 100);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "网络";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBox_DownloadThreadNum
            // 
            this.textBox_DownloadThreadNum.Location = new System.Drawing.Point(70, 72);
            this.textBox_DownloadThreadNum.Name = "textBox_DownloadThreadNum";
            this.textBox_DownloadThreadNum.Size = new System.Drawing.Size(62, 21);
            this.textBox_DownloadThreadNum.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "下载线程:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(70, 48);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(62, 21);
            this.textBox2.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "代理端口:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(70, 24);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(98, 21);
            this.textBox1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "代理地址:";
            // 
            // checkBox_IsCheckUpdate
            // 
            this.checkBox_IsCheckUpdate.AutoSize = true;
            this.checkBox_IsCheckUpdate.Location = new System.Drawing.Point(146, 6);
            this.checkBox_IsCheckUpdate.Name = "checkBox_IsCheckUpdate";
            this.checkBox_IsCheckUpdate.Size = new System.Drawing.Size(72, 16);
            this.checkBox_IsCheckUpdate.TabIndex = 0;
            this.checkBox_IsCheckUpdate.Text = "检测更新";
            this.checkBox_IsCheckUpdate.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(9, 6);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "网络代理";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.checkBox_IsReplaceLyricFile);
            this.tabPage2.Controls.Add(this.textBox_ExtensionsName);
            this.tabPage2.Controls.Add(this.comboBox_EncodingPages);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(231, 100);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "输出";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox_IsReplaceLyricFile
            // 
            this.checkBox_IsReplaceLyricFile.AutoSize = true;
            this.checkBox_IsReplaceLyricFile.Location = new System.Drawing.Point(9, 54);
            this.checkBox_IsReplaceLyricFile.Name = "checkBox_IsReplaceLyricFile";
            this.checkBox_IsReplaceLyricFile.Size = new System.Drawing.Size(96, 16);
            this.checkBox_IsReplaceLyricFile.TabIndex = 3;
            this.checkBox_IsReplaceLyricFile.Text = "覆盖已有歌词";
            this.checkBox_IsReplaceLyricFile.UseVisualStyleBackColor = true;
            // 
            // textBox_ExtensionsName
            // 
            this.textBox_ExtensionsName.Location = new System.Drawing.Point(70, 30);
            this.textBox_ExtensionsName.Name = "textBox_ExtensionsName";
            this.textBox_ExtensionsName.Size = new System.Drawing.Size(155, 21);
            this.textBox_ExtensionsName.TabIndex = 2;
            // 
            // comboBox_EncodingPages
            // 
            this.comboBox_EncodingPages.FormattingEnabled = true;
            this.comboBox_EncodingPages.Location = new System.Drawing.Point(70, 6);
            this.comboBox_EncodingPages.Name = "comboBox_EncodingPages";
            this.comboBox_EncodingPages.Size = new System.Drawing.Size(155, 20);
            this.comboBox_EncodingPages.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "搜索后缀:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "输出编码:";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textBox_PluginOptions);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(231, 100);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "插件配置";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textBox_PluginOptions
            // 
            this.textBox_PluginOptions.Location = new System.Drawing.Point(6, 6);
            this.textBox_PluginOptions.Multiline = true;
            this.textBox_PluginOptions.Name = "textBox_PluginOptions";
            this.textBox_PluginOptions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_PluginOptions.Size = new System.Drawing.Size(222, 92);
            this.textBox_PluginOptions.TabIndex = 0;
            // 
            // button_SaveAndExit
            // 
            this.button_SaveAndExit.Location = new System.Drawing.Point(172, 143);
            this.button_SaveAndExit.Name = "button_SaveAndExit";
            this.button_SaveAndExit.Size = new System.Drawing.Size(75, 21);
            this.button_SaveAndExit.TabIndex = 1;
            this.button_SaveAndExit.Text = "保存并应用";
            this.button_SaveAndExit.UseVisualStyleBackColor = true;
            this.button_SaveAndExit.Click += new System.EventHandler(this.button_SaveAndExit_Click);
            // 
            // button_selectProxiesFile
            // 
            this.button_selectProxiesFile.Location = new System.Drawing.Point(174, 24);
            this.button_selectProxiesFile.Name = "button_selectProxiesFile";
            this.button_selectProxiesFile.Size = new System.Drawing.Size(44, 21);
            this.button_selectProxiesFile.TabIndex = 3;
            this.button_selectProxiesFile.Text = "...";
            this.button_selectProxiesFile.UseVisualStyleBackColor = true;
            this.button_selectProxiesFile.Click += new System.EventHandler(this.button_selectProxiesFile_Click);
            // 
            // Form_Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(258, 176);
            this.Controls.Add(this.button_SaveAndExit);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form_Setting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置";
            this.Load += new System.EventHandler(this.Form_Setting_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox_DownloadThreadNum;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_SaveAndExit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_EncodingPages;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_ExtensionsName;
        private System.Windows.Forms.CheckBox checkBox_IsReplaceLyricFile;
        private System.Windows.Forms.CheckBox checkBox_IsCheckUpdate;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox textBox_PluginOptions;
        private System.Windows.Forms.Button button_selectProxiesFile;
    }
}