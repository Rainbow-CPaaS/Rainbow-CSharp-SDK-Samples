namespace SampleChannels
{
    partial class FormChannel
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
            this.btnGetChannels = new System.Windows.Forms.Button();
            this.cbChannels = new System.Windows.Forms.ComboBox();
            this.tbInformation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbChannelName = new System.Windows.Forms.TextBox();
            this.tbChannelCategory = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbChannelTopic = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbChannelId = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pbAvatar = new System.Windows.Forms.PictureBox();
            this.btnCreateChannel = new System.Windows.Forms.Button();
            this.btnUpdateChannel = new System.Windows.Forms.Button();
            this.btnDeleteChannel = new System.Windows.Forms.Button();
            this.btnDeleteAvatar = new System.Windows.Forms.Button();
            this.btnUpdateAvatar = new System.Windows.Forms.Button();
            this.lbContacts = new System.Windows.Forms.ListBox();
            this.lbMembers = new System.Windows.Forms.ListBox();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnRemoveMember = new System.Windows.Forms.Button();
            this.cbMemberAsPublisher = new System.Windows.Forms.CheckBox();
            this.btnGetMembers = new System.Windows.Forms.Button();
            this.btnUnsubscribe = new System.Windows.Forms.Button();
            this.btnAcceptInvitation = new System.Windows.Forms.Button();
            this.btnDeclineInvitation = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btnManageItems = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnGetContacts = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cbChannelInvited = new System.Windows.Forms.CheckBox();
            this.gbCreateChannel = new System.Windows.Forms.GroupBox();
            this.rbCompanyClosed = new System.Windows.Forms.RadioButton();
            this.rbCompanyPrivate = new System.Windows.Forms.RadioButton();
            this.rbCompanyPublic = new System.Windows.Forms.RadioButton();
            this.gbAvatar = new System.Windows.Forms.GroupBox();
            this.btnGetAvatar = new System.Windows.Forms.Button();
            this.tbChannelCreator = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbChannelMode = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbChannelNbUsers = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbChannelNbSubscribers = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cbChannelMuted = new System.Windows.Forms.CheckBox();
            this.btnChannelMute = new System.Windows.Forms.Button();
            this.tbMassProOk = new System.Windows.Forms.Button();
            this.tbMassProNbChannels = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbMassProNbPublishers = new System.Windows.Forms.TextBox();
            this.tbMassProDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            this.gbCreateChannel.SuspendLayout();
            this.gbAvatar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGetChannels
            // 
            this.btnGetChannels.Location = new System.Drawing.Point(21, 12);
            this.btnGetChannels.Name = "btnGetChannels";
            this.btnGetChannels.Size = new System.Drawing.Size(120, 23);
            this.btnGetChannels.TabIndex = 1;
            this.btnGetChannels.Text = "Get Channels";
            this.btnGetChannels.UseVisualStyleBackColor = true;
            this.btnGetChannels.Click += new System.EventHandler(this.btnGetChannels_Click);
            // 
            // cbChannels
            // 
            this.cbChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChannels.FormattingEnabled = true;
            this.cbChannels.Location = new System.Drawing.Point(105, 41);
            this.cbChannels.Name = "cbChannels";
            this.cbChannels.Size = new System.Drawing.Size(296, 23);
            this.cbChannels.TabIndex = 2;
            this.cbChannels.SelectionChangeCommitted += new System.EventHandler(this.cbChannels_SelectionChangeCommitted);
            // 
            // tbInformation
            // 
            this.tbInformation.Location = new System.Drawing.Point(12, 395);
            this.tbInformation.Multiline = true;
            this.tbInformation.Name = "tbInformation";
            this.tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInformation.Size = new System.Drawing.Size(776, 202);
            this.tbInformation.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Name";
            // 
            // tbChannelName
            // 
            this.tbChannelName.Location = new System.Drawing.Point(67, 105);
            this.tbChannelName.Name = "tbChannelName";
            this.tbChannelName.Size = new System.Drawing.Size(127, 23);
            this.tbChannelName.TabIndex = 10;
            // 
            // tbChannelCategory
            // 
            this.tbChannelCategory.Location = new System.Drawing.Point(285, 105);
            this.tbChannelCategory.Name = "tbChannelCategory";
            this.tbChannelCategory.Size = new System.Drawing.Size(116, 23);
            this.tbChannelCategory.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Category";
            // 
            // tbChannelTopic
            // 
            this.tbChannelTopic.Location = new System.Drawing.Point(67, 134);
            this.tbChannelTopic.Name = "tbChannelTopic";
            this.tbChannelTopic.Size = new System.Drawing.Size(333, 23);
            this.tbChannelTopic.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 137);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 15);
            this.label3.TabIndex = 13;
            this.label3.Text = "Topic";
            // 
            // tbChannelId
            // 
            this.tbChannelId.Location = new System.Drawing.Point(67, 163);
            this.tbChannelId.Name = "tbChannelId";
            this.tbChannelId.ReadOnly = true;
            this.tbChannelId.Size = new System.Drawing.Size(333, 23);
            this.tbChannelId.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 166);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 15);
            this.label4.TabIndex = 15;
            this.label4.Text = "Id";
            // 
            // pbAvatar
            // 
            this.pbAvatar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbAvatar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbAvatar.Location = new System.Drawing.Point(6, 22);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.Size = new System.Drawing.Size(100, 101);
            this.pbAvatar.TabIndex = 17;
            this.pbAvatar.TabStop = false;
            // 
            // btnCreateChannel
            // 
            this.btnCreateChannel.Location = new System.Drawing.Point(125, 51);
            this.btnCreateChannel.Name = "btnCreateChannel";
            this.btnCreateChannel.Size = new System.Drawing.Size(57, 23);
            this.btnCreateChannel.TabIndex = 18;
            this.btnCreateChannel.Text = "Create";
            this.btnCreateChannel.UseVisualStyleBackColor = true;
            this.btnCreateChannel.Click += new System.EventHandler(this.btnCreateChannel_Click);
            // 
            // btnUpdateChannel
            // 
            this.btnUpdateChannel.Location = new System.Drawing.Point(246, 293);
            this.btnUpdateChannel.Name = "btnUpdateChannel";
            this.btnUpdateChannel.Size = new System.Drawing.Size(74, 23);
            this.btnUpdateChannel.TabIndex = 19;
            this.btnUpdateChannel.Text = "Update";
            this.btnUpdateChannel.UseVisualStyleBackColor = true;
            this.btnUpdateChannel.Click += new System.EventHandler(this.btnUpdateChannel_Click);
            // 
            // btnDeleteChannel
            // 
            this.btnDeleteChannel.Location = new System.Drawing.Point(326, 293);
            this.btnDeleteChannel.Name = "btnDeleteChannel";
            this.btnDeleteChannel.Size = new System.Drawing.Size(74, 23);
            this.btnDeleteChannel.TabIndex = 20;
            this.btnDeleteChannel.Text = "Delete";
            this.btnDeleteChannel.UseVisualStyleBackColor = true;
            this.btnDeleteChannel.Click += new System.EventHandler(this.btnDeleteChannel_Click);
            // 
            // btnDeleteAvatar
            // 
            this.btnDeleteAvatar.Location = new System.Drawing.Point(6, 158);
            this.btnDeleteAvatar.Name = "btnDeleteAvatar";
            this.btnDeleteAvatar.Size = new System.Drawing.Size(100, 23);
            this.btnDeleteAvatar.TabIndex = 21;
            this.btnDeleteAvatar.Text = "Delete";
            this.btnDeleteAvatar.UseVisualStyleBackColor = true;
            this.btnDeleteAvatar.Click += new System.EventHandler(this.btnDeleteAvatar_Click);
            // 
            // btnUpdateAvatar
            // 
            this.btnUpdateAvatar.Location = new System.Drawing.Point(6, 187);
            this.btnUpdateAvatar.Name = "btnUpdateAvatar";
            this.btnUpdateAvatar.Size = new System.Drawing.Size(100, 23);
            this.btnUpdateAvatar.TabIndex = 22;
            this.btnUpdateAvatar.Text = "Update";
            this.btnUpdateAvatar.UseVisualStyleBackColor = true;
            this.btnUpdateAvatar.Click += new System.EventHandler(this.btnUpdateAvatar_Click);
            // 
            // lbContacts
            // 
            this.lbContacts.FormattingEnabled = true;
            this.lbContacts.ItemHeight = 15;
            this.lbContacts.Location = new System.Drawing.Point(545, 225);
            this.lbContacts.Name = "lbContacts";
            this.lbContacts.Size = new System.Drawing.Size(242, 124);
            this.lbContacts.TabIndex = 23;
            // 
            // lbMembers
            // 
            this.lbMembers.FormattingEnabled = true;
            this.lbMembers.ItemHeight = 15;
            this.lbMembers.Location = new System.Drawing.Point(546, 23);
            this.lbMembers.Name = "lbMembers";
            this.lbMembers.Size = new System.Drawing.Size(242, 124);
            this.lbMembers.TabIndex = 24;
            // 
            // btnAddMember
            // 
            this.btnAddMember.Location = new System.Drawing.Point(545, 355);
            this.btnAddMember.Name = "btnAddMember";
            this.btnAddMember.Size = new System.Drawing.Size(139, 23);
            this.btnAddMember.TabIndex = 25;
            this.btnAddMember.Text = "Add / Invite as Member";
            this.btnAddMember.UseVisualStyleBackColor = true;
            this.btnAddMember.Click += new System.EventHandler(this.btnAddMember_Click);
            // 
            // btnRemoveMember
            // 
            this.btnRemoveMember.Location = new System.Drawing.Point(618, 153);
            this.btnRemoveMember.Name = "btnRemoveMember";
            this.btnRemoveMember.Size = new System.Drawing.Size(100, 23);
            this.btnRemoveMember.TabIndex = 27;
            this.btnRemoveMember.Text = "Remove Member";
            this.btnRemoveMember.UseVisualStyleBackColor = true;
            this.btnRemoveMember.Click += new System.EventHandler(this.btnRemoveMember_Click);
            // 
            // cbMemberAsPublisher
            // 
            this.cbMemberAsPublisher.AutoSize = true;
            this.cbMemberAsPublisher.Location = new System.Drawing.Point(698, 357);
            this.cbMemberAsPublisher.Name = "cbMemberAsPublisher";
            this.cbMemberAsPublisher.Size = new System.Drawing.Size(89, 19);
            this.cbMemberAsPublisher.TabIndex = 28;
            this.cbMemberAsPublisher.Text = "as publisher";
            this.cbMemberAsPublisher.UseVisualStyleBackColor = true;
            // 
            // btnGetMembers
            // 
            this.btnGetMembers.Location = new System.Drawing.Point(618, 0);
            this.btnGetMembers.Name = "btnGetMembers";
            this.btnGetMembers.Size = new System.Drawing.Size(48, 23);
            this.btnGetMembers.TabIndex = 29;
            this.btnGetMembers.Text = "Get";
            this.btnGetMembers.UseVisualStyleBackColor = true;
            this.btnGetMembers.Click += new System.EventHandler(this.btnGetMembers_Click);
            // 
            // btnUnsubscribe
            // 
            this.btnUnsubscribe.Location = new System.Drawing.Point(281, 70);
            this.btnUnsubscribe.Name = "btnUnsubscribe";
            this.btnUnsubscribe.Size = new System.Drawing.Size(120, 23);
            this.btnUnsubscribe.TabIndex = 30;
            this.btnUnsubscribe.Text = "Unsubscribe";
            this.btnUnsubscribe.UseVisualStyleBackColor = true;
            this.btnUnsubscribe.Click += new System.EventHandler(this.btnUnsubscribe_Click);
            // 
            // btnAcceptInvitation
            // 
            this.btnAcceptInvitation.Location = new System.Drawing.Point(21, 70);
            this.btnAcceptInvitation.Name = "btnAcceptInvitation";
            this.btnAcceptInvitation.Size = new System.Drawing.Size(120, 23);
            this.btnAcceptInvitation.TabIndex = 31;
            this.btnAcceptInvitation.Text = "Accept invit.";
            this.btnAcceptInvitation.UseVisualStyleBackColor = true;
            this.btnAcceptInvitation.Click += new System.EventHandler(this.btnAcceptInvitation_Click);
            // 
            // btnDeclineInvitation
            // 
            this.btnDeclineInvitation.Location = new System.Drawing.Point(150, 70);
            this.btnDeclineInvitation.Name = "btnDeclineInvitation";
            this.btnDeclineInvitation.Size = new System.Drawing.Size(120, 23);
            this.btnDeclineInvitation.TabIndex = 32;
            this.btnDeclineInvitation.Text = "Decline Invit.";
            this.btnDeclineInvitation.UseVisualStyleBackColor = true;
            this.btnDeclineInvitation.Click += new System.EventHandler(this.btnDeclineInvitation_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(546, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 15);
            this.label5.TabIndex = 33;
            this.label5.Text = "Members";
            // 
            // btnManageItems
            // 
            this.btnManageItems.Location = new System.Drawing.Point(277, 322);
            this.btnManageItems.Name = "btnManageItems";
            this.btnManageItems.Size = new System.Drawing.Size(102, 23);
            this.btnManageItems.TabIndex = 34;
            this.btnManageItems.Text = "Manage Items";
            this.btnManageItems.UseVisualStyleBackColor = true;
            this.btnManageItems.Click += new System.EventHandler(this.btnManageItems_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(548, 206);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 15);
            this.label6.TabIndex = 36;
            this.label6.Text = "Contacts";
            // 
            // btnGetContacts
            // 
            this.btnGetContacts.Location = new System.Drawing.Point(618, 201);
            this.btnGetContacts.Name = "btnGetContacts";
            this.btnGetContacts.Size = new System.Drawing.Size(48, 23);
            this.btnGetContacts.TabIndex = 35;
            this.btnGetContacts.Text = "Get";
            this.btnGetContacts.UseVisualStyleBackColor = true;
            this.btnGetContacts.Click += new System.EventHandler(this.btnGetContacts_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 44);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 15);
            this.label7.TabIndex = 37;
            this.label7.Text = "Name";
            // 
            // cbChannelInvited
            // 
            this.cbChannelInvited.AutoSize = true;
            this.cbChannelInvited.Location = new System.Drawing.Point(150, 15);
            this.cbChannelInvited.Name = "cbChannelInvited";
            this.cbChannelInvited.Size = new System.Drawing.Size(107, 19);
            this.cbChannelInvited.TabIndex = 38;
            this.cbChannelInvited.Text = "invited channel";
            this.cbChannelInvited.UseVisualStyleBackColor = true;
            // 
            // gbCreateChannel
            // 
            this.gbCreateChannel.Controls.Add(this.rbCompanyClosed);
            this.gbCreateChannel.Controls.Add(this.rbCompanyPrivate);
            this.gbCreateChannel.Controls.Add(this.rbCompanyPublic);
            this.gbCreateChannel.Controls.Add(this.btnCreateChannel);
            this.gbCreateChannel.Location = new System.Drawing.Point(42, 264);
            this.gbCreateChannel.Name = "gbCreateChannel";
            this.gbCreateChannel.Size = new System.Drawing.Size(182, 102);
            this.gbCreateChannel.TabIndex = 42;
            this.gbCreateChannel.TabStop = false;
            this.gbCreateChannel.Text = "Create";
            // 
            // rbCompanyClosed
            // 
            this.rbCompanyClosed.AutoSize = true;
            this.rbCompanyClosed.Location = new System.Drawing.Point(6, 72);
            this.rbCompanyClosed.Name = "rbCompanyClosed";
            this.rbCompanyClosed.Size = new System.Drawing.Size(113, 19);
            this.rbCompanyClosed.TabIndex = 45;
            this.rbCompanyClosed.Text = "CompanyClosed";
            this.rbCompanyClosed.UseVisualStyleBackColor = true;
            // 
            // rbCompanyPrivate
            // 
            this.rbCompanyPrivate.AutoSize = true;
            this.rbCompanyPrivate.Location = new System.Drawing.Point(6, 47);
            this.rbCompanyPrivate.Name = "rbCompanyPrivate";
            this.rbCompanyPrivate.Size = new System.Drawing.Size(113, 19);
            this.rbCompanyPrivate.TabIndex = 44;
            this.rbCompanyPrivate.Text = "CompanyPrivate";
            this.rbCompanyPrivate.UseVisualStyleBackColor = true;
            // 
            // rbCompanyPublic
            // 
            this.rbCompanyPublic.AutoSize = true;
            this.rbCompanyPublic.Checked = true;
            this.rbCompanyPublic.Location = new System.Drawing.Point(6, 22);
            this.rbCompanyPublic.Name = "rbCompanyPublic";
            this.rbCompanyPublic.Size = new System.Drawing.Size(110, 19);
            this.rbCompanyPublic.TabIndex = 43;
            this.rbCompanyPublic.TabStop = true;
            this.rbCompanyPublic.Text = "CompanyPublic";
            this.rbCompanyPublic.UseVisualStyleBackColor = true;
            // 
            // gbAvatar
            // 
            this.gbAvatar.Controls.Add(this.btnGetAvatar);
            this.gbAvatar.Controls.Add(this.pbAvatar);
            this.gbAvatar.Controls.Add(this.btnDeleteAvatar);
            this.gbAvatar.Controls.Add(this.btnUpdateAvatar);
            this.gbAvatar.Location = new System.Drawing.Point(418, 5);
            this.gbAvatar.Name = "gbAvatar";
            this.gbAvatar.Size = new System.Drawing.Size(112, 219);
            this.gbAvatar.TabIndex = 43;
            this.gbAvatar.TabStop = false;
            this.gbAvatar.Text = "Avatar";
            // 
            // btnGetAvatar
            // 
            this.btnGetAvatar.Location = new System.Drawing.Point(6, 129);
            this.btnGetAvatar.Name = "btnGetAvatar";
            this.btnGetAvatar.Size = new System.Drawing.Size(100, 23);
            this.btnGetAvatar.TabIndex = 23;
            this.btnGetAvatar.Text = "Get";
            this.btnGetAvatar.UseVisualStyleBackColor = true;
            this.btnGetAvatar.Click += new System.EventHandler(this.btnGetAvatar_Click);
            // 
            // tbChannelCreator
            // 
            this.tbChannelCreator.Location = new System.Drawing.Point(67, 192);
            this.tbChannelCreator.Name = "tbChannelCreator";
            this.tbChannelCreator.ReadOnly = true;
            this.tbChannelCreator.Size = new System.Drawing.Size(158, 23);
            this.tbChannelCreator.TabIndex = 45;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(22, 195);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(46, 15);
            this.label10.TabIndex = 44;
            this.label10.Text = "Creator";
            // 
            // tbChannelMode
            // 
            this.tbChannelMode.Location = new System.Drawing.Point(278, 192);
            this.tbChannelMode.Name = "tbChannelMode";
            this.tbChannelMode.ReadOnly = true;
            this.tbChannelMode.Size = new System.Drawing.Size(122, 23);
            this.tbChannelMode.TabIndex = 47;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(234, 195);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(38, 15);
            this.label11.TabIndex = 46;
            this.label11.Text = "Mode";
            // 
            // tbChannelNbUsers
            // 
            this.tbChannelNbUsers.Location = new System.Drawing.Point(67, 221);
            this.tbChannelNbUsers.Name = "tbChannelNbUsers";
            this.tbChannelNbUsers.ReadOnly = true;
            this.tbChannelNbUsers.Size = new System.Drawing.Size(55, 23);
            this.tbChannelNbUsers.TabIndex = 49;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(22, 224);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 15);
            this.label12.TabIndex = 48;
            this.label12.Text = "Users";
            // 
            // tbChannelNbSubscribers
            // 
            this.tbChannelNbSubscribers.Location = new System.Drawing.Point(197, 221);
            this.tbChannelNbSubscribers.Name = "tbChannelNbSubscribers";
            this.tbChannelNbSubscribers.ReadOnly = true;
            this.tbChannelNbSubscribers.Size = new System.Drawing.Size(48, 23);
            this.tbChannelNbSubscribers.TabIndex = 51;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(127, 224);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(67, 15);
            this.label13.TabIndex = 50;
            this.label13.Text = "Subscribers";
            // 
            // cbChannelMuted
            // 
            this.cbChannelMuted.AutoCheck = false;
            this.cbChannelMuted.CausesValidation = false;
            this.cbChannelMuted.Location = new System.Drawing.Point(265, 223);
            this.cbChannelMuted.Name = "cbChannelMuted";
            this.cbChannelMuted.Size = new System.Drawing.Size(61, 19);
            this.cbChannelMuted.TabIndex = 52;
            this.cbChannelMuted.Text = "Muted";
            this.cbChannelMuted.UseVisualStyleBackColor = true;
            // 
            // btnChannelMute
            // 
            this.btnChannelMute.Location = new System.Drawing.Point(277, 264);
            this.btnChannelMute.Name = "btnChannelMute";
            this.btnChannelMute.Size = new System.Drawing.Size(102, 23);
            this.btnChannelMute.TabIndex = 53;
            this.btnChannelMute.Text = "Mute / Unmute";
            this.btnChannelMute.UseVisualStyleBackColor = true;
            this.btnChannelMute.Click += new System.EventHandler(this.btnChannelMute_Click);
            // 
            // tbMassProOk
            // 
            this.tbMassProOk.Location = new System.Drawing.Point(18, 84);
            this.tbMassProOk.Name = "tbMassProOk";
            this.tbMassProOk.Size = new System.Drawing.Size(102, 23);
            this.tbMassProOk.TabIndex = 54;
            this.tbMassProOk.Text = "Ok";
            this.tbMassProOk.UseVisualStyleBackColor = true;
            this.tbMassProOk.Click += new System.EventHandler(this.tbMassProOk_Click);
            // 
            // tbMassProNbChannels
            // 
            this.tbMassProNbChannels.Location = new System.Drawing.Point(99, 23);
            this.tbMassProNbChannels.Name = "tbMassProNbChannels";
            this.tbMassProNbChannels.Size = new System.Drawing.Size(25, 23);
            this.tbMassProNbChannels.TabIndex = 56;
            this.tbMassProNbChannels.Text = "20";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 15);
            this.label8.TabIndex = 55;
            this.label8.Text = "Nb Channels";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbMassProDelete);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.tbMassProNbPublishers);
            this.groupBox1.Controls.Add(this.tbMassProOk);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.tbMassProNbChannels);
            this.groupBox1.Location = new System.Drawing.Point(406, 240);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(133, 149);
            this.groupBox1.TabIndex = 57;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mass provisionning";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 58);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(87, 15);
            this.label9.TabIndex = 57;
            this.label9.Text = "Max Nb Publis.";
            // 
            // tbMassProNbPublishers
            // 
            this.tbMassProNbPublishers.Location = new System.Drawing.Point(99, 55);
            this.tbMassProNbPublishers.Name = "tbMassProNbPublishers";
            this.tbMassProNbPublishers.Size = new System.Drawing.Size(25, 23);
            this.tbMassProNbPublishers.TabIndex = 58;
            this.tbMassProNbPublishers.Text = "3";
            // 
            // tbMassProDelete
            // 
            this.tbMassProDelete.Location = new System.Drawing.Point(18, 113);
            this.tbMassProDelete.Name = "tbMassProDelete";
            this.tbMassProDelete.Size = new System.Drawing.Size(102, 23);
            this.tbMassProDelete.TabIndex = 59;
            this.tbMassProDelete.Text = "Delete";
            this.tbMassProDelete.UseVisualStyleBackColor = true;
            this.tbMassProDelete.Click += new System.EventHandler(this.tbMassProDelete_Click);
            // 
            // FormChannel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 609);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnChannelMute);
            this.Controls.Add(this.cbChannelMuted);
            this.Controls.Add(this.tbChannelNbSubscribers);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tbChannelNbUsers);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbChannelMode);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbChannelCreator);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.gbAvatar);
            this.Controls.Add(this.gbCreateChannel);
            this.Controls.Add(this.cbChannelInvited);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnGetContacts);
            this.Controls.Add(this.btnManageItems);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnDeclineInvitation);
            this.Controls.Add(this.btnAcceptInvitation);
            this.Controls.Add(this.btnUnsubscribe);
            this.Controls.Add(this.btnGetMembers);
            this.Controls.Add(this.cbMemberAsPublisher);
            this.Controls.Add(this.btnRemoveMember);
            this.Controls.Add(this.btnAddMember);
            this.Controls.Add(this.lbMembers);
            this.Controls.Add(this.lbContacts);
            this.Controls.Add(this.btnDeleteChannel);
            this.Controls.Add(this.btnUpdateChannel);
            this.Controls.Add(this.tbChannelId);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbChannelTopic);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbChannelCategory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbChannelName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.cbChannels);
            this.Controls.Add(this.btnGetChannels);
            this.Name = "FormChannel";
            this.Text = "FormChannel";
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            this.gbCreateChannel.ResumeLayout(false);
            this.gbCreateChannel.PerformLayout();
            this.gbAvatar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnGetChannels;
        private ComboBox cbChannels;
        private TextBox tbInformation;
        private Label label1;
        private TextBox tbChannelName;
        private TextBox tbChannelCategory;
        private Label label2;
        private TextBox tbChannelTopic;
        private Label label3;
        private TextBox tbChannelId;
        private Label label4;
        private PictureBox pbAvatar;
        private Button btnCreateChannel;
        private Button btnUpdateChannel;
        private Button btnDeleteChannel;
        private Button btnDeleteAvatar;
        private Button btnUpdateAvatar;
        private ListBox lbContacts;
        private ListBox lbMembers;
        private Button btnAddMember;
        private Button btnRemoveMember;
        private CheckBox cbMemberAsPublisher;
        private Button btnGetMembers;
        private Button btnUnsubscribe;
        private Button btnAcceptInvitation;
        private Button btnDeclineInvitation;
        private Label label5;
        private Button btnManageItems;
        private Label label6;
        private Button btnGetContacts;
        private Label label7;
        private CheckBox cbChannelInvited;
        private GroupBox gbCreateChannel;
        private RadioButton rbCompanyPublic;
        private RadioButton rbCompanyClosed;
        private RadioButton rbCompanyPrivate;
        private GroupBox gbAvatar;
        private Button btnGetAvatar;
        private TextBox tbChannelCreator;
        private Label label10;
        private TextBox tbChannelMode;
        private Label label11;
        private TextBox tbChannelNbUsers;
        private Label label12;
        private TextBox tbChannelNbSubscribers;
        private Label label13;
        private CheckBox cbChannelMuted;
        private Button btnChannelMute;
        private Button tbMassProOk;
        private TextBox tbMassProNbChannels;
        private Label label8;
        private GroupBox groupBox1;
        private Label label9;
        private TextBox tbMassProNbPublishers;
        private Button tbMassProDelete;
    }
}