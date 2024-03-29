﻿namespace SampleChannels
{
    partial class FormLogin
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.tbHostName = new System.Windows.Forms.TextBox();
            this.tbInformation = new System.Windows.Forms.TextBox();
            this.btnOpenTestForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(66, 113);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(120, 34);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(113, 55);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.PlaceholderText = "Enter login";
            this.tbLogin.Size = new System.Drawing.Size(189, 23);
            this.tbLogin.TabIndex = 2;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(113, 84);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.PlaceholderText = "Enter password";
            this.tbPassword.Size = new System.Drawing.Size(189, 23);
            this.tbPassword.TabIndex = 4;
            // 
            // tbHostName
            // 
            this.tbHostName.Location = new System.Drawing.Point(113, 26);
            this.tbHostName.Name = "tbHostName";
            this.tbHostName.PlaceholderText = "Hostname";
            this.tbHostName.ReadOnly = true;
            this.tbHostName.Size = new System.Drawing.Size(189, 23);
            this.tbHostName.TabIndex = 6;
            // 
            // tbInformation
            // 
            this.tbInformation.Location = new System.Drawing.Point(12, 153);
            this.tbInformation.Multiline = true;
            this.tbInformation.Name = "tbInformation";
            this.tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInformation.Size = new System.Drawing.Size(381, 243);
            this.tbInformation.TabIndex = 7;
            // 
            // btnOpenTestForm
            // 
            this.btnOpenTestForm.Enabled = false;
            this.btnOpenTestForm.Location = new System.Drawing.Point(203, 113);
            this.btnOpenTestForm.Name = "btnOpenTestForm";
            this.btnOpenTestForm.Size = new System.Drawing.Size(120, 34);
            this.btnOpenTestForm.TabIndex = 8;
            this.btnOpenTestForm.Text = "Open test form";
            this.btnOpenTestForm.UseVisualStyleBackColor = true;
            this.btnOpenTestForm.Click += new System.EventHandler(this.btnOpenTestForm_Click);
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 408);
            this.Controls.Add(this.btnOpenTestForm);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.tbHostName);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.btnConnect);
            this.Name = "FormLogin";
            this.Text = "FormLogin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnConnect;
        private TextBox tbLogin;
        private TextBox tbPassword;
        private TextBox tbHostName;
        private TextBox tbInformation;
        private Button btnOpenTestForm;
    }
}