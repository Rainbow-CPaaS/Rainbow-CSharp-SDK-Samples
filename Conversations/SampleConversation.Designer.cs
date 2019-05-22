namespace Sample_Contacts
{
    partial class SampleConversationForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.label21 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.btnLoginLogout = new System.Windows.Forms.Button();
            this.tbState = new System.Windows.Forms.TextBox();
            this.cbConversationsList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbContactsList = new System.Windows.Forms.ComboBox();
            this.btnContactAddToConversation = new System.Windows.Forms.Button();
            this.btnConversationRemove = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbFavoritesList = new System.Windows.Forms.ComboBox();
            this.btnContactAddToFavorite = new System.Windows.Forms.Button();
            this.btnFavoriteRemove = new System.Windows.Forms.Button();
            this.tbFavoritePosition = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnFavoritePosition = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(628, 61);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(53, 13);
            this.label21.TabIndex = 50;
            this.label21.Text = "Password";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(628, 77);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(164, 20);
            this.tbPassword.TabIndex = 49;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(628, 22);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(33, 13);
            this.label22.TabIndex = 48;
            this.label22.Text = "Login";
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(628, 38);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(164, 20);
            this.tbLogin.TabIndex = 47;
            // 
            // btnLoginLogout
            // 
            this.btnLoginLogout.Location = new System.Drawing.Point(666, 103);
            this.btnLoginLogout.Name = "btnLoginLogout";
            this.btnLoginLogout.Size = new System.Drawing.Size(75, 23);
            this.btnLoginLogout.TabIndex = 51;
            this.btnLoginLogout.Text = "Login";
            this.btnLoginLogout.UseVisualStyleBackColor = true;
            this.btnLoginLogout.Click += new System.EventHandler(this.btnLoginLogout_Click);
            // 
            // tbState
            // 
            this.tbState.Location = new System.Drawing.Point(12, 439);
            this.tbState.Multiline = true;
            this.tbState.Name = "tbState";
            this.tbState.Size = new System.Drawing.Size(780, 133);
            this.tbState.TabIndex = 52;
            // 
            // cbConversationsList
            // 
            this.cbConversationsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbConversationsList.FormattingEnabled = true;
            this.cbConversationsList.Location = new System.Drawing.Point(88, 40);
            this.cbConversationsList.Name = "cbConversationsList";
            this.cbConversationsList.Size = new System.Drawing.Size(188, 21);
            this.cbConversationsList.TabIndex = 54;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 55;
            this.label1.Text = "Conversations";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 58;
            this.label2.Text = "Contacts";
            // 
            // cbContactsList
            // 
            this.cbContactsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbContactsList.FormattingEnabled = true;
            this.cbContactsList.Location = new System.Drawing.Point(88, 162);
            this.cbContactsList.Name = "cbContactsList";
            this.cbContactsList.Size = new System.Drawing.Size(188, 21);
            this.cbContactsList.TabIndex = 57;
            this.cbContactsList.SelectedIndexChanged += new System.EventHandler(this.cbContactsList_SelectedIndexChanged);
            // 
            // btnContactAddToConversation
            // 
            this.btnContactAddToConversation.Location = new System.Drawing.Point(14, 189);
            this.btnContactAddToConversation.Name = "btnContactAddToConversation";
            this.btnContactAddToConversation.Size = new System.Drawing.Size(133, 23);
            this.btnContactAddToConversation.TabIndex = 59;
            this.btnContactAddToConversation.Text = "Add to conversation";
            this.btnContactAddToConversation.UseVisualStyleBackColor = true;
            this.btnContactAddToConversation.Click += new System.EventHandler(this.btnContactAddToConversation_Click);
            // 
            // btnConversationRemove
            // 
            this.btnConversationRemove.Location = new System.Drawing.Point(88, 67);
            this.btnConversationRemove.Name = "btnConversationRemove";
            this.btnConversationRemove.Size = new System.Drawing.Size(133, 23);
            this.btnConversationRemove.TabIndex = 60;
            this.btnConversationRemove.Text = "Remove conversation";
            this.btnConversationRemove.UseVisualStyleBackColor = true;
            this.btnConversationRemove.Click += new System.EventHandler(this.btnConversationRemove_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(317, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 63;
            this.label3.Text = "Favorites ordered";
            // 
            // cbFavoritesList
            // 
            this.cbFavoritesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFavoritesList.FormattingEnabled = true;
            this.cbFavoritesList.Location = new System.Drawing.Point(412, 38);
            this.cbFavoritesList.Name = "cbFavoritesList";
            this.cbFavoritesList.Size = new System.Drawing.Size(188, 21);
            this.cbFavoritesList.TabIndex = 62;
            this.cbFavoritesList.SelectedIndexChanged += new System.EventHandler(this.cbFavoritesList_SelectedIndexChanged);
            // 
            // btnContactAddToFavorite
            // 
            this.btnContactAddToFavorite.Location = new System.Drawing.Point(153, 189);
            this.btnContactAddToFavorite.Name = "btnContactAddToFavorite";
            this.btnContactAddToFavorite.Size = new System.Drawing.Size(133, 23);
            this.btnContactAddToFavorite.TabIndex = 64;
            this.btnContactAddToFavorite.Text = "Add to favorite";
            this.btnContactAddToFavorite.UseVisualStyleBackColor = true;
            this.btnContactAddToFavorite.Click += new System.EventHandler(this.btnContactAddToFavorite_Click);
            // 
            // btnFavoriteRemove
            // 
            this.btnFavoriteRemove.Location = new System.Drawing.Point(412, 96);
            this.btnFavoriteRemove.Name = "btnFavoriteRemove";
            this.btnFavoriteRemove.Size = new System.Drawing.Size(133, 23);
            this.btnFavoriteRemove.TabIndex = 65;
            this.btnFavoriteRemove.Text = "Remove favorite";
            this.btnFavoriteRemove.UseVisualStyleBackColor = true;
            this.btnFavoriteRemove.Click += new System.EventHandler(this.btnFavoriteRemove_Click);
            // 
            // tbFavoritePosition
            // 
            this.tbFavoritePosition.Location = new System.Drawing.Point(412, 69);
            this.tbFavoritePosition.Name = "tbFavoritePosition";
            this.tbFavoritePosition.Size = new System.Drawing.Size(73, 20);
            this.tbFavoritePosition.TabIndex = 66;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(317, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 67;
            this.label4.Text = "Favorite position";
            // 
            // btnFavoritePosition
            // 
            this.btnFavoritePosition.Location = new System.Drawing.Point(491, 67);
            this.btnFavoritePosition.Name = "btnFavoritePosition";
            this.btnFavoritePosition.Size = new System.Drawing.Size(54, 23);
            this.btnFavoritePosition.TabIndex = 68;
            this.btnFavoritePosition.Text = "Ok";
            this.btnFavoritePosition.UseVisualStyleBackColor = true;
            this.btnFavoritePosition.Click += new System.EventHandler(this.btnFavoritePosition_Click);
            // 
            // SampleConversationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 584);
            this.Controls.Add(this.btnFavoritePosition);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbFavoritePosition);
            this.Controls.Add(this.btnFavoriteRemove);
            this.Controls.Add(this.btnContactAddToFavorite);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbFavoritesList);
            this.Controls.Add(this.btnConversationRemove);
            this.Controls.Add(this.btnContactAddToConversation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbContactsList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbConversationsList);
            this.Controls.Add(this.tbState);
            this.Controls.Add(this.btnLoginLogout);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.tbLogin);
            this.Name = "SampleConversationForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox tbLogin;
        private System.Windows.Forms.Button btnLoginLogout;
        private System.Windows.Forms.TextBox tbState;
        private System.Windows.Forms.ComboBox cbConversationsList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbContactsList;
        private System.Windows.Forms.Button btnContactAddToConversation;
        private System.Windows.Forms.Button btnConversationRemove;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbFavoritesList;
        private System.Windows.Forms.Button btnContactAddToFavorite;
        private System.Windows.Forms.Button btnFavoriteRemove;
        private System.Windows.Forms.TextBox tbFavoritePosition;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnFavoritePosition;
    }
}

