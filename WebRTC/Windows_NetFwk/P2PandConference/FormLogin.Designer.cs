using System.Windows.Forms;

namespace Sample_P2PandConference
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
            this.cbUseConfV2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(66, 113);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(120, 33);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(113, 55);
            this.tbLogin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(190, 23);
            this.tbLogin.TabIndex = 2;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(113, 84);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(190, 23);
            this.tbPassword.TabIndex = 4;
            // 
            // tbHostName
            // 
            this.tbHostName.Location = new System.Drawing.Point(113, 27);
            this.tbHostName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbHostName.Name = "tbHostName";
            this.tbHostName.ReadOnly = true;
            this.tbHostName.Size = new System.Drawing.Size(190, 23);
            this.tbHostName.TabIndex = 6;
            // 
            // tbInformation
            // 
            this.tbInformation.Location = new System.Drawing.Point(12, 178);
            this.tbInformation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbInformation.Multiline = true;
            this.tbInformation.Name = "tbInformation";
            this.tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInformation.Size = new System.Drawing.Size(381, 219);
            this.tbInformation.TabIndex = 7;
            // 
            // btnOpenTestForm
            // 
            this.btnOpenTestForm.Enabled = false;
            this.btnOpenTestForm.Location = new System.Drawing.Point(203, 113);
            this.btnOpenTestForm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenTestForm.Name = "btnOpenTestForm";
            this.btnOpenTestForm.Size = new System.Drawing.Size(120, 33);
            this.btnOpenTestForm.TabIndex = 8;
            this.btnOpenTestForm.Text = "Open WeRTC form";
            this.btnOpenTestForm.UseVisualStyleBackColor = true;
            this.btnOpenTestForm.Click += new System.EventHandler(this.btnOpenTestForm_Click);
            // 
            // cbUseConfV2
            // 
            this.cbUseConfV2.AutoSize = true;
            this.cbUseConfV2.Checked = true;
            this.cbUseConfV2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseConfV2.Enabled = false;
            this.cbUseConfV2.Location = new System.Drawing.Point(146, 153);
            this.cbUseConfV2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbUseConfV2.Name = "cbUseConfV2";
            this.cbUseConfV2.Size = new System.Drawing.Size(146, 19);
            this.cbUseConfV2.TabIndex = 9;
            this.cbUseConfV2.Text = "Use Conference API V2";
            this.cbUseConfV2.UseVisualStyleBackColor = true;
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 408);
            this.Controls.Add(this.cbUseConfV2);
            this.Controls.Add(this.btnOpenTestForm);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.tbHostName);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.btnConnect);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
        private CheckBox cbUseConfV2;
    }
}