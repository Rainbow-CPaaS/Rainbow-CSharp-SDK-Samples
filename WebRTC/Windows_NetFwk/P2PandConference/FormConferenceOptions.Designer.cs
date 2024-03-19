namespace SDK.UIForm.WebRTC
{
    partial class FormConferenceOptions
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
            components = new System.ComponentModel.Container();
            btn_ParticipantDelegate = new System.Windows.Forms.Button();
            lb_ActiveTalkers = new System.Windows.Forms.ListBox();
            lb_PublishersVideo = new System.Windows.Forms.ListBox();
            lb_Participants = new System.Windows.Forms.ListBox();
            btn_ParticipantDrop = new System.Windows.Forms.Button();
            btn_ParticipantMute = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lb_PublishersSharing = new System.Windows.Forms.ListBox();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            lb_Members = new System.Windows.Forms.ListBox();
            label5 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            tbInformation = new System.Windows.Forms.TextBox();
            btnConferenceLock = new System.Windows.Forms.Button();
            btnConferenceMute = new System.Windows.Forms.Button();
            btnConferenceStop = new System.Windows.Forms.Button();
            btnConferenceRecordingStart = new System.Windows.Forms.Button();
            btnConferenceRecordingPause = new System.Windows.Forms.Button();
            lblConferenceRecordingStatus = new System.Windows.Forms.Label();
            lbl_ConferenceInProgress = new System.Windows.Forms.Label();
            lbl_ConferenceDetails = new System.Windows.Forms.Label();
            btn_OutputRemoteVideoInput = new System.Windows.Forms.Button();
            btn_OutputRemoteSharingInput = new System.Windows.Forms.Button();
            btn_SubscribeRemoteVideoInput = new System.Windows.Forms.Button();
            btn_SubscribeRemoteSharingInput = new System.Windows.Forms.Button();
            btn_SubscribeRemoteAudioInput = new System.Windows.Forms.Button();
            btn_SubscribeDynamicFeedInput = new System.Windows.Forms.Button();
            btn_OutputDynamicFeedInput = new System.Windows.Forms.Button();
            btn_SubscribeRemoteDataChannelInput = new System.Windows.Forms.Button();
            lb_PublishersDataChannel = new System.Windows.Forms.ListBox();
            label6 = new System.Windows.Forms.Label();
            btn_DataChannelSend = new System.Windows.Forms.Button();
            label8 = new System.Windows.Forms.Label();
            tbDataChannel = new System.Windows.Forms.TextBox();
            btn_SubscribeMediaServiceInput = new System.Windows.Forms.Button();
            btn_OutputMediaServiceInput = new System.Windows.Forms.Button();
            lb_MediaService = new System.Windows.Forms.ListBox();
            label10 = new System.Windows.Forms.Label();
            btn_CloseRemoteDataChannelInput = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            btn_RaiseHand = new System.Windows.Forms.Button();
            btn_ParticipantLowerHand = new System.Windows.Forms.Button();
            btn_ParticipantLowerAllHands = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btn_ParticipantDelegate
            // 
            btn_ParticipantDelegate.Enabled = false;
            btn_ParticipantDelegate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_ParticipantDelegate.Location = new System.Drawing.Point(317, 474);
            btn_ParticipantDelegate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantDelegate.Name = "btn_ParticipantDelegate";
            btn_ParticipantDelegate.Size = new System.Drawing.Size(30, 27);
            btn_ParticipantDelegate.TabIndex = 1281;
            btn_ParticipantDelegate.Text = "🔑";
            toolTip1.SetToolTip(btn_ParticipantDelegate, "Delegate");
            btn_ParticipantDelegate.UseVisualStyleBackColor = true;
            btn_ParticipantDelegate.Click += btn_ParticipantDelegate_Click;
            // 
            // lb_ActiveTalkers
            // 
            lb_ActiveTalkers.Font = new System.Drawing.Font("Segoe UI", 8F);
            lb_ActiveTalkers.FormattingEnabled = true;
            lb_ActiveTalkers.ItemHeight = 13;
            lb_ActiveTalkers.Location = new System.Drawing.Point(17, 125);
            lb_ActiveTalkers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_ActiveTalkers.Name = "lb_ActiveTalkers";
            lb_ActiveTalkers.ScrollAlwaysVisible = true;
            lb_ActiveTalkers.Size = new System.Drawing.Size(200, 43);
            lb_ActiveTalkers.TabIndex = 1280;
            lb_ActiveTalkers.SelectedIndexChanged += lb_ActiveTalkers_SelectedIndexChanged;
            // 
            // lb_PublishersVideo
            // 
            lb_PublishersVideo.Font = new System.Drawing.Font("Segoe UI", 8F);
            lb_PublishersVideo.FormattingEnabled = true;
            lb_PublishersVideo.ItemHeight = 13;
            lb_PublishersVideo.Location = new System.Drawing.Point(15, 214);
            lb_PublishersVideo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_PublishersVideo.Name = "lb_PublishersVideo";
            lb_PublishersVideo.ScrollAlwaysVisible = true;
            lb_PublishersVideo.Size = new System.Drawing.Size(200, 43);
            lb_PublishersVideo.TabIndex = 1279;
            lb_PublishersVideo.SelectedIndexChanged += lb_PublishersVideo_SelectedIndexChanged;
            // 
            // lb_Participants
            // 
            lb_Participants.Font = new System.Drawing.Font("Segoe UI", 12F);
            lb_Participants.FormattingEnabled = true;
            lb_Participants.ItemHeight = 21;
            lb_Participants.Location = new System.Drawing.Point(233, 237);
            lb_Participants.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_Participants.Name = "lb_Participants";
            lb_Participants.ScrollAlwaysVisible = true;
            lb_Participants.Size = new System.Drawing.Size(246, 235);
            lb_Participants.TabIndex = 1278;
            lb_Participants.SelectedIndexChanged += lb_Participants_SelectedIndexChanged;
            // 
            // btn_ParticipantDrop
            // 
            btn_ParticipantDrop.Enabled = false;
            btn_ParticipantDrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_ParticipantDrop.Location = new System.Drawing.Point(353, 474);
            btn_ParticipantDrop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantDrop.Name = "btn_ParticipantDrop";
            btn_ParticipantDrop.Size = new System.Drawing.Size(30, 27);
            btn_ParticipantDrop.TabIndex = 1277;
            btn_ParticipantDrop.Text = "❌";
            toolTip1.SetToolTip(btn_ParticipantDrop, "Drop");
            btn_ParticipantDrop.UseVisualStyleBackColor = true;
            btn_ParticipantDrop.Click += btn_ParticipantDrop_Click;
            // 
            // btn_ParticipantMute
            // 
            btn_ParticipantMute.Enabled = false;
            btn_ParticipantMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_ParticipantMute.Location = new System.Drawing.Point(246, 474);
            btn_ParticipantMute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantMute.Name = "btn_ParticipantMute";
            btn_ParticipantMute.Size = new System.Drawing.Size(65, 27);
            btn_ParticipantMute.TabIndex = 1276;
            btn_ParticipantMute.Text = "🔇 / 🔈";
            toolTip1.SetToolTip(btn_ParticipantMute, "Unmute / Mute");
            btn_ParticipantMute.UseVisualStyleBackColor = true;
            btn_ParticipantMute.Click += btn_ParticipantMute_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label1.Location = new System.Drawing.Point(12, 198);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(113, 13);
            label1.TabIndex = 1275;
            label1.Text = "Publishers - Video:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label9.Location = new System.Drawing.Point(232, 222);
            label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(78, 13);
            label9.TabIndex = 1274;
            label9.Text = "Participants:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label2.Location = new System.Drawing.Point(14, 109);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(93, 13);
            label2.TabIndex = 1282;
            label2.Text = "Active Talkers:";
            // 
            // lb_PublishersSharing
            // 
            lb_PublishersSharing.Font = new System.Drawing.Font("Segoe UI", 8F);
            lb_PublishersSharing.FormattingEnabled = true;
            lb_PublishersSharing.ItemHeight = 13;
            lb_PublishersSharing.Location = new System.Drawing.Point(15, 409);
            lb_PublishersSharing.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_PublishersSharing.Name = "lb_PublishersSharing";
            lb_PublishersSharing.Size = new System.Drawing.Size(201, 17);
            lb_PublishersSharing.TabIndex = 1284;
            lb_PublishersSharing.SelectedIndexChanged += lb_PublishersSharing_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label3.Location = new System.Drawing.Point(14, 393);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(124, 13);
            label3.TabIndex = 1283;
            label3.Text = "Publishers - Sharing:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label4.Location = new System.Drawing.Point(15, 9);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(76, 13);
            label4.TabIndex = 1287;
            label4.Text = "Conference:";
            // 
            // lb_Members
            // 
            lb_Members.Font = new System.Drawing.Font("Segoe UI", 12F);
            lb_Members.FormattingEnabled = true;
            lb_Members.ItemHeight = 21;
            lb_Members.Location = new System.Drawing.Point(233, 25);
            lb_Members.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_Members.Name = "lb_Members";
            lb_Members.ScrollAlwaysVisible = true;
            lb_Members.Size = new System.Drawing.Size(246, 172);
            lb_Members.TabIndex = 1289;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label5.Location = new System.Drawing.Point(232, 9);
            label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(199, 13);
            label5.TabIndex = 1288;
            label5.Text = "Members (not in  the conference):";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label7.Location = new System.Drawing.Point(485, 7);
            label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(70, 13);
            label7.TabIndex = 1291;
            label7.Text = "Information";
            // 
            // tbInformation
            // 
            tbInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            tbInformation.Location = new System.Drawing.Point(485, 23);
            tbInformation.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            tbInformation.Multiline = true;
            tbInformation.Name = "tbInformation";
            tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbInformation.Size = new System.Drawing.Size(284, 388);
            tbInformation.TabIndex = 1290;
            // 
            // btnConferenceLock
            // 
            btnConferenceLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btnConferenceLock.Location = new System.Drawing.Point(128, 48);
            btnConferenceLock.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConferenceLock.Name = "btnConferenceLock";
            btnConferenceLock.Size = new System.Drawing.Size(86, 20);
            btnConferenceLock.TabIndex = 1293;
            btnConferenceLock.Text = "Lock/Unlock";
            btnConferenceLock.UseVisualStyleBackColor = true;
            btnConferenceLock.Click += btnConferenceLock_Click;
            // 
            // btnConferenceMute
            // 
            btnConferenceMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btnConferenceMute.Location = new System.Drawing.Point(13, 48);
            btnConferenceMute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConferenceMute.Name = "btnConferenceMute";
            btnConferenceMute.Size = new System.Drawing.Size(86, 20);
            btnConferenceMute.TabIndex = 1292;
            btnConferenceMute.Text = "Mute/Unmute";
            btnConferenceMute.UseVisualStyleBackColor = true;
            btnConferenceMute.Click += btnConferenceMute_Click;
            // 
            // btnConferenceStop
            // 
            btnConferenceStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btnConferenceStop.Location = new System.Drawing.Point(174, 7);
            btnConferenceStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConferenceStop.Name = "btnConferenceStop";
            btnConferenceStop.Size = new System.Drawing.Size(39, 20);
            btnConferenceStop.TabIndex = 1294;
            btnConferenceStop.Text = "Stop";
            btnConferenceStop.UseVisualStyleBackColor = true;
            btnConferenceStop.Click += btnConferenceStop_Click;
            // 
            // btnConferenceRecordingStart
            // 
            btnConferenceRecordingStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btnConferenceRecordingStart.Location = new System.Drawing.Point(65, 75);
            btnConferenceRecordingStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConferenceRecordingStart.Name = "btnConferenceRecordingStart";
            btnConferenceRecordingStart.Size = new System.Drawing.Size(70, 20);
            btnConferenceRecordingStart.TabIndex = 1297;
            btnConferenceRecordingStart.Text = "Start/Stop";
            btnConferenceRecordingStart.UseVisualStyleBackColor = true;
            btnConferenceRecordingStart.Click += btnConferenceRecordingStart_Click;
            // 
            // btnConferenceRecordingPause
            // 
            btnConferenceRecordingPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btnConferenceRecordingPause.Location = new System.Drawing.Point(134, 76);
            btnConferenceRecordingPause.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConferenceRecordingPause.Name = "btnConferenceRecordingPause";
            btnConferenceRecordingPause.Size = new System.Drawing.Size(86, 20);
            btnConferenceRecordingPause.TabIndex = 1296;
            btnConferenceRecordingPause.Text = "Pause/Resume";
            btnConferenceRecordingPause.UseVisualStyleBackColor = true;
            btnConferenceRecordingPause.Click += btnConferenceRecordingPause_Click;
            // 
            // lblConferenceRecordingStatus
            // 
            lblConferenceRecordingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            lblConferenceRecordingStatus.Location = new System.Drawing.Point(15, 78);
            lblConferenceRecordingStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblConferenceRecordingStatus.Name = "lblConferenceRecordingStatus";
            lblConferenceRecordingStatus.Size = new System.Drawing.Size(52, 17);
            lblConferenceRecordingStatus.TabIndex = 1295;
            lblConferenceRecordingStatus.Text = "Record:";
            // 
            // lbl_ConferenceInProgress
            // 
            lbl_ConferenceInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            lbl_ConferenceInProgress.ForeColor = System.Drawing.Color.Firebrick;
            lbl_ConferenceInProgress.Location = new System.Drawing.Point(86, 9);
            lbl_ConferenceInProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lbl_ConferenceInProgress.Name = "lbl_ConferenceInProgress";
            lbl_ConferenceInProgress.Size = new System.Drawing.Size(82, 18);
            lbl_ConferenceInProgress.TabIndex = 1298;
            lbl_ConferenceInProgress.Text = "In progress";
            lbl_ConferenceInProgress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            lbl_ConferenceInProgress.Visible = false;
            // 
            // lbl_ConferenceDetails
            // 
            lbl_ConferenceDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            lbl_ConferenceDetails.ForeColor = System.Drawing.Color.Blue;
            lbl_ConferenceDetails.Location = new System.Drawing.Point(15, 26);
            lbl_ConferenceDetails.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lbl_ConferenceDetails.Name = "lbl_ConferenceDetails";
            lbl_ConferenceDetails.Size = new System.Drawing.Size(202, 21);
            lbl_ConferenceDetails.TabIndex = 1299;
            lbl_ConferenceDetails.Text = "details";
            lbl_ConferenceDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_OutputRemoteVideoInput
            // 
            btn_OutputRemoteVideoInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn_OutputRemoteVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_OutputRemoteVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputRemoteVideoInput.Location = new System.Drawing.Point(151, 260);
            btn_OutputRemoteVideoInput.Name = "btn_OutputRemoteVideoInput";
            btn_OutputRemoteVideoInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputRemoteVideoInput.TabIndex = 1303;
            btn_OutputRemoteVideoInput.UseVisualStyleBackColor = true;
            btn_OutputRemoteVideoInput.Click += btn_OutputRemoteVideoInput_Click;
            // 
            // btn_OutputRemoteSharingInput
            // 
            btn_OutputRemoteSharingInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn_OutputRemoteSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_OutputRemoteSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputRemoteSharingInput.Location = new System.Drawing.Point(153, 430);
            btn_OutputRemoteSharingInput.Name = "btn_OutputRemoteSharingInput";
            btn_OutputRemoteSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputRemoteSharingInput.TabIndex = 1304;
            btn_OutputRemoteSharingInput.UseVisualStyleBackColor = true;
            btn_OutputRemoteSharingInput.Click += btn_OutputRemoteSharingInput_Click;
            // 
            // btn_SubscribeRemoteVideoInput
            // 
            btn_SubscribeRemoteVideoInput.Enabled = false;
            btn_SubscribeRemoteVideoInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteVideoInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteVideoInput.Location = new System.Drawing.Point(72, 258);
            btn_SubscribeRemoteVideoInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_SubscribeRemoteVideoInput.Name = "btn_SubscribeRemoteVideoInput";
            btn_SubscribeRemoteVideoInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteVideoInput.TabIndex = 1306;
            btn_SubscribeRemoteVideoInput.Text = "Subscribe";
            btn_SubscribeRemoteVideoInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteVideoInput.Click += btn_SubscribeRemoteVideoInput_Click;
            // 
            // btn_SubscribeRemoteSharingInput
            // 
            btn_SubscribeRemoteSharingInput.Enabled = false;
            btn_SubscribeRemoteSharingInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteSharingInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteSharingInput.Location = new System.Drawing.Point(74, 427);
            btn_SubscribeRemoteSharingInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_SubscribeRemoteSharingInput.Name = "btn_SubscribeRemoteSharingInput";
            btn_SubscribeRemoteSharingInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteSharingInput.TabIndex = 1307;
            btn_SubscribeRemoteSharingInput.Text = "Subscribe";
            btn_SubscribeRemoteSharingInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteSharingInput.Click += btn_SubscribeRemoteSharingInput_Click;
            // 
            // btn_SubscribeRemoteAudioInput
            // 
            btn_SubscribeRemoteAudioInput.Enabled = false;
            btn_SubscribeRemoteAudioInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteAudioInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteAudioInput.Location = new System.Drawing.Point(74, 169);
            btn_SubscribeRemoteAudioInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_SubscribeRemoteAudioInput.Name = "btn_SubscribeRemoteAudioInput";
            btn_SubscribeRemoteAudioInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteAudioInput.TabIndex = 1308;
            btn_SubscribeRemoteAudioInput.Text = "Subscribe";
            btn_SubscribeRemoteAudioInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteAudioInput.Click += btn_SubscribeRemoteAudioInput_Click;
            // 
            // btn_SubscribeDynamicFeedInput
            // 
            btn_SubscribeDynamicFeedInput.Enabled = false;
            btn_SubscribeDynamicFeedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeDynamicFeedInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeDynamicFeedInput.Location = new System.Drawing.Point(41, 277);
            btn_SubscribeDynamicFeedInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_SubscribeDynamicFeedInput.Name = "btn_SubscribeDynamicFeedInput";
            btn_SubscribeDynamicFeedInput.Size = new System.Drawing.Size(140, 22);
            btn_SubscribeDynamicFeedInput.TabIndex = 1309;
            btn_SubscribeDynamicFeedInput.Text = "Subscribe Dynamic Feed";
            btn_SubscribeDynamicFeedInput.UseVisualStyleBackColor = true;
            btn_SubscribeDynamicFeedInput.Click += btn_SubscribeDynamicFeedInput_Click;
            // 
            // btn_OutputDynamicFeedInput
            // 
            btn_OutputDynamicFeedInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn_OutputDynamicFeedInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_OutputDynamicFeedInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputDynamicFeedInput.Location = new System.Drawing.Point(186, 279);
            btn_OutputDynamicFeedInput.Name = "btn_OutputDynamicFeedInput";
            btn_OutputDynamicFeedInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputDynamicFeedInput.TabIndex = 1310;
            btn_OutputDynamicFeedInput.UseVisualStyleBackColor = true;
            btn_OutputDynamicFeedInput.Click += btn_OutputDynamicFeedInput_Click;
            // 
            // btn_SubscribeRemoteDataChannelInput
            // 
            btn_SubscribeRemoteDataChannelInput.Enabled = false;
            btn_SubscribeRemoteDataChannelInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteDataChannelInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteDataChannelInput.Location = new System.Drawing.Point(72, 368);
            btn_SubscribeRemoteDataChannelInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_SubscribeRemoteDataChannelInput.Name = "btn_SubscribeRemoteDataChannelInput";
            btn_SubscribeRemoteDataChannelInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteDataChannelInput.TabIndex = 1314;
            btn_SubscribeRemoteDataChannelInput.Text = "Subscribe";
            btn_SubscribeRemoteDataChannelInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteDataChannelInput.Click += btn_SubscribeRemoteDataChannelInput_Click;
            // 
            // lb_PublishersDataChannel
            // 
            lb_PublishersDataChannel.Font = new System.Drawing.Font("Segoe UI", 8F);
            lb_PublishersDataChannel.FormattingEnabled = true;
            lb_PublishersDataChannel.ItemHeight = 13;
            lb_PublishersDataChannel.Location = new System.Drawing.Point(15, 324);
            lb_PublishersDataChannel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_PublishersDataChannel.Name = "lb_PublishersDataChannel";
            lb_PublishersDataChannel.ScrollAlwaysVisible = true;
            lb_PublishersDataChannel.Size = new System.Drawing.Size(200, 43);
            lb_PublishersDataChannel.TabIndex = 1312;
            lb_PublishersDataChannel.SelectedIndexChanged += lb_PublishersDataChannel_SelectedIndexChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label6.Location = new System.Drawing.Point(12, 308);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(154, 13);
            label6.TabIndex = 1311;
            label6.Text = "Publishers - DataChannel:";
            // 
            // btn_DataChannelSend
            // 
            btn_DataChannelSend.Enabled = false;
            btn_DataChannelSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            btn_DataChannelSend.Location = new System.Drawing.Point(605, 475);
            btn_DataChannelSend.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_DataChannelSend.Name = "btn_DataChannelSend";
            btn_DataChannelSend.Size = new System.Drawing.Size(58, 27);
            btn_DataChannelSend.TabIndex = 1316;
            btn_DataChannelSend.Text = "Send";
            btn_DataChannelSend.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label8.Location = new System.Drawing.Point(485, 414);
            label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(84, 13);
            label8.TabIndex = 1317;
            label8.Text = "DataChannel:";
            // 
            // tbDataChannel
            // 
            tbDataChannel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            tbDataChannel.Location = new System.Drawing.Point(485, 430);
            tbDataChannel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            tbDataChannel.Multiline = true;
            tbDataChannel.Name = "tbDataChannel";
            tbDataChannel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbDataChannel.Size = new System.Drawing.Size(284, 42);
            tbDataChannel.TabIndex = 1318;
            // 
            // btn_SubscribeMediaServiceInput
            // 
            btn_SubscribeMediaServiceInput.Enabled = false;
            btn_SubscribeMediaServiceInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeMediaServiceInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeMediaServiceInput.Location = new System.Drawing.Point(73, 488);
            btn_SubscribeMediaServiceInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_SubscribeMediaServiceInput.Name = "btn_SubscribeMediaServiceInput";
            btn_SubscribeMediaServiceInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeMediaServiceInput.TabIndex = 1322;
            btn_SubscribeMediaServiceInput.Text = "Subscribe";
            btn_SubscribeMediaServiceInput.UseVisualStyleBackColor = true;
            btn_SubscribeMediaServiceInput.Click += btn_SubscribeMediaServiceInput_Click;
            // 
            // btn_OutputMediaServiceInput
            // 
            btn_OutputMediaServiceInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn_OutputMediaServiceInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_OutputMediaServiceInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputMediaServiceInput.Location = new System.Drawing.Point(152, 491);
            btn_OutputMediaServiceInput.Name = "btn_OutputMediaServiceInput";
            btn_OutputMediaServiceInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputMediaServiceInput.TabIndex = 1321;
            btn_OutputMediaServiceInput.UseVisualStyleBackColor = true;
            btn_OutputMediaServiceInput.Click += btn_OutputMediaServiceInput_Click;
            // 
            // lb_MediaService
            // 
            lb_MediaService.Font = new System.Drawing.Font("Segoe UI", 8F);
            lb_MediaService.FormattingEnabled = true;
            lb_MediaService.ItemHeight = 13;
            lb_MediaService.Location = new System.Drawing.Point(14, 470);
            lb_MediaService.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_MediaService.Name = "lb_MediaService";
            lb_MediaService.Size = new System.Drawing.Size(201, 17);
            lb_MediaService.TabIndex = 1320;
            lb_MediaService.SelectedIndexChanged += lb_MediaService_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label10.Location = new System.Drawing.Point(13, 454);
            label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(124, 13);
            label10.TabIndex = 1319;
            label10.Text = "Publishers - Service:";
            // 
            // btn_CloseRemoteDataChannelInput
            // 
            btn_CloseRemoteDataChannelInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btn_CloseRemoteDataChannelInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_CloseRemoteDataChannelInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_CloseRemoteDataChannelInput.Location = new System.Drawing.Point(149, 370);
            btn_CloseRemoteDataChannelInput.Name = "btn_CloseRemoteDataChannelInput";
            btn_CloseRemoteDataChannelInput.Size = new System.Drawing.Size(16, 16);
            btn_CloseRemoteDataChannelInput.TabIndex = 1323;
            btn_CloseRemoteDataChannelInput.UseVisualStyleBackColor = true;
            btn_CloseRemoteDataChannelInput.Click += btn_CloseRemoteDataChannelInput_Click;
            // 
            // btn_RaiseHand
            // 
            btn_RaiseHand.Enabled = false;
            btn_RaiseHand.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_RaiseHand.Location = new System.Drawing.Point(451, 210);
            btn_RaiseHand.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_RaiseHand.Name = "btn_RaiseHand";
            btn_RaiseHand.Size = new System.Drawing.Size(30, 27);
            btn_RaiseHand.TabIndex = 1324;
            btn_RaiseHand.Text = "🖐️";
            toolTip1.SetToolTip(btn_RaiseHand, "Raise hand");
            btn_RaiseHand.UseVisualStyleBackColor = true;
            btn_RaiseHand.Click += btn_RaiseHand_Click;
            // 
            // btn_ParticipantLowerHand
            // 
            btn_ParticipantLowerHand.Enabled = false;
            btn_ParticipantLowerHand.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_ParticipantLowerHand.Location = new System.Drawing.Point(389, 474);
            btn_ParticipantLowerHand.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantLowerHand.Name = "btn_ParticipantLowerHand";
            btn_ParticipantLowerHand.Size = new System.Drawing.Size(30, 27);
            btn_ParticipantLowerHand.TabIndex = 1325;
            btn_ParticipantLowerHand.Text = "👇";
            toolTip1.SetToolTip(btn_ParticipantLowerHand, "Lower hand");
            btn_ParticipantLowerHand.UseVisualStyleBackColor = true;
            btn_ParticipantLowerHand.Click += btn_ParticipantLowerHand_Click;
            // 
            // btn_ParticipantLowerAllHands
            // 
            btn_ParticipantLowerAllHands.Enabled = false;
            btn_ParticipantLowerAllHands.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_ParticipantLowerAllHands.Location = new System.Drawing.Point(311, 210);
            btn_ParticipantLowerAllHands.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantLowerAllHands.Name = "btn_ParticipantLowerAllHands";
            btn_ParticipantLowerAllHands.Size = new System.Drawing.Size(30, 27);
            btn_ParticipantLowerAllHands.TabIndex = 1326;
            btn_ParticipantLowerAllHands.Text = "👇";
            toolTip1.SetToolTip(btn_ParticipantLowerAllHands, "Lower all hands");
            btn_ParticipantLowerAllHands.UseVisualStyleBackColor = true;
            btn_ParticipantLowerAllHands.Click += btn_ParticipantLowerAllHands_Click;
            // 
            // FormConferenceOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(780, 513);
            Controls.Add(btn_ParticipantLowerAllHands);
            Controls.Add(btn_ParticipantLowerHand);
            Controls.Add(btn_RaiseHand);
            Controls.Add(btn_CloseRemoteDataChannelInput);
            Controls.Add(btn_SubscribeMediaServiceInput);
            Controls.Add(btn_OutputMediaServiceInput);
            Controls.Add(lb_MediaService);
            Controls.Add(label10);
            Controls.Add(tbDataChannel);
            Controls.Add(label8);
            Controls.Add(btn_DataChannelSend);
            Controls.Add(btn_SubscribeRemoteDataChannelInput);
            Controls.Add(lb_PublishersDataChannel);
            Controls.Add(label6);
            Controls.Add(btn_OutputDynamicFeedInput);
            Controls.Add(btn_SubscribeDynamicFeedInput);
            Controls.Add(btn_SubscribeRemoteAudioInput);
            Controls.Add(btn_SubscribeRemoteSharingInput);
            Controls.Add(btn_SubscribeRemoteVideoInput);
            Controls.Add(btn_OutputRemoteSharingInput);
            Controls.Add(btn_OutputRemoteVideoInput);
            Controls.Add(lbl_ConferenceDetails);
            Controls.Add(lbl_ConferenceInProgress);
            Controls.Add(btnConferenceRecordingStart);
            Controls.Add(btnConferenceRecordingPause);
            Controls.Add(lblConferenceRecordingStatus);
            Controls.Add(btnConferenceStop);
            Controls.Add(btnConferenceLock);
            Controls.Add(btnConferenceMute);
            Controls.Add(label7);
            Controls.Add(tbInformation);
            Controls.Add(lb_Members);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(lb_PublishersSharing);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btn_ParticipantDelegate);
            Controls.Add(lb_ActiveTalkers);
            Controls.Add(lb_PublishersVideo);
            Controls.Add(lb_Participants);
            Controls.Add(btn_ParticipantDrop);
            Controls.Add(btn_ParticipantMute);
            Controls.Add(label1);
            Controls.Add(label9);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormConferenceOptions";
            Text = "FormConferenceOptions";
            toolTip1.SetToolTip(this, "Lower hand");
            FormClosing += FormConferenceOptions_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btn_ParticipantDelegate;
        private System.Windows.Forms.ListBox lb_ActiveTalkers;
        private System.Windows.Forms.ListBox lb_PublishersVideo;
        private System.Windows.Forms.ListBox lb_Participants;
        private System.Windows.Forms.Button btn_ParticipantDrop;
        private System.Windows.Forms.Button btn_ParticipantMute;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lb_PublishersSharing;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lb_Members;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbInformation;
        private System.Windows.Forms.Button btnConferenceLock;
        private System.Windows.Forms.Button btnConferenceMute;
        private System.Windows.Forms.Button btnConferenceStop;
        private System.Windows.Forms.Button btnConferenceRecordingStart;
        private System.Windows.Forms.Button btnConferenceRecordingPause;
        private System.Windows.Forms.Label lblConferenceRecordingStatus;
        private System.Windows.Forms.Label lbl_ConferenceInProgress;
        private System.Windows.Forms.Label lbl_ConferenceDetails;
        private System.Windows.Forms.Button btn_OutputRemoteVideoInput;
        private System.Windows.Forms.Button btn_OutputRemoteSharingInput;
        private System.Windows.Forms.Button btn_SubscribeRemoteVideoInput;
        private System.Windows.Forms.Button btn_SubscribeRemoteSharingInput;
        private System.Windows.Forms.Button btn_SubscribeRemoteAudioInput;
        private System.Windows.Forms.Button btn_SubscribeDynamicFeedInput;
        private System.Windows.Forms.Button btn_OutputDynamicFeedInput;
        private System.Windows.Forms.Button btn_SubscribeRemoteDataChannelInput;
        private System.Windows.Forms.ListBox lb_PublishersDataChannel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btn_DataChannelSend;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbDataChannel;
        private System.Windows.Forms.Button btn_SubscribeMediaServiceInput;
        private System.Windows.Forms.Button btn_OutputMediaServiceInput;
        private System.Windows.Forms.ListBox lb_MediaService;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btn_CloseRemoteDataChannelInput;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_RaiseHand;
        private System.Windows.Forms.Button btn_ParticipantLowerHand;
        private System.Windows.Forms.Button btn_ParticipantLowerAllHands;
    }
}