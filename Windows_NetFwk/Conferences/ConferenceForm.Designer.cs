namespace SampleConferences
{
    partial class ConferenceForm
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
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConferenceLock = new System.Windows.Forms.Button();
            this.btnConferenceMute = new System.Windows.Forms.Button();
            this.btnConferenceStart = new System.Windows.Forms.Button();
            this.btnConferenceStop = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.cbBubbles = new System.Windows.Forms.ComboBox();
            this.btnConferenceParticipantDrop = new System.Windows.Forms.Button();
            this.btnConferenceParticipantMute = new System.Windows.Forms.Button();
            this.lbParticipants = new System.Windows.Forms.ListBox();
            this.lbPublishers = new System.Windows.Forms.ListBox();
            this.lbTalkers = new System.Windows.Forms.ListBox();
            this.cbConferenceFeatureAuthorized = new System.Windows.Forms.CheckBox();
            this.cbConferenceV2Used = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbConferenceInProgress = new System.Windows.Forms.ComboBox();
            this.lblConferenceRecordingStatus = new System.Windows.Forms.Label();
            this.btnConferenceRecordingStart = new System.Windows.Forms.Button();
            this.btnConferenceRecordingPause = new System.Windows.Forms.Button();
            this.rbUsePersonalMeeting = new System.Windows.Forms.RadioButton();
            this.rbUseWebRtc = new System.Windows.Forms.RadioButton();
            this.btnGetMeetingUrl = new System.Windows.Forms.Button();
            this.btnResetMeetingUrl = new System.Windows.Forms.Button();
            this.cbPlaySound = new System.Windows.Forms.CheckBox();
            this.cbMuteParticipant = new System.Windows.Forms.CheckBox();
            this.btSetConferenceParticipantParameters = new System.Windows.Forms.Button();
            this.btnPersonalConferenceGetPhonesList = new System.Windows.Forms.Button();
            this.btnPersonalConferenceGetPassCodes = new System.Windows.Forms.Button();
            this.btnPersonalConferenceResetPassCodes = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cbConferenceJoinMuted = new System.Windows.Forms.CheckBox();
            this.label26 = new System.Windows.Forms.Label();
            this.cbConferenceJoinAsModerator = new System.Windows.Forms.CheckBox();
            this.btnConferenceJoin = new System.Windows.Forms.Button();
            this.tbConferenceJoinPhoneNumber = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbPersonalConferenceFeatureAuthorized = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbInformation = new System.Windows.Forms.TextBox();
            this.btnConferencesGet = new System.Windows.Forms.Button();
            this.btnConferenceGetFullSnapshot = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnConferenceAddPstn = new System.Windows.Forms.Button();
            this.tbConferencePstn = new System.Windows.Forms.TextBox();
            this.btnConferenceParticipantDelegate = new System.Windows.Forms.Button();
            this.btnConferenceReject = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(610, 9);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 36;
            this.label9.Text = "Participants";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(285, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 38;
            this.label1.Text = "Publishers";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(285, 201);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Talkers";
            // 
            // btnConferenceLock
            // 
            this.btnConferenceLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceLock.Location = new System.Drawing.Point(201, 436);
            this.btnConferenceLock.Name = "btnConferenceLock";
            this.btnConferenceLock.Size = new System.Drawing.Size(81, 23);
            this.btnConferenceLock.TabIndex = 133;
            this.btnConferenceLock.Text = "Lock/Unlock";
            this.btnConferenceLock.UseVisualStyleBackColor = true;
            this.btnConferenceLock.Click += new System.EventHandler(this.btnConferenceLock_Click);
            // 
            // btnConferenceMute
            // 
            this.btnConferenceMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceMute.Location = new System.Drawing.Point(109, 436);
            this.btnConferenceMute.Name = "btnConferenceMute";
            this.btnConferenceMute.Size = new System.Drawing.Size(81, 23);
            this.btnConferenceMute.TabIndex = 131;
            this.btnConferenceMute.Text = "Mute/Unmute";
            this.btnConferenceMute.UseVisualStyleBackColor = true;
            this.btnConferenceMute.Click += new System.EventHandler(this.btnConferenceMute_Click);
            // 
            // btnConferenceStart
            // 
            this.btnConferenceStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceStart.Location = new System.Drawing.Point(10, 207);
            this.btnConferenceStart.Name = "btnConferenceStart";
            this.btnConferenceStart.Size = new System.Drawing.Size(272, 23);
            this.btnConferenceStart.TabIndex = 135;
            this.btnConferenceStart.Text = "Start Conference";
            this.btnConferenceStart.UseVisualStyleBackColor = true;
            this.btnConferenceStart.Click += new System.EventHandler(this.btnConferenceStart_Click);
            // 
            // btnConferenceStop
            // 
            this.btnConferenceStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceStop.Location = new System.Drawing.Point(10, 436);
            this.btnConferenceStop.Name = "btnConferenceStop";
            this.btnConferenceStop.Size = new System.Drawing.Size(81, 23);
            this.btnConferenceStop.TabIndex = 136;
            this.btnConferenceStop.Text = "Stop";
            this.btnConferenceStop.UseVisualStyleBackColor = true;
            this.btnConferenceStop.Click += new System.EventHandler(this.btnConferenceStop_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(16, 183);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(109, 13);
            this.label16.TabIndex = 138;
            this.label16.Text = "Bubbles as moderator";
            // 
            // cbBubbles
            // 
            this.cbBubbles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBubbles.FormattingEnabled = true;
            this.cbBubbles.Location = new System.Drawing.Point(126, 180);
            this.cbBubbles.Name = "cbBubbles";
            this.cbBubbles.Size = new System.Drawing.Size(156, 21);
            this.cbBubbles.TabIndex = 137;
            this.cbBubbles.SelectedIndexChanged += new System.EventHandler(this.cbBubbles_SelectedIndexChanged);
            // 
            // btnConferenceParticipantDrop
            // 
            this.btnConferenceParticipantDrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceParticipantDrop.Location = new System.Drawing.Point(841, 368);
            this.btnConferenceParticipantDrop.Name = "btnConferenceParticipantDrop";
            this.btnConferenceParticipantDrop.Size = new System.Drawing.Size(91, 23);
            this.btnConferenceParticipantDrop.TabIndex = 1232;
            this.btnConferenceParticipantDrop.Text = "Drop";
            this.btnConferenceParticipantDrop.UseVisualStyleBackColor = true;
            this.btnConferenceParticipantDrop.Click += new System.EventHandler(this.btnConferenceParticipantDrop_Click);
            // 
            // btnConferenceParticipantMute
            // 
            this.btnConferenceParticipantMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceParticipantMute.Location = new System.Drawing.Point(613, 368);
            this.btnConferenceParticipantMute.Name = "btnConferenceParticipantMute";
            this.btnConferenceParticipantMute.Size = new System.Drawing.Size(91, 23);
            this.btnConferenceParticipantMute.TabIndex = 1231;
            this.btnConferenceParticipantMute.Text = "Mute / Unmute";
            this.btnConferenceParticipantMute.UseVisualStyleBackColor = true;
            this.btnConferenceParticipantMute.Click += new System.EventHandler(this.btnConferenceParticipantMute_Click);
            // 
            // lbParticipants
            // 
            this.lbParticipants.FormattingEnabled = true;
            this.lbParticipants.Location = new System.Drawing.Point(613, 25);
            this.lbParticipants.Name = "lbParticipants";
            this.lbParticipants.Size = new System.Drawing.Size(319, 342);
            this.lbParticipants.TabIndex = 1234;
            // 
            // lbPublishers
            // 
            this.lbPublishers.FormattingEnabled = true;
            this.lbPublishers.Location = new System.Drawing.Point(288, 25);
            this.lbPublishers.Name = "lbPublishers";
            this.lbPublishers.Size = new System.Drawing.Size(319, 173);
            this.lbPublishers.TabIndex = 1235;
            // 
            // lbTalkers
            // 
            this.lbTalkers.FormattingEnabled = true;
            this.lbTalkers.Location = new System.Drawing.Point(288, 217);
            this.lbTalkers.Name = "lbTalkers";
            this.lbTalkers.Size = new System.Drawing.Size(319, 173);
            this.lbTalkers.TabIndex = 1236;
            // 
            // cbConferenceFeatureAuthorized
            // 
            this.cbConferenceFeatureAuthorized.AutoCheck = false;
            this.cbConferenceFeatureAuthorized.AutoSize = true;
            this.cbConferenceFeatureAuthorized.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbConferenceFeatureAuthorized.Location = new System.Drawing.Point(14, 25);
            this.cbConferenceFeatureAuthorized.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbConferenceFeatureAuthorized.Name = "cbConferenceFeatureAuthorized";
            this.cbConferenceFeatureAuthorized.Size = new System.Drawing.Size(211, 17);
            this.cbConferenceFeatureAuthorized.TabIndex = 1238;
            this.cbConferenceFeatureAuthorized.Text = "Conference feature is authorized";
            this.cbConferenceFeatureAuthorized.UseVisualStyleBackColor = true;
            // 
            // cbConferenceV2Used
            // 
            this.cbConferenceV2Used.AutoCheck = false;
            this.cbConferenceV2Used.AutoSize = true;
            this.cbConferenceV2Used.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbConferenceV2Used.Location = new System.Drawing.Point(14, 71);
            this.cbConferenceV2Used.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbConferenceV2Used.Name = "cbConferenceV2Used";
            this.cbConferenceV2Used.Size = new System.Drawing.Size(154, 17);
            this.cbConferenceV2Used.TabIndex = 1240;
            this.cbConferenceV2Used.Text = "Conference V2 is used";
            this.cbConferenceV2Used.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 408);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 1242;
            this.label3.Text = "Conference in progress";
            // 
            // cbConferenceInProgress
            // 
            this.cbConferenceInProgress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbConferenceInProgress.FormattingEnabled = true;
            this.cbConferenceInProgress.Items.AddRange(new object[] {
            "Online",
            "Xa",
            "Away",
            "Busy (audio)",
            "Busy (video)",
            "Busy (sharing)",
            "Dnd",
            "Dnd (presentation)"});
            this.cbConferenceInProgress.Location = new System.Drawing.Point(126, 405);
            this.cbConferenceInProgress.Name = "cbConferenceInProgress";
            this.cbConferenceInProgress.Size = new System.Drawing.Size(806, 21);
            this.cbConferenceInProgress.TabIndex = 1241;
            this.cbConferenceInProgress.SelectedIndexChanged += new System.EventHandler(this.cbConferenceInProgress_SelectedIndexChanged);
            // 
            // lblConferenceRecordingStatus
            // 
            this.lblConferenceRecordingStatus.AutoSize = true;
            this.lblConferenceRecordingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConferenceRecordingStatus.Location = new System.Drawing.Point(9, 504);
            this.lblConferenceRecordingStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblConferenceRecordingStatus.Name = "lblConferenceRecordingStatus";
            this.lblConferenceRecordingStatus.Size = new System.Drawing.Size(56, 13);
            this.lblConferenceRecordingStatus.TabIndex = 1243;
            this.lblConferenceRecordingStatus.Text = "Recording";
            // 
            // btnConferenceRecordingStart
            // 
            this.btnConferenceRecordingStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceRecordingStart.Location = new System.Drawing.Point(10, 520);
            this.btnConferenceRecordingStart.Name = "btnConferenceRecordingStart";
            this.btnConferenceRecordingStart.Size = new System.Drawing.Size(124, 23);
            this.btnConferenceRecordingStart.TabIndex = 1246;
            this.btnConferenceRecordingStart.Text = "Start/Stop";
            this.btnConferenceRecordingStart.UseVisualStyleBackColor = true;
            this.btnConferenceRecordingStart.Click += new System.EventHandler(this.btnConferenceRecordingStart_Click);
            // 
            // btnConferenceRecordingPause
            // 
            this.btnConferenceRecordingPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceRecordingPause.Location = new System.Drawing.Point(158, 520);
            this.btnConferenceRecordingPause.Name = "btnConferenceRecordingPause";
            this.btnConferenceRecordingPause.Size = new System.Drawing.Size(124, 23);
            this.btnConferenceRecordingPause.TabIndex = 1244;
            this.btnConferenceRecordingPause.Text = "Pause/Resume";
            this.btnConferenceRecordingPause.UseVisualStyleBackColor = true;
            this.btnConferenceRecordingPause.Click += new System.EventHandler(this.btnConferenceRecordingPause_Click);
            // 
            // rbUsePersonalMeeting
            // 
            this.rbUsePersonalMeeting.AutoSize = true;
            this.rbUsePersonalMeeting.Location = new System.Drawing.Point(12, 100);
            this.rbUsePersonalMeeting.Name = "rbUsePersonalMeeting";
            this.rbUsePersonalMeeting.Size = new System.Drawing.Size(157, 17);
            this.rbUsePersonalMeeting.TabIndex = 1247;
            this.rbUsePersonalMeeting.TabStop = true;
            this.rbUsePersonalMeeting.Text = "User Personal Conf. (PSTN)";
            this.rbUsePersonalMeeting.UseVisualStyleBackColor = true;
            this.rbUsePersonalMeeting.CheckedChanged += new System.EventHandler(this.rbUsePersonalMeeting_CheckedChanged);
            // 
            // rbUseWebRtc
            // 
            this.rbUseWebRtc.AutoSize = true;
            this.rbUseWebRtc.Location = new System.Drawing.Point(12, 157);
            this.rbUseWebRtc.Name = "rbUseWebRtc";
            this.rbUseWebRtc.Size = new System.Drawing.Size(123, 17);
            this.rbUseWebRtc.TabIndex = 1248;
            this.rbUseWebRtc.TabStop = true;
            this.rbUseWebRtc.Text = "User WebRTC Conf.";
            this.rbUseWebRtc.UseVisualStyleBackColor = true;
            this.rbUseWebRtc.CheckedChanged += new System.EventHandler(this.rbUseWebRtc_CheckedChanged);
            // 
            // btnGetMeetingUrl
            // 
            this.btnGetMeetingUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGetMeetingUrl.Location = new System.Drawing.Point(10, 285);
            this.btnGetMeetingUrl.Name = "btnGetMeetingUrl";
            this.btnGetMeetingUrl.Size = new System.Drawing.Size(135, 23);
            this.btnGetMeetingUrl.TabIndex = 1249;
            this.btnGetMeetingUrl.Text = "Get Conf. URL";
            this.btnGetMeetingUrl.UseVisualStyleBackColor = true;
            this.btnGetMeetingUrl.Click += new System.EventHandler(this.btnGetMeetingUrl_Click);
            // 
            // btnResetMeetingUrl
            // 
            this.btnResetMeetingUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetMeetingUrl.Location = new System.Drawing.Point(147, 285);
            this.btnResetMeetingUrl.Name = "btnResetMeetingUrl";
            this.btnResetMeetingUrl.Size = new System.Drawing.Size(135, 23);
            this.btnResetMeetingUrl.TabIndex = 1250;
            this.btnResetMeetingUrl.Text = "Reset Conf. URL";
            this.btnResetMeetingUrl.UseVisualStyleBackColor = true;
            this.btnResetMeetingUrl.Click += new System.EventHandler(this.btnResetMeetingUrl_Click);
            // 
            // cbPlaySound
            // 
            this.cbPlaySound.AutoSize = true;
            this.cbPlaySound.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbPlaySound.Location = new System.Drawing.Point(12, 239);
            this.cbPlaySound.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbPlaySound.Name = "cbPlaySound";
            this.cbPlaySound.Size = new System.Drawing.Size(201, 17);
            this.cbPlaySound.TabIndex = 1251;
            this.cbPlaySound.Text = "Play a sound when a participant joins";
            this.cbPlaySound.UseVisualStyleBackColor = true;
            // 
            // cbMuteParticipant
            // 
            this.cbMuteParticipant.AutoSize = true;
            this.cbMuteParticipant.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMuteParticipant.Location = new System.Drawing.Point(12, 262);
            this.cbMuteParticipant.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbMuteParticipant.Name = "cbMuteParticipant";
            this.cbMuteParticipant.Size = new System.Drawing.Size(178, 17);
            this.cbMuteParticipant.TabIndex = 1252;
            this.cbMuteParticipant.Text = "Mute participants when they join";
            this.cbMuteParticipant.UseVisualStyleBackColor = true;
            // 
            // btSetConferenceParticipantParameters
            // 
            this.btSetConferenceParticipantParameters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSetConferenceParticipantParameters.Location = new System.Drawing.Point(236, 247);
            this.btSetConferenceParticipantParameters.Name = "btSetConferenceParticipantParameters";
            this.btSetConferenceParticipantParameters.Size = new System.Drawing.Size(46, 23);
            this.btSetConferenceParticipantParameters.TabIndex = 1253;
            this.btSetConferenceParticipantParameters.Text = "Set";
            this.btSetConferenceParticipantParameters.UseVisualStyleBackColor = true;
            this.btSetConferenceParticipantParameters.Click += new System.EventHandler(this.btSetConferenceParticipantParameters_Click);
            // 
            // btnPersonalConferenceGetPhonesList
            // 
            this.btnPersonalConferenceGetPhonesList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPersonalConferenceGetPhonesList.Location = new System.Drawing.Point(185, 97);
            this.btnPersonalConferenceGetPhonesList.Name = "btnPersonalConferenceGetPhonesList";
            this.btnPersonalConferenceGetPhonesList.Size = new System.Drawing.Size(97, 23);
            this.btnPersonalConferenceGetPhonesList.TabIndex = 1254;
            this.btnPersonalConferenceGetPhonesList.Text = "Get Phones List";
            this.btnPersonalConferenceGetPhonesList.UseVisualStyleBackColor = true;
            this.btnPersonalConferenceGetPhonesList.Click += new System.EventHandler(this.btnPersonalConferenceGetPhonesList_Click);
            // 
            // btnPersonalConferenceGetPassCodes
            // 
            this.btnPersonalConferenceGetPassCodes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPersonalConferenceGetPassCodes.Location = new System.Drawing.Point(120, 123);
            this.btnPersonalConferenceGetPassCodes.Name = "btnPersonalConferenceGetPassCodes";
            this.btnPersonalConferenceGetPassCodes.Size = new System.Drawing.Size(78, 23);
            this.btnPersonalConferenceGetPassCodes.TabIndex = 1255;
            this.btnPersonalConferenceGetPassCodes.Text = "Get";
            this.btnPersonalConferenceGetPassCodes.UseVisualStyleBackColor = true;
            this.btnPersonalConferenceGetPassCodes.Click += new System.EventHandler(this.btnPersonalConferenceGetPassCodes_Click);
            // 
            // btnPersonalConferenceResetPassCodes
            // 
            this.btnPersonalConferenceResetPassCodes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPersonalConferenceResetPassCodes.Location = new System.Drawing.Point(204, 123);
            this.btnPersonalConferenceResetPassCodes.Name = "btnPersonalConferenceResetPassCodes";
            this.btnPersonalConferenceResetPassCodes.Size = new System.Drawing.Size(78, 23);
            this.btnPersonalConferenceResetPassCodes.TabIndex = 1256;
            this.btnPersonalConferenceResetPassCodes.Text = "Reset";
            this.btnPersonalConferenceResetPassCodes.UseVisualStyleBackColor = true;
            this.btnPersonalConferenceResetPassCodes.Click += new System.EventHandler(this.btnPersonalConferenceResetPassCodes_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(48, 128);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 1257;
            this.label5.Text = "PassCodes";
            // 
            // cbConferenceJoinMuted
            // 
            this.cbConferenceJoinMuted.AutoSize = true;
            this.cbConferenceJoinMuted.Location = new System.Drawing.Point(133, 636);
            this.cbConferenceJoinMuted.Name = "cbConferenceJoinMuted";
            this.cbConferenceJoinMuted.Size = new System.Drawing.Size(55, 17);
            this.cbConferenceJoinMuted.TabIndex = 1262;
            this.cbConferenceJoinMuted.Text = "muted";
            this.cbConferenceJoinMuted.UseVisualStyleBackColor = true;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(22, 615);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(79, 13);
            this.label26.TabIndex = 1258;
            this.label26.Text = "Phone number:";
            // 
            // cbConferenceJoinAsModerator
            // 
            this.cbConferenceJoinAsModerator.AutoSize = true;
            this.cbConferenceJoinAsModerator.Location = new System.Drawing.Point(192, 615);
            this.cbConferenceJoinAsModerator.Name = "cbConferenceJoinAsModerator";
            this.cbConferenceJoinAsModerator.Size = new System.Drawing.Size(88, 17);
            this.cbConferenceJoinAsModerator.TabIndex = 1259;
            this.cbConferenceJoinAsModerator.Text = "as Moderator";
            this.cbConferenceJoinAsModerator.UseVisualStyleBackColor = true;
            // 
            // btnConferenceJoin
            // 
            this.btnConferenceJoin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceJoin.Location = new System.Drawing.Point(223, 632);
            this.btnConferenceJoin.Name = "btnConferenceJoin";
            this.btnConferenceJoin.Size = new System.Drawing.Size(57, 23);
            this.btnConferenceJoin.TabIndex = 1261;
            this.btnConferenceJoin.Text = "Join";
            this.btnConferenceJoin.UseVisualStyleBackColor = true;
            this.btnConferenceJoin.Click += new System.EventHandler(this.btnConferenceJoin_Click);
            // 
            // tbConferenceJoinPhoneNumber
            // 
            this.tbConferenceJoinPhoneNumber.Location = new System.Drawing.Point(103, 612);
            this.tbConferenceJoinPhoneNumber.Name = "tbConferenceJoinPhoneNumber";
            this.tbConferenceJoinPhoneNumber.Size = new System.Drawing.Size(83, 20);
            this.tbConferenceJoinPhoneNumber.TabIndex = 1260;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 596);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 1263;
            this.label6.Text = "Join conference:";
            // 
            // cbPersonalConferenceFeatureAuthorized
            // 
            this.cbPersonalConferenceFeatureAuthorized.AutoCheck = false;
            this.cbPersonalConferenceFeatureAuthorized.AutoSize = true;
            this.cbPersonalConferenceFeatureAuthorized.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbPersonalConferenceFeatureAuthorized.Location = new System.Drawing.Point(14, 48);
            this.cbPersonalConferenceFeatureAuthorized.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbPersonalConferenceFeatureAuthorized.Name = "cbPersonalConferenceFeatureAuthorized";
            this.cbPersonalConferenceFeatureAuthorized.Size = new System.Drawing.Size(264, 17);
            this.cbPersonalConferenceFeatureAuthorized.TabIndex = 1265;
            this.cbPersonalConferenceFeatureAuthorized.Text = "Personal Conference feature is authorized";
            this.cbPersonalConferenceFeatureAuthorized.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(287, 429);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 1267;
            this.label7.Text = "Information";
            // 
            // tbInformation
            // 
            this.tbInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbInformation.Location = new System.Drawing.Point(288, 445);
            this.tbInformation.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tbInformation.Multiline = true;
            this.tbInformation.Name = "tbInformation";
            this.tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInformation.Size = new System.Drawing.Size(644, 246);
            this.tbInformation.TabIndex = 1266;
            // 
            // btnConferencesGet
            // 
            this.btnConferencesGet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferencesGet.Location = new System.Drawing.Point(10, 344);
            this.btnConferencesGet.Name = "btnConferencesGet";
            this.btnConferencesGet.Size = new System.Drawing.Size(272, 23);
            this.btnConferencesGet.TabIndex = 1268;
            this.btnConferencesGet.Text = "Get Conferences in progress";
            this.btnConferencesGet.UseVisualStyleBackColor = true;
            this.btnConferencesGet.Click += new System.EventHandler(this.btnConferencesGet_Click);
            // 
            // btnConferenceGetFullSnapshot
            // 
            this.btnConferenceGetFullSnapshot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceGetFullSnapshot.Location = new System.Drawing.Point(133, 465);
            this.btnConferenceGetFullSnapshot.Name = "btnConferenceGetFullSnapshot";
            this.btnConferenceGetFullSnapshot.Size = new System.Drawing.Size(135, 23);
            this.btnConferenceGetFullSnapshot.TabIndex = 1269;
            this.btnConferenceGetFullSnapshot.Text = "Get Full Snapshot";
            this.btnConferenceGetFullSnapshot.UseVisualStyleBackColor = true;
            this.btnConferenceGetFullSnapshot.Click += new System.EventHandler(this.btnConferenceGetFullSnapshot_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 562);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 1270;
            this.label4.Text = "Phone number:";
            // 
            // btnConferenceAddPstn
            // 
            this.btnConferenceAddPstn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceAddPstn.Location = new System.Drawing.Point(223, 557);
            this.btnConferenceAddPstn.Name = "btnConferenceAddPstn";
            this.btnConferenceAddPstn.Size = new System.Drawing.Size(57, 23);
            this.btnConferenceAddPstn.TabIndex = 1272;
            this.btnConferenceAddPstn.Text = "Add";
            this.btnConferenceAddPstn.UseVisualStyleBackColor = true;
            this.btnConferenceAddPstn.Click += new System.EventHandler(this.btnConferenceAddPstn_Click);
            // 
            // tbConferencePstn
            // 
            this.tbConferencePstn.Location = new System.Drawing.Point(103, 559);
            this.tbConferencePstn.Name = "tbConferencePstn";
            this.tbConferencePstn.Size = new System.Drawing.Size(114, 20);
            this.tbConferencePstn.TabIndex = 1271;
            // 
            // btnConferenceParticipantDelegate
            // 
            this.btnConferenceParticipantDelegate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceParticipantDelegate.Location = new System.Drawing.Point(727, 368);
            this.btnConferenceParticipantDelegate.Name = "btnConferenceParticipantDelegate";
            this.btnConferenceParticipantDelegate.Size = new System.Drawing.Size(91, 23);
            this.btnConferenceParticipantDelegate.TabIndex = 1273;
            this.btnConferenceParticipantDelegate.Text = "Delegate";
            this.btnConferenceParticipantDelegate.UseVisualStyleBackColor = true;
            this.btnConferenceParticipantDelegate.Click += new System.EventHandler(this.btnConferenceParticipantDelegate_Click);
            // 
            // btnConferenceReject
            // 
            this.btnConferenceReject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConferenceReject.Location = new System.Drawing.Point(10, 465);
            this.btnConferenceReject.Name = "btnConferenceReject";
            this.btnConferenceReject.Size = new System.Drawing.Size(81, 23);
            this.btnConferenceReject.TabIndex = 1274;
            this.btnConferenceReject.Text = "Reject";
            this.btnConferenceReject.UseVisualStyleBackColor = true;
            this.btnConferenceReject.Click += new System.EventHandler(this.btnConferenceReject_Click);
            // 
            // ConferenceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(944, 701);
            this.Controls.Add(this.btnConferenceReject);
            this.Controls.Add(this.btnConferenceParticipantDelegate);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnConferenceAddPstn);
            this.Controls.Add(this.tbConferencePstn);
            this.Controls.Add(this.btnConferenceGetFullSnapshot);
            this.Controls.Add(this.btnConferencesGet);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.cbPersonalConferenceFeatureAuthorized);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cbConferenceJoinMuted);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.cbConferenceJoinAsModerator);
            this.Controls.Add(this.btnConferenceJoin);
            this.Controls.Add(this.tbConferenceJoinPhoneNumber);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnPersonalConferenceGetPhonesList);
            this.Controls.Add(this.btnPersonalConferenceGetPassCodes);
            this.Controls.Add(this.btnPersonalConferenceResetPassCodes);
            this.Controls.Add(this.btSetConferenceParticipantParameters);
            this.Controls.Add(this.cbMuteParticipant);
            this.Controls.Add(this.cbPlaySound);
            this.Controls.Add(this.btnGetMeetingUrl);
            this.Controls.Add(this.btnResetMeetingUrl);
            this.Controls.Add(this.rbUseWebRtc);
            this.Controls.Add(this.rbUsePersonalMeeting);
            this.Controls.Add(this.btnConferenceRecordingStart);
            this.Controls.Add(this.btnConferenceRecordingPause);
            this.Controls.Add(this.lblConferenceRecordingStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbConferenceInProgress);
            this.Controls.Add(this.cbConferenceV2Used);
            this.Controls.Add(this.cbConferenceFeatureAuthorized);
            this.Controls.Add(this.lbTalkers);
            this.Controls.Add(this.lbPublishers);
            this.Controls.Add(this.lbParticipants);
            this.Controls.Add(this.btnConferenceParticipantDrop);
            this.Controls.Add(this.btnConferenceParticipantMute);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.cbBubbles);
            this.Controls.Add(this.btnConferenceStart);
            this.Controls.Add(this.btnConferenceStop);
            this.Controls.Add(this.btnConferenceLock);
            this.Controls.Add(this.btnConferenceMute);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label9);
            this.Name = "ConferenceForm";
            this.Text = "ConferenceForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConferenceLock;
        private System.Windows.Forms.Button btnConferenceMute;
        private System.Windows.Forms.Button btnConferenceStart;
        private System.Windows.Forms.Button btnConferenceStop;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox cbBubbles;
        private System.Windows.Forms.Button btnConferenceParticipantDrop;
        private System.Windows.Forms.Button btnConferenceParticipantMute;
        private System.Windows.Forms.ListBox lbParticipants;
        private System.Windows.Forms.ListBox lbPublishers;
        private System.Windows.Forms.ListBox lbTalkers;
        private System.Windows.Forms.CheckBox cbConferenceFeatureAuthorized;
        private System.Windows.Forms.CheckBox cbConferenceV2Used;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbConferenceInProgress;
        private System.Windows.Forms.Label lblConferenceRecordingStatus;
        private System.Windows.Forms.Button btnConferenceRecordingStart;
        private System.Windows.Forms.Button btnConferenceRecordingPause;
        private System.Windows.Forms.RadioButton rbUsePersonalMeeting;
        private System.Windows.Forms.RadioButton rbUseWebRtc;
        private System.Windows.Forms.Button btnGetMeetingUrl;
        private System.Windows.Forms.Button btnResetMeetingUrl;
        private System.Windows.Forms.CheckBox cbPlaySound;
        private System.Windows.Forms.CheckBox cbMuteParticipant;
        private System.Windows.Forms.Button btSetConferenceParticipantParameters;
        private System.Windows.Forms.Button btnPersonalConferenceGetPhonesList;
        private System.Windows.Forms.Button btnPersonalConferenceGetPassCodes;
        private System.Windows.Forms.Button btnPersonalConferenceResetPassCodes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cbConferenceJoinMuted;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.CheckBox cbConferenceJoinAsModerator;
        private System.Windows.Forms.Button btnConferenceJoin;
        private System.Windows.Forms.TextBox tbConferenceJoinPhoneNumber;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbPersonalConferenceFeatureAuthorized;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbInformation;
        private System.Windows.Forms.Button btnConferencesGet;
        private System.Windows.Forms.Button btnConferenceGetFullSnapshot;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnConferenceAddPstn;
        private System.Windows.Forms.TextBox tbConferencePstn;
        private System.Windows.Forms.Button btnConferenceParticipantDelegate;
        private System.Windows.Forms.Button btnConferenceReject;
    }
}