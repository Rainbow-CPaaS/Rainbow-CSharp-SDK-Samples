namespace Sample_P2PandConference
{
    partial class WebRTCForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebRTCForm));
            this.label2 = new System.Windows.Forms.Label();
            this.cb_AudioPlaybackDevices = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_AudioSecondaryPlaybackDevices = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_AudioRecordingDevices = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_ContactsList = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_VideoRecordingDevices = new System.Windows.Forms.ComboBox();
            this.check_LocalVideo = new System.Windows.Forms.CheckBox();
            this.check_LocalSharing = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.check_RemoteSharing = new System.Windows.Forms.CheckBox();
            this.check_RemoteVideo = new System.Windows.Forms.CheckBox();
            this.check_RemoteAudio = new System.Windows.Forms.CheckBox();
            this.cb_MakeCallMedias = new System.Windows.Forms.ComboBox();
            this.btn_MakeCall = new System.Windows.Forms.Button();
            this.btn_HangUp = new System.Windows.Forms.Button();
            this.btn_Decline = new System.Windows.Forms.Button();
            this.btn_AnswerCall = new System.Windows.Forms.Button();
            this.cb_AnswerCallMedias = new System.Windows.Forms.ComboBox();
            this.btn_AddMedia = new System.Windows.Forms.Button();
            this.cb_AddMedia = new System.Windows.Forms.ComboBox();
            this.btn_RemoveMedia = new System.Windows.Forms.Button();
            this.cb_RemoveMedia = new System.Windows.Forms.ComboBox();
            this.tb_Subject = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_Information = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.check_LocalAudioMuted = new System.Windows.Forms.CheckBox();
            this.btn_MuteMedia = new System.Windows.Forms.Button();
            this.cb_MuteMedia = new System.Windows.Forms.ComboBox();
            this.btn_BrowseAudioRecordingFile = new System.Windows.Forms.Button();
            this.tb_AudioRecordingFile = new System.Windows.Forms.TextBox();
            this.tb_VideoRecordingFile = new System.Windows.Forms.TextBox();
            this.btn_BrowseVideoRecordingFile = new System.Windows.Forms.Button();
            this.check_LocalAudio = new System.Windows.Forms.CheckBox();
            this.check_VideoRecordingFileWithAudio = new System.Windows.Forms.CheckBox();
            this.btn_UnmuteMedia = new System.Windows.Forms.Button();
            this.cb_UnmuteMedia = new System.Windows.Forms.ComboBox();
            this.check_LocalVideoMuted = new System.Windows.Forms.CheckBox();
            this.check_LocalSharingMuted = new System.Windows.Forms.CheckBox();
            this.check_AudioRecordingFileLoop = new System.Windows.Forms.CheckBox();
            this.check_VideoRecordingFileLoop = new System.Windows.Forms.CheckBox();
            this.check_VideoSecondaryRecordingFileLoop = new System.Windows.Forms.CheckBox();
            this.tb_VideoSecondaryRecordingFile = new System.Windows.Forms.TextBox();
            this.btn_BrowseVideoSecondaryRecordingFile = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.cb_VideoSecondaryRecordingDevices = new System.Windows.Forms.ComboBox();
            this.picture_VideoRemote = new System.Windows.Forms.PictureBox();
            this.picture_VideoLocal = new System.Windows.Forms.PictureBox();
            this.picture_SharingRemote = new System.Windows.Forms.PictureBox();
            this.picture_SharingLocal = new System.Windows.Forms.PictureBox();
            this.cb_SubscribeVideoLocal = new System.Windows.Forms.CheckBox();
            this.cb_SubscribeSharingLocal = new System.Windows.Forms.CheckBox();
            this.cb_P2PSubscribeVideoRemote = new System.Windows.Forms.CheckBox();
            this.cb_P2PSubscribeSharingRemote = new System.Windows.Forms.CheckBox();
            this.btn_JoinConf = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.cb_BubblesList = new System.Windows.Forms.ComboBox();
            this.btn_StartConf = new System.Windows.Forms.Button();
            this.tb_BubbleInfo = new System.Windows.Forms.TextBox();
            this.lb_VideoPublishers = new System.Windows.Forms.ListBox();
            this.label13 = new System.Windows.Forms.Label();
            this.lb_Participants = new System.Windows.Forms.ListBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btn_SubscribeVideoMedia = new System.Windows.Forms.Button();
            this.btn_UnsubscribeVideoMedia = new System.Windows.Forms.Button();
            this.btn_UnmuteParticipant = new System.Windows.Forms.Button();
            this.btn_MuteParticipant = new System.Windows.Forms.Button();
            this.btn_ViewVideoMedia = new System.Windows.Forms.Button();
            this.btn_DelegateParticipant = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.tb_ActiveSpeaker = new System.Windows.Forms.TextBox();
            this.tb_ConferenceName = new System.Windows.Forms.TextBox();
            this.tb_VideoLocalInfo = new System.Windows.Forms.TextBox();
            this.tb_SharingLocalInfo = new System.Windows.Forms.TextBox();
            this.tb_VideoRemoteInfo = new System.Windows.Forms.TextBox();
            this.tb_SharingRemoteInfo = new System.Windows.Forms.TextBox();
            this.cb_AutoSubscriptionSharing = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.tb_RemoteSharingInfo = new System.Windows.Forms.TextBox();
            this.btn_UnsubscribeSharingMedia = new System.Windows.Forms.Button();
            this.btn_SubscribeSharingMedia = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.tb_MaxSubscriptionVideo = new System.Windows.Forms.TextBox();
            this.cb_AutoSubscriptionVideo = new System.Windows.Forms.CheckBox();
            this.btn_AskToShare = new System.Windows.Forms.Button();
            this.btn_DropParticipant = new System.Windows.Forms.Button();
            this.btn_AudioRecordingDevicesRefresh = new System.Windows.Forms.Button();
            this.btn_AudioPlaybackDevicesRefresh = new System.Windows.Forms.Button();
            this.btn_VideoRecordingDevicesRefresh = new System.Windows.Forms.Button();
            this.btn_VideoSecondaryRecordingDevicesRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picture_VideoRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_VideoLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_SharingRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_SharingLocal)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Audio Output Device";
            // 
            // cb_AudioPlaybackDevices
            // 
            this.cb_AudioPlaybackDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AudioPlaybackDevices.FormattingEnabled = true;
            this.cb_AudioPlaybackDevices.Location = new System.Drawing.Point(12, 25);
            this.cb_AudioPlaybackDevices.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AudioPlaybackDevices.Name = "cb_AudioPlaybackDevices";
            this.cb_AudioPlaybackDevices.Size = new System.Drawing.Size(244, 21);
            this.cb_AudioPlaybackDevices.TabIndex = 4;
            this.cb_AudioPlaybackDevices.SelectedIndexChanged += new System.EventHandler(this.Cb_AudioPlaybackDevices_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(287, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Audio Secondary Output Device";
            // 
            // cb_AudioSecondaryPlaybackDevices
            // 
            this.cb_AudioSecondaryPlaybackDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AudioSecondaryPlaybackDevices.FormattingEnabled = true;
            this.cb_AudioSecondaryPlaybackDevices.Location = new System.Drawing.Point(288, 25);
            this.cb_AudioSecondaryPlaybackDevices.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AudioSecondaryPlaybackDevices.Name = "cb_AudioSecondaryPlaybackDevices";
            this.cb_AudioSecondaryPlaybackDevices.Size = new System.Drawing.Size(246, 21);
            this.cb_AudioSecondaryPlaybackDevices.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 53);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Audio Input Device";
            // 
            // cb_AudioRecordingDevices
            // 
            this.cb_AudioRecordingDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AudioRecordingDevices.FormattingEnabled = true;
            this.cb_AudioRecordingDevices.Location = new System.Drawing.Point(10, 69);
            this.cb_AudioRecordingDevices.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AudioRecordingDevices.Name = "cb_AudioRecordingDevices";
            this.cb_AudioRecordingDevices.Size = new System.Drawing.Size(246, 21);
            this.cb_AudioRecordingDevices.TabIndex = 8;
            this.cb_AudioRecordingDevices.SelectedIndexChanged += new System.EventHandler(this.Cb_AudioRecordingDevices_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(11, 215);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Contacts";
            // 
            // cb_ContactsList
            // 
            this.cb_ContactsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_ContactsList.FormattingEnabled = true;
            this.cb_ContactsList.Location = new System.Drawing.Point(11, 231);
            this.cb_ContactsList.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_ContactsList.Name = "cb_ContactsList";
            this.cb_ContactsList.Size = new System.Drawing.Size(245, 21);
            this.cb_ContactsList.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 104);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Video Input Device";
            // 
            // cb_VideoRecordingDevices
            // 
            this.cb_VideoRecordingDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_VideoRecordingDevices.FormattingEnabled = true;
            this.cb_VideoRecordingDevices.Location = new System.Drawing.Point(10, 120);
            this.cb_VideoRecordingDevices.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_VideoRecordingDevices.Name = "cb_VideoRecordingDevices";
            this.cb_VideoRecordingDevices.Size = new System.Drawing.Size(246, 21);
            this.cb_VideoRecordingDevices.TabIndex = 12;
            this.cb_VideoRecordingDevices.SelectedIndexChanged += new System.EventHandler(this.cb_VideoRecordingDevices_SelectedIndexChanged);
            // 
            // check_LocalVideo
            // 
            this.check_LocalVideo.AutoCheck = false;
            this.check_LocalVideo.AutoSize = true;
            this.check_LocalVideo.Location = new System.Drawing.Point(346, 281);
            this.check_LocalVideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalVideo.Name = "check_LocalVideo";
            this.check_LocalVideo.Size = new System.Drawing.Size(53, 17);
            this.check_LocalVideo.TabIndex = 15;
            this.check_LocalVideo.Text = "Video";
            this.check_LocalVideo.UseVisualStyleBackColor = true;
            // 
            // check_LocalSharing
            // 
            this.check_LocalSharing.AutoCheck = false;
            this.check_LocalSharing.AutoSize = true;
            this.check_LocalSharing.Location = new System.Drawing.Point(346, 304);
            this.check_LocalSharing.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalSharing.Name = "check_LocalSharing";
            this.check_LocalSharing.Size = new System.Drawing.Size(62, 17);
            this.check_LocalSharing.TabIndex = 16;
            this.check_LocalSharing.Text = "Sharing";
            this.check_LocalSharing.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(363, 239);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Local Medias";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(363, 351);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Remote Medias";
            // 
            // check_RemoteSharing
            // 
            this.check_RemoteSharing.AutoCheck = false;
            this.check_RemoteSharing.AutoSize = true;
            this.check_RemoteSharing.Location = new System.Drawing.Point(375, 420);
            this.check_RemoteSharing.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_RemoteSharing.Name = "check_RemoteSharing";
            this.check_RemoteSharing.Size = new System.Drawing.Size(62, 17);
            this.check_RemoteSharing.TabIndex = 20;
            this.check_RemoteSharing.Text = "Sharing";
            this.check_RemoteSharing.UseVisualStyleBackColor = true;
            // 
            // check_RemoteVideo
            // 
            this.check_RemoteVideo.AutoCheck = false;
            this.check_RemoteVideo.AutoSize = true;
            this.check_RemoteVideo.Location = new System.Drawing.Point(375, 397);
            this.check_RemoteVideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_RemoteVideo.Name = "check_RemoteVideo";
            this.check_RemoteVideo.Size = new System.Drawing.Size(53, 17);
            this.check_RemoteVideo.TabIndex = 19;
            this.check_RemoteVideo.Text = "Video";
            this.check_RemoteVideo.UseVisualStyleBackColor = true;
            // 
            // check_RemoteAudio
            // 
            this.check_RemoteAudio.AutoCheck = false;
            this.check_RemoteAudio.AutoSize = true;
            this.check_RemoteAudio.Location = new System.Drawing.Point(375, 373);
            this.check_RemoteAudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_RemoteAudio.Name = "check_RemoteAudio";
            this.check_RemoteAudio.Size = new System.Drawing.Size(53, 17);
            this.check_RemoteAudio.TabIndex = 18;
            this.check_RemoteAudio.Text = "Audio";
            this.check_RemoteAudio.UseVisualStyleBackColor = true;
            // 
            // cb_MakeCallMedias
            // 
            this.cb_MakeCallMedias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_MakeCallMedias.FormattingEnabled = true;
            this.cb_MakeCallMedias.Location = new System.Drawing.Point(10, 279);
            this.cb_MakeCallMedias.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_MakeCallMedias.Name = "cb_MakeCallMedias";
            this.cb_MakeCallMedias.Size = new System.Drawing.Size(123, 21);
            this.cb_MakeCallMedias.TabIndex = 22;
            // 
            // btn_MakeCall
            // 
            this.btn_MakeCall.Location = new System.Drawing.Point(142, 278);
            this.btn_MakeCall.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_MakeCall.Name = "btn_MakeCall";
            this.btn_MakeCall.Size = new System.Drawing.Size(74, 23);
            this.btn_MakeCall.TabIndex = 23;
            this.btn_MakeCall.Text = "Make Call";
            this.btn_MakeCall.UseVisualStyleBackColor = true;
            this.btn_MakeCall.Click += new System.EventHandler(this.btn_MakeCall_Click);
            // 
            // btn_HangUp
            // 
            this.btn_HangUp.Location = new System.Drawing.Point(220, 279);
            this.btn_HangUp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_HangUp.Name = "btn_HangUp";
            this.btn_HangUp.Size = new System.Drawing.Size(81, 23);
            this.btn_HangUp.TabIndex = 24;
            this.btn_HangUp.Text = "Hang Up";
            this.btn_HangUp.UseVisualStyleBackColor = true;
            this.btn_HangUp.Click += new System.EventHandler(this.btn_HangUp_Click);
            // 
            // btn_Decline
            // 
            this.btn_Decline.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Decline.FlatAppearance.BorderSize = 0;
            this.btn_Decline.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Decline.Location = new System.Drawing.Point(219, 306);
            this.btn_Decline.Margin = new System.Windows.Forms.Padding(0);
            this.btn_Decline.Name = "btn_Decline";
            this.btn_Decline.Size = new System.Drawing.Size(82, 23);
            this.btn_Decline.TabIndex = 25;
            this.btn_Decline.Text = "Decline";
            this.btn_Decline.UseVisualStyleBackColor = true;
            this.btn_Decline.Click += new System.EventHandler(this.btn_Decline_Click);
            // 
            // btn_AnswerCall
            // 
            this.btn_AnswerCall.Location = new System.Drawing.Point(142, 306);
            this.btn_AnswerCall.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AnswerCall.Name = "btn_AnswerCall";
            this.btn_AnswerCall.Size = new System.Drawing.Size(74, 23);
            this.btn_AnswerCall.TabIndex = 27;
            this.btn_AnswerCall.Text = "Answer Call";
            this.btn_AnswerCall.UseVisualStyleBackColor = true;
            this.btn_AnswerCall.Click += new System.EventHandler(this.btn_AnswerCall_Click);
            // 
            // cb_AnswerCallMedias
            // 
            this.cb_AnswerCallMedias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AnswerCallMedias.FormattingEnabled = true;
            this.cb_AnswerCallMedias.Location = new System.Drawing.Point(10, 306);
            this.cb_AnswerCallMedias.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AnswerCallMedias.Name = "cb_AnswerCallMedias";
            this.cb_AnswerCallMedias.Size = new System.Drawing.Size(123, 21);
            this.cb_AnswerCallMedias.TabIndex = 26;
            // 
            // btn_AddMedia
            // 
            this.btn_AddMedia.Location = new System.Drawing.Point(139, 369);
            this.btn_AddMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AddMedia.Name = "btn_AddMedia";
            this.btn_AddMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_AddMedia.TabIndex = 29;
            this.btn_AddMedia.Text = "Add Media";
            this.btn_AddMedia.UseVisualStyleBackColor = true;
            this.btn_AddMedia.Click += new System.EventHandler(this.btn_AddMedia_Click);
            // 
            // cb_AddMedia
            // 
            this.cb_AddMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AddMedia.FormattingEnabled = true;
            this.cb_AddMedia.Location = new System.Drawing.Point(10, 369);
            this.cb_AddMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AddMedia.Name = "cb_AddMedia";
            this.cb_AddMedia.Size = new System.Drawing.Size(123, 21);
            this.cb_AddMedia.TabIndex = 28;
            this.cb_AddMedia.SelectedIndexChanged += new System.EventHandler(this.cb_AddMedia_SelectedIndexChanged);
            // 
            // btn_RemoveMedia
            // 
            this.btn_RemoveMedia.Location = new System.Drawing.Point(139, 341);
            this.btn_RemoveMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_RemoveMedia.Name = "btn_RemoveMedia";
            this.btn_RemoveMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_RemoveMedia.TabIndex = 31;
            this.btn_RemoveMedia.Text = "Rmv Media";
            this.btn_RemoveMedia.UseVisualStyleBackColor = true;
            this.btn_RemoveMedia.Click += new System.EventHandler(this.btn_RemoveMedia_Click);
            // 
            // cb_RemoveMedia
            // 
            this.cb_RemoveMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_RemoveMedia.FormattingEnabled = true;
            this.cb_RemoveMedia.Location = new System.Drawing.Point(10, 342);
            this.cb_RemoveMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_RemoveMedia.Name = "cb_RemoveMedia";
            this.cb_RemoveMedia.Size = new System.Drawing.Size(123, 21);
            this.cb_RemoveMedia.TabIndex = 30;
            // 
            // tb_Subject
            // 
            this.tb_Subject.Location = new System.Drawing.Point(58, 253);
            this.tb_Subject.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_Subject.Name = "tb_Subject";
            this.tb_Subject.Size = new System.Drawing.Size(198, 20);
            this.tb_Subject.TabIndex = 32;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(11, 256);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(46, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Subject:";
            // 
            // tb_Information
            // 
            this.tb_Information.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_Information.Location = new System.Drawing.Point(791, 26);
            this.tb_Information.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_Information.Multiline = true;
            this.tb_Information.Name = "tb_Information";
            this.tb_Information.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Information.Size = new System.Drawing.Size(480, 295);
            this.tb_Information.TabIndex = 34;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(793, 10);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 13);
            this.label9.TabIndex = 35;
            this.label9.Text = "Information";
            // 
            // check_LocalAudioMuted
            // 
            this.check_LocalAudioMuted.AutoCheck = false;
            this.check_LocalAudioMuted.AutoSize = true;
            this.check_LocalAudioMuted.Location = new System.Drawing.Point(418, 257);
            this.check_LocalAudioMuted.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalAudioMuted.Name = "check_LocalAudioMuted";
            this.check_LocalAudioMuted.Size = new System.Drawing.Size(56, 17);
            this.check_LocalAudioMuted.TabIndex = 37;
            this.check_LocalAudioMuted.Text = "Muted";
            this.check_LocalAudioMuted.UseVisualStyleBackColor = true;
            // 
            // btn_MuteMedia
            // 
            this.btn_MuteMedia.Location = new System.Drawing.Point(139, 395);
            this.btn_MuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_MuteMedia.Name = "btn_MuteMedia";
            this.btn_MuteMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_MuteMedia.TabIndex = 39;
            this.btn_MuteMedia.Text = "Mute";
            this.btn_MuteMedia.UseVisualStyleBackColor = true;
            this.btn_MuteMedia.Click += new System.EventHandler(this.btn_MuteMedia_Click);
            // 
            // cb_MuteMedia
            // 
            this.cb_MuteMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_MuteMedia.FormattingEnabled = true;
            this.cb_MuteMedia.Location = new System.Drawing.Point(10, 396);
            this.cb_MuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_MuteMedia.Name = "cb_MuteMedia";
            this.cb_MuteMedia.Size = new System.Drawing.Size(123, 21);
            this.cb_MuteMedia.TabIndex = 38;
            // 
            // btn_BrowseAudioRecordingFile
            // 
            this.btn_BrowseAudioRecordingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_BrowseAudioRecordingFile.Location = new System.Drawing.Point(495, 70);
            this.btn_BrowseAudioRecordingFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_BrowseAudioRecordingFile.Name = "btn_BrowseAudioRecordingFile";
            this.btn_BrowseAudioRecordingFile.Size = new System.Drawing.Size(39, 20);
            this.btn_BrowseAudioRecordingFile.TabIndex = 40;
            this.btn_BrowseAudioRecordingFile.Text = "...";
            this.btn_BrowseAudioRecordingFile.UseVisualStyleBackColor = true;
            this.btn_BrowseAudioRecordingFile.Click += new System.EventHandler(this.btn_BrowseAudioRecordingFile_Click);
            // 
            // tb_AudioRecordingFile
            // 
            this.tb_AudioRecordingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_AudioRecordingFile.Location = new System.Drawing.Point(288, 71);
            this.tb_AudioRecordingFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_AudioRecordingFile.Name = "tb_AudioRecordingFile";
            this.tb_AudioRecordingFile.Size = new System.Drawing.Size(204, 18);
            this.tb_AudioRecordingFile.TabIndex = 41;
            this.tb_AudioRecordingFile.Text = "https://upload.wikimedia.org/wikipedia/commons/0/0f/Pop_RockBrit_%28exploration%2" +
    "9-en_wave.wav";
            // 
            // tb_VideoRecordingFile
            // 
            this.tb_VideoRecordingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_VideoRecordingFile.Location = new System.Drawing.Point(288, 121);
            this.tb_VideoRecordingFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_VideoRecordingFile.Name = "tb_VideoRecordingFile";
            this.tb_VideoRecordingFile.Size = new System.Drawing.Size(204, 18);
            this.tb_VideoRecordingFile.TabIndex = 43;
            this.tb_VideoRecordingFile.Text = "C:\\media\\Joy_and_Heron.webm";
            // 
            // btn_BrowseVideoRecordingFile
            // 
            this.btn_BrowseVideoRecordingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_BrowseVideoRecordingFile.Location = new System.Drawing.Point(495, 120);
            this.btn_BrowseVideoRecordingFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_BrowseVideoRecordingFile.Name = "btn_BrowseVideoRecordingFile";
            this.btn_BrowseVideoRecordingFile.Size = new System.Drawing.Size(39, 20);
            this.btn_BrowseVideoRecordingFile.TabIndex = 42;
            this.btn_BrowseVideoRecordingFile.Text = "...";
            this.btn_BrowseVideoRecordingFile.UseVisualStyleBackColor = true;
            this.btn_BrowseVideoRecordingFile.Click += new System.EventHandler(this.btn_BrowseVideoRecordingFile_Click);
            // 
            // check_LocalAudio
            // 
            this.check_LocalAudio.AutoCheck = false;
            this.check_LocalAudio.AutoSize = true;
            this.check_LocalAudio.Location = new System.Drawing.Point(346, 257);
            this.check_LocalAudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalAudio.Name = "check_LocalAudio";
            this.check_LocalAudio.Size = new System.Drawing.Size(53, 17);
            this.check_LocalAudio.TabIndex = 14;
            this.check_LocalAudio.Text = "Audio";
            this.check_LocalAudio.UseVisualStyleBackColor = true;
            // 
            // check_VideoRecordingFileWithAudio
            // 
            this.check_VideoRecordingFileWithAudio.AutoSize = true;
            this.check_VideoRecordingFileWithAudio.Location = new System.Drawing.Point(341, 141);
            this.check_VideoRecordingFileWithAudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_VideoRecordingFileWithAudio.Name = "check_VideoRecordingFileWithAudio";
            this.check_VideoRecordingFileWithAudio.Size = new System.Drawing.Size(77, 17);
            this.check_VideoRecordingFileWithAudio.TabIndex = 44;
            this.check_VideoRecordingFileWithAudio.Text = "With audio";
            this.check_VideoRecordingFileWithAudio.UseVisualStyleBackColor = true;
            this.check_VideoRecordingFileWithAudio.CheckedChanged += new System.EventHandler(this.Check_VideoRecordingFileWithAudio_CheckedChanged);
            // 
            // btn_UnmuteMedia
            // 
            this.btn_UnmuteMedia.Location = new System.Drawing.Point(139, 422);
            this.btn_UnmuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_UnmuteMedia.Name = "btn_UnmuteMedia";
            this.btn_UnmuteMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_UnmuteMedia.TabIndex = 46;
            this.btn_UnmuteMedia.Text = "Unmute";
            this.btn_UnmuteMedia.UseVisualStyleBackColor = true;
            this.btn_UnmuteMedia.Click += new System.EventHandler(this.btn_UnmuteMedia_Click);
            // 
            // cb_UnmuteMedia
            // 
            this.cb_UnmuteMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_UnmuteMedia.FormattingEnabled = true;
            this.cb_UnmuteMedia.Location = new System.Drawing.Point(10, 423);
            this.cb_UnmuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_UnmuteMedia.Name = "cb_UnmuteMedia";
            this.cb_UnmuteMedia.Size = new System.Drawing.Size(123, 21);
            this.cb_UnmuteMedia.TabIndex = 45;
            // 
            // check_LocalVideoMuted
            // 
            this.check_LocalVideoMuted.AutoCheck = false;
            this.check_LocalVideoMuted.AutoSize = true;
            this.check_LocalVideoMuted.Location = new System.Drawing.Point(418, 281);
            this.check_LocalVideoMuted.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalVideoMuted.Name = "check_LocalVideoMuted";
            this.check_LocalVideoMuted.Size = new System.Drawing.Size(56, 17);
            this.check_LocalVideoMuted.TabIndex = 47;
            this.check_LocalVideoMuted.Text = "Muted";
            this.check_LocalVideoMuted.UseVisualStyleBackColor = true;
            // 
            // check_LocalSharingMuted
            // 
            this.check_LocalSharingMuted.AutoCheck = false;
            this.check_LocalSharingMuted.AutoSize = true;
            this.check_LocalSharingMuted.Location = new System.Drawing.Point(418, 304);
            this.check_LocalSharingMuted.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalSharingMuted.Name = "check_LocalSharingMuted";
            this.check_LocalSharingMuted.Size = new System.Drawing.Size(56, 17);
            this.check_LocalSharingMuted.TabIndex = 48;
            this.check_LocalSharingMuted.Text = "Muted";
            this.check_LocalSharingMuted.UseVisualStyleBackColor = true;
            // 
            // check_AudioRecordingFileLoop
            // 
            this.check_AudioRecordingFileLoop.AutoSize = true;
            this.check_AudioRecordingFileLoop.Checked = true;
            this.check_AudioRecordingFileLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_AudioRecordingFileLoop.Location = new System.Drawing.Point(288, 91);
            this.check_AudioRecordingFileLoop.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_AudioRecordingFileLoop.Name = "check_AudioRecordingFileLoop";
            this.check_AudioRecordingFileLoop.Size = new System.Drawing.Size(50, 17);
            this.check_AudioRecordingFileLoop.TabIndex = 49;
            this.check_AudioRecordingFileLoop.Text = "Loop";
            this.check_AudioRecordingFileLoop.UseVisualStyleBackColor = true;
            // 
            // check_VideoRecordingFileLoop
            // 
            this.check_VideoRecordingFileLoop.AutoSize = true;
            this.check_VideoRecordingFileLoop.Checked = true;
            this.check_VideoRecordingFileLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_VideoRecordingFileLoop.Location = new System.Drawing.Point(287, 141);
            this.check_VideoRecordingFileLoop.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_VideoRecordingFileLoop.Name = "check_VideoRecordingFileLoop";
            this.check_VideoRecordingFileLoop.Size = new System.Drawing.Size(50, 17);
            this.check_VideoRecordingFileLoop.TabIndex = 50;
            this.check_VideoRecordingFileLoop.Text = "Loop";
            this.check_VideoRecordingFileLoop.UseVisualStyleBackColor = true;
            // 
            // check_VideoSecondaryRecordingFileLoop
            // 
            this.check_VideoSecondaryRecordingFileLoop.AutoSize = true;
            this.check_VideoSecondaryRecordingFileLoop.Checked = true;
            this.check_VideoSecondaryRecordingFileLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_VideoSecondaryRecordingFileLoop.Location = new System.Drawing.Point(287, 194);
            this.check_VideoSecondaryRecordingFileLoop.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_VideoSecondaryRecordingFileLoop.Name = "check_VideoSecondaryRecordingFileLoop";
            this.check_VideoSecondaryRecordingFileLoop.Size = new System.Drawing.Size(50, 17);
            this.check_VideoSecondaryRecordingFileLoop.TabIndex = 55;
            this.check_VideoSecondaryRecordingFileLoop.Text = "Loop";
            this.check_VideoSecondaryRecordingFileLoop.UseVisualStyleBackColor = true;
            // 
            // tb_VideoSecondaryRecordingFile
            // 
            this.tb_VideoSecondaryRecordingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_VideoSecondaryRecordingFile.Location = new System.Drawing.Point(288, 174);
            this.tb_VideoSecondaryRecordingFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_VideoSecondaryRecordingFile.Name = "tb_VideoSecondaryRecordingFile";
            this.tb_VideoSecondaryRecordingFile.Size = new System.Drawing.Size(204, 18);
            this.tb_VideoSecondaryRecordingFile.TabIndex = 54;
            this.tb_VideoSecondaryRecordingFile.Text = "http://5.48.50.194:8081/mjpg/video.mjpg";
            // 
            // btn_BrowseVideoSecondaryRecordingFile
            // 
            this.btn_BrowseVideoSecondaryRecordingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_BrowseVideoSecondaryRecordingFile.Location = new System.Drawing.Point(495, 173);
            this.btn_BrowseVideoSecondaryRecordingFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_BrowseVideoSecondaryRecordingFile.Name = "btn_BrowseVideoSecondaryRecordingFile";
            this.btn_BrowseVideoSecondaryRecordingFile.Size = new System.Drawing.Size(39, 20);
            this.btn_BrowseVideoSecondaryRecordingFile.TabIndex = 53;
            this.btn_BrowseVideoSecondaryRecordingFile.Text = "...";
            this.btn_BrowseVideoSecondaryRecordingFile.UseVisualStyleBackColor = true;
            this.btn_BrowseVideoSecondaryRecordingFile.Click += new System.EventHandler(this.btn_BrowseVideoSecondaryRecordingFile_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(12, 157);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(180, 13);
            this.label10.TabIndex = 52;
            this.label10.Text = "Video Secondary Input Device";
            // 
            // cb_VideoSecondaryRecordingDevices
            // 
            this.cb_VideoSecondaryRecordingDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_VideoSecondaryRecordingDevices.FormattingEnabled = true;
            this.cb_VideoSecondaryRecordingDevices.Location = new System.Drawing.Point(10, 173);
            this.cb_VideoSecondaryRecordingDevices.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_VideoSecondaryRecordingDevices.Name = "cb_VideoSecondaryRecordingDevices";
            this.cb_VideoSecondaryRecordingDevices.Size = new System.Drawing.Size(246, 21);
            this.cb_VideoSecondaryRecordingDevices.TabIndex = 51;
            this.cb_VideoSecondaryRecordingDevices.SelectedIndexChanged += new System.EventHandler(this.cb_VideoSecondaryRecordingDevices_SelectedIndexChanged);
            // 
            // picture_VideoRemote
            // 
            this.picture_VideoRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture_VideoRemote.Location = new System.Drawing.Point(541, 496);
            this.picture_VideoRemote.Name = "picture_VideoRemote";
            this.picture_VideoRemote.Size = new System.Drawing.Size(240, 135);
            this.picture_VideoRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture_VideoRemote.TabIndex = 56;
            this.picture_VideoRemote.TabStop = false;
            // 
            // picture_VideoLocal
            // 
            this.picture_VideoLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture_VideoLocal.Location = new System.Drawing.Point(10, 496);
            this.picture_VideoLocal.Name = "picture_VideoLocal";
            this.picture_VideoLocal.Size = new System.Drawing.Size(240, 135);
            this.picture_VideoLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture_VideoLocal.TabIndex = 57;
            this.picture_VideoLocal.TabStop = false;
            // 
            // picture_SharingRemote
            // 
            this.picture_SharingRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture_SharingRemote.Location = new System.Drawing.Point(791, 361);
            this.picture_SharingRemote.Name = "picture_SharingRemote";
            this.picture_SharingRemote.Size = new System.Drawing.Size(480, 270);
            this.picture_SharingRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture_SharingRemote.TabIndex = 58;
            this.picture_SharingRemote.TabStop = false;
            // 
            // picture_SharingLocal
            // 
            this.picture_SharingLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture_SharingLocal.Location = new System.Drawing.Point(256, 496);
            this.picture_SharingLocal.Name = "picture_SharingLocal";
            this.picture_SharingLocal.Size = new System.Drawing.Size(240, 135);
            this.picture_SharingLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture_SharingLocal.TabIndex = 61;
            this.picture_SharingLocal.TabStop = false;
            // 
            // cb_SubscribeVideoLocal
            // 
            this.cb_SubscribeVideoLocal.AutoSize = true;
            this.cb_SubscribeVideoLocal.Checked = true;
            this.cb_SubscribeVideoLocal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_SubscribeVideoLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_SubscribeVideoLocal.Location = new System.Drawing.Point(94, 479);
            this.cb_SubscribeVideoLocal.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_SubscribeVideoLocal.Name = "cb_SubscribeVideoLocal";
            this.cb_SubscribeVideoLocal.Size = new System.Drawing.Size(82, 17);
            this.cb_SubscribeVideoLocal.TabIndex = 64;
            this.cb_SubscribeVideoLocal.Text = "Subscribe";
            this.cb_SubscribeVideoLocal.UseVisualStyleBackColor = true;
            this.cb_SubscribeVideoLocal.CheckedChanged += new System.EventHandler(this.cb_SubscribeVideoLocal_CheckedChanged);
            // 
            // cb_SubscribeSharingLocal
            // 
            this.cb_SubscribeSharingLocal.AutoSize = true;
            this.cb_SubscribeSharingLocal.Checked = true;
            this.cb_SubscribeSharingLocal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_SubscribeSharingLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_SubscribeSharingLocal.Location = new System.Drawing.Point(352, 479);
            this.cb_SubscribeSharingLocal.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_SubscribeSharingLocal.Name = "cb_SubscribeSharingLocal";
            this.cb_SubscribeSharingLocal.Size = new System.Drawing.Size(82, 17);
            this.cb_SubscribeSharingLocal.TabIndex = 65;
            this.cb_SubscribeSharingLocal.Text = "Subscribe";
            this.cb_SubscribeSharingLocal.UseVisualStyleBackColor = true;
            this.cb_SubscribeSharingLocal.CheckedChanged += new System.EventHandler(this.cb_SubscribeSharingLocal_CheckedChanged);
            // 
            // cb_P2PSubscribeVideoRemote
            // 
            this.cb_P2PSubscribeVideoRemote.AutoSize = true;
            this.cb_P2PSubscribeVideoRemote.Checked = true;
            this.cb_P2PSubscribeVideoRemote.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_P2PSubscribeVideoRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_P2PSubscribeVideoRemote.Location = new System.Drawing.Point(637, 479);
            this.cb_P2PSubscribeVideoRemote.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_P2PSubscribeVideoRemote.Name = "cb_P2PSubscribeVideoRemote";
            this.cb_P2PSubscribeVideoRemote.Size = new System.Drawing.Size(82, 17);
            this.cb_P2PSubscribeVideoRemote.TabIndex = 66;
            this.cb_P2PSubscribeVideoRemote.Text = "Subscribe";
            this.cb_P2PSubscribeVideoRemote.UseVisualStyleBackColor = true;
            this.cb_P2PSubscribeVideoRemote.Visible = false;
            this.cb_P2PSubscribeVideoRemote.CheckedChanged += new System.EventHandler(this.cb_SubscribeVideoRemote_CheckedChanged);
            // 
            // cb_P2PSubscribeSharingRemote
            // 
            this.cb_P2PSubscribeSharingRemote.AutoSize = true;
            this.cb_P2PSubscribeSharingRemote.Checked = true;
            this.cb_P2PSubscribeSharingRemote.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_P2PSubscribeSharingRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_P2PSubscribeSharingRemote.Location = new System.Drawing.Point(898, 340);
            this.cb_P2PSubscribeSharingRemote.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_P2PSubscribeSharingRemote.Name = "cb_P2PSubscribeSharingRemote";
            this.cb_P2PSubscribeSharingRemote.Size = new System.Drawing.Size(82, 17);
            this.cb_P2PSubscribeSharingRemote.TabIndex = 67;
            this.cb_P2PSubscribeSharingRemote.Text = "Subscribe";
            this.cb_P2PSubscribeSharingRemote.UseVisualStyleBackColor = true;
            this.cb_P2PSubscribeSharingRemote.Visible = false;
            this.cb_P2PSubscribeSharingRemote.CheckedChanged += new System.EventHandler(this.cb_SubscribeSharingRemote_CheckedChanged);
            // 
            // btn_JoinConf
            // 
            this.btn_JoinConf.Location = new System.Drawing.Point(740, 96);
            this.btn_JoinConf.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_JoinConf.Name = "btn_JoinConf";
            this.btn_JoinConf.Size = new System.Drawing.Size(40, 23);
            this.btn_JoinConf.TabIndex = 68;
            this.btn_JoinConf.Text = "Join";
            this.btn_JoinConf.UseVisualStyleBackColor = true;
            this.btn_JoinConf.Click += new System.EventHandler(this.btn_JoinConf_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(542, 9);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(52, 13);
            this.label11.TabIndex = 70;
            this.label11.Text = "Bubbles";
            // 
            // cb_BubblesList
            // 
            this.cb_BubblesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_BubblesList.FormattingEnabled = true;
            this.cb_BubblesList.Location = new System.Drawing.Point(542, 25);
            this.cb_BubblesList.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_BubblesList.Name = "cb_BubblesList";
            this.cb_BubblesList.Size = new System.Drawing.Size(239, 21);
            this.cb_BubblesList.TabIndex = 69;
            // 
            // btn_StartConf
            // 
            this.btn_StartConf.Location = new System.Drawing.Point(712, 73);
            this.btn_StartConf.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_StartConf.Name = "btn_StartConf";
            this.btn_StartConf.Size = new System.Drawing.Size(68, 23);
            this.btn_StartConf.TabIndex = 71;
            this.btn_StartConf.Text = "Start Conf";
            this.btn_StartConf.UseVisualStyleBackColor = true;
            this.btn_StartConf.Click += new System.EventHandler(this.btn_StartConf_Click);
            // 
            // tb_BubbleInfo
            // 
            this.tb_BubbleInfo.Location = new System.Drawing.Point(541, 75);
            this.tb_BubbleInfo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_BubbleInfo.Name = "tb_BubbleInfo";
            this.tb_BubbleInfo.ReadOnly = true;
            this.tb_BubbleInfo.Size = new System.Drawing.Size(167, 20);
            this.tb_BubbleInfo.TabIndex = 72;
            // 
            // lb_VideoPublishers
            // 
            this.lb_VideoPublishers.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_VideoPublishers.FormattingEnabled = true;
            this.lb_VideoPublishers.ItemHeight = 12;
            this.lb_VideoPublishers.Location = new System.Drawing.Point(541, 330);
            this.lb_VideoPublishers.Name = "lb_VideoPublishers";
            this.lb_VideoPublishers.Size = new System.Drawing.Size(239, 112);
            this.lb_VideoPublishers.TabIndex = 76;
            this.lb_VideoPublishers.SelectedIndexChanged += new System.EventHandler(this.lb_Subscribers_SelectedIndexChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(542, 314);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(112, 13);
            this.label13.TabIndex = 77;
            this.label13.Text = "Video publisher(s):";
            // 
            // lb_Participants
            // 
            this.lb_Participants.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_Participants.FormattingEnabled = true;
            this.lb_Participants.ItemHeight = 12;
            this.lb_Participants.Location = new System.Drawing.Point(542, 141);
            this.lb_Participants.Name = "lb_Participants";
            this.lb_Participants.Size = new System.Drawing.Size(239, 112);
            this.lb_Participants.TabIndex = 78;
            this.lb_Participants.SelectedIndexChanged += new System.EventHandler(this.lb_Participants_SelectedIndexChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(542, 125);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(86, 13);
            this.label14.TabIndex = 79;
            this.label14.Text = "Participant(s):";
            // 
            // btn_SubscribeVideoMedia
            // 
            this.btn_SubscribeVideoMedia.Location = new System.Drawing.Point(541, 445);
            this.btn_SubscribeVideoMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_SubscribeVideoMedia.Name = "btn_SubscribeVideoMedia";
            this.btn_SubscribeVideoMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_SubscribeVideoMedia.TabIndex = 80;
            this.btn_SubscribeVideoMedia.Text = "Subscribe";
            this.btn_SubscribeVideoMedia.UseVisualStyleBackColor = true;
            this.btn_SubscribeVideoMedia.Click += new System.EventHandler(this.btn_SubscribeVideoMedia_Click);
            // 
            // btn_UnsubscribeVideoMedia
            // 
            this.btn_UnsubscribeVideoMedia.Location = new System.Drawing.Point(622, 445);
            this.btn_UnsubscribeVideoMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_UnsubscribeVideoMedia.Name = "btn_UnsubscribeVideoMedia";
            this.btn_UnsubscribeVideoMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_UnsubscribeVideoMedia.TabIndex = 81;
            this.btn_UnsubscribeVideoMedia.Text = "Unubscribe";
            this.btn_UnsubscribeVideoMedia.UseVisualStyleBackColor = true;
            this.btn_UnsubscribeVideoMedia.Click += new System.EventHandler(this.btn_UnubscribeVideoMedia_Click);
            // 
            // btn_UnmuteParticipant
            // 
            this.btn_UnmuteParticipant.Location = new System.Drawing.Point(588, 256);
            this.btn_UnmuteParticipant.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_UnmuteParticipant.Name = "btn_UnmuteParticipant";
            this.btn_UnmuteParticipant.Size = new System.Drawing.Size(63, 23);
            this.btn_UnmuteParticipant.TabIndex = 83;
            this.btn_UnmuteParticipant.Text = "Unmute";
            this.btn_UnmuteParticipant.UseVisualStyleBackColor = true;
            this.btn_UnmuteParticipant.Click += new System.EventHandler(this.btn_UnmuteParticipant_Click);
            // 
            // btn_MuteParticipant
            // 
            this.btn_MuteParticipant.Location = new System.Drawing.Point(542, 256);
            this.btn_MuteParticipant.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_MuteParticipant.Name = "btn_MuteParticipant";
            this.btn_MuteParticipant.Size = new System.Drawing.Size(42, 23);
            this.btn_MuteParticipant.TabIndex = 82;
            this.btn_MuteParticipant.Text = "Mute";
            this.btn_MuteParticipant.UseVisualStyleBackColor = true;
            this.btn_MuteParticipant.Click += new System.EventHandler(this.btn_MuteParticipant_Click);
            // 
            // btn_ViewVideoMedia
            // 
            this.btn_ViewVideoMedia.Location = new System.Drawing.Point(703, 445);
            this.btn_ViewVideoMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_ViewVideoMedia.Name = "btn_ViewVideoMedia";
            this.btn_ViewVideoMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_ViewVideoMedia.TabIndex = 84;
            this.btn_ViewVideoMedia.Text = "View";
            this.btn_ViewVideoMedia.UseVisualStyleBackColor = true;
            this.btn_ViewVideoMedia.Click += new System.EventHandler(this.btn_ViewVideoMedia_Click);
            // 
            // btn_DelegateParticipant
            // 
            this.btn_DelegateParticipant.Location = new System.Drawing.Point(718, 256);
            this.btn_DelegateParticipant.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_DelegateParticipant.Name = "btn_DelegateParticipant";
            this.btn_DelegateParticipant.Size = new System.Drawing.Size(63, 23);
            this.btn_DelegateParticipant.TabIndex = 85;
            this.btn_DelegateParticipant.Text = "Delegate";
            this.btn_DelegateParticipant.UseVisualStyleBackColor = true;
            this.btn_DelegateParticipant.Click += new System.EventHandler(this.btn_DelegateParticipant_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(539, 292);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(83, 13);
            this.label15.TabIndex = 87;
            this.label15.Text = "Active talker:";
            // 
            // tb_ActiveSpeaker
            // 
            this.tb_ActiveSpeaker.Location = new System.Drawing.Point(622, 289);
            this.tb_ActiveSpeaker.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_ActiveSpeaker.Name = "tb_ActiveSpeaker";
            this.tb_ActiveSpeaker.ReadOnly = true;
            this.tb_ActiveSpeaker.Size = new System.Drawing.Size(159, 20);
            this.tb_ActiveSpeaker.TabIndex = 86;
            // 
            // tb_ConferenceName
            // 
            this.tb_ConferenceName.Location = new System.Drawing.Point(616, 98);
            this.tb_ConferenceName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_ConferenceName.Name = "tb_ConferenceName";
            this.tb_ConferenceName.ReadOnly = true;
            this.tb_ConferenceName.Size = new System.Drawing.Size(120, 20);
            this.tb_ConferenceName.TabIndex = 89;
            // 
            // tb_VideoLocalInfo
            // 
            this.tb_VideoLocalInfo.Location = new System.Drawing.Point(10, 637);
            this.tb_VideoLocalInfo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_VideoLocalInfo.Name = "tb_VideoLocalInfo";
            this.tb_VideoLocalInfo.ReadOnly = true;
            this.tb_VideoLocalInfo.Size = new System.Drawing.Size(240, 20);
            this.tb_VideoLocalInfo.TabIndex = 90;
            // 
            // tb_SharingLocalInfo
            // 
            this.tb_SharingLocalInfo.Location = new System.Drawing.Point(256, 637);
            this.tb_SharingLocalInfo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_SharingLocalInfo.Name = "tb_SharingLocalInfo";
            this.tb_SharingLocalInfo.ReadOnly = true;
            this.tb_SharingLocalInfo.Size = new System.Drawing.Size(240, 20);
            this.tb_SharingLocalInfo.TabIndex = 91;
            // 
            // tb_VideoRemoteInfo
            // 
            this.tb_VideoRemoteInfo.Location = new System.Drawing.Point(540, 637);
            this.tb_VideoRemoteInfo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_VideoRemoteInfo.Name = "tb_VideoRemoteInfo";
            this.tb_VideoRemoteInfo.ReadOnly = true;
            this.tb_VideoRemoteInfo.Size = new System.Drawing.Size(240, 20);
            this.tb_VideoRemoteInfo.TabIndex = 92;
            // 
            // tb_SharingRemoteInfo
            // 
            this.tb_SharingRemoteInfo.Location = new System.Drawing.Point(791, 637);
            this.tb_SharingRemoteInfo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_SharingRemoteInfo.Name = "tb_SharingRemoteInfo";
            this.tb_SharingRemoteInfo.ReadOnly = true;
            this.tb_SharingRemoteInfo.Size = new System.Drawing.Size(480, 20);
            this.tb_SharingRemoteInfo.TabIndex = 93;
            // 
            // cb_AutoSubscriptionSharing
            // 
            this.cb_AutoSubscriptionSharing.AutoSize = true;
            this.cb_AutoSubscriptionSharing.Checked = true;
            this.cb_AutoSubscriptionSharing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_AutoSubscriptionSharing.Location = new System.Drawing.Point(611, 52);
            this.cb_AutoSubscriptionSharing.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AutoSubscriptionSharing.Name = "cb_AutoSubscriptionSharing";
            this.cb_AutoSubscriptionSharing.Size = new System.Drawing.Size(62, 17);
            this.cb_AutoSubscriptionSharing.TabIndex = 94;
            this.cb_AutoSubscriptionSharing.Text = "Sharing";
            this.cb_AutoSubscriptionSharing.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(544, 101);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(74, 13);
            this.label12.TabIndex = 96;
            this.label12.Text = "In progress:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(542, 53);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(65, 13);
            this.label16.TabIndex = 97;
            this.label16.Text = "Auto sub.:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(793, 341);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(101, 13);
            this.label17.TabIndex = 98;
            this.label17.Text = "Remote Sharing:";
            // 
            // tb_RemoteSharingInfo
            // 
            this.tb_RemoteSharingInfo.Location = new System.Drawing.Point(898, 338);
            this.tb_RemoteSharingInfo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_RemoteSharingInfo.Name = "tb_RemoteSharingInfo";
            this.tb_RemoteSharingInfo.ReadOnly = true;
            this.tb_RemoteSharingInfo.Size = new System.Drawing.Size(211, 20);
            this.tb_RemoteSharingInfo.TabIndex = 99;
            // 
            // btn_UnsubscribeSharingMedia
            // 
            this.btn_UnsubscribeSharingMedia.Location = new System.Drawing.Point(1194, 336);
            this.btn_UnsubscribeSharingMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_UnsubscribeSharingMedia.Name = "btn_UnsubscribeSharingMedia";
            this.btn_UnsubscribeSharingMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_UnsubscribeSharingMedia.TabIndex = 101;
            this.btn_UnsubscribeSharingMedia.Text = "Unubscribe";
            this.btn_UnsubscribeSharingMedia.UseVisualStyleBackColor = true;
            this.btn_UnsubscribeSharingMedia.Click += new System.EventHandler(this.btn_UnsubscribeSharingMedia_Click);
            // 
            // btn_SubscribeSharingMedia
            // 
            this.btn_SubscribeSharingMedia.Location = new System.Drawing.Point(1113, 336);
            this.btn_SubscribeSharingMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_SubscribeSharingMedia.Name = "btn_SubscribeSharingMedia";
            this.btn_SubscribeSharingMedia.Size = new System.Drawing.Size(77, 23);
            this.btn_SubscribeSharingMedia.TabIndex = 100;
            this.btn_SubscribeSharingMedia.Text = "Subscribe";
            this.btn_SubscribeSharingMedia.UseVisualStyleBackColor = true;
            this.btn_SubscribeSharingMedia.Click += new System.EventHandler(this.btn_SubscribeSharingMedia_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(12, 480);
            this.label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(78, 13);
            this.label18.TabIndex = 102;
            this.label18.Text = "Local Video:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(259, 480);
            this.label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(89, 13);
            this.label19.TabIndex = 103;
            this.label19.Text = "Local Sharing:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(543, 480);
            this.label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(90, 13);
            this.label20.TabIndex = 104;
            this.label20.Text = "Remote Video:";
            // 
            // tb_MaxSubscriptionVideo
            // 
            this.tb_MaxSubscriptionVideo.Location = new System.Drawing.Point(755, 50);
            this.tb_MaxSubscriptionVideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_MaxSubscriptionVideo.Name = "tb_MaxSubscriptionVideo";
            this.tb_MaxSubscriptionVideo.Size = new System.Drawing.Size(25, 20);
            this.tb_MaxSubscriptionVideo.TabIndex = 105;
            this.tb_MaxSubscriptionVideo.Text = "2";
            // 
            // cb_AutoSubscriptionVideo
            // 
            this.cb_AutoSubscriptionVideo.AutoSize = true;
            this.cb_AutoSubscriptionVideo.Checked = true;
            this.cb_AutoSubscriptionVideo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_AutoSubscriptionVideo.Location = new System.Drawing.Point(673, 52);
            this.cb_AutoSubscriptionVideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AutoSubscriptionVideo.Name = "cb_AutoSubscriptionVideo";
            this.cb_AutoSubscriptionVideo.Size = new System.Drawing.Size(79, 17);
            this.cb_AutoSubscriptionVideo.TabIndex = 107;
            this.cb_AutoSubscriptionVideo.Text = "Video-Max.";
            this.cb_AutoSubscriptionVideo.UseVisualStyleBackColor = true;
            // 
            // btn_AskToShare
            // 
            this.btn_AskToShare.Enabled = false;
            this.btn_AskToShare.Location = new System.Drawing.Point(220, 369);
            this.btn_AskToShare.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AskToShare.Name = "btn_AskToShare";
            this.btn_AskToShare.Size = new System.Drawing.Size(81, 23);
            this.btn_AskToShare.TabIndex = 108;
            this.btn_AskToShare.Text = "Ask to Share";
            this.btn_AskToShare.UseVisualStyleBackColor = true;
            this.btn_AskToShare.Click += new System.EventHandler(this.btn_AskToShare_Click);
            // 
            // btn_DropParticipant
            // 
            this.btn_DropParticipant.Location = new System.Drawing.Point(653, 256);
            this.btn_DropParticipant.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_DropParticipant.Name = "btn_DropParticipant";
            this.btn_DropParticipant.Size = new System.Drawing.Size(63, 23);
            this.btn_DropParticipant.TabIndex = 109;
            this.btn_DropParticipant.Text = "Drop";
            this.btn_DropParticipant.UseVisualStyleBackColor = true;
            this.btn_DropParticipant.Click += new System.EventHandler(this.btn_DropParticipant_Click);
            // 
            // btn_AudioRecordingDevicesRefresh
            // 
            this.btn_AudioRecordingDevicesRefresh.FlatAppearance.BorderSize = 0;
            this.btn_AudioRecordingDevicesRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_AudioRecordingDevicesRefresh.Image = ((System.Drawing.Image)(resources.GetObject("Refresh.Image")));
            this.btn_AudioRecordingDevicesRefresh.Location = new System.Drawing.Point(256, 68);
            this.btn_AudioRecordingDevicesRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btn_AudioRecordingDevicesRefresh.Name = "btn_AudioRecordingDevicesRefresh";
            this.btn_AudioRecordingDevicesRefresh.Size = new System.Drawing.Size(23, 23);
            this.btn_AudioRecordingDevicesRefresh.TabIndex = 110;
            this.btn_AudioRecordingDevicesRefresh.UseVisualStyleBackColor = true;
            this.btn_AudioRecordingDevicesRefresh.Click += new System.EventHandler(this.btn_AudioRecordingDevicesRefresh_Click);
            // 
            // btn_AudioPlaybackDevicesRefresh
            // 
            this.btn_AudioPlaybackDevicesRefresh.FlatAppearance.BorderSize = 0;
            this.btn_AudioPlaybackDevicesRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_AudioPlaybackDevicesRefresh.Image = ((System.Drawing.Image)(resources.GetObject("Refresh.Image")));
            this.btn_AudioPlaybackDevicesRefresh.Location = new System.Drawing.Point(257, 24);
            this.btn_AudioPlaybackDevicesRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btn_AudioPlaybackDevicesRefresh.Name = "btn_AudioPlaybackDevicesRefresh";
            this.btn_AudioPlaybackDevicesRefresh.Size = new System.Drawing.Size(23, 23);
            this.btn_AudioPlaybackDevicesRefresh.TabIndex = 111;
            this.btn_AudioPlaybackDevicesRefresh.UseVisualStyleBackColor = true;
            this.btn_AudioPlaybackDevicesRefresh.Click += new System.EventHandler(this.btn_AudioPlaybackDevicesRefresh_Click);
            // 
            // btn_VideoRecordingDevicesRefresh
            // 
            this.btn_VideoRecordingDevicesRefresh.FlatAppearance.BorderSize = 0;
            this.btn_VideoRecordingDevicesRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_VideoRecordingDevicesRefresh.Image = ((System.Drawing.Image)(resources.GetObject("Refresh.Image")));
            this.btn_VideoRecordingDevicesRefresh.Location = new System.Drawing.Point(257, 119);
            this.btn_VideoRecordingDevicesRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btn_VideoRecordingDevicesRefresh.Name = "btn_VideoRecordingDevicesRefresh";
            this.btn_VideoRecordingDevicesRefresh.Size = new System.Drawing.Size(23, 23);
            this.btn_VideoRecordingDevicesRefresh.TabIndex = 112;
            this.btn_VideoRecordingDevicesRefresh.UseVisualStyleBackColor = true;
            this.btn_VideoRecordingDevicesRefresh.Click += new System.EventHandler(this.btn_VideoRecordingDevicesRefresh_Click);
            // 
            // btn_VideoSecondaryRecordingDevicesRefresh
            // 
            this.btn_VideoSecondaryRecordingDevicesRefresh.FlatAppearance.BorderSize = 0;
            this.btn_VideoSecondaryRecordingDevicesRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_VideoSecondaryRecordingDevicesRefresh.Image = ((System.Drawing.Image)(resources.GetObject("Refresh.Image")));
            this.btn_VideoSecondaryRecordingDevicesRefresh.Location = new System.Drawing.Point(257, 172);
            this.btn_VideoSecondaryRecordingDevicesRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btn_VideoSecondaryRecordingDevicesRefresh.Name = "btn_VideoSecondaryRecordingDevicesRefresh";
            this.btn_VideoSecondaryRecordingDevicesRefresh.Size = new System.Drawing.Size(23, 23);
            this.btn_VideoSecondaryRecordingDevicesRefresh.TabIndex = 113;
            this.btn_VideoSecondaryRecordingDevicesRefresh.UseVisualStyleBackColor = true;
            this.btn_VideoSecondaryRecordingDevicesRefresh.Click += new System.EventHandler(this.btn_VideoSecondaryRecordingDevicesRefresh_Click);
            // 
            // WebRTCForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1279, 662);
            this.Controls.Add(this.btn_VideoSecondaryRecordingDevicesRefresh);
            this.Controls.Add(this.btn_VideoRecordingDevicesRefresh);
            this.Controls.Add(this.btn_AudioPlaybackDevicesRefresh);
            this.Controls.Add(this.btn_AudioRecordingDevicesRefresh);
            this.Controls.Add(this.btn_DropParticipant);
            this.Controls.Add(this.picture_VideoLocal);
            this.Controls.Add(this.btn_AskToShare);
            this.Controls.Add(this.cb_AutoSubscriptionVideo);
            this.Controls.Add(this.tb_MaxSubscriptionVideo);
            this.Controls.Add(this.cb_AutoSubscriptionSharing);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.btn_UnsubscribeSharingMedia);
            this.Controls.Add(this.btn_SubscribeSharingMedia);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.tb_ConferenceName);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tb_SharingRemoteInfo);
            this.Controls.Add(this.tb_VideoRemoteInfo);
            this.Controls.Add(this.tb_SharingLocalInfo);
            this.Controls.Add(this.tb_VideoLocalInfo);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.tb_ActiveSpeaker);
            this.Controls.Add(this.btn_DelegateParticipant);
            this.Controls.Add(this.btn_ViewVideoMedia);
            this.Controls.Add(this.btn_UnmuteParticipant);
            this.Controls.Add(this.btn_MuteParticipant);
            this.Controls.Add(this.btn_UnsubscribeVideoMedia);
            this.Controls.Add(this.btn_SubscribeVideoMedia);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.lb_Participants);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.lb_VideoPublishers);
            this.Controls.Add(this.tb_BubbleInfo);
            this.Controls.Add(this.btn_StartConf);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.cb_BubblesList);
            this.Controls.Add(this.btn_JoinConf);
            this.Controls.Add(this.cb_P2PSubscribeSharingRemote);
            this.Controls.Add(this.cb_P2PSubscribeVideoRemote);
            this.Controls.Add(this.cb_SubscribeSharingLocal);
            this.Controls.Add(this.cb_SubscribeVideoLocal);
            this.Controls.Add(this.picture_SharingLocal);
            this.Controls.Add(this.picture_SharingRemote);
            this.Controls.Add(this.picture_VideoRemote);
            this.Controls.Add(this.check_VideoSecondaryRecordingFileLoop);
            this.Controls.Add(this.tb_VideoSecondaryRecordingFile);
            this.Controls.Add(this.btn_BrowseVideoSecondaryRecordingFile);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cb_VideoSecondaryRecordingDevices);
            this.Controls.Add(this.check_VideoRecordingFileLoop);
            this.Controls.Add(this.check_AudioRecordingFileLoop);
            this.Controls.Add(this.check_LocalSharingMuted);
            this.Controls.Add(this.check_LocalVideoMuted);
            this.Controls.Add(this.btn_UnmuteMedia);
            this.Controls.Add(this.cb_UnmuteMedia);
            this.Controls.Add(this.check_VideoRecordingFileWithAudio);
            this.Controls.Add(this.tb_VideoRecordingFile);
            this.Controls.Add(this.btn_BrowseVideoRecordingFile);
            this.Controls.Add(this.tb_AudioRecordingFile);
            this.Controls.Add(this.btn_BrowseAudioRecordingFile);
            this.Controls.Add(this.btn_MuteMedia);
            this.Controls.Add(this.cb_MuteMedia);
            this.Controls.Add(this.check_LocalAudioMuted);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tb_Information);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tb_Subject);
            this.Controls.Add(this.btn_RemoveMedia);
            this.Controls.Add(this.cb_RemoveMedia);
            this.Controls.Add(this.btn_AddMedia);
            this.Controls.Add(this.cb_AddMedia);
            this.Controls.Add(this.btn_AnswerCall);
            this.Controls.Add(this.cb_AnswerCallMedias);
            this.Controls.Add(this.btn_Decline);
            this.Controls.Add(this.btn_HangUp);
            this.Controls.Add(this.btn_MakeCall);
            this.Controls.Add(this.cb_MakeCallMedias);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.check_RemoteSharing);
            this.Controls.Add(this.check_RemoteVideo);
            this.Controls.Add(this.check_RemoteAudio);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.check_LocalSharing);
            this.Controls.Add(this.check_LocalVideo);
            this.Controls.Add(this.check_LocalAudio);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cb_VideoRecordingDevices);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cb_ContactsList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cb_AudioRecordingDevices);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_AudioSecondaryPlaybackDevices);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_AudioPlaybackDevices);
            this.Controls.Add(this.tb_RemoteSharingInfo);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "WebRTCForm";
            this.Text = "WebRTCForm";
            ((System.ComponentModel.ISupportInitialize)(this.picture_VideoRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_VideoLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_SharingRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_SharingLocal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb_AudioSecondaryPlaybackDevices;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_AudioRecordingDevices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cb_ContactsList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cb_VideoRecordingDevices;
        private System.Windows.Forms.CheckBox check_LocalVideo;
        private System.Windows.Forms.CheckBox check_LocalSharing;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox check_RemoteSharing;
        private System.Windows.Forms.CheckBox check_RemoteVideo;
        private System.Windows.Forms.CheckBox check_RemoteAudio;
        private System.Windows.Forms.ComboBox cb_MakeCallMedias;
        private System.Windows.Forms.Button btn_MakeCall;
        private System.Windows.Forms.Button btn_HangUp;
        private System.Windows.Forms.Button btn_Decline;
        private System.Windows.Forms.Button btn_AnswerCall;
        private System.Windows.Forms.ComboBox cb_AnswerCallMedias;
        private System.Windows.Forms.Button btn_AddMedia;
        private System.Windows.Forms.ComboBox cb_AddMedia;
        private System.Windows.Forms.Button btn_RemoveMedia;
        private System.Windows.Forms.ComboBox cb_RemoveMedia;
        private System.Windows.Forms.TextBox tb_Subject;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_Information;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox check_LocalAudioMuted;
        private System.Windows.Forms.Button btn_MuteMedia;
        private System.Windows.Forms.ComboBox cb_MuteMedia;
        private System.Windows.Forms.Button btn_BrowseAudioRecordingFile;
        private System.Windows.Forms.TextBox tb_AudioRecordingFile;
        private System.Windows.Forms.TextBox tb_VideoRecordingFile;
        private System.Windows.Forms.Button btn_BrowseVideoRecordingFile;
        private System.Windows.Forms.CheckBox check_LocalAudio;
        private System.Windows.Forms.CheckBox check_VideoRecordingFileWithAudio;
        private System.Windows.Forms.Button btn_UnmuteMedia;
        private System.Windows.Forms.ComboBox cb_UnmuteMedia;
        private System.Windows.Forms.CheckBox check_LocalVideoMuted;
        private System.Windows.Forms.CheckBox check_LocalSharingMuted;
        private System.Windows.Forms.CheckBox check_AudioRecordingFileLoop;
        private System.Windows.Forms.CheckBox check_VideoRecordingFileLoop;
        private System.Windows.Forms.CheckBox check_VideoSecondaryRecordingFileLoop;
        private System.Windows.Forms.TextBox tb_VideoSecondaryRecordingFile;
        private System.Windows.Forms.Button btn_BrowseVideoSecondaryRecordingFile;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cb_VideoSecondaryRecordingDevices;
        private System.Windows.Forms.PictureBox picture_VideoRemote;
        private System.Windows.Forms.PictureBox picture_VideoLocal;
        private System.Windows.Forms.PictureBox picture_SharingRemote;
        private System.Windows.Forms.PictureBox picture_SharingLocal;
        private System.Windows.Forms.CheckBox cb_SubscribeVideoLocal;
        private System.Windows.Forms.CheckBox cb_SubscribeSharingLocal;
        private System.Windows.Forms.CheckBox cb_P2PSubscribeVideoRemote;
        private System.Windows.Forms.CheckBox cb_P2PSubscribeSharingRemote;
        private System.Windows.Forms.Button btn_JoinConf;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cb_BubblesList;
        private System.Windows.Forms.Button btn_StartConf;
        private System.Windows.Forms.TextBox tb_BubbleInfo;
        private System.Windows.Forms.ListBox lb_VideoPublishers;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ListBox lb_Participants;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btn_SubscribeVideoMedia;
        private System.Windows.Forms.Button btn_UnsubscribeVideoMedia;
        private System.Windows.Forms.Button btn_UnmuteParticipant;
        private System.Windows.Forms.Button btn_MuteParticipant;
        private System.Windows.Forms.Button btn_ViewVideoMedia;
        private System.Windows.Forms.Button btn_DelegateParticipant;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tb_ActiveSpeaker;
        private System.Windows.Forms.TextBox tb_ConferenceName;
        private System.Windows.Forms.TextBox tb_VideoLocalInfo;
        private System.Windows.Forms.TextBox tb_SharingLocalInfo;
        private System.Windows.Forms.TextBox tb_VideoRemoteInfo;
        private System.Windows.Forms.TextBox tb_SharingRemoteInfo;
        private System.Windows.Forms.CheckBox cb_AutoSubscriptionSharing;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tb_RemoteSharingInfo;
        private System.Windows.Forms.Button btn_UnsubscribeSharingMedia;
        private System.Windows.Forms.Button btn_SubscribeSharingMedia;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox tb_MaxSubscriptionVideo;
        private System.Windows.Forms.CheckBox cb_AutoSubscriptionVideo;
        private System.Windows.Forms.Button btn_AskToShare;
        private System.Windows.Forms.Button btn_DropParticipant;
        private System.Windows.Forms.Button btn_AudioRecordingDevicesRefresh;
        private System.Windows.Forms.Button btn_AudioPlaybackDevicesRefresh;
        private System.Windows.Forms.Button btn_VideoRecordingDevicesRefresh;
        private System.Windows.Forms.Button btn_VideoSecondaryRecordingDevicesRefresh;
        private System.Windows.Forms.ComboBox cb_AudioPlaybackDevices;
    }
}