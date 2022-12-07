namespace SDK.UIForm.WebRTC
{
    partial class FormWebcam
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
            this.cb_Webcams = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pb_VideoStream = new System.Windows.Forms.PictureBox();
            this.cb_WebcamSize = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_WebcamFps = new System.Windows.Forms.TextBox();
            this.lbl_WebcamMaxFps = new System.Windows.Forms.Label();
            this.btn_WebcamSet = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_NameOfNewInputStream = new System.Windows.Forms.TextBox();
            this.btn_AddNewInputStream = new System.Windows.Forms.Button();
            this.btn_RefreshWebcam = new System.Windows.Forms.Button();
            this.btn_StartWebcam = new System.Windows.Forms.Button();
            this.btn_StopWebcam = new System.Windows.Forms.Button();
            this.lbl_Info = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).BeginInit();
            this.SuspendLayout();
            // 
            // cb_Webcams
            // 
            this.cb_Webcams.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_Webcams.FormattingEnabled = true;
            this.cb_Webcams.Location = new System.Drawing.Point(97, 12);
            this.cb_Webcams.Name = "cb_Webcams";
            this.cb_Webcams.Size = new System.Drawing.Size(174, 23);
            this.cb_Webcams.TabIndex = 146;
            this.cb_Webcams.SelectedIndexChanged += new System.EventHandler(this.cb_Webcams_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 15);
            this.label2.TabIndex = 145;
            this.label2.Text = "Webcams:";
            // 
            // pb_VideoStream
            // 
            this.pb_VideoStream.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_VideoStream.Location = new System.Drawing.Point(12, 116);
            this.pb_VideoStream.Name = "pb_VideoStream";
            this.pb_VideoStream.Size = new System.Drawing.Size(320, 240);
            this.pb_VideoStream.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_VideoStream.TabIndex = 229;
            this.pb_VideoStream.TabStop = false;
            // 
            // cb_WebcamSize
            // 
            this.cb_WebcamSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_WebcamSize.Enabled = false;
            this.cb_WebcamSize.FormattingEnabled = true;
            this.cb_WebcamSize.Location = new System.Drawing.Point(97, 41);
            this.cb_WebcamSize.Name = "cb_WebcamSize";
            this.cb_WebcamSize.Size = new System.Drawing.Size(119, 23);
            this.cb_WebcamSize.TabIndex = 230;
            this.cb_WebcamSize.SelectedIndexChanged += new System.EventHandler(this.cb_WebcamSize_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 15);
            this.label1.TabIndex = 231;
            this.label1.Text = "Size:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(65, 74);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 13);
            this.label8.TabIndex = 233;
            this.label8.Text = "Fps:";
            // 
            // tb_WebcamFps
            // 
            this.tb_WebcamFps.Enabled = false;
            this.tb_WebcamFps.Location = new System.Drawing.Point(96, 70);
            this.tb_WebcamFps.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_WebcamFps.Name = "tb_WebcamFps";
            this.tb_WebcamFps.Size = new System.Drawing.Size(43, 23);
            this.tb_WebcamFps.TabIndex = 232;
            // 
            // lbl_WebcamMaxFps
            // 
            this.lbl_WebcamMaxFps.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_WebcamMaxFps.Location = new System.Drawing.Point(143, 74);
            this.lbl_WebcamMaxFps.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_WebcamMaxFps.Name = "lbl_WebcamMaxFps";
            this.lbl_WebcamMaxFps.Size = new System.Drawing.Size(72, 13);
            this.lbl_WebcamMaxFps.TabIndex = 234;
            this.lbl_WebcamMaxFps.Text = "Max 0 Fps";
            // 
            // btn_WebcamSet
            // 
            this.btn_WebcamSet.Enabled = false;
            this.btn_WebcamSet.FlatAppearance.BorderSize = 0;
            this.btn_WebcamSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_WebcamSet.Location = new System.Drawing.Point(217, 70);
            this.btn_WebcamSet.Margin = new System.Windows.Forms.Padding(0);
            this.btn_WebcamSet.Name = "btn_WebcamSet";
            this.btn_WebcamSet.Size = new System.Drawing.Size(54, 23);
            this.btn_WebcamSet.TabIndex = 365;
            this.btn_WebcamSet.Text = "Set";
            this.btn_WebcamSet.UseVisualStyleBackColor = true;
            this.btn_WebcamSet.Click += new System.EventHandler(this.btn_WebcamSet_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(49, 365);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 15);
            this.label6.TabIndex = 368;
            this.label6.Text = "Name:";
            // 
            // tb_NameOfNewInputStream
            // 
            this.tb_NameOfNewInputStream.Location = new System.Drawing.Point(97, 362);
            this.tb_NameOfNewInputStream.Name = "tb_NameOfNewInputStream";
            this.tb_NameOfNewInputStream.Size = new System.Drawing.Size(158, 23);
            this.tb_NameOfNewInputStream.TabIndex = 367;
            // 
            // btn_AddNewInputStream
            // 
            this.btn_AddNewInputStream.Location = new System.Drawing.Point(261, 362);
            this.btn_AddNewInputStream.Name = "btn_AddNewInputStream";
            this.btn_AddNewInputStream.Size = new System.Drawing.Size(71, 23);
            this.btn_AddNewInputStream.TabIndex = 366;
            this.btn_AddNewInputStream.Text = "Add";
            this.btn_AddNewInputStream.UseVisualStyleBackColor = true;
            this.btn_AddNewInputStream.Click += new System.EventHandler(this.btn_AddNewInputStream_Click);
            // 
            // btn_RefreshWebcam
            // 
            this.btn_RefreshWebcam.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_RefreshWebcam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_RefreshWebcam.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_RefreshWebcam.Location = new System.Drawing.Point(316, 16);
            this.btn_RefreshWebcam.Name = "btn_RefreshWebcam";
            this.btn_RefreshWebcam.Size = new System.Drawing.Size(16, 16);
            this.btn_RefreshWebcam.TabIndex = 369;
            this.btn_RefreshWebcam.UseVisualStyleBackColor = true;
            this.btn_RefreshWebcam.Click += new System.EventHandler(this.btn_RefreshWebcam_Click);
            // 
            // btn_StartWebcam
            // 
            this.btn_StartWebcam.BackColor = System.Drawing.Color.Transparent;
            this.btn_StartWebcam.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StartWebcam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StartWebcam.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StartWebcam.Location = new System.Drawing.Point(276, 16);
            this.btn_StartWebcam.Name = "btn_StartWebcam";
            this.btn_StartWebcam.Size = new System.Drawing.Size(16, 16);
            this.btn_StartWebcam.TabIndex = 370;
            this.btn_StartWebcam.UseVisualStyleBackColor = false;
            this.btn_StartWebcam.Click += new System.EventHandler(this.btn_StartWebcam_Click);
            // 
            // btn_StopWebcam
            // 
            this.btn_StopWebcam.BackColor = System.Drawing.Color.Transparent;
            this.btn_StopWebcam.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StopWebcam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StopWebcam.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StopWebcam.Location = new System.Drawing.Point(294, 16);
            this.btn_StopWebcam.Name = "btn_StopWebcam";
            this.btn_StopWebcam.Size = new System.Drawing.Size(16, 16);
            this.btn_StopWebcam.TabIndex = 371;
            this.btn_StopWebcam.UseVisualStyleBackColor = false;
            this.btn_StopWebcam.Click += new System.EventHandler(this.btn_StopWebcam_Click);
            // 
            // lbl_Info
            // 
            this.lbl_Info.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_Info.ForeColor = System.Drawing.Color.Red;
            this.lbl_Info.Location = new System.Drawing.Point(12, 100);
            this.lbl_Info.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_Info.Name = "lbl_Info";
            this.lbl_Info.Size = new System.Drawing.Size(320, 13);
            this.lbl_Info.TabIndex = 372;
            this.lbl_Info.Text = "Info";
            this.lbl_Info.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormWebcam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 393);
            this.Controls.Add(this.lbl_Info);
            this.Controls.Add(this.btn_StartWebcam);
            this.Controls.Add(this.btn_StopWebcam);
            this.Controls.Add(this.btn_RefreshWebcam);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_NameOfNewInputStream);
            this.Controls.Add(this.btn_AddNewInputStream);
            this.Controls.Add(this.btn_WebcamSet);
            this.Controls.Add(this.lbl_WebcamMaxFps);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tb_WebcamFps);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_WebcamSize);
            this.Controls.Add(this.pb_VideoStream);
            this.Controls.Add(this.cb_Webcams);
            this.Controls.Add(this.label2);
            this.Name = "FormWebcam";
            this.Text = "FormWebcam";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWebcam_FormClosing);
            this.Shown += new System.EventHandler(this.FormWebcam_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cb_Webcams;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pb_VideoStream;
        private System.Windows.Forms.ComboBox cb_WebcamSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_WebcamFps;
        private System.Windows.Forms.Label lbl_WebcamMaxFps;
        private System.Windows.Forms.Button btn_WebcamSet;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_NameOfNewInputStream;
        private System.Windows.Forms.Button btn_AddNewInputStream;
        private System.Windows.Forms.Button btn_RefreshWebcam;
        private System.Windows.Forms.Button btn_StartWebcam;
        private System.Windows.Forms.Button btn_StopWebcam;
        private System.Windows.Forms.Label lbl_Info;
    }
}