namespace SDK.UIForm.WebRTC
{
    partial class FormScreen
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
            this.cb_Screens = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pb_VideoStream = new System.Windows.Forms.PictureBox();
            this.cb_ScreenSize = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_ScreenFps = new System.Windows.Forms.TextBox();
            this.lbl_ScreenMaxFps = new System.Windows.Forms.Label();
            this.btn_ScreenSet = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_NameOfNewInputStream = new System.Windows.Forms.TextBox();
            this.btn_AddNewInputStream = new System.Windows.Forms.Button();
            this.btn_RefreshScreen = new System.Windows.Forms.Button();
            this.btn_StartScreen = new System.Windows.Forms.Button();
            this.btn_StopScreen = new System.Windows.Forms.Button();
            this.lbl_Info = new System.Windows.Forms.Label();
            this.cb_ScreenPrimary = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).BeginInit();
            this.SuspendLayout();
            // 
            // cb_Screens
            // 
            this.cb_Screens.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_Screens.FormattingEnabled = true;
            this.cb_Screens.Location = new System.Drawing.Point(90, 12);
            this.cb_Screens.Name = "cb_Screens";
            this.cb_Screens.Size = new System.Drawing.Size(174, 23);
            this.cb_Screens.TabIndex = 146;
            this.cb_Screens.SelectedIndexChanged += new System.EventHandler(this.cb_Screens_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 15);
            this.label2.TabIndex = 145;
            this.label2.Text = "Screen:";
            // 
            // pb_VideoStream
            // 
            this.pb_VideoStream.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_VideoStream.Location = new System.Drawing.Point(5, 145);
            this.pb_VideoStream.Name = "pb_VideoStream";
            this.pb_VideoStream.Size = new System.Drawing.Size(320, 180);
            this.pb_VideoStream.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_VideoStream.TabIndex = 229;
            this.pb_VideoStream.TabStop = false;
            // 
            // cb_ScreenSize
            // 
            this.cb_ScreenSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_ScreenSize.Enabled = false;
            this.cb_ScreenSize.FormattingEnabled = true;
            this.cb_ScreenSize.Location = new System.Drawing.Point(90, 41);
            this.cb_ScreenSize.Name = "cb_ScreenSize";
            this.cb_ScreenSize.Size = new System.Drawing.Size(119, 23);
            this.cb_ScreenSize.TabIndex = 230;
            this.cb_ScreenSize.SelectedIndexChanged += new System.EventHandler(this.cb_ScreenSize_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 15);
            this.label1.TabIndex = 231;
            this.label1.Text = "Size:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(58, 74);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 13);
            this.label8.TabIndex = 233;
            this.label8.Text = "Fps:";
            // 
            // tb_ScreenFps
            // 
            this.tb_ScreenFps.Enabled = false;
            this.tb_ScreenFps.Location = new System.Drawing.Point(89, 70);
            this.tb_ScreenFps.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_ScreenFps.Name = "tb_ScreenFps";
            this.tb_ScreenFps.Size = new System.Drawing.Size(43, 23);
            this.tb_ScreenFps.TabIndex = 232;
            // 
            // lbl_ScreenMaxFps
            // 
            this.lbl_ScreenMaxFps.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_ScreenMaxFps.Location = new System.Drawing.Point(136, 74);
            this.lbl_ScreenMaxFps.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ScreenMaxFps.Name = "lbl_ScreenMaxFps";
            this.lbl_ScreenMaxFps.Size = new System.Drawing.Size(72, 13);
            this.lbl_ScreenMaxFps.TabIndex = 234;
            this.lbl_ScreenMaxFps.Text = "Max 0 Fps";
            // 
            // btn_ScreenSet
            // 
            this.btn_ScreenSet.Enabled = false;
            this.btn_ScreenSet.FlatAppearance.BorderSize = 0;
            this.btn_ScreenSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_ScreenSet.Location = new System.Drawing.Point(210, 70);
            this.btn_ScreenSet.Margin = new System.Windows.Forms.Padding(0);
            this.btn_ScreenSet.Name = "btn_ScreenSet";
            this.btn_ScreenSet.Size = new System.Drawing.Size(54, 23);
            this.btn_ScreenSet.TabIndex = 365;
            this.btn_ScreenSet.Text = "Set";
            this.btn_ScreenSet.UseVisualStyleBackColor = true;
            this.btn_ScreenSet.Click += new System.EventHandler(this.btn_ScreenSet_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 15);
            this.label6.TabIndex = 368;
            this.label6.Text = "Name:";
            // 
            // tb_NameOfNewInputStream
            // 
            this.tb_NameOfNewInputStream.Location = new System.Drawing.Point(58, 116);
            this.tb_NameOfNewInputStream.Name = "tb_NameOfNewInputStream";
            this.tb_NameOfNewInputStream.Size = new System.Drawing.Size(190, 23);
            this.tb_NameOfNewInputStream.TabIndex = 367;
            // 
            // btn_AddNewInputStream
            // 
            this.btn_AddNewInputStream.Location = new System.Drawing.Point(254, 116);
            this.btn_AddNewInputStream.Name = "btn_AddNewInputStream";
            this.btn_AddNewInputStream.Size = new System.Drawing.Size(71, 23);
            this.btn_AddNewInputStream.TabIndex = 366;
            this.btn_AddNewInputStream.Text = "Add";
            this.btn_AddNewInputStream.UseVisualStyleBackColor = true;
            this.btn_AddNewInputStream.Click += new System.EventHandler(this.btn_AddNewInputStream_Click);
            // 
            // btn_RefreshScreen
            // 
            this.btn_RefreshScreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_RefreshScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_RefreshScreen.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_RefreshScreen.Location = new System.Drawing.Point(309, 16);
            this.btn_RefreshScreen.Name = "btn_RefreshScreen";
            this.btn_RefreshScreen.Size = new System.Drawing.Size(16, 16);
            this.btn_RefreshScreen.TabIndex = 369;
            this.btn_RefreshScreen.UseVisualStyleBackColor = true;
            this.btn_RefreshScreen.Click += new System.EventHandler(this.btn_RefreshScreen_Click);
            // 
            // btn_StartScreen
            // 
            this.btn_StartScreen.BackColor = System.Drawing.Color.Transparent;
            this.btn_StartScreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StartScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StartScreen.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StartScreen.Location = new System.Drawing.Point(269, 16);
            this.btn_StartScreen.Name = "btn_StartScreen";
            this.btn_StartScreen.Size = new System.Drawing.Size(16, 16);
            this.btn_StartScreen.TabIndex = 370;
            this.btn_StartScreen.UseVisualStyleBackColor = false;
            this.btn_StartScreen.Click += new System.EventHandler(this.btn_StartScreen_Click);
            // 
            // btn_StopScreen
            // 
            this.btn_StopScreen.BackColor = System.Drawing.Color.Transparent;
            this.btn_StopScreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StopScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StopScreen.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StopScreen.Location = new System.Drawing.Point(287, 16);
            this.btn_StopScreen.Name = "btn_StopScreen";
            this.btn_StopScreen.Size = new System.Drawing.Size(16, 16);
            this.btn_StopScreen.TabIndex = 371;
            this.btn_StopScreen.UseVisualStyleBackColor = false;
            this.btn_StopScreen.Click += new System.EventHandler(this.btn_StopScreen_Click);
            // 
            // lbl_Info
            // 
            this.lbl_Info.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_Info.ForeColor = System.Drawing.Color.Red;
            this.lbl_Info.Location = new System.Drawing.Point(5, 100);
            this.lbl_Info.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_Info.Name = "lbl_Info";
            this.lbl_Info.Size = new System.Drawing.Size(320, 13);
            this.lbl_Info.TabIndex = 372;
            this.lbl_Info.Text = "Info";
            this.lbl_Info.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cb_ScreenPrimary
            // 
            this.cb_ScreenPrimary.AutoCheck = false;
            this.cb_ScreenPrimary.AutoSize = true;
            this.cb_ScreenPrimary.Location = new System.Drawing.Point(214, 43);
            this.cb_ScreenPrimary.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_ScreenPrimary.Name = "cb_ScreenPrimary";
            this.cb_ScreenPrimary.Size = new System.Drawing.Size(67, 19);
            this.cb_ScreenPrimary.TabIndex = 373;
            this.cb_ScreenPrimary.Text = "Primary";
            this.cb_ScreenPrimary.UseVisualStyleBackColor = true;
            // 
            // FormScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 331);
            this.Controls.Add(this.cb_ScreenPrimary);
            this.Controls.Add(this.lbl_Info);
            this.Controls.Add(this.btn_StartScreen);
            this.Controls.Add(this.btn_StopScreen);
            this.Controls.Add(this.btn_RefreshScreen);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_NameOfNewInputStream);
            this.Controls.Add(this.btn_AddNewInputStream);
            this.Controls.Add(this.btn_ScreenSet);
            this.Controls.Add(this.lbl_ScreenMaxFps);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tb_ScreenFps);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_ScreenSize);
            this.Controls.Add(this.pb_VideoStream);
            this.Controls.Add(this.cb_Screens);
            this.Controls.Add(this.label2);
            this.Name = "FormScreen";
            this.Text = "FormWebcam";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormScreen_FormClosing);
            this.Shown += new System.EventHandler(this.FormScreen_Shown);
            this.ClientSizeChanged += new System.EventHandler(this.FormScreen_ClientSizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cb_Screens;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pb_VideoStream;
        private System.Windows.Forms.ComboBox cb_ScreenSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_ScreenFps;
        private System.Windows.Forms.Label lbl_ScreenMaxFps;
        private System.Windows.Forms.Button btn_ScreenSet;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_NameOfNewInputStream;
        private System.Windows.Forms.Button btn_AddNewInputStream;
        private System.Windows.Forms.Button btn_RefreshScreen;
        private System.Windows.Forms.Button btn_StartScreen;
        private System.Windows.Forms.Button btn_StopScreen;
        private System.Windows.Forms.Label lbl_Info;
        private System.Windows.Forms.CheckBox cb_ScreenPrimary;
    }
}