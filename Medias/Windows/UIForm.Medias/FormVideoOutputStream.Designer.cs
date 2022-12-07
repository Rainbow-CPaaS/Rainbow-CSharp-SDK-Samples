using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    partial class FormVideoOutputStream
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
            this.pb_VideoStream = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_ListOfInputStreams = new System.Windows.Forms.ComboBox();
            this.btn_StopVideo = new System.Windows.Forms.Button();
            this.btn_StartVideo = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_Codecs = new System.Windows.Forms.ComboBox();
            this.cb_BindVideoSample = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).BeginInit();
            this.SuspendLayout();
            // 
            // pb_VideoStream
            // 
            this.pb_VideoStream.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_VideoStream.Location = new System.Drawing.Point(10, 73);
            this.pb_VideoStream.Name = "pb_VideoStream";
            this.pb_VideoStream.Size = new System.Drawing.Size(320, 240);
            this.pb_VideoStream.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_VideoStream.TabIndex = 228;
            this.pb_VideoStream.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 15);
            this.label1.TabIndex = 230;
            this.label1.Text = "List:";
            // 
            // cb_ListOfInputStreams
            // 
            this.cb_ListOfInputStreams.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_ListOfInputStreams.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cb_ListOfInputStreams.FormattingEnabled = true;
            this.cb_ListOfInputStreams.Location = new System.Drawing.Point(62, 12);
            this.cb_ListOfInputStreams.Name = "cb_ListOfInputStreams";
            this.cb_ListOfInputStreams.Size = new System.Drawing.Size(231, 21);
            this.cb_ListOfInputStreams.TabIndex = 229;
            this.cb_ListOfInputStreams.SelectedIndexChanged += new System.EventHandler(this.cb_ListOfInputStreams_SelectedIndexChanged);
            // 
            // btn_StopVideo
            // 
            this.btn_StopVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StopVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StopVideo.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StopVideo.Location = new System.Drawing.Point(314, 14);
            this.btn_StopVideo.Name = "btn_StopVideo";
            this.btn_StopVideo.Size = new System.Drawing.Size(16, 16);
            this.btn_StopVideo.TabIndex = 289;
            this.btn_StopVideo.UseVisualStyleBackColor = true;
            this.btn_StopVideo.Click += new System.EventHandler(this.btn_StopVideo_Click);
            // 
            // btn_StartVideo
            // 
            this.btn_StartVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StartVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StartVideo.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StartVideo.Location = new System.Drawing.Point(295, 14);
            this.btn_StartVideo.Name = "btn_StartVideo";
            this.btn_StartVideo.Size = new System.Drawing.Size(16, 16);
            this.btn_StartVideo.TabIndex = 288;
            this.btn_StartVideo.UseVisualStyleBackColor = true;
            this.btn_StartVideo.Click += new System.EventHandler(this.btn_StartVideo_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 15);
            this.label2.TabIndex = 291;
            this.label2.Text = "Codec:";
            // 
            // cb_Codecs
            // 
            this.cb_Codecs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_Codecs.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cb_Codecs.FormattingEnabled = true;
            this.cb_Codecs.Location = new System.Drawing.Point(62, 39);
            this.cb_Codecs.Name = "cb_Codecs";
            this.cb_Codecs.Size = new System.Drawing.Size(139, 21);
            this.cb_Codecs.TabIndex = 290;
            this.cb_Codecs.SelectedIndexChanged += new System.EventHandler(this.cb_Codecs_SelectedIndexChanged);
            // 
            // cb_BindVideoSample
            // 
            this.cb_BindVideoSample.AutoSize = true;
            this.cb_BindVideoSample.Location = new System.Drawing.Point(206, 41);
            this.cb_BindVideoSample.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_BindVideoSample.Name = "cb_BindVideoSample";
            this.cb_BindVideoSample.Size = new System.Drawing.Size(124, 19);
            this.cb_BindVideoSample.TabIndex = 292;
            this.cb_BindVideoSample.Text = "Bind Video sample";
            this.cb_BindVideoSample.UseVisualStyleBackColor = true;
            this.cb_BindVideoSample.CheckedChanged += new System.EventHandler(this.cb_BindVideoSample_CheckedChanged);
            // 
            // FormVideoOutputStream
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 325);
            this.Controls.Add(this.cb_BindVideoSample);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_Codecs);
            this.Controls.Add(this.btn_StopVideo);
            this.Controls.Add(this.btn_StartVideo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_ListOfInputStreams);
            this.Controls.Add(this.pb_VideoStream);
            this.MinimumSize = new System.Drawing.Size(358, 340);
            this.Name = "FormVideoOutputStream";
            this.Text = "FormVideoOutputStream";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormVideoOutputStream_FormClosing);
            this.ClientSizeChanged += new System.EventHandler(this.FormVideoOutputStream_ClientSizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pb_VideoStream;
        private Label label1;
        private ComboBox cb_ListOfInputStreams;
        private Button btn_StopVideo;
        private Button btn_StartVideo;
        private Label label2;
        private ComboBox cb_Codecs;
        private CheckBox cb_BindVideoSample;
    }
}