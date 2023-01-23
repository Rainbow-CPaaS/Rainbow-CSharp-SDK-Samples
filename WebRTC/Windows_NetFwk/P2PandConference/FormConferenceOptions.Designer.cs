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
            this.btn_ParticipantDelegate = new System.Windows.Forms.Button();
            this.lb_ActiveTalkers = new System.Windows.Forms.ListBox();
            this.lb_PublishersVideo = new System.Windows.Forms.ListBox();
            this.lb_Participants = new System.Windows.Forms.ListBox();
            this.btn_ParticipantDrop = new System.Windows.Forms.Button();
            this.btn_ParticipantMute = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lb_PublishersSharing = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lb_Members = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbInformation = new System.Windows.Forms.TextBox();
            this.btnConferenceLock = new System.Windows.Forms.Button();
            this.btnConferenceMute = new System.Windows.Forms.Button();
            this.btnConferenceStop = new System.Windows.Forms.Button();
            this.btnConferenceRecordingStart = new System.Windows.Forms.Button();
            this.btnConferenceRecordingPause = new System.Windows.Forms.Button();
            this.lblConferenceRecordingStatus = new System.Windows.Forms.Label();
            this.lbl_ConferenceInProgress = new System.Windows.Forms.Label();
            this.lbl_ConferenceDetails = new System.Windows.Forms.Label();
            this.btn_OutputRemoteVideoInput = new System.Windows.Forms.Button();
            this.btn_OutputRemoteSharingInput = new System.Windows.Forms.Button();
            this.btn_SubscribeRemoteVideoInput = new System.Windows.Forms.Button();
            this.btn_SubscribeRemoteSharingInput = new System.Windows.Forms.Button();
            this.btn_SubscribeRemoteAudioInput = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_ParticipantDelegate
            // 
            this.btn_ParticipantDelegate.Enabled = false;
            this.btn_ParticipantDelegate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_ParticipantDelegate.Location = new System.Drawing.Point(321, 474);
            this.btn_ParticipantDelegate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_ParticipantDelegate.Name = "btn_ParticipantDelegate";
            this.btn_ParticipantDelegate.Size = new System.Drawing.Size(58, 27);
            this.btn_ParticipantDelegate.TabIndex = 1281;
            this.btn_ParticipantDelegate.Text = "Delegate";
            this.btn_ParticipantDelegate.UseVisualStyleBackColor = true;
            this.btn_ParticipantDelegate.Click += new System.EventHandler(this.btn_ParticipantDelegate_Click);
            // 
            // lb_ActiveTalkers
            // 
            this.lb_ActiveTalkers.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lb_ActiveTalkers.FormattingEnabled = true;
            this.lb_ActiveTalkers.Location = new System.Drawing.Point(17, 142);
            this.lb_ActiveTalkers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lb_ActiveTalkers.Name = "lb_ActiveTalkers";
            this.lb_ActiveTalkers.Size = new System.Drawing.Size(200, 56);
            this.lb_ActiveTalkers.TabIndex = 1280;
            this.lb_ActiveTalkers.SelectedIndexChanged += new System.EventHandler(this.lb_ActiveTalkers_SelectedIndexChanged);
            // 
            // lb_PublishersVideo
            // 
            this.lb_PublishersVideo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lb_PublishersVideo.FormattingEnabled = true;
            this.lb_PublishersVideo.Location = new System.Drawing.Point(15, 247);
            this.lb_PublishersVideo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lb_PublishersVideo.Name = "lb_PublishersVideo";
            this.lb_PublishersVideo.Size = new System.Drawing.Size(200, 147);
            this.lb_PublishersVideo.TabIndex = 1279;
            this.lb_PublishersVideo.SelectedIndexChanged += new System.EventHandler(this.lb_PublishersVideo_SelectedIndexChanged);
            // 
            // lb_Participants
            // 
            this.lb_Participants.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lb_Participants.FormattingEnabled = true;
            this.lb_Participants.Location = new System.Drawing.Point(233, 235);
            this.lb_Participants.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lb_Participants.Name = "lb_Participants";
            this.lb_Participants.Size = new System.Drawing.Size(201, 238);
            this.lb_Participants.TabIndex = 1278;
            this.lb_Participants.SelectedIndexChanged += new System.EventHandler(this.lb_Participants_SelectedIndexChanged);
            // 
            // btn_ParticipantDrop
            // 
            this.btn_ParticipantDrop.Enabled = false;
            this.btn_ParticipantDrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_ParticipantDrop.Location = new System.Drawing.Point(382, 474);
            this.btn_ParticipantDrop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_ParticipantDrop.Name = "btn_ParticipantDrop";
            this.btn_ParticipantDrop.Size = new System.Drawing.Size(52, 27);
            this.btn_ParticipantDrop.TabIndex = 1277;
            this.btn_ParticipantDrop.Text = "Drop";
            this.btn_ParticipantDrop.UseVisualStyleBackColor = true;
            this.btn_ParticipantDrop.Click += new System.EventHandler(this.btn_ParticipantDrop_Click);
            // 
            // btn_ParticipantMute
            // 
            this.btn_ParticipantMute.Enabled = false;
            this.btn_ParticipantMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_ParticipantMute.Location = new System.Drawing.Point(232, 474);
            this.btn_ParticipantMute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_ParticipantMute.Name = "btn_ParticipantMute";
            this.btn_ParticipantMute.Size = new System.Drawing.Size(85, 27);
            this.btn_ParticipantMute.TabIndex = 1276;
            this.btn_ParticipantMute.Text = "Mute/Unmute";
            this.btn_ParticipantMute.UseVisualStyleBackColor = true;
            this.btn_ParticipantMute.Click += new System.EventHandler(this.btn_ParticipantMute_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(11, 231);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 1275;
            this.label1.Text = "Publishers - Video:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(232, 219);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 1274;
            this.label9.Text = "Participants:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(13, 126);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 1282;
            this.label2.Text = "Active Talkers:";
            // 
            // lb_PublishersSharing
            // 
            this.lb_PublishersSharing.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lb_PublishersSharing.FormattingEnabled = true;
            this.lb_PublishersSharing.Location = new System.Drawing.Point(15, 449);
            this.lb_PublishersSharing.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lb_PublishersSharing.Name = "lb_PublishersSharing";
            this.lb_PublishersSharing.Size = new System.Drawing.Size(201, 30);
            this.lb_PublishersSharing.TabIndex = 1284;
            this.lb_PublishersSharing.SelectedIndexChanged += new System.EventHandler(this.lb_PublishersSharing_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(11, 433);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 1283;
            this.label3.Text = "Publishers - Sharing:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(15, 9);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 1287;
            this.label4.Text = "Conference:";
            // 
            // lb_Members
            // 
            this.lb_Members.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lb_Members.FormattingEnabled = true;
            this.lb_Members.Location = new System.Drawing.Point(233, 25);
            this.lb_Members.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lb_Members.Name = "lb_Members";
            this.lb_Members.Size = new System.Drawing.Size(200, 186);
            this.lb_Members.TabIndex = 1289;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.Location = new System.Drawing.Point(232, 9);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(199, 13);
            this.label5.TabIndex = 1288;
            this.label5.Text = "Members (not in  the conference):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(439, 7);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 1291;
            this.label7.Text = "Information";
            // 
            // tbInformation
            // 
            this.tbInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbInformation.Location = new System.Drawing.Point(440, 23);
            this.tbInformation.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tbInformation.Multiline = true;
            this.tbInformation.Name = "tbInformation";
            this.tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInformation.Size = new System.Drawing.Size(329, 476);
            this.tbInformation.TabIndex = 1290;
            // 
            // btnConferenceLock
            // 
            this.btnConferenceLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConferenceLock.Location = new System.Drawing.Point(128, 48);
            this.btnConferenceLock.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConferenceLock.Name = "btnConferenceLock";
            this.btnConferenceLock.Size = new System.Drawing.Size(86, 20);
            this.btnConferenceLock.TabIndex = 1293;
            this.btnConferenceLock.Text = "Lock/Unlock";
            this.btnConferenceLock.UseVisualStyleBackColor = true;
            this.btnConferenceLock.Click += new System.EventHandler(this.btnConferenceLock_Click);
            // 
            // btnConferenceMute
            // 
            this.btnConferenceMute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConferenceMute.Location = new System.Drawing.Point(13, 48);
            this.btnConferenceMute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConferenceMute.Name = "btnConferenceMute";
            this.btnConferenceMute.Size = new System.Drawing.Size(86, 20);
            this.btnConferenceMute.TabIndex = 1292;
            this.btnConferenceMute.Text = "Mute/Unmute";
            this.btnConferenceMute.UseVisualStyleBackColor = true;
            this.btnConferenceMute.Click += new System.EventHandler(this.btnConferenceMute_Click);
            // 
            // btnConferenceStop
            // 
            this.btnConferenceStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConferenceStop.Location = new System.Drawing.Point(174, 7);
            this.btnConferenceStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConferenceStop.Name = "btnConferenceStop";
            this.btnConferenceStop.Size = new System.Drawing.Size(39, 20);
            this.btnConferenceStop.TabIndex = 1294;
            this.btnConferenceStop.Text = "Stop";
            this.btnConferenceStop.UseVisualStyleBackColor = true;
            this.btnConferenceStop.Click += new System.EventHandler(this.btnConferenceStop_Click);
            // 
            // btnConferenceRecordingStart
            // 
            this.btnConferenceRecordingStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConferenceRecordingStart.Location = new System.Drawing.Point(13, 94);
            this.btnConferenceRecordingStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConferenceRecordingStart.Name = "btnConferenceRecordingStart";
            this.btnConferenceRecordingStart.Size = new System.Drawing.Size(86, 20);
            this.btnConferenceRecordingStart.TabIndex = 1297;
            this.btnConferenceRecordingStart.Text = "Start/Stop";
            this.btnConferenceRecordingStart.UseVisualStyleBackColor = true;
            this.btnConferenceRecordingStart.Click += new System.EventHandler(this.btnConferenceRecordingStart_Click);
            // 
            // btnConferenceRecordingPause
            // 
            this.btnConferenceRecordingPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConferenceRecordingPause.Location = new System.Drawing.Point(128, 94);
            this.btnConferenceRecordingPause.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConferenceRecordingPause.Name = "btnConferenceRecordingPause";
            this.btnConferenceRecordingPause.Size = new System.Drawing.Size(86, 20);
            this.btnConferenceRecordingPause.TabIndex = 1296;
            this.btnConferenceRecordingPause.Text = "Pause/Resume";
            this.btnConferenceRecordingPause.UseVisualStyleBackColor = true;
            this.btnConferenceRecordingPause.Click += new System.EventHandler(this.btnConferenceRecordingPause_Click);
            // 
            // lblConferenceRecordingStatus
            // 
            this.lblConferenceRecordingStatus.AutoSize = true;
            this.lblConferenceRecordingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblConferenceRecordingStatus.Location = new System.Drawing.Point(15, 78);
            this.lblConferenceRecordingStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblConferenceRecordingStatus.Name = "lblConferenceRecordingStatus";
            this.lblConferenceRecordingStatus.Size = new System.Drawing.Size(69, 13);
            this.lblConferenceRecordingStatus.TabIndex = 1295;
            this.lblConferenceRecordingStatus.Text = "Recording:";
            // 
            // lbl_ConferenceInProgress
            // 
            this.lbl_ConferenceInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_ConferenceInProgress.ForeColor = System.Drawing.Color.Firebrick;
            this.lbl_ConferenceInProgress.Location = new System.Drawing.Point(86, 9);
            this.lbl_ConferenceInProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ConferenceInProgress.Name = "lbl_ConferenceInProgress";
            this.lbl_ConferenceInProgress.Size = new System.Drawing.Size(82, 18);
            this.lbl_ConferenceInProgress.TabIndex = 1298;
            this.lbl_ConferenceInProgress.Text = "In progress";
            this.lbl_ConferenceInProgress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_ConferenceInProgress.Visible = false;
            // 
            // lbl_ConferenceDetails
            // 
            this.lbl_ConferenceDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_ConferenceDetails.ForeColor = System.Drawing.Color.Blue;
            this.lbl_ConferenceDetails.Location = new System.Drawing.Point(15, 26);
            this.lbl_ConferenceDetails.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ConferenceDetails.Name = "lbl_ConferenceDetails";
            this.lbl_ConferenceDetails.Size = new System.Drawing.Size(202, 21);
            this.lbl_ConferenceDetails.TabIndex = 1299;
            this.lbl_ConferenceDetails.Text = "details";
            this.lbl_ConferenceDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_OutputRemoteVideoInput
            // 
            this.btn_OutputRemoteVideoInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_OutputRemoteVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputRemoteVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputRemoteVideoInput.Location = new System.Drawing.Point(151, 398);
            this.btn_OutputRemoteVideoInput.Name = "btn_OutputRemoteVideoInput";
            this.btn_OutputRemoteVideoInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputRemoteVideoInput.TabIndex = 1303;
            this.btn_OutputRemoteVideoInput.UseVisualStyleBackColor = true;
            // 
            // btn_OutputRemoteSharingInput
            // 
            this.btn_OutputRemoteSharingInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_OutputRemoteSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputRemoteSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputRemoteSharingInput.Location = new System.Drawing.Point(151, 483);
            this.btn_OutputRemoteSharingInput.Name = "btn_OutputRemoteSharingInput";
            this.btn_OutputRemoteSharingInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputRemoteSharingInput.TabIndex = 1304;
            this.btn_OutputRemoteSharingInput.UseVisualStyleBackColor = true;
            // 
            // btn_SubscribeRemoteVideoInput
            // 
            this.btn_SubscribeRemoteVideoInput.Enabled = false;
            this.btn_SubscribeRemoteVideoInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_SubscribeRemoteVideoInput.ForeColor = System.Drawing.Color.DarkGreen;
            this.btn_SubscribeRemoteVideoInput.Location = new System.Drawing.Point(72, 395);
            this.btn_SubscribeRemoteVideoInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_SubscribeRemoteVideoInput.Name = "btn_SubscribeRemoteVideoInput";
            this.btn_SubscribeRemoteVideoInput.Size = new System.Drawing.Size(73, 20);
            this.btn_SubscribeRemoteVideoInput.TabIndex = 1306;
            this.btn_SubscribeRemoteVideoInput.Text = "Subscribe";
            this.btn_SubscribeRemoteVideoInput.UseVisualStyleBackColor = true;
            // 
            // btn_SubscribeRemoteSharingInput
            // 
            this.btn_SubscribeRemoteSharingInput.Enabled = false;
            this.btn_SubscribeRemoteSharingInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_SubscribeRemoteSharingInput.ForeColor = System.Drawing.Color.DarkGreen;
            this.btn_SubscribeRemoteSharingInput.Location = new System.Drawing.Point(72, 481);
            this.btn_SubscribeRemoteSharingInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_SubscribeRemoteSharingInput.Name = "btn_SubscribeRemoteSharingInput";
            this.btn_SubscribeRemoteSharingInput.Size = new System.Drawing.Size(73, 20);
            this.btn_SubscribeRemoteSharingInput.TabIndex = 1307;
            this.btn_SubscribeRemoteSharingInput.Text = "Subscribe";
            this.btn_SubscribeRemoteSharingInput.UseVisualStyleBackColor = true;
            // 
            // btn_SubscribeRemoteAudioInput
            // 
            this.btn_SubscribeRemoteAudioInput.Enabled = false;
            this.btn_SubscribeRemoteAudioInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_SubscribeRemoteAudioInput.ForeColor = System.Drawing.Color.DarkGreen;
            this.btn_SubscribeRemoteAudioInput.Location = new System.Drawing.Point(74, 200);
            this.btn_SubscribeRemoteAudioInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_SubscribeRemoteAudioInput.Name = "btn_SubscribeRemoteAudioInput";
            this.btn_SubscribeRemoteAudioInput.Size = new System.Drawing.Size(73, 20);
            this.btn_SubscribeRemoteAudioInput.TabIndex = 1308;
            this.btn_SubscribeRemoteAudioInput.Text = "Subscribe";
            this.btn_SubscribeRemoteAudioInput.UseVisualStyleBackColor = true;
            // 
            // FormConferenceOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 513);
            this.Controls.Add(this.btn_SubscribeRemoteAudioInput);
            this.Controls.Add(this.btn_SubscribeRemoteSharingInput);
            this.Controls.Add(this.btn_SubscribeRemoteVideoInput);
            this.Controls.Add(this.btn_OutputRemoteSharingInput);
            this.Controls.Add(this.btn_OutputRemoteVideoInput);
            this.Controls.Add(this.lbl_ConferenceDetails);
            this.Controls.Add(this.lbl_ConferenceInProgress);
            this.Controls.Add(this.btnConferenceRecordingStart);
            this.Controls.Add(this.btnConferenceRecordingPause);
            this.Controls.Add(this.lblConferenceRecordingStatus);
            this.Controls.Add(this.btnConferenceStop);
            this.Controls.Add(this.btnConferenceLock);
            this.Controls.Add(this.btnConferenceMute);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.lb_Members);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lb_PublishersSharing);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_ParticipantDelegate);
            this.Controls.Add(this.lb_ActiveTalkers);
            this.Controls.Add(this.lb_PublishersVideo);
            this.Controls.Add(this.lb_Participants);
            this.Controls.Add(this.btn_ParticipantDrop);
            this.Controls.Add(this.btn_ParticipantMute);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label9);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConferenceOptions";
            this.Text = "FormConferenceOptions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormConferenceOptions_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}