using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    partial class FormVideoOutputStreamWebRTC
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
            pb_VideoStream = new PictureBox();
            label1 = new Label();
            lbl_VideoStreamTrack = new Label();
            btn_Hold = new Button();
            btn_Unhold = new Button();
            ((System.ComponentModel.ISupportInitialize)pb_VideoStream).BeginInit();
            SuspendLayout();
            // 
            // pb_VideoStream
            // 
            pb_VideoStream.BorderStyle = BorderStyle.FixedSingle;
            pb_VideoStream.Location = new System.Drawing.Point(12, 39);
            pb_VideoStream.Name = "pb_VideoStream";
            pb_VideoStream.Size = new System.Drawing.Size(640, 360);
            pb_VideoStream.SizeMode = PictureBoxSizeMode.Zoom;
            pb_VideoStream.TabIndex = 228;
            pb_VideoStream.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 15);
            label1.TabIndex = 230;
            label1.Text = "Video Stream Track:";
            // 
            // lbl_VideoStreamTrack
            // 
            lbl_VideoStreamTrack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            lbl_VideoStreamTrack.ForeColor = System.Drawing.Color.Blue;
            lbl_VideoStreamTrack.Location = new System.Drawing.Point(125, 16);
            lbl_VideoStreamTrack.Margin = new Padding(2, 0, 2, 0);
            lbl_VideoStreamTrack.Name = "lbl_VideoStreamTrack";
            lbl_VideoStreamTrack.Size = new System.Drawing.Size(384, 20);
            lbl_VideoStreamTrack.TabIndex = 345;
            lbl_VideoStreamTrack.Text = "details";
            // 
            // btn_Hold
            // 
            btn_Hold.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_Hold.Location = new System.Drawing.Point(539, 13);
            btn_Hold.Margin = new Padding(4, 3, 4, 3);
            btn_Hold.Name = "btn_Hold";
            btn_Hold.Size = new System.Drawing.Size(56, 20);
            btn_Hold.TabIndex = 1307;
            btn_Hold.Text = "Hold";
            btn_Hold.UseVisualStyleBackColor = true;
            btn_Hold.Click += btn_Hold_Click;
            // 
            // btn_Unhold
            // 
            btn_Unhold.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_Unhold.Location = new System.Drawing.Point(598, 13);
            btn_Unhold.Margin = new Padding(4, 3, 4, 3);
            btn_Unhold.Name = "btn_Unhold";
            btn_Unhold.Size = new System.Drawing.Size(56, 20);
            btn_Unhold.TabIndex = 1308;
            btn_Unhold.Text = "Unhold";
            btn_Unhold.UseVisualStyleBackColor = true;
            btn_Unhold.Click += btn_Unhold_Click;
            // 
            // FormVideoOutputStreamWebRTC
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(667, 406);
            Controls.Add(btn_Unhold);
            Controls.Add(btn_Hold);
            Controls.Add(lbl_VideoStreamTrack);
            Controls.Add(label1);
            Controls.Add(pb_VideoStream);
            MinimumSize = new System.Drawing.Size(683, 445);
            Name = "FormVideoOutputStreamWebRTC";
            Text = "FormVideoOutputStreamWebRTC";
            FormClosing += FormVideoOutputStreamWebRTC_FormClosing;
            ClientSizeChanged += FormVideoOutputStreamWebRTC_ClientSizeChanged;
            ((System.ComponentModel.ISupportInitialize)pb_VideoStream).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pb_VideoStream;
        private Label label1;
        private Label lbl_VideoStreamTrack;
        private Button btn_Hold;
        private Button btn_Unhold;
    }
}