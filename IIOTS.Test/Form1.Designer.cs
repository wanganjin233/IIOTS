namespace IIOTS.Test
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConnButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.IPAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.TextBox();
            this.CloseButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DriveChoose = new System.Windows.Forms.ComboBox();
            this.Ping = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.EncodeR = new System.Windows.Forms.ComboBox();
            this.StringRead = new System.Windows.Forms.Button();
            this.doubleRead = new System.Windows.Forms.Button();
            this.FloatRead = new System.Windows.Forms.Button();
            this.ULongRead = new System.Windows.Forms.Button();
            this.LongRead = new System.Windows.Forms.Button();
            this.UintRead = new System.Windows.Forms.Button();
            this.IntRead = new System.Windows.Forms.Button();
            this.UShortRead = new System.Windows.Forms.Button();
            this.ShortRead = new System.Windows.Forms.Button();
            this.BoolRead = new System.Windows.Forms.Button();
            this.ReadResult = new System.Windows.Forms.TextBox();
            this.Address = new System.Windows.Forms.TextBox();
            this.StringLength = new System.Windows.Forms.TextBox();
            this.Length = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.EncodeW = new System.Windows.Forms.ComboBox();
            this.stringW = new System.Windows.Forms.Button();
            this.doubleW = new System.Windows.Forms.Button();
            this.floatW = new System.Windows.Forms.Button();
            this.UlongW = new System.Windows.Forms.Button();
            this.longW = new System.Windows.Forms.Button();
            this.uintW = new System.Windows.Forms.Button();
            this.intW = new System.Windows.Forms.Button();
            this.UshortW = new System.Windows.Forms.Button();
            this.shortW = new System.Windows.Forms.Button();
            this.BoolW = new System.Windows.Forms.Button();
            this.Waddress = new System.Windows.Forms.TextBox();
            this.WValue = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConnButton
            // 
            this.ConnButton.Location = new System.Drawing.Point(301, 18);
            this.ConnButton.Name = "ConnButton";
            this.ConnButton.Size = new System.Drawing.Size(86, 30);
            this.ConnButton.TabIndex = 0;
            this.ConnButton.Text = "连接";
            this.ConnButton.UseVisualStyleBackColor = true;
            this.ConnButton.Click += new System.EventHandler(this.ConnButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP地址：";
            // 
            // IPAddress
            // 
            this.IPAddress.Location = new System.Drawing.Point(66, 22);
            this.IPAddress.Name = "IPAddress";
            this.IPAddress.Size = new System.Drawing.Size(100, 23);
            this.IPAddress.TabIndex = 2;
            this.IPAddress.Text = "127.0.0.1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(172, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "端口号：";
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(225, 22);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(59, 23);
            this.Port.TabIndex = 2;
            this.Port.Text = "6000";
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(389, 18);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(86, 30);
            this.CloseButton.TabIndex = 0;
            this.CloseButton.Text = "断开";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.DriveChoose);
            this.groupBox1.Controls.Add(this.Ping);
            this.groupBox1.Controls.Add(this.ConnButton);
            this.groupBox1.Controls.Add(this.Port);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.CloseButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.IPAddress);
            this.groupBox1.Location = new System.Drawing.Point(3, -2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(854, 57);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // DriveChoose
            // 
            this.DriveChoose.FormattingEnabled = true;
            this.DriveChoose.Items.AddRange(new object[] {
            "欧姆龙Fins",
            "三菱MC(binary)",
            "ModbusRtu",
            "ModbusRtu(TCP)",
            "Modbus(TCP)"});
            this.DriveChoose.Location = new System.Drawing.Point(481, 22);
            this.DriveChoose.Name = "DriveChoose";
            this.DriveChoose.Size = new System.Drawing.Size(121, 25);
            this.DriveChoose.TabIndex = 6;
            this.DriveChoose.Text = "三菱MC(binary)";
            this.DriveChoose.SelectedIndexChanged += new System.EventHandler(this.DriveChoose_SelectedIndexChanged);
            // 
            // Ping
            // 
            this.Ping.AutoSize = true;
            this.Ping.BackColor = System.Drawing.Color.Transparent;
            this.Ping.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Ping.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.Ping.Location = new System.Drawing.Point(700, 21);
            this.Ping.Name = "Ping";
            this.Ping.Size = new System.Drawing.Size(52, 27);
            this.Ping.TabIndex = 5;
            this.Ping.Text = "0ms";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label5.Location = new System.Drawing.Point(608, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 27);
            this.label5.TabIndex = 5;
            this.label5.Text = "通讯耗时：";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.EncodeR);
            this.groupBox2.Controls.Add(this.StringRead);
            this.groupBox2.Controls.Add(this.doubleRead);
            this.groupBox2.Controls.Add(this.FloatRead);
            this.groupBox2.Controls.Add(this.ULongRead);
            this.groupBox2.Controls.Add(this.LongRead);
            this.groupBox2.Controls.Add(this.UintRead);
            this.groupBox2.Controls.Add(this.IntRead);
            this.groupBox2.Controls.Add(this.UShortRead);
            this.groupBox2.Controls.Add(this.ShortRead);
            this.groupBox2.Controls.Add(this.BoolRead);
            this.groupBox2.Controls.Add(this.ReadResult);
            this.groupBox2.Controls.Add(this.Address);
            this.groupBox2.Controls.Add(this.StringLength);
            this.groupBox2.Controls.Add(this.Length);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(5, 67);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(485, 269);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "数据读取测试";
            // 
            // EncodeR
            // 
            this.EncodeR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EncodeR.FormattingEnabled = true;
            this.EncodeR.Items.AddRange(new object[] {
            "ASCII",
            "Unicode",
            "Unicode-big",
            "UTF8",
            "UTF32",
            "ANSI",
            "GB2312"});
            this.EncodeR.Location = new System.Drawing.Point(394, 225);
            this.EncodeR.Name = "EncodeR";
            this.EncodeR.Size = new System.Drawing.Size(85, 25);
            this.EncodeR.TabIndex = 3;
            this.EncodeR.Text = "ASCII";
            // 
            // StringRead
            // 
            this.StringRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StringRead.Location = new System.Drawing.Point(394, 192);
            this.StringRead.Name = "StringRead";
            this.StringRead.Size = new System.Drawing.Size(86, 30);
            this.StringRead.TabIndex = 0;
            this.StringRead.Text = "字符串读取";
            this.StringRead.UseVisualStyleBackColor = true;
            this.StringRead.Click += new System.EventHandler(this.StringRead_Click);
            // 
            // doubleRead
            // 
            this.doubleRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.doubleRead.Location = new System.Drawing.Point(394, 158);
            this.doubleRead.Name = "doubleRead";
            this.doubleRead.Size = new System.Drawing.Size(86, 30);
            this.doubleRead.TabIndex = 0;
            this.doubleRead.Text = "double读取";
            this.doubleRead.UseVisualStyleBackColor = true;
            this.doubleRead.Click += new System.EventHandler(this.doubleRead_Click);
            // 
            // FloatRead
            // 
            this.FloatRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatRead.Location = new System.Drawing.Point(299, 158);
            this.FloatRead.Name = "FloatRead";
            this.FloatRead.Size = new System.Drawing.Size(86, 30);
            this.FloatRead.TabIndex = 0;
            this.FloatRead.Text = "float读取";
            this.FloatRead.UseVisualStyleBackColor = true;
            this.FloatRead.Click += new System.EventHandler(this.FloatRead_Click);
            // 
            // ULongRead
            // 
            this.ULongRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ULongRead.Location = new System.Drawing.Point(394, 124);
            this.ULongRead.Name = "ULongRead";
            this.ULongRead.Size = new System.Drawing.Size(86, 30);
            this.ULongRead.TabIndex = 0;
            this.ULongRead.Text = "ulong读取";
            this.ULongRead.UseVisualStyleBackColor = true;
            this.ULongRead.Click += new System.EventHandler(this.ULongRead_Click);
            // 
            // LongRead
            // 
            this.LongRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LongRead.Location = new System.Drawing.Point(300, 124);
            this.LongRead.Name = "LongRead";
            this.LongRead.Size = new System.Drawing.Size(86, 30);
            this.LongRead.TabIndex = 0;
            this.LongRead.Text = "long读取";
            this.LongRead.UseVisualStyleBackColor = true;
            this.LongRead.Click += new System.EventHandler(this.LongRead_Click);
            // 
            // UintRead
            // 
            this.UintRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UintRead.Location = new System.Drawing.Point(394, 87);
            this.UintRead.Name = "UintRead";
            this.UintRead.Size = new System.Drawing.Size(86, 30);
            this.UintRead.TabIndex = 0;
            this.UintRead.Text = "uint读取";
            this.UintRead.UseVisualStyleBackColor = true;
            this.UintRead.Click += new System.EventHandler(this.UintRead_Click);
            // 
            // IntRead
            // 
            this.IntRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.IntRead.Location = new System.Drawing.Point(300, 87);
            this.IntRead.Name = "IntRead";
            this.IntRead.Size = new System.Drawing.Size(86, 30);
            this.IntRead.TabIndex = 0;
            this.IntRead.Text = "int读取";
            this.IntRead.UseVisualStyleBackColor = true;
            this.IntRead.Click += new System.EventHandler(this.IntRead_Click);
            // 
            // UShortRead
            // 
            this.UShortRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UShortRead.Location = new System.Drawing.Point(394, 53);
            this.UShortRead.Name = "UShortRead";
            this.UShortRead.Size = new System.Drawing.Size(86, 30);
            this.UShortRead.TabIndex = 0;
            this.UShortRead.Text = "ushort读取";
            this.UShortRead.UseVisualStyleBackColor = true;
            this.UShortRead.Click += new System.EventHandler(this.UShortRead_Click);
            // 
            // ShortRead
            // 
            this.ShortRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShortRead.Location = new System.Drawing.Point(300, 53);
            this.ShortRead.Name = "ShortRead";
            this.ShortRead.Size = new System.Drawing.Size(86, 30);
            this.ShortRead.TabIndex = 0;
            this.ShortRead.Text = "short读取";
            this.ShortRead.UseVisualStyleBackColor = true;
            this.ShortRead.Click += new System.EventHandler(this.ShortRead_Click);
            // 
            // BoolRead
            // 
            this.BoolRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BoolRead.Location = new System.Drawing.Point(300, 19);
            this.BoolRead.Name = "BoolRead";
            this.BoolRead.Size = new System.Drawing.Size(86, 30);
            this.BoolRead.TabIndex = 0;
            this.BoolRead.Text = "bool读取";
            this.BoolRead.UseVisualStyleBackColor = true;
            this.BoolRead.Click += new System.EventHandler(this.BoolRead_Click);
            // 
            // ReadResult
            // 
            this.ReadResult.AcceptsReturn = true;
            this.ReadResult.AcceptsTab = true;
            this.ReadResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReadResult.Location = new System.Drawing.Point(44, 61);
            this.ReadResult.Multiline = true;
            this.ReadResult.Name = "ReadResult";
            this.ReadResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ReadResult.Size = new System.Drawing.Size(250, 199);
            this.ReadResult.TabIndex = 2;
            this.ReadResult.WordWrap = false;
            // 
            // Address
            // 
            this.Address.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Address.Location = new System.Drawing.Point(44, 22);
            this.Address.Name = "Address";
            this.Address.Size = new System.Drawing.Size(185, 23);
            this.Address.TabIndex = 2;
            this.Address.Text = "D100";
            // 
            // StringLength
            // 
            this.StringLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StringLength.Location = new System.Drawing.Point(334, 195);
            this.StringLength.Name = "StringLength";
            this.StringLength.Size = new System.Drawing.Size(51, 23);
            this.StringLength.TabIndex = 2;
            this.StringLength.Text = "10";
            // 
            // Length
            // 
            this.Length.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Length.Location = new System.Drawing.Point(236, 22);
            this.Length.Name = "Length";
            this.Length.Size = new System.Drawing.Size(59, 23);
            this.Length.TabIndex = 2;
            this.Length.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 17);
            this.label4.TabIndex = 1;
            this.label4.Text = "结果：";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(341, 228);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 17);
            this.label7.TabIndex = 1;
            this.label7.Text = "编码：";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(300, 198);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 17);
            this.label6.TabIndex = 1;
            this.label6.Text = "长度：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "地址：";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.EncodeW);
            this.groupBox3.Controls.Add(this.stringW);
            this.groupBox3.Controls.Add(this.doubleW);
            this.groupBox3.Controls.Add(this.floatW);
            this.groupBox3.Controls.Add(this.UlongW);
            this.groupBox3.Controls.Add(this.longW);
            this.groupBox3.Controls.Add(this.uintW);
            this.groupBox3.Controls.Add(this.intW);
            this.groupBox3.Controls.Add(this.UshortW);
            this.groupBox3.Controls.Add(this.shortW);
            this.groupBox3.Controls.Add(this.BoolW);
            this.groupBox3.Controls.Add(this.Waddress);
            this.groupBox3.Controls.Add(this.WValue);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Location = new System.Drawing.Point(511, 67);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(346, 269);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "数据读取测试";
            // 
            // EncodeW
            // 
            this.EncodeW.FormattingEnabled = true;
            this.EncodeW.Items.AddRange(new object[] {
            "ASCII",
            "Unicode",
            "Unicode-big",
            "UTF8",
            "UTF32",
            "ANSI",
            "GB2312"});
            this.EncodeW.Location = new System.Drawing.Point(187, 194);
            this.EncodeW.Name = "EncodeW";
            this.EncodeW.Size = new System.Drawing.Size(70, 25);
            this.EncodeW.TabIndex = 3;
            this.EncodeW.Text = "ASCII";
            // 
            // stringW
            // 
            this.stringW.Location = new System.Drawing.Point(263, 191);
            this.stringW.Name = "stringW";
            this.stringW.Size = new System.Drawing.Size(71, 29);
            this.stringW.TabIndex = 0;
            this.stringW.Text = "字符串写";
            this.stringW.UseVisualStyleBackColor = true;
            this.stringW.Click += new System.EventHandler(this.stringW_Click);
            // 
            // doubleW
            // 
            this.doubleW.Location = new System.Drawing.Point(263, 157);
            this.doubleW.Name = "doubleW";
            this.doubleW.Size = new System.Drawing.Size(71, 29);
            this.doubleW.TabIndex = 0;
            this.doubleW.Text = "double写";
            this.doubleW.UseVisualStyleBackColor = true;
            this.doubleW.Click += new System.EventHandler(this.doubleW_Click);
            // 
            // floatW
            // 
            this.floatW.Location = new System.Drawing.Point(185, 157);
            this.floatW.Name = "floatW";
            this.floatW.Size = new System.Drawing.Size(71, 29);
            this.floatW.TabIndex = 0;
            this.floatW.Text = "float写";
            this.floatW.UseVisualStyleBackColor = true;
            this.floatW.Click += new System.EventHandler(this.floatW_Click);
            // 
            // UlongW
            // 
            this.UlongW.Location = new System.Drawing.Point(263, 123);
            this.UlongW.Name = "UlongW";
            this.UlongW.Size = new System.Drawing.Size(71, 29);
            this.UlongW.TabIndex = 0;
            this.UlongW.Text = "ulong写";
            this.UlongW.UseVisualStyleBackColor = true;
            this.UlongW.Click += new System.EventHandler(this.UlongW_Click);
            // 
            // longW
            // 
            this.longW.Location = new System.Drawing.Point(186, 123);
            this.longW.Name = "longW";
            this.longW.Size = new System.Drawing.Size(71, 29);
            this.longW.TabIndex = 0;
            this.longW.Text = "long写";
            this.longW.UseVisualStyleBackColor = true;
            this.longW.Click += new System.EventHandler(this.longW_Click);
            // 
            // uintW
            // 
            this.uintW.Location = new System.Drawing.Point(263, 86);
            this.uintW.Name = "uintW";
            this.uintW.Size = new System.Drawing.Size(71, 29);
            this.uintW.TabIndex = 0;
            this.uintW.Text = "uint写";
            this.uintW.UseVisualStyleBackColor = true;
            this.uintW.Click += new System.EventHandler(this.uintW_Click);
            // 
            // intW
            // 
            this.intW.Location = new System.Drawing.Point(186, 86);
            this.intW.Name = "intW";
            this.intW.Size = new System.Drawing.Size(71, 29);
            this.intW.TabIndex = 0;
            this.intW.Text = "int写";
            this.intW.UseVisualStyleBackColor = true;
            this.intW.Click += new System.EventHandler(this.intW_Click);
            // 
            // UshortW
            // 
            this.UshortW.Location = new System.Drawing.Point(263, 52);
            this.UshortW.Name = "UshortW";
            this.UshortW.Size = new System.Drawing.Size(71, 29);
            this.UshortW.TabIndex = 0;
            this.UshortW.Text = "ushort写";
            this.UshortW.UseVisualStyleBackColor = true;
            this.UshortW.Click += new System.EventHandler(this.UshortW_Click);
            // 
            // shortW
            // 
            this.shortW.Location = new System.Drawing.Point(186, 52);
            this.shortW.Name = "shortW";
            this.shortW.Size = new System.Drawing.Size(71, 29);
            this.shortW.TabIndex = 0;
            this.shortW.Text = "short写";
            this.shortW.UseVisualStyleBackColor = true;
            this.shortW.Click += new System.EventHandler(this.shortW_Click);
            // 
            // BoolW
            // 
            this.BoolW.Location = new System.Drawing.Point(185, 16);
            this.BoolW.Name = "BoolW";
            this.BoolW.Size = new System.Drawing.Size(71, 29);
            this.BoolW.TabIndex = 0;
            this.BoolW.Text = "bool写";
            this.BoolW.UseVisualStyleBackColor = true;
            this.BoolW.Click += new System.EventHandler(this.BoolW_Click);
            // 
            // Waddress
            // 
            this.Waddress.Location = new System.Drawing.Point(44, 22);
            this.Waddress.Name = "Waddress";
            this.Waddress.Size = new System.Drawing.Size(103, 23);
            this.Waddress.TabIndex = 2;
            this.Waddress.Text = "D100";
            // 
            // WValue
            // 
            this.WValue.Location = new System.Drawing.Point(44, 61);
            this.WValue.Name = "WValue";
            this.WValue.Size = new System.Drawing.Size(103, 23);
            this.WValue.TabIndex = 2;
            this.WValue.Text = "1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 17);
            this.label8.TabIndex = 1;
            this.label8.Text = "结果：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(137, 198);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 17);
            this.label9.TabIndex = 1;
            this.label9.Text = "编码：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(44, 17);
            this.label11.TabIndex = 1;
            this.label11.Text = "地址：";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(774, 349);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 30);
            this.button1.TabIndex = 0;
            this.button1.Text = "short读取";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(5, 349);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(763, 23);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "D100";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(774, 396);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(86, 30);
            this.button2.TabIndex = 0;
            this.button2.Text = "short读取";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 473);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button ConnButton;
        private Label label1;
        private TextBox IPAddress;
        private Label label2;
        private TextBox Port;
        private Button CloseButton;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Button ShortRead;
        private Button BoolRead;
        private TextBox ReadResult;
        private TextBox Address;
        private TextBox Length;
        private Label label4;
        private Label label3;
        private Button UShortRead;
        private Label label5;
        private Label Ping;
        private ComboBox EncodeR;
        private Button StringRead;
        private Button doubleRead;
        private Button FloatRead;
        private Button ULongRead;
        private Button LongRead;
        private Button UintRead;
        private Button IntRead;
        private TextBox StringLength;
        private Label label7;
        private Label label6;
        private GroupBox groupBox3;
        private ComboBox EncodeW;
        private Button stringW;
        private Button doubleW;
        private Button floatW;
        private Button UlongW;
        private Button longW;
        private Button uintW;
        private Button intW;
        private Button UshortW;
        private Button shortW;
        private Button BoolW;
        private TextBox Waddress;
        private TextBox WValue;
        private Label label8;
        private Label label9;
        private Label label11;
        private ComboBox DriveChoose;
        private Button button1;
        private TextBox textBox1;
        private Button button2;
    }
}