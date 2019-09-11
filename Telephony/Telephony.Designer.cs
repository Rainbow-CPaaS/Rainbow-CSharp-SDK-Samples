namespace Sample_Telephony
{
    partial class SampleTelephonyForm
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
            this.btnLoginLogout = new System.Windows.Forms.Button();
            this.label21 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbTelephonyService = new System.Windows.Forms.CheckBox();
            this.tbPBXAgent = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbVoiceMail = new System.Windows.Forms.CheckBox();
            this.tbNbVoiceMessages = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbVoiceMessages = new System.Windows.Forms.ComboBox();
            this.btVMDownload = new System.Windows.Forms.Button();
            this.btVMDelete = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.cbCallFwd = new System.Windows.Forms.CheckBox();
            this.cbCall1IsConference = new System.Windows.Forms.CheckBox();
            this.cbNomadic = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbState = new System.Windows.Forms.TextBox();
            this.tbVoiceMessagePhoneNumber = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.gbCallFwd = new System.Windows.Forms.GroupBox();
            this.btCallFwdSet = new System.Windows.Forms.Button();
            this.tbCallFwdPhoneNumber = new System.Windows.Forms.TextBox();
            this.rbCallFwdToPhoneNumber = new System.Windows.Forms.RadioButton();
            this.rbCallFwdToVm = new System.Windows.Forms.RadioButton();
            this.rbCallFwdDisable = new System.Windows.Forms.RadioButton();
            this.gbNomadic = new System.Windows.Forms.GroupBox();
            this.btNomadicSet = new System.Windows.Forms.Button();
            this.rbNomadicToOfficePhone = new System.Windows.Forms.RadioButton();
            this.tbNomadicPhoneNumber = new System.Windows.Forms.TextBox();
            this.rbNomadicToPhoneNumber = new System.Windows.Forms.RadioButton();
            this.rbNomadicToComputer = new System.Windows.Forms.RadioButton();
            this.gbVoiceMail = new System.Windows.Forms.GroupBox();
            this.lblVMInfo = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.gbFirstCall = new System.Windows.Forms.GroupBox();
            this.cbCall1Dtmf = new System.Windows.Forms.ComboBox();
            this.btn3Call1 = new System.Windows.Forms.Button();
            this.btn2Call1 = new System.Windows.Forms.Button();
            this.btn1Call1 = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.tbCall1DeviceType = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tbCall1RemoteMedia = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbCall1LocalMedia = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbCall1Status = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbCall1Id = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbCall2Dtmf = new System.Windows.Forms.ComboBox();
            this.btn3Call2 = new System.Windows.Forms.Button();
            this.btn2Call2 = new System.Windows.Forms.Button();
            this.btn1Call2 = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.tbCall2DeviceType = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.tbCall2RemoteMedia = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.tbCall2LocalMedia = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.tbCall2Status = new System.Windows.Forms.TextBox();
            this.cbCall2IsConference = new System.Windows.Forms.CheckBox();
            this.label23 = new System.Windows.Forms.Label();
            this.tbCall2Id = new System.Windows.Forms.TextBox();
            this.btCallTransfer = new System.Windows.Forms.Button();
            this.btCallConference = new System.Windows.Forms.Button();
            this.tbCall1Participants = new System.Windows.Forms.TextBox();
            this.tbCall2Participants = new System.Windows.Forms.TextBox();
            this.tbMakeCall = new System.Windows.Forms.TextBox();
            this.btMakeCall = new System.Windows.Forms.Button();
            this.gbCallFwd.SuspendLayout();
            this.gbNomadic.SuspendLayout();
            this.gbVoiceMail.SuspendLayout();
            this.gbFirstCall.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoginLogout
            // 
            this.btnLoginLogout.Location = new System.Drawing.Point(257, 42);
            this.btnLoginLogout.Name = "btnLoginLogout";
            this.btnLoginLogout.Size = new System.Drawing.Size(75, 23);
            this.btnLoginLogout.TabIndex = 56;
            this.btnLoginLogout.Text = "Login";
            this.btnLoginLogout.UseVisualStyleBackColor = true;
            this.btnLoginLogout.Click += new System.EventHandler(this.btnLoginLogout_Click);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(28, 47);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(56, 13);
            this.label21.TabIndex = 55;
            this.label21.Text = "Password:";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(87, 44);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(164, 20);
            this.tbPassword.TabIndex = 54;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(48, 22);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(36, 13);
            this.label22.TabIndex = 53;
            this.label22.Text = "Login:";
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(87, 20);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(164, 20);
            this.tbLogin.TabIndex = 52;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 20);
            this.label1.TabIndex = 57;
            this.label1.Text = "Telephony service";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 59;
            this.label3.Text = "PBX Agent:";
            // 
            // cbTelephonyService
            // 
            this.cbTelephonyService.AutoCheck = false;
            this.cbTelephonyService.AutoSize = true;
            this.cbTelephonyService.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbTelephonyService.Location = new System.Drawing.Point(257, 13);
            this.cbTelephonyService.Name = "cbTelephonyService";
            this.cbTelephonyService.Size = new System.Drawing.Size(78, 17);
            this.cbTelephonyService.TabIndex = 60;
            this.cbTelephonyService.Text = "Available";
            this.cbTelephonyService.UseVisualStyleBackColor = true;
            // 
            // tbPBXAgent
            // 
            this.tbPBXAgent.Location = new System.Drawing.Point(86, 32);
            this.tbPBXAgent.Name = "tbPBXAgent";
            this.tbPBXAgent.ReadOnly = true;
            this.tbPBXAgent.Size = new System.Drawing.Size(164, 20);
            this.tbPBXAgent.TabIndex = 61;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 20);
            this.label2.TabIndex = 62;
            this.label2.Text = "Voice mail";
            // 
            // cbVoiceMail
            // 
            this.cbVoiceMail.AutoCheck = false;
            this.cbVoiceMail.AutoSize = true;
            this.cbVoiceMail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbVoiceMail.Location = new System.Drawing.Point(256, 74);
            this.cbVoiceMail.Name = "cbVoiceMail";
            this.cbVoiceMail.Size = new System.Drawing.Size(78, 17);
            this.cbVoiceMail.TabIndex = 63;
            this.cbVoiceMail.Text = "Available";
            this.cbVoiceMail.UseVisualStyleBackColor = true;
            // 
            // tbNbVoiceMessages
            // 
            this.tbNbVoiceMessages.Location = new System.Drawing.Point(122, 13);
            this.tbNbVoiceMessages.Name = "tbNbVoiceMessages";
            this.tbNbVoiceMessages.ReadOnly = true;
            this.tbNbVoiceMessages.Size = new System.Drawing.Size(34, 20);
            this.tbNbVoiceMessages.TabIndex = 65;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 64;
            this.label4.Text = "Nb Voice message(s):";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 67;
            this.label5.Text = "Voice message(s)";
            // 
            // cbVoiceMessages
            // 
            this.cbVoiceMessages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVoiceMessages.FormattingEnabled = true;
            this.cbVoiceMessages.Location = new System.Drawing.Point(102, 45);
            this.cbVoiceMessages.Name = "cbVoiceMessages";
            this.cbVoiceMessages.Size = new System.Drawing.Size(206, 21);
            this.cbVoiceMessages.TabIndex = 66;
            // 
            // btVMDownload
            // 
            this.btVMDownload.Location = new System.Drawing.Point(116, 72);
            this.btVMDownload.Name = "btVMDownload";
            this.btVMDownload.Size = new System.Drawing.Size(75, 23);
            this.btVMDownload.TabIndex = 68;
            this.btVMDownload.Text = "Download";
            this.btVMDownload.UseVisualStyleBackColor = true;
            // 
            // btVMDelete
            // 
            this.btVMDelete.Location = new System.Drawing.Point(197, 72);
            this.btVMDelete.Name = "btVMDelete";
            this.btVMDelete.Size = new System.Drawing.Size(75, 23);
            this.btVMDelete.TabIndex = 69;
            this.btVMDelete.Text = "Delete";
            this.btVMDelete.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 228);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 20);
            this.label6.TabIndex = 70;
            this.label6.Text = "Call forward";
            // 
            // cbCallFwd
            // 
            this.cbCallFwd.AutoCheck = false;
            this.cbCallFwd.AutoSize = true;
            this.cbCallFwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCallFwd.Location = new System.Drawing.Point(257, 232);
            this.cbCallFwd.Name = "cbCallFwd";
            this.cbCallFwd.Size = new System.Drawing.Size(78, 17);
            this.cbCallFwd.TabIndex = 71;
            this.cbCallFwd.Text = "Available";
            this.cbCallFwd.UseVisualStyleBackColor = true;
            // 
            // cbCall1IsConference
            // 
            this.cbCall1IsConference.AutoCheck = false;
            this.cbCall1IsConference.AutoSize = true;
            this.cbCall1IsConference.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbCall1IsConference.Location = new System.Drawing.Point(244, 66);
            this.cbCall1IsConference.Name = "cbCall1IsConference";
            this.cbCall1IsConference.Size = new System.Drawing.Size(95, 17);
            this.cbCall1IsConference.TabIndex = 72;
            this.cbCall1IsConference.Text = "Is Conference:";
            this.cbCall1IsConference.UseVisualStyleBackColor = true;
            // 
            // cbNomadic
            // 
            this.cbNomadic.AutoCheck = false;
            this.cbNomadic.AutoSize = true;
            this.cbNomadic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNomadic.Location = new System.Drawing.Point(257, 375);
            this.cbNomadic.Name = "cbNomadic";
            this.cbNomadic.Size = new System.Drawing.Size(78, 17);
            this.cbNomadic.TabIndex = 79;
            this.cbNomadic.Text = "Available";
            this.cbNomadic.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(12, 372);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 20);
            this.label7.TabIndex = 78;
            this.label7.Text = "Nomadic";
            // 
            // tbState
            // 
            this.tbState.Location = new System.Drawing.Point(4, 559);
            this.tbState.Multiline = true;
            this.tbState.Name = "tbState";
            this.tbState.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbState.Size = new System.Drawing.Size(791, 90);
            this.tbState.TabIndex = 84;
            // 
            // tbVoiceMessagePhoneNumber
            // 
            this.tbVoiceMessagePhoneNumber.Location = new System.Drawing.Point(240, 13);
            this.tbVoiceMessagePhoneNumber.Name = "tbVoiceMessagePhoneNumber";
            this.tbVoiceMessagePhoneNumber.ReadOnly = true;
            this.tbVoiceMessagePhoneNumber.Size = new System.Drawing.Size(67, 20);
            this.tbVoiceMessagePhoneNumber.TabIndex = 86;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(165, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(69, 13);
            this.label8.TabIndex = 85;
            this.label8.Text = "Phone Num.:";
            // 
            // gbCallFwd
            // 
            this.gbCallFwd.Controls.Add(this.btCallFwdSet);
            this.gbCallFwd.Controls.Add(this.tbCallFwdPhoneNumber);
            this.gbCallFwd.Controls.Add(this.rbCallFwdToPhoneNumber);
            this.gbCallFwd.Controls.Add(this.rbCallFwdToVm);
            this.gbCallFwd.Controls.Add(this.rbCallFwdDisable);
            this.gbCallFwd.Enabled = false;
            this.gbCallFwd.Location = new System.Drawing.Point(16, 255);
            this.gbCallFwd.Name = "gbCallFwd";
            this.gbCallFwd.Size = new System.Drawing.Size(320, 100);
            this.gbCallFwd.TabIndex = 87;
            this.gbCallFwd.TabStop = false;
            // 
            // btCallFwdSet
            // 
            this.btCallFwdSet.Location = new System.Drawing.Point(234, 19);
            this.btCallFwdSet.Name = "btCallFwdSet";
            this.btCallFwdSet.Size = new System.Drawing.Size(75, 23);
            this.btCallFwdSet.TabIndex = 89;
            this.btCallFwdSet.Text = "Set";
            this.btCallFwdSet.UseVisualStyleBackColor = true;
            this.btCallFwdSet.Click += new System.EventHandler(this.btCallFwdSet_Click);
            // 
            // tbCallFwdPhoneNumber
            // 
            this.tbCallFwdPhoneNumber.Location = new System.Drawing.Point(160, 65);
            this.tbCallFwdPhoneNumber.Name = "tbCallFwdPhoneNumber";
            this.tbCallFwdPhoneNumber.Size = new System.Drawing.Size(148, 20);
            this.tbCallFwdPhoneNumber.TabIndex = 3;
            // 
            // rbCallFwdToPhoneNumber
            // 
            this.rbCallFwdToPhoneNumber.AutoSize = true;
            this.rbCallFwdToPhoneNumber.Location = new System.Drawing.Point(7, 66);
            this.rbCallFwdToPhoneNumber.Name = "rbCallFwdToPhoneNumber";
            this.rbCallFwdToPhoneNumber.Size = new System.Drawing.Size(150, 17);
            this.rbCallFwdToPhoneNumber.TabIndex = 2;
            this.rbCallFwdToPhoneNumber.TabStop = true;
            this.rbCallFwdToPhoneNumber.Text = "Enable on Phone Number:";
            this.rbCallFwdToPhoneNumber.UseVisualStyleBackColor = true;
            // 
            // rbCallFwdToVm
            // 
            this.rbCallFwdToVm.AutoSize = true;
            this.rbCallFwdToVm.Location = new System.Drawing.Point(7, 43);
            this.rbCallFwdToVm.Name = "rbCallFwdToVm";
            this.rbCallFwdToVm.Size = new System.Drawing.Size(125, 17);
            this.rbCallFwdToVm.TabIndex = 1;
            this.rbCallFwdToVm.TabStop = true;
            this.rbCallFwdToVm.Text = "Enable on Voice Mail";
            this.rbCallFwdToVm.UseVisualStyleBackColor = true;
            // 
            // rbCallFwdDisable
            // 
            this.rbCallFwdDisable.AutoSize = true;
            this.rbCallFwdDisable.Location = new System.Drawing.Point(7, 19);
            this.rbCallFwdDisable.Name = "rbCallFwdDisable";
            this.rbCallFwdDisable.Size = new System.Drawing.Size(60, 17);
            this.rbCallFwdDisable.TabIndex = 0;
            this.rbCallFwdDisable.TabStop = true;
            this.rbCallFwdDisable.Text = "Disable";
            this.rbCallFwdDisable.UseVisualStyleBackColor = true;
            // 
            // gbNomadic
            // 
            this.gbNomadic.Controls.Add(this.btNomadicSet);
            this.gbNomadic.Controls.Add(this.rbNomadicToOfficePhone);
            this.gbNomadic.Controls.Add(this.tbNomadicPhoneNumber);
            this.gbNomadic.Controls.Add(this.rbNomadicToPhoneNumber);
            this.gbNomadic.Controls.Add(this.rbNomadicToComputer);
            this.gbNomadic.Enabled = false;
            this.gbNomadic.Location = new System.Drawing.Point(16, 395);
            this.gbNomadic.Name = "gbNomadic";
            this.gbNomadic.Size = new System.Drawing.Size(320, 97);
            this.gbNomadic.TabIndex = 88;
            this.gbNomadic.TabStop = false;
            // 
            // btNomadicSet
            // 
            this.btNomadicSet.Location = new System.Drawing.Point(232, 13);
            this.btNomadicSet.Name = "btNomadicSet";
            this.btNomadicSet.Size = new System.Drawing.Size(75, 23);
            this.btNomadicSet.TabIndex = 90;
            this.btNomadicSet.Text = "Set";
            this.btNomadicSet.UseVisualStyleBackColor = true;
            this.btNomadicSet.Click += new System.EventHandler(this.btNomadicSet_Click);
            // 
            // rbNomadicToOfficePhone
            // 
            this.rbNomadicToOfficePhone.AutoSize = true;
            this.rbNomadicToOfficePhone.Checked = true;
            this.rbNomadicToOfficePhone.Location = new System.Drawing.Point(6, 19);
            this.rbNomadicToOfficePhone.Name = "rbNomadicToOfficePhone";
            this.rbNomadicToOfficePhone.Size = new System.Drawing.Size(138, 17);
            this.rbNomadicToOfficePhone.TabIndex = 4;
            this.rbNomadicToOfficePhone.TabStop = true;
            this.rbNomadicToOfficePhone.Text = "Enable on Office Phone";
            this.rbNomadicToOfficePhone.UseVisualStyleBackColor = true;
            // 
            // tbNomadicPhoneNumber
            // 
            this.tbNomadicPhoneNumber.Location = new System.Drawing.Point(159, 64);
            this.tbNomadicPhoneNumber.Name = "tbNomadicPhoneNumber";
            this.tbNomadicPhoneNumber.Size = new System.Drawing.Size(148, 20);
            this.tbNomadicPhoneNumber.TabIndex = 3;
            // 
            // rbNomadicToPhoneNumber
            // 
            this.rbNomadicToPhoneNumber.AutoSize = true;
            this.rbNomadicToPhoneNumber.Location = new System.Drawing.Point(6, 65);
            this.rbNomadicToPhoneNumber.Name = "rbNomadicToPhoneNumber";
            this.rbNomadicToPhoneNumber.Size = new System.Drawing.Size(150, 17);
            this.rbNomadicToPhoneNumber.TabIndex = 2;
            this.rbNomadicToPhoneNumber.TabStop = true;
            this.rbNomadicToPhoneNumber.Text = "Enable on Phone Number:";
            this.rbNomadicToPhoneNumber.UseVisualStyleBackColor = true;
            // 
            // rbNomadicToComputer
            // 
            this.rbNomadicToComputer.AutoSize = true;
            this.rbNomadicToComputer.Location = new System.Drawing.Point(6, 42);
            this.rbNomadicToComputer.Name = "rbNomadicToComputer";
            this.rbNomadicToComputer.Size = new System.Drawing.Size(121, 17);
            this.rbNomadicToComputer.TabIndex = 1;
            this.rbNomadicToComputer.TabStop = true;
            this.rbNomadicToComputer.Text = "Enable on Computer";
            this.rbNomadicToComputer.UseVisualStyleBackColor = true;
            // 
            // gbVoiceMail
            // 
            this.gbVoiceMail.Controls.Add(this.lblVMInfo);
            this.gbVoiceMail.Controls.Add(this.label4);
            this.gbVoiceMail.Controls.Add(this.tbNbVoiceMessages);
            this.gbVoiceMail.Controls.Add(this.label8);
            this.gbVoiceMail.Controls.Add(this.tbVoiceMessagePhoneNumber);
            this.gbVoiceMail.Controls.Add(this.label5);
            this.gbVoiceMail.Controls.Add(this.cbVoiceMessages);
            this.gbVoiceMail.Controls.Add(this.btVMDownload);
            this.gbVoiceMail.Controls.Add(this.btVMDelete);
            this.gbVoiceMail.Enabled = false;
            this.gbVoiceMail.Location = new System.Drawing.Point(16, 99);
            this.gbVoiceMail.Name = "gbVoiceMail";
            this.gbVoiceMail.Size = new System.Drawing.Size(317, 126);
            this.gbVoiceMail.TabIndex = 89;
            this.gbVoiceMail.TabStop = false;
            // 
            // lblVMInfo
            // 
            this.lblVMInfo.AutoSize = true;
            this.lblVMInfo.Location = new System.Drawing.Point(33, 98);
            this.lblVMInfo.Name = "lblVMInfo";
            this.lblVMInfo.Size = new System.Drawing.Size(254, 13);
            this.lblVMInfo.TabIndex = 90;
            this.lblVMInfo.Text = "VM download / delete is available only on OXO PBX";
            this.lblVMInfo.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(386, 99);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 20);
            this.label9.TabIndex = 90;
            this.label9.Text = "Call in progress";
            // 
            // gbFirstCall
            // 
            this.gbFirstCall.Controls.Add(this.tbCall1Participants);
            this.gbFirstCall.Controls.Add(this.cbCall1Dtmf);
            this.gbFirstCall.Controls.Add(this.btn3Call1);
            this.gbFirstCall.Controls.Add(this.btn2Call1);
            this.gbFirstCall.Controls.Add(this.btn1Call1);
            this.gbFirstCall.Controls.Add(this.label15);
            this.gbFirstCall.Controls.Add(this.label14);
            this.gbFirstCall.Controls.Add(this.tbCall1DeviceType);
            this.gbFirstCall.Controls.Add(this.label13);
            this.gbFirstCall.Controls.Add(this.tbCall1RemoteMedia);
            this.gbFirstCall.Controls.Add(this.label12);
            this.gbFirstCall.Controls.Add(this.tbCall1LocalMedia);
            this.gbFirstCall.Controls.Add(this.label11);
            this.gbFirstCall.Controls.Add(this.tbCall1Status);
            this.gbFirstCall.Controls.Add(this.cbCall1IsConference);
            this.gbFirstCall.Controls.Add(this.label10);
            this.gbFirstCall.Controls.Add(this.tbCall1Id);
            this.gbFirstCall.Location = new System.Drawing.Point(390, 122);
            this.gbFirstCall.Name = "gbFirstCall";
            this.gbFirstCall.Size = new System.Drawing.Size(372, 193);
            this.gbFirstCall.TabIndex = 91;
            this.gbFirstCall.TabStop = false;
            this.gbFirstCall.Text = "Call 1";
            // 
            // cbCall1Dtmf
            // 
            this.cbCall1Dtmf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCall1Dtmf.FormattingEnabled = true;
            this.cbCall1Dtmf.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.cbCall1Dtmf.Location = new System.Drawing.Point(265, 136);
            this.cbCall1Dtmf.Name = "cbCall1Dtmf";
            this.cbCall1Dtmf.Size = new System.Drawing.Size(37, 21);
            this.cbCall1Dtmf.TabIndex = 103;
            this.cbCall1Dtmf.Visible = false;
            // 
            // btn3Call1
            // 
            this.btn3Call1.Enabled = false;
            this.btn3Call1.Location = new System.Drawing.Point(148, 164);
            this.btn3Call1.Name = "btn3Call1";
            this.btn3Call1.Size = new System.Drawing.Size(75, 23);
            this.btn3Call1.TabIndex = 92;
            this.btn3Call1.Text = "Release";
            this.btn3Call1.UseVisualStyleBackColor = true;
            this.btn3Call1.Click += new System.EventHandler(this.btn3Call1_Click);
            // 
            // btn2Call1
            // 
            this.btn2Call1.Enabled = false;
            this.btn2Call1.Location = new System.Drawing.Point(184, 135);
            this.btn2Call1.Name = "btn2Call1";
            this.btn2Call1.Size = new System.Drawing.Size(75, 23);
            this.btn2Call1.TabIndex = 102;
            this.btn2Call1.Text = "To VM";
            this.btn2Call1.UseVisualStyleBackColor = true;
            this.btn2Call1.Click += new System.EventHandler(this.btn2Call1_Click);
            // 
            // btn1Call1
            // 
            this.btn1Call1.Enabled = false;
            this.btn1Call1.Location = new System.Drawing.Point(103, 135);
            this.btn1Call1.Name = "btn1Call1";
            this.btn1Call1.Size = new System.Drawing.Size(75, 23);
            this.btn1Call1.TabIndex = 90;
            this.btn1Call1.Text = "Answer";
            this.btn1Call1.UseVisualStyleBackColor = true;
            this.btn1Call1.Click += new System.EventHandler(this.btn1Call1_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(16, 93);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(65, 13);
            this.label15.TabIndex = 101;
            this.label15.Text = "Participants:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 70);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(71, 13);
            this.label14.TabIndex = 99;
            this.label14.Text = "Device Type:";
            // 
            // tbCall1DeviceType
            // 
            this.tbCall1DeviceType.Location = new System.Drawing.Point(87, 67);
            this.tbCall1DeviceType.Name = "tbCall1DeviceType";
            this.tbCall1DeviceType.ReadOnly = true;
            this.tbCall1DeviceType.Size = new System.Drawing.Size(120, 20);
            this.tbCall1DeviceType.TabIndex = 100;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(241, 44);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 13);
            this.label13.TabIndex = 97;
            this.label13.Text = "Remote Media:";
            // 
            // tbCall1RemoteMedia
            // 
            this.tbCall1RemoteMedia.Location = new System.Drawing.Point(326, 41);
            this.tbCall1RemoteMedia.Name = "tbCall1RemoteMedia";
            this.tbCall1RemoteMedia.ReadOnly = true;
            this.tbCall1RemoteMedia.Size = new System.Drawing.Size(41, 20);
            this.tbCall1RemoteMedia.TabIndex = 98;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(252, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(68, 13);
            this.label12.TabIndex = 95;
            this.label12.Text = "Local Media:";
            // 
            // tbCall1LocalMedia
            // 
            this.tbCall1LocalMedia.Location = new System.Drawing.Point(326, 20);
            this.tbCall1LocalMedia.Name = "tbCall1LocalMedia";
            this.tbCall1LocalMedia.ReadOnly = true;
            this.tbCall1LocalMedia.Size = new System.Drawing.Size(40, 20);
            this.tbCall1LocalMedia.TabIndex = 96;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(41, 44);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 93;
            this.label11.Text = "Status:";
            // 
            // tbCall1Status
            // 
            this.tbCall1Status.Location = new System.Drawing.Point(87, 41);
            this.tbCall1Status.Name = "tbCall1Status";
            this.tbCall1Status.ReadOnly = true;
            this.tbCall1Status.Size = new System.Drawing.Size(120, 20);
            this.tbCall1Status.TabIndex = 94;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(60, 20);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 13);
            this.label10.TabIndex = 91;
            this.label10.Text = "ID:";
            // 
            // tbCall1Id
            // 
            this.tbCall1Id.Location = new System.Drawing.Point(87, 17);
            this.tbCall1Id.Name = "tbCall1Id";
            this.tbCall1Id.ReadOnly = true;
            this.tbCall1Id.Size = new System.Drawing.Size(120, 20);
            this.tbCall1Id.TabIndex = 92;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbLogin);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.tbPassword);
            this.groupBox1.Controls.Add(this.label21);
            this.groupBox1.Controls.Add(this.btnLoginLogout);
            this.groupBox1.Location = new System.Drawing.Point(390, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(372, 79);
            this.groupBox1.TabIndex = 92;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbCall2Participants);
            this.groupBox2.Controls.Add(this.cbCall2Dtmf);
            this.groupBox2.Controls.Add(this.btn3Call2);
            this.groupBox2.Controls.Add(this.btn2Call2);
            this.groupBox2.Controls.Add(this.btn1Call2);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.tbCall2DeviceType);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.tbCall2RemoteMedia);
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Controls.Add(this.tbCall2LocalMedia);
            this.groupBox2.Controls.Add(this.label20);
            this.groupBox2.Controls.Add(this.tbCall2Status);
            this.groupBox2.Controls.Add(this.cbCall2IsConference);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Controls.Add(this.tbCall2Id);
            this.groupBox2.Location = new System.Drawing.Point(390, 320);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(372, 189);
            this.groupBox2.TabIndex = 104;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Call 2";
            // 
            // cbCall2Dtmf
            // 
            this.cbCall2Dtmf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCall2Dtmf.FormattingEnabled = true;
            this.cbCall2Dtmf.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.cbCall2Dtmf.Location = new System.Drawing.Point(265, 133);
            this.cbCall2Dtmf.Name = "cbCall2Dtmf";
            this.cbCall2Dtmf.Size = new System.Drawing.Size(37, 21);
            this.cbCall2Dtmf.TabIndex = 103;
            this.cbCall2Dtmf.Visible = false;
            // 
            // btn3Call2
            // 
            this.btn3Call2.Enabled = false;
            this.btn3Call2.Location = new System.Drawing.Point(148, 161);
            this.btn3Call2.Name = "btn3Call2";
            this.btn3Call2.Size = new System.Drawing.Size(75, 23);
            this.btn3Call2.TabIndex = 92;
            this.btn3Call2.Text = "Release";
            this.btn3Call2.UseVisualStyleBackColor = true;
            this.btn3Call2.Click += new System.EventHandler(this.btn3Call2_Click);
            // 
            // btn2Call2
            // 
            this.btn2Call2.Enabled = false;
            this.btn2Call2.Location = new System.Drawing.Point(184, 132);
            this.btn2Call2.Name = "btn2Call2";
            this.btn2Call2.Size = new System.Drawing.Size(75, 23);
            this.btn2Call2.TabIndex = 102;
            this.btn2Call2.Text = "To VM";
            this.btn2Call2.UseVisualStyleBackColor = true;
            this.btn2Call2.Click += new System.EventHandler(this.btn2Call2_Click);
            // 
            // btn1Call2
            // 
            this.btn1Call2.Enabled = false;
            this.btn1Call2.Location = new System.Drawing.Point(103, 132);
            this.btn1Call2.Name = "btn1Call2";
            this.btn1Call2.Size = new System.Drawing.Size(75, 23);
            this.btn1Call2.TabIndex = 90;
            this.btn1Call2.Text = "Answer";
            this.btn1Call2.UseVisualStyleBackColor = true;
            this.btn1Call2.Click += new System.EventHandler(this.btn1Call2_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(16, 93);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(65, 13);
            this.label16.TabIndex = 101;
            this.label16.Text = "Participants:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(10, 70);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(71, 13);
            this.label17.TabIndex = 99;
            this.label17.Text = "Device Type:";
            // 
            // tbCall2DeviceType
            // 
            this.tbCall2DeviceType.Location = new System.Drawing.Point(87, 67);
            this.tbCall2DeviceType.Name = "tbCall2DeviceType";
            this.tbCall2DeviceType.ReadOnly = true;
            this.tbCall2DeviceType.Size = new System.Drawing.Size(120, 20);
            this.tbCall2DeviceType.TabIndex = 100;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(241, 44);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(79, 13);
            this.label18.TabIndex = 97;
            this.label18.Text = "Remote Media:";
            // 
            // tbCall2RemoteMedia
            // 
            this.tbCall2RemoteMedia.Location = new System.Drawing.Point(326, 41);
            this.tbCall2RemoteMedia.Name = "tbCall2RemoteMedia";
            this.tbCall2RemoteMedia.ReadOnly = true;
            this.tbCall2RemoteMedia.Size = new System.Drawing.Size(41, 20);
            this.tbCall2RemoteMedia.TabIndex = 98;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(252, 22);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(68, 13);
            this.label19.TabIndex = 95;
            this.label19.Text = "Local Media:";
            // 
            // tbCall2LocalMedia
            // 
            this.tbCall2LocalMedia.Location = new System.Drawing.Point(326, 20);
            this.tbCall2LocalMedia.Name = "tbCall2LocalMedia";
            this.tbCall2LocalMedia.ReadOnly = true;
            this.tbCall2LocalMedia.Size = new System.Drawing.Size(40, 20);
            this.tbCall2LocalMedia.TabIndex = 96;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(41, 44);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(40, 13);
            this.label20.TabIndex = 93;
            this.label20.Text = "Status:";
            // 
            // tbCall2Status
            // 
            this.tbCall2Status.Location = new System.Drawing.Point(87, 41);
            this.tbCall2Status.Name = "tbCall2Status";
            this.tbCall2Status.ReadOnly = true;
            this.tbCall2Status.Size = new System.Drawing.Size(120, 20);
            this.tbCall2Status.TabIndex = 94;
            // 
            // cbCall2IsConference
            // 
            this.cbCall2IsConference.AutoCheck = false;
            this.cbCall2IsConference.AutoSize = true;
            this.cbCall2IsConference.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbCall2IsConference.Location = new System.Drawing.Point(244, 66);
            this.cbCall2IsConference.Name = "cbCall2IsConference";
            this.cbCall2IsConference.Size = new System.Drawing.Size(95, 17);
            this.cbCall2IsConference.TabIndex = 72;
            this.cbCall2IsConference.Text = "Is Conference:";
            this.cbCall2IsConference.UseVisualStyleBackColor = true;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(60, 20);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(21, 13);
            this.label23.TabIndex = 91;
            this.label23.Text = "ID:";
            // 
            // tbCall2Id
            // 
            this.tbCall2Id.Location = new System.Drawing.Point(87, 17);
            this.tbCall2Id.Name = "tbCall2Id";
            this.tbCall2Id.ReadOnly = true;
            this.tbCall2Id.Size = new System.Drawing.Size(120, 20);
            this.tbCall2Id.TabIndex = 92;
            // 
            // btCallTransfer
            // 
            this.btCallTransfer.Enabled = false;
            this.btCallTransfer.Location = new System.Drawing.Point(493, 515);
            this.btCallTransfer.Name = "btCallTransfer";
            this.btCallTransfer.Size = new System.Drawing.Size(75, 23);
            this.btCallTransfer.TabIndex = 104;
            this.btCallTransfer.Text = "Transfer";
            this.btCallTransfer.UseVisualStyleBackColor = true;
            this.btCallTransfer.Click += new System.EventHandler(this.btCallTransfer_Click);
            // 
            // btCallConference
            // 
            this.btCallConference.Enabled = false;
            this.btCallConference.Location = new System.Drawing.Point(574, 515);
            this.btCallConference.Name = "btCallConference";
            this.btCallConference.Size = new System.Drawing.Size(75, 23);
            this.btCallConference.TabIndex = 105;
            this.btCallConference.Text = "Conference";
            this.btCallConference.UseVisualStyleBackColor = true;
            this.btCallConference.Click += new System.EventHandler(this.btCallConference_Click);
            // 
            // tbCall1Participants
            // 
            this.tbCall1Participants.Location = new System.Drawing.Point(87, 93);
            this.tbCall1Participants.Multiline = true;
            this.tbCall1Participants.Name = "tbCall1Participants";
            this.tbCall1Participants.ReadOnly = true;
            this.tbCall1Participants.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbCall1Participants.Size = new System.Drawing.Size(280, 33);
            this.tbCall1Participants.TabIndex = 106;
            // 
            // tbCall2Participants
            // 
            this.tbCall2Participants.Location = new System.Drawing.Point(87, 93);
            this.tbCall2Participants.Multiline = true;
            this.tbCall2Participants.Name = "tbCall2Participants";
            this.tbCall2Participants.ReadOnly = true;
            this.tbCall2Participants.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbCall2Participants.Size = new System.Drawing.Size(280, 33);
            this.tbCall2Participants.TabIndex = 107;
            // 
            // tbMakeCall
            // 
            this.tbMakeCall.Enabled = false;
            this.tbMakeCall.Location = new System.Drawing.Point(574, 100);
            this.tbMakeCall.Name = "tbMakeCall";
            this.tbMakeCall.Size = new System.Drawing.Size(121, 20);
            this.tbMakeCall.TabIndex = 106;
            // 
            // btMakeCall
            // 
            this.btMakeCall.Enabled = false;
            this.btMakeCall.Location = new System.Drawing.Point(698, 99);
            this.btMakeCall.Name = "btMakeCall";
            this.btMakeCall.Size = new System.Drawing.Size(64, 23);
            this.btMakeCall.TabIndex = 57;
            this.btMakeCall.Text = "Make Call";
            this.btMakeCall.UseVisualStyleBackColor = true;
            this.btMakeCall.Click += new System.EventHandler(this.btMakeCall_Click);
            // 
            // SampleTelephonyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 651);
            this.Controls.Add(this.btMakeCall);
            this.Controls.Add(this.tbMakeCall);
            this.Controls.Add(this.btCallConference);
            this.Controls.Add(this.btCallTransfer);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbFirstCall);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.gbVoiceMail);
            this.Controls.Add(this.gbNomadic);
            this.Controls.Add(this.gbCallFwd);
            this.Controls.Add(this.tbState);
            this.Controls.Add(this.cbNomadic);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cbCallFwd);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cbVoiceMail);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbPBXAgent);
            this.Controls.Add(this.cbTelephonyService);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "SampleTelephonyForm";
            this.Text = "Form1";
            this.gbCallFwd.ResumeLayout(false);
            this.gbCallFwd.PerformLayout();
            this.gbNomadic.ResumeLayout(false);
            this.gbNomadic.PerformLayout();
            this.gbVoiceMail.ResumeLayout(false);
            this.gbVoiceMail.PerformLayout();
            this.gbFirstCall.ResumeLayout(false);
            this.gbFirstCall.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoginLogout;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox tbLogin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbTelephonyService;
        private System.Windows.Forms.TextBox tbPBXAgent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbVoiceMail;
        private System.Windows.Forms.TextBox tbNbVoiceMessages;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbVoiceMessages;
        private System.Windows.Forms.Button btVMDownload;
        private System.Windows.Forms.Button btVMDelete;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbCallFwd;
        private System.Windows.Forms.CheckBox cbCall1IsConference;
        private System.Windows.Forms.CheckBox cbNomadic;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbState;
        private System.Windows.Forms.TextBox tbVoiceMessagePhoneNumber;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox gbCallFwd;
        private System.Windows.Forms.TextBox tbCallFwdPhoneNumber;
        private System.Windows.Forms.RadioButton rbCallFwdToPhoneNumber;
        private System.Windows.Forms.RadioButton rbCallFwdToVm;
        private System.Windows.Forms.RadioButton rbCallFwdDisable;
        private System.Windows.Forms.GroupBox gbNomadic;
        private System.Windows.Forms.TextBox tbNomadicPhoneNumber;
        private System.Windows.Forms.RadioButton rbNomadicToPhoneNumber;
        private System.Windows.Forms.RadioButton rbNomadicToComputer;
        private System.Windows.Forms.RadioButton rbNomadicToOfficePhone;
        private System.Windows.Forms.Button btCallFwdSet;
        private System.Windows.Forms.Button btNomadicSet;
        private System.Windows.Forms.GroupBox gbVoiceMail;
        private System.Windows.Forms.Label lblVMInfo;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox gbFirstCall;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox tbCall1DeviceType;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbCall1RemoteMedia;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbCall1LocalMedia;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbCall1Status;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbCall1Id;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btn3Call1;
        private System.Windows.Forms.Button btn2Call1;
        private System.Windows.Forms.Button btn1Call1;
        private System.Windows.Forms.ComboBox cbCall1Dtmf;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cbCall2Dtmf;
        private System.Windows.Forms.Button btn3Call2;
        private System.Windows.Forms.Button btn2Call2;
        private System.Windows.Forms.Button btn1Call2;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbCall2DeviceType;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox tbCall2RemoteMedia;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox tbCall2LocalMedia;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox tbCall2Status;
        private System.Windows.Forms.CheckBox cbCall2IsConference;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox tbCall2Id;
        private System.Windows.Forms.Button btCallTransfer;
        private System.Windows.Forms.Button btCallConference;
        private System.Windows.Forms.TextBox tbCall1Participants;
        private System.Windows.Forms.TextBox tbCall2Participants;
        private System.Windows.Forms.TextBox tbMakeCall;
        private System.Windows.Forms.Button btMakeCall;
    }
}

