namespace Zony.Lib.NCMConverter.UI
{
    partial class NCMConverter
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
            this.listView1 = new Zony.Lib.UIComponents.ListViewNF();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_selectFolder = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(554, 374);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "文件路径";
            this.columnHeader1.Width = 457;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "转换状态";
            // 
            // button_selectFolder
            // 
            this.button_selectFolder.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_selectFolder.Location = new System.Drawing.Point(573, 13);
            this.button_selectFolder.Name = "button_selectFolder";
            this.button_selectFolder.Size = new System.Drawing.Size(152, 75);
            this.button_selectFolder.TabIndex = 1;
            this.button_selectFolder.Text = "选择文件夹";
            this.button_selectFolder.UseVisualStyleBackColor = true;
            this.button_selectFolder.Click += new System.EventHandler(this.button_selectFolder_Click);
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold);
            this.button_start.Location = new System.Drawing.Point(573, 94);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(152, 75);
            this.button_start.TabIndex = 1;
            this.button_start.Text = "开始转换";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // NCMConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 398);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.button_selectFolder);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NCMConverter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "网易云 NCM 格式转 MP3/FLAC";
            this.ResumeLayout(false);

        }

        #endregion

        private Zony.Lib.UIComponents.ListViewNF listView1;
        private System.Windows.Forms.Button button_selectFolder;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}