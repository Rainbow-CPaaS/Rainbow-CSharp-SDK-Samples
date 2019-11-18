namespace Sample_Contacts
{
    partial class SampleInstantMessagingForm
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
            this.tbSelectionInfo = new System.Windows.Forms.TextBox();
            this.tbMessagesExchanged = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbMessage = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.btnLoadOlderMessages = new System.Windows.Forms.Button();
            this.cbIsTypingStatus = new System.Windows.Forms.CheckBox();
            this.btnMarkLastMessageRead = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tbContactPresence = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbPresenceList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(628, 45);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(53, 13);
            this.label21.TabIndex = 50;
            this.label21.Text = "Password";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(628, 61);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(164, 20);
            this.tbPassword.TabIndex = 49;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(628, 6);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(33, 13);
            this.label22.TabIndex = 48;
            this.label22.Text = "Login";
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(628, 22);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(164, 20);
            this.tbLogin.TabIndex = 47;
            // 
            // btnLoginLogout
            // 
            this.btnLoginLogout.Location = new System.Drawing.Point(666, 87);
            this.btnLoginLogout.Name = "btnLoginLogout";
            this.btnLoginLogout.Size = new System.Drawing.Size(75, 23);
            this.btnLoginLogout.TabIndex = 51;
            this.btnLoginLogout.Text = "Login";
            this.btnLoginLogout.UseVisualStyleBackColor = true;
            this.btnLoginLogout.Click += new System.EventHandler(this.btnLoginLogout_Click);
            // 
            // tbState
            // 
            this.tbState.Location = new System.Drawing.Point(8, 399);
            this.tbState.Multiline = true;
            this.tbState.Name = "tbState";
            this.tbState.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbState.Size = new System.Drawing.Size(784, 173);
            this.tbState.TabIndex = 52;
            // 
            // cbConversationsList
            // 
            this.cbConversationsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbConversationsList.FormattingEnabled = true;
            this.cbConversationsList.Location = new System.Drawing.Point(103, 22);
            this.cbConversationsList.Name = "cbConversationsList";
            this.cbConversationsList.Size = new System.Drawing.Size(188, 21);
            this.cbConversationsList.TabIndex = 54;
            this.cbConversationsList.SelectedIndexChanged += new System.EventHandler(this.cbConversationsList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 55;
            this.label1.Text = "Conversations list";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 58;
            this.label2.Text = "Contacts list";
            // 
            // cbContactsList
            // 
            this.cbContactsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbContactsList.FormattingEnabled = true;
            this.cbContactsList.Location = new System.Drawing.Point(103, 51);
            this.cbContactsList.Name = "cbContactsList";
            this.cbContactsList.Size = new System.Drawing.Size(188, 21);
            this.cbContactsList.TabIndex = 57;
            this.cbContactsList.SelectedIndexChanged += new System.EventHandler(this.cbContactsList_SelectedIndexChanged);
            // 
            // tbSelectionInfo
            // 
            this.tbSelectionInfo.Location = new System.Drawing.Point(8, 128);
            this.tbSelectionInfo.Name = "tbSelectionInfo";
            this.tbSelectionInfo.ReadOnly = true;
            this.tbSelectionInfo.Size = new System.Drawing.Size(331, 20);
            this.tbSelectionInfo.TabIndex = 59;
            // 
            // tbMessagesExchanged
            // 
            this.tbMessagesExchanged.Location = new System.Drawing.Point(8, 153);
            this.tbMessagesExchanged.Multiline = true;
            this.tbMessagesExchanged.Name = "tbMessagesExchanged";
            this.tbMessagesExchanged.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbMessagesExchanged.Size = new System.Drawing.Size(784, 189);
            this.tbMessagesExchanged.TabIndex = 61;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(345, 131);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 62;
            this.label4.Text = "Messages exchanged:";
            // 
            // tbMessage
            // 
            this.tbMessage.Location = new System.Drawing.Point(88, 348);
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.Size = new System.Drawing.Size(292, 20);
            this.tbMessage.TabIndex = 63;
            this.tbMessage.TextChanged += new System.EventHandler(this.tbMessage_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 351);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 64;
            this.label5.Text = "Message";
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Location = new System.Drawing.Point(386, 346);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(75, 23);
            this.btnSendMessage.TabIndex = 65;
            this.btnSendMessage.Text = "Send";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // btnLoadOlderMessages
            // 
            this.btnLoadOlderMessages.Location = new System.Drawing.Point(465, 126);
            this.btnLoadOlderMessages.Name = "btnLoadOlderMessages";
            this.btnLoadOlderMessages.Size = new System.Drawing.Size(164, 23);
            this.btnLoadOlderMessages.TabIndex = 66;
            this.btnLoadOlderMessages.Text = "Load older messages";
            this.btnLoadOlderMessages.UseVisualStyleBackColor = true;
            this.btnLoadOlderMessages.Click += new System.EventHandler(this.btnLoadOlderMessages_Click);
            // 
            // cbIsTypingStatus
            // 
            this.cbIsTypingStatus.AutoSize = true;
            this.cbIsTypingStatus.Checked = true;
            this.cbIsTypingStatus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIsTypingStatus.Location = new System.Drawing.Point(479, 350);
            this.cbIsTypingStatus.Name = "cbIsTypingStatus";
            this.cbIsTypingStatus.Size = new System.Drawing.Size(132, 17);
            this.cbIsTypingStatus.TabIndex = 67;
            this.cbIsTypingStatus.Text = "send \"isTyping\" status";
            this.cbIsTypingStatus.UseVisualStyleBackColor = true;
            this.cbIsTypingStatus.CheckedChanged += new System.EventHandler(this.cbIsTypingStatus_CheckedChanged);
            // 
            // btnMarkLastMessageRead
            // 
            this.btnMarkLastMessageRead.Location = new System.Drawing.Point(628, 346);
            this.btnMarkLastMessageRead.Name = "btnMarkLastMessageRead";
            this.btnMarkLastMessageRead.Size = new System.Drawing.Size(164, 23);
            this.btnMarkLastMessageRead.TabIndex = 68;
            this.btnMarkLastMessageRead.Text = "Mark last message as read";
            this.btnMarkLastMessageRead.UseVisualStyleBackColor = true;
            this.btnMarkLastMessageRead.Click += new System.EventHandler(this.btnMarkLastMessageRead_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(361, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 69;
            this.label6.Text = "Your presence:";
            // 
            // tbContactPresence
            // 
            this.tbContactPresence.Location = new System.Drawing.Point(151, 102);
            this.tbContactPresence.Name = "tbContactPresence";
            this.tbContactPresence.ReadOnly = true;
            this.tbContactPresence.Size = new System.Drawing.Size(188, 20);
            this.tbContactPresence.TabIndex = 70;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 105);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(134, 13);
            this.label7.TabIndex = 71;
            this.label7.Text = "Contact selected presence";
            // 
            // cbPresenceList
            // 
            this.cbPresenceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPresenceList.FormattingEnabled = true;
            this.cbPresenceList.Location = new System.Drawing.Point(443, 22);
            this.cbPresenceList.Name = "cbPresenceList";
            this.cbPresenceList.Size = new System.Drawing.Size(168, 21);
            this.cbPresenceList.TabIndex = 72;
            this.cbPresenceList.SelectedIndexChanged += new System.EventHandler(this.cbPresenceList_SelectedIndexChanged);
            // 
            // SampleInstantMessagingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 584);
            this.Controls.Add(this.cbPresenceList);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbContactPresence);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnMarkLastMessageRead);
            this.Controls.Add(this.cbIsTypingStatus);
            this.Controls.Add(this.btnLoadOlderMessages);
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbMessage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbMessagesExchanged);
            this.Controls.Add(this.tbSelectionInfo);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SampleInstantMessagingForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
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
        private System.Windows.Forms.TextBox tbSelectionInfo;
        private System.Windows.Forms.TextBox tbMessagesExchanged;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbMessage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.Button btnLoadOlderMessages;
        private System.Windows.Forms.CheckBox cbIsTypingStatus;
        private System.Windows.Forms.Button btnMarkLastMessageRead;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbContactPresence;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbPresenceList;
    }
}

