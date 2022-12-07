using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
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
            this.lbl_in01 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_AutoLogin = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(62, 138);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(120, 24);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(125, 55);
            this.tbLogin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(178, 23);
            this.tbLogin.TabIndex = 2;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(125, 84);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(178, 23);
            this.tbPassword.TabIndex = 4;
            // 
            // tbHostName
            // 
            this.tbHostName.Location = new System.Drawing.Point(125, 27);
            this.tbHostName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbHostName.Name = "tbHostName";
            this.tbHostName.ReadOnly = true;
            this.tbHostName.Size = new System.Drawing.Size(178, 23);
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
            this.btnOpenTestForm.Location = new System.Drawing.Point(200, 138);
            this.btnOpenTestForm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenTestForm.Name = "btnOpenTestForm";
            this.btnOpenTestForm.Size = new System.Drawing.Size(120, 24);
            this.btnOpenTestForm.TabIndex = 8;
            this.btnOpenTestForm.Text = "Open WebRTC form";
            this.btnOpenTestForm.UseVisualStyleBackColor = true;
            this.btnOpenTestForm.Click += new System.EventHandler(this.btnOpenTestForm_Click);
            // 
            // lbl_in01
            // 
            this.lbl_in01.AutoSize = true;
            this.lbl_in01.Location = new System.Drawing.Point(53, 58);
            this.lbl_in01.Name = "lbl_in01";
            this.lbl_in01.Size = new System.Drawing.Size(40, 15);
            this.lbl_in01.TabIndex = 198;
            this.lbl_in01.Text = "Login:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(53, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 199;
            this.label1.Text = "Password:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(53, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 15);
            this.label2.TabIndex = 200;
            this.label2.Text = "Hostname:";
            // 
            // cb_AutoLogin
            // 
            this.cb_AutoLogin.AutoSize = true;
            this.cb_AutoLogin.Location = new System.Drawing.Point(125, 113);
            this.cb_AutoLogin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cb_AutoLogin.Name = "cb_AutoLogin";
            this.cb_AutoLogin.Size = new System.Drawing.Size(82, 19);
            this.cb_AutoLogin.TabIndex = 201;
            this.cb_AutoLogin.Text = "Auto login";
            this.cb_AutoLogin.UseVisualStyleBackColor = true;
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 408);
            this.Controls.Add(this.cb_AutoLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_in01);
            this.Controls.Add(this.btnOpenTestForm);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.tbHostName);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.btnConnect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
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
        private Label lbl_in01;
        private Label label1;
        private Label label2;
        private CheckBox cb_AutoLogin;
    }
}
