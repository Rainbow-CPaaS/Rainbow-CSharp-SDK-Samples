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
            this.pb_VideoStream = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_ListOfInputStreams = new System.Windows.Forms.ComboBox();
            this.btn_Subscribe = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).BeginInit();
            this.SuspendLayout();
            // 
            // pb_VideoStream
            // 
            this.pb_VideoStream.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_VideoStream.Location = new System.Drawing.Point(12, 39);
            this.pb_VideoStream.Name = "pb_VideoStream";
            this.pb_VideoStream.Size = new System.Drawing.Size(640, 360);
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
            this.cb_ListOfInputStreams.Location = new System.Drawing.Point(44, 12);
            this.cb_ListOfInputStreams.Name = "cb_ListOfInputStreams";
            this.cb_ListOfInputStreams.Size = new System.Drawing.Size(459, 21);
            this.cb_ListOfInputStreams.TabIndex = 229;
            this.cb_ListOfInputStreams.SelectedIndexChanged += new System.EventHandler(this.cb_ListOfInputStreams_SelectedIndexChanged);
            // 
            // btn_Subscribe
            // 
            this.btn_Subscribe.Enabled = false;
            this.btn_Subscribe.FlatAppearance.BorderSize = 0;
            this.btn_Subscribe.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Subscribe.Location = new System.Drawing.Point(506, 11);
            this.btn_Subscribe.Margin = new System.Windows.Forms.Padding(0);
            this.btn_Subscribe.Name = "btn_Subscribe";
            this.btn_Subscribe.Size = new System.Drawing.Size(146, 23);
            this.btn_Subscribe.TabIndex = 365;
            this.btn_Subscribe.Text = "Subscribe / Unsubscribe";
            this.btn_Subscribe.UseVisualStyleBackColor = true;
            // 
            // FormVideoOutputStreamWebRTC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 406);
            this.Controls.Add(this.btn_Subscribe);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_ListOfInputStreams);
            this.Controls.Add(this.pb_VideoStream);
            this.MinimumSize = new System.Drawing.Size(683, 445);
            this.Name = "FormVideoOutputStreamWebRTC";
            this.Text = "FormVideoOutputStreamWebRTC";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormVideoOutputStreamWebRTC_FormClosing);
            this.ClientSizeChanged += new System.EventHandler(this.FormVideoOutputStreamWebRTC_ClientSizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pb_VideoStream)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pb_VideoStream;
        private Label label1;
        private ComboBox cb_ListOfInputStreams;
        private Button btn_Subscribe;
    }
}