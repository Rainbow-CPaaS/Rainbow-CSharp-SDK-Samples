namespace Sample_Contacts
{
    partial class SampleContactForm
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
            this.pbMyContact = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFirstName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbLastName = new System.Windows.Forms.TextBox();
            this.btnAvatarDelete = new System.Windows.Forms.Button();
            this.btnAvatarUpdate = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbNickName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbDisplayName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbJobTitle = new System.Windows.Forms.TextBox();
            this.btnMyContactUpdate = new System.Windows.Forms.Button();
            this.btnContactsList = new System.Windows.Forms.Button();
            this.cbContactsList = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tbContactJobTitle = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbContactTitle = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbContactDisplayName = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbContactNickName = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tbContactLastName = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.tbContactFirstName = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.tbContactId = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.tbContactJid = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.tbSearch = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.cbContactsListSearch = new System.Windows.Forms.ComboBox();
            this.pbContact = new System.Windows.Forms.PictureBox();
            this.label21 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.btnLoginLogout = new System.Windows.Forms.Button();
            this.tbState = new System.Windows.Forms.TextBox();
            this.btnContactAvatar = new System.Windows.Forms.Button();
            this.btnAvatarGet = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.btnRosterAddRemove = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbMyContact)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbContact)).BeginInit();
            this.SuspendLayout();
            // 
            // pbMyContact
            // 
            this.pbMyContact.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbMyContact.Location = new System.Drawing.Point(15, 12);
            this.pbMyContact.Name = "pbMyContact";
            this.pbMyContact.Size = new System.Drawing.Size(80, 80);
            this.pbMyContact.TabIndex = 0;
            this.pbMyContact.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "My Avatar";
            // 
            // tbFirstName
            // 
            this.tbFirstName.Location = new System.Drawing.Point(12, 169);
            this.tbFirstName.Name = "tbFirstName";
            this.tbFirstName.Size = new System.Drawing.Size(164, 20);
            this.tbFirstName.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 153);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "First Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 192);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Last Name";
            // 
            // tbLastName
            // 
            this.tbLastName.Location = new System.Drawing.Point(12, 208);
            this.tbLastName.Name = "tbLastName";
            this.tbLastName.Size = new System.Drawing.Size(164, 20);
            this.tbLastName.TabIndex = 5;
            // 
            // btnAvatarDelete
            // 
            this.btnAvatarDelete.Location = new System.Drawing.Point(101, 40);
            this.btnAvatarDelete.Name = "btnAvatarDelete";
            this.btnAvatarDelete.Size = new System.Drawing.Size(75, 23);
            this.btnAvatarDelete.TabIndex = 7;
            this.btnAvatarDelete.Text = "Delete";
            this.btnAvatarDelete.UseVisualStyleBackColor = true;
            this.btnAvatarDelete.Click += new System.EventHandler(this.btnAvatarDelete_Click);
            // 
            // btnAvatarUpdate
            // 
            this.btnAvatarUpdate.Location = new System.Drawing.Point(101, 69);
            this.btnAvatarUpdate.Name = "btnAvatarUpdate";
            this.btnAvatarUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnAvatarUpdate.TabIndex = 10;
            this.btnAvatarUpdate.Text = "Update";
            this.btnAvatarUpdate.UseVisualStyleBackColor = true;
            this.btnAvatarUpdate.Click += new System.EventHandler(this.btnAvatarUpdate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 227);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Nick Name";
            // 
            // tbNickName
            // 
            this.tbNickName.Location = new System.Drawing.Point(12, 243);
            this.tbNickName.Name = "tbNickName";
            this.tbNickName.Size = new System.Drawing.Size(164, 20);
            this.tbNickName.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 265);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Display Name";
            // 
            // tbDisplayName
            // 
            this.tbDisplayName.Location = new System.Drawing.Point(12, 281);
            this.tbDisplayName.Name = "tbDisplayName";
            this.tbDisplayName.Size = new System.Drawing.Size(164, 20);
            this.tbDisplayName.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 302);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(27, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Title";
            // 
            // tbTitle
            // 
            this.tbTitle.Location = new System.Drawing.Point(12, 318);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(164, 20);
            this.tbTitle.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 341);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Job Title";
            // 
            // tbJobTitle
            // 
            this.tbJobTitle.Location = new System.Drawing.Point(12, 357);
            this.tbJobTitle.Name = "tbJobTitle";
            this.tbJobTitle.Size = new System.Drawing.Size(164, 20);
            this.tbJobTitle.TabIndex = 17;
            // 
            // btnMyContactUpdate
            // 
            this.btnMyContactUpdate.Location = new System.Drawing.Point(43, 383);
            this.btnMyContactUpdate.Name = "btnMyContactUpdate";
            this.btnMyContactUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnMyContactUpdate.TabIndex = 19;
            this.btnMyContactUpdate.Text = "Update";
            this.btnMyContactUpdate.UseVisualStyleBackColor = true;
            this.btnMyContactUpdate.Click += new System.EventHandler(this.btnMyContactUpdate_Click);
            // 
            // btnContactsList
            // 
            this.btnContactsList.Location = new System.Drawing.Point(377, 12);
            this.btnContactsList.Name = "btnContactsList";
            this.btnContactsList.Size = new System.Drawing.Size(94, 23);
            this.btnContactsList.TabIndex = 20;
            this.btnContactsList.Text = "Get Contacts";
            this.btnContactsList.UseVisualStyleBackColor = true;
            this.btnContactsList.Click += new System.EventHandler(this.btnContactsList_Click);
            // 
            // cbContactsList
            // 
            this.cbContactsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbContactsList.FormattingEnabled = true;
            this.cbContactsList.Location = new System.Drawing.Point(307, 41);
            this.cbContactsList.Name = "cbContactsList";
            this.cbContactsList.Size = new System.Drawing.Size(255, 21);
            this.cbContactsList.TabIndex = 22;
            this.cbContactsList.SelectedIndexChanged += new System.EventHandler(this.cbContactsList_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(234, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Contacts list:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(304, 397);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 13);
            this.label9.TabIndex = 35;
            this.label9.Text = "Job Title";
            // 
            // tbContactJobTitle
            // 
            this.tbContactJobTitle.Location = new System.Drawing.Point(304, 413);
            this.tbContactJobTitle.Name = "tbContactJobTitle";
            this.tbContactJobTitle.ReadOnly = true;
            this.tbContactJobTitle.Size = new System.Drawing.Size(164, 20);
            this.tbContactJobTitle.TabIndex = 34;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(304, 358);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(27, 13);
            this.label10.TabIndex = 33;
            this.label10.Text = "Title";
            // 
            // tbContactTitle
            // 
            this.tbContactTitle.Location = new System.Drawing.Point(304, 374);
            this.tbContactTitle.Name = "tbContactTitle";
            this.tbContactTitle.ReadOnly = true;
            this.tbContactTitle.Size = new System.Drawing.Size(164, 20);
            this.tbContactTitle.TabIndex = 32;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(304, 321);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 31;
            this.label11.Text = "Display Name";
            // 
            // tbContactDisplayName
            // 
            this.tbContactDisplayName.Location = new System.Drawing.Point(304, 337);
            this.tbContactDisplayName.Name = "tbContactDisplayName";
            this.tbContactDisplayName.ReadOnly = true;
            this.tbContactDisplayName.Size = new System.Drawing.Size(164, 20);
            this.tbContactDisplayName.TabIndex = 30;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(304, 283);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 29;
            this.label12.Text = "Nick Name";
            // 
            // tbContactNickName
            // 
            this.tbContactNickName.Location = new System.Drawing.Point(304, 299);
            this.tbContactNickName.Name = "tbContactNickName";
            this.tbContactNickName.ReadOnly = true;
            this.tbContactNickName.Size = new System.Drawing.Size(164, 20);
            this.tbContactNickName.TabIndex = 28;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(304, 248);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(58, 13);
            this.label13.TabIndex = 27;
            this.label13.Text = "Last Name";
            // 
            // tbContactLastName
            // 
            this.tbContactLastName.Location = new System.Drawing.Point(304, 264);
            this.tbContactLastName.Name = "tbContactLastName";
            this.tbContactLastName.ReadOnly = true;
            this.tbContactLastName.Size = new System.Drawing.Size(164, 20);
            this.tbContactLastName.TabIndex = 26;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(304, 209);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(57, 13);
            this.label14.TabIndex = 25;
            this.label14.Text = "First Name";
            // 
            // tbContactFirstName
            // 
            this.tbContactFirstName.Location = new System.Drawing.Point(304, 225);
            this.tbContactFirstName.Name = "tbContactFirstName";
            this.tbContactFirstName.ReadOnly = true;
            this.tbContactFirstName.Size = new System.Drawing.Size(164, 20);
            this.tbContactFirstName.TabIndex = 24;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(304, 120);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(123, 13);
            this.label15.TabIndex = 36;
            this.label15.Text = "Contact selected details:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 133);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(97, 13);
            this.label16.TabIndex = 37;
            this.label16.Text = "My Contact details:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(304, 135);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(18, 13);
            this.label17.TabIndex = 39;
            this.label17.Text = "ID";
            // 
            // tbContactId
            // 
            this.tbContactId.Location = new System.Drawing.Point(304, 151);
            this.tbContactId.Name = "tbContactId";
            this.tbContactId.ReadOnly = true;
            this.tbContactId.Size = new System.Drawing.Size(164, 20);
            this.tbContactId.TabIndex = 38;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(304, 171);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(23, 13);
            this.label18.TabIndex = 41;
            this.label18.Text = "JID";
            // 
            // tbContactJid
            // 
            this.tbContactJid.Location = new System.Drawing.Point(304, 187);
            this.tbContactJid.Name = "tbContactJid";
            this.tbContactJid.ReadOnly = true;
            this.tbContactJid.Size = new System.Drawing.Size(164, 20);
            this.tbContactJid.TabIndex = 40;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(234, 71);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(41, 13);
            this.label19.TabIndex = 42;
            this.label19.Text = "Search";
            // 
            // tbSearch
            // 
            this.tbSearch.Location = new System.Drawing.Point(307, 68);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(192, 20);
            this.tbSearch.TabIndex = 43;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(234, 97);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(72, 13);
            this.label20.TabIndex = 45;
            this.label20.Text = "Search result:";
            // 
            // cbContactsListSearch
            // 
            this.cbContactsListSearch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbContactsListSearch.FormattingEnabled = true;
            this.cbContactsListSearch.Location = new System.Drawing.Point(307, 94);
            this.cbContactsListSearch.Name = "cbContactsListSearch";
            this.cbContactsListSearch.Size = new System.Drawing.Size(255, 21);
            this.cbContactsListSearch.TabIndex = 44;
            this.cbContactsListSearch.SelectedIndexChanged += new System.EventHandler(this.cbContactsListSearch_SelectedIndexChanged);
            // 
            // pbContact
            // 
            this.pbContact.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbContact.Location = new System.Drawing.Point(482, 151);
            this.pbContact.Name = "pbContact";
            this.pbContact.Size = new System.Drawing.Size(80, 80);
            this.pbContact.TabIndex = 46;
            this.pbContact.TabStop = false;
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
            // btnContactAvatar
            // 
            this.btnContactAvatar.Location = new System.Drawing.Point(482, 237);
            this.btnContactAvatar.Name = "btnContactAvatar";
            this.btnContactAvatar.Size = new System.Drawing.Size(80, 23);
            this.btnContactAvatar.TabIndex = 53;
            this.btnContactAvatar.Text = "Get";
            this.btnContactAvatar.UseVisualStyleBackColor = true;
            this.btnContactAvatar.Click += new System.EventHandler(this.btnContactAvatar_Click);
            // 
            // btnAvatarGet
            // 
            this.btnAvatarGet.Location = new System.Drawing.Point(101, 11);
            this.btnAvatarGet.Name = "btnAvatarGet";
            this.btnAvatarGet.Size = new System.Drawing.Size(75, 23);
            this.btnAvatarGet.TabIndex = 54;
            this.btnAvatarGet.Text = "Get";
            this.btnAvatarGet.UseVisualStyleBackColor = true;
            this.btnAvatarGet.Click += new System.EventHandler(this.btnAvatarGet_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(504, 135);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(38, 13);
            this.label23.TabIndex = 55;
            this.label23.Text = "Avatar";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(507, 66);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(55, 23);
            this.btnSearch.TabIndex = 56;
            this.btnSearch.Text = "Ok";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(504, 288);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(38, 13);
            this.label24.TabIndex = 57;
            this.label24.Text = "Roster";
            // 
            // btnRosterAddRemove
            // 
            this.btnRosterAddRemove.Location = new System.Drawing.Point(482, 304);
            this.btnRosterAddRemove.Name = "btnRosterAddRemove";
            this.btnRosterAddRemove.Size = new System.Drawing.Size(80, 23);
            this.btnRosterAddRemove.TabIndex = 58;
            this.btnRosterAddRemove.Text = "Add";
            this.btnRosterAddRemove.UseVisualStyleBackColor = true;
            this.btnRosterAddRemove.Click += new System.EventHandler(this.btnRosterAddRemove_Click);
            // 
            // SampleContactForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 584);
            this.Controls.Add(this.btnRosterAddRemove);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.btnAvatarGet);
            this.Controls.Add(this.btnContactAvatar);
            this.Controls.Add(this.tbState);
            this.Controls.Add(this.btnLoginLogout);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.pbContact);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.cbContactsListSearch);
            this.Controls.Add(this.tbSearch);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.tbContactJid);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.tbContactId);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbContactJobTitle);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tbContactTitle);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbContactDisplayName);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbContactNickName);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tbContactLastName);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.tbContactFirstName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbContactsList);
            this.Controls.Add(this.btnContactsList);
            this.Controls.Add(this.btnMyContactUpdate);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbJobTitle);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbDisplayName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbNickName);
            this.Controls.Add(this.btnAvatarUpdate);
            this.Controls.Add(this.btnAvatarDelete);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbLastName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbFirstName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbMyContact);
            this.Name = "SampleContactForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pbMyContact)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbContact)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbMyContact;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFirstName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbLastName;
        private System.Windows.Forms.Button btnAvatarDelete;
        private System.Windows.Forms.Button btnAvatarUpdate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbNickName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbDisplayName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbJobTitle;
        private System.Windows.Forms.Button btnMyContactUpdate;
        private System.Windows.Forms.Button btnContactsList;
        private System.Windows.Forms.ComboBox cbContactsList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbContactJobTitle;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbContactTitle;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbContactDisplayName;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbContactNickName;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbContactLastName;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox tbContactFirstName;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbContactId;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox tbContactJid;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox tbSearch;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.ComboBox cbContactsListSearch;
        private System.Windows.Forms.PictureBox pbContact;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox tbLogin;
        private System.Windows.Forms.Button btnLoginLogout;
        private System.Windows.Forms.TextBox tbState;
        private System.Windows.Forms.Button btnContactAvatar;
        private System.Windows.Forms.Button btnAvatarGet;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Button btnRosterAddRemove;
    }
}

