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
            SuspendLayout();
            // 
            // btn_ParticipantDelegate
            // 
            btn_ParticipantDelegate.Enabled = false;
            btn_ParticipantDelegate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_ParticipantDelegate.Location = new System.Drawing.Point(321, 474);
            btn_ParticipantDelegate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantDelegate.Name = "btn_ParticipantDelegate";
            btn_ParticipantDelegate.Size = new System.Drawing.Size(58, 27);
            btn_ParticipantDelegate.TabIndex = 1281;
            btn_ParticipantDelegate.Text = "Delegate";
            btn_ParticipantDelegate.UseVisualStyleBackColor = true;
            btn_ParticipantDelegate.Click += btn_ParticipantDelegate_Click;
            // 
            // lb_ActiveTalkers
            // 
            lb_ActiveTalkers.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lb_ActiveTalkers.FormattingEnabled = true;
            lb_ActiveTalkers.Location = new System.Drawing.Point(17, 142);
            lb_ActiveTalkers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_ActiveTalkers.Name = "lb_ActiveTalkers";
            lb_ActiveTalkers.Size = new System.Drawing.Size(200, 56);
            lb_ActiveTalkers.TabIndex = 1280;
            lb_ActiveTalkers.SelectedIndexChanged += lb_ActiveTalkers_SelectedIndexChanged;
            // 
            // lb_PublishersVideo
            // 
            lb_PublishersVideo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lb_PublishersVideo.FormattingEnabled = true;
            lb_PublishersVideo.Location = new System.Drawing.Point(15, 247);
            lb_PublishersVideo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_PublishersVideo.Name = "lb_PublishersVideo";
            lb_PublishersVideo.Size = new System.Drawing.Size(200, 147);
            lb_PublishersVideo.TabIndex = 1279;
            lb_PublishersVideo.SelectedIndexChanged += lb_PublishersVideo_SelectedIndexChanged;
            // 
            // lb_Participants
            // 
            lb_Participants.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lb_Participants.FormattingEnabled = true;
            lb_Participants.Location = new System.Drawing.Point(233, 235);
            lb_Participants.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_Participants.Name = "lb_Participants";
            lb_Participants.Size = new System.Drawing.Size(201, 238);
            lb_Participants.TabIndex = 1278;
            lb_Participants.SelectedIndexChanged += lb_Participants_SelectedIndexChanged;
            // 
            // btn_ParticipantDrop
            // 
            btn_ParticipantDrop.Enabled = false;
            btn_ParticipantDrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_ParticipantDrop.Location = new System.Drawing.Point(382, 474);
            btn_ParticipantDrop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantDrop.Name = "btn_ParticipantDrop";
            btn_ParticipantDrop.Size = new System.Drawing.Size(52, 27);
            btn_ParticipantDrop.TabIndex = 1277;
            btn_ParticipantDrop.Text = "Drop";
            btn_ParticipantDrop.UseVisualStyleBackColor = true;
            btn_ParticipantDrop.Click += btn_ParticipantDrop_Click;
            // 
            // btn_ParticipantMute
            // 
            btn_ParticipantMute.Enabled = false;
            btn_ParticipantMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_ParticipantMute.Location = new System.Drawing.Point(232, 474);
            btn_ParticipantMute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_ParticipantMute.Name = "btn_ParticipantMute";
            btn_ParticipantMute.Size = new System.Drawing.Size(85, 27);
            btn_ParticipantMute.TabIndex = 1276;
            btn_ParticipantMute.Text = "Mute/Unmute";
            btn_ParticipantMute.UseVisualStyleBackColor = true;
            btn_ParticipantMute.Click += btn_ParticipantMute_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(11, 231);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(113, 13);
            label1.TabIndex = 1275;
            label1.Text = "Publishers - Video:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label9.Location = new System.Drawing.Point(232, 219);
            label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(78, 13);
            label9.TabIndex = 1274;
            label9.Text = "Participants:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(13, 126);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(93, 13);
            label2.TabIndex = 1282;
            label2.Text = "Active Talkers:";
            // 
            // lb_PublishersSharing
            // 
            lb_PublishersSharing.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lb_PublishersSharing.FormattingEnabled = true;
            lb_PublishersSharing.Location = new System.Drawing.Point(15, 449);
            lb_PublishersSharing.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_PublishersSharing.Name = "lb_PublishersSharing";
            lb_PublishersSharing.Size = new System.Drawing.Size(201, 30);
            lb_PublishersSharing.TabIndex = 1284;
            lb_PublishersSharing.SelectedIndexChanged += lb_PublishersSharing_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(11, 433);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(124, 13);
            label3.TabIndex = 1283;
            label3.Text = "Publishers - Sharing:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label4.Location = new System.Drawing.Point(15, 9);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(76, 13);
            label4.TabIndex = 1287;
            label4.Text = "Conference:";
            // 
            // lb_Members
            // 
            lb_Members.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lb_Members.FormattingEnabled = true;
            lb_Members.Location = new System.Drawing.Point(233, 25);
            lb_Members.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_Members.Name = "lb_Members";
            lb_Members.Size = new System.Drawing.Size(200, 186);
            lb_Members.TabIndex = 1289;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
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
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label7.Location = new System.Drawing.Point(439, 7);
            label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(70, 13);
            label7.TabIndex = 1291;
            label7.Text = "Information";
            // 
            // tbInformation
            // 
            tbInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tbInformation.Location = new System.Drawing.Point(440, 23);
            tbInformation.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            tbInformation.Multiline = true;
            tbInformation.Name = "tbInformation";
            tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbInformation.Size = new System.Drawing.Size(329, 476);
            tbInformation.TabIndex = 1290;
            // 
            // btnConferenceLock
            // 
            btnConferenceLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
            btnConferenceMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
            btnConferenceStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
            btnConferenceRecordingStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnConferenceRecordingStart.Location = new System.Drawing.Point(13, 94);
            btnConferenceRecordingStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConferenceRecordingStart.Name = "btnConferenceRecordingStart";
            btnConferenceRecordingStart.Size = new System.Drawing.Size(86, 20);
            btnConferenceRecordingStart.TabIndex = 1297;
            btnConferenceRecordingStart.Text = "Start/Stop";
            btnConferenceRecordingStart.UseVisualStyleBackColor = true;
            btnConferenceRecordingStart.Click += btnConferenceRecordingStart_Click;
            // 
            // btnConferenceRecordingPause
            // 
            btnConferenceRecordingPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnConferenceRecordingPause.Location = new System.Drawing.Point(128, 94);
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
            lblConferenceRecordingStatus.AutoSize = true;
            lblConferenceRecordingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblConferenceRecordingStatus.Location = new System.Drawing.Point(15, 78);
            lblConferenceRecordingStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblConferenceRecordingStatus.Name = "lblConferenceRecordingStatus";
            lblConferenceRecordingStatus.Size = new System.Drawing.Size(69, 13);
            lblConferenceRecordingStatus.TabIndex = 1295;
            lblConferenceRecordingStatus.Text = "Recording:";
            // 
            // lbl_ConferenceInProgress
            // 
            lbl_ConferenceInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
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
            lbl_ConferenceDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
            btn_OutputRemoteVideoInput.Location = new System.Drawing.Point(151, 397);
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
            btn_OutputRemoteSharingInput.Location = new System.Drawing.Point(151, 483);
            btn_OutputRemoteSharingInput.Name = "btn_OutputRemoteSharingInput";
            btn_OutputRemoteSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputRemoteSharingInput.TabIndex = 1304;
            btn_OutputRemoteSharingInput.UseVisualStyleBackColor = true;
            btn_OutputRemoteSharingInput.Click += btn_OutputRemoteSharingInput_Click;
            // 
            // btn_SubscribeRemoteVideoInput
            // 
            btn_SubscribeRemoteVideoInput.Enabled = false;
            btn_SubscribeRemoteVideoInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_SubscribeRemoteVideoInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteVideoInput.Location = new System.Drawing.Point(72, 394);
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
            btn_SubscribeRemoteSharingInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_SubscribeRemoteSharingInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteSharingInput.Location = new System.Drawing.Point(72, 481);
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
            btn_SubscribeRemoteAudioInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_SubscribeRemoteAudioInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteAudioInput.Location = new System.Drawing.Point(74, 200);
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
            btn_SubscribeDynamicFeedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btn_SubscribeDynamicFeedInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeDynamicFeedInput.Location = new System.Drawing.Point(41, 413);
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
            btn_OutputDynamicFeedInput.Location = new System.Drawing.Point(186, 416);
            btn_OutputDynamicFeedInput.Name = "btn_OutputDynamicFeedInput";
            btn_OutputDynamicFeedInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputDynamicFeedInput.TabIndex = 1310;
            btn_OutputDynamicFeedInput.UseVisualStyleBackColor = true;
            btn_OutputDynamicFeedInput.Click += btn_OutputDynamicFeedInput_Click;
            // 
            // FormConferenceOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(780, 513);
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
    }
}