namespace IIOTS.Test
{
    partial class Form2
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.点位名称 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.地址 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.地址类型 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.时间 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.值 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.缩放后值 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.旧值 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.质量戳 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.备注 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.点位名称,
            this.地址,
            this.地址类型,
            this.时间,
            this.值,
            this.缩放后值,
            this.旧值,
            this.质量戳,
            this.备注});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.Size = new System.Drawing.Size(739, 263);
            this.dataGridView1.TabIndex = 5;
            // 
            // 点位名称
            // 
            this.点位名称.DataPropertyName = "TagName";
            this.点位名称.HeaderText = "点位名称";
            this.点位名称.Name = "点位名称";
            this.点位名称.Width = 150;
            // 
            // 地址
            // 
            this.地址.DataPropertyName = "Address";
            this.地址.HeaderText = "地址";
            this.地址.Name = "地址";
            this.地址.Width = 57;
            // 
            // 地址类型
            // 
            this.地址类型.DataPropertyName = "DataType";
            this.地址类型.HeaderText = "地址类型";
            this.地址类型.Name = "地址类型";
            this.地址类型.Width = 80;
            // 
            // 时间
            // 
            this.时间.DataPropertyName = "Timestamp";
            dataGridViewCellStyle1.Format = "G";
            dataGridViewCellStyle1.NullValue = null;
            this.时间.DefaultCellStyle = dataGridViewCellStyle1;
            this.时间.HeaderText = "时间";
            this.时间.Name = "时间";
            this.时间.Width = 140;
            // 
            // 值
            // 
            this.值.DataPropertyName = "Value";
            this.值.HeaderText = "值";
            this.值.Name = "值";
            this.值.Width = 80;
            // 
            // 缩放后值
            // 
            this.缩放后值.DataPropertyName = "ZoomValue";
            this.缩放后值.HeaderText = "缩放后值";
            this.缩放后值.Name = "缩放后值";
            this.缩放后值.Width = 81;
            // 
            // 旧值
            // 
            this.旧值.DataPropertyName = "OldValue";
            this.旧值.HeaderText = "旧值";
            this.旧值.Name = "旧值";
            this.旧值.Width = 80;
            // 
            // 质量戳
            // 
            this.质量戳.DataPropertyName = "Quality";
            this.质量戳.HeaderText = "质量戳";
            this.质量戳.Name = "质量戳";
            this.质量戳.Width = 69;
            // 
            // 备注
            // 
            this.备注.DataPropertyName = "Description";
            this.备注.HeaderText = "备注";
            this.备注.Name = "备注";
            this.备注.Width = 200;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(739, 154);
            this.listView1.TabIndex = 4;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "260.jpg");
            this.imageList1.Images.SetKeyName(1, "R-C.jpg");
            this.imageList1.Images.SetKeyName(2, "R-C (1).jpg");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(739, 421);
            this.splitContainer1.SplitterDistance = 263;
            this.splitContainer1.TabIndex = 6;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(739, 421);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form2";
            this.Text = "Log";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private ListView listView1;
        private ImageList imageList1;
        private DataGridView dataGridView1;
        private SplitContainer splitContainer1;
        private System.Windows.Forms.Timer timer1;
        private DataGridViewTextBoxColumn 点位名称;
        private DataGridViewTextBoxColumn 地址;
        private DataGridViewTextBoxColumn 地址类型;
        private DataGridViewTextBoxColumn 时间;
        private DataGridViewTextBoxColumn 值;
        private DataGridViewTextBoxColumn 缩放后值;
        private DataGridViewTextBoxColumn 旧值;
        private DataGridViewTextBoxColumn 质量戳;
        private DataGridViewTextBoxColumn 备注;
    }
}