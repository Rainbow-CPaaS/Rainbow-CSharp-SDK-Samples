using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    partial class FormWebRTC
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
            this.cb_AudioOutputs = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_AudioInputs = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_VideoInputs = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_SharingInputs = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.check_RemoteSharing = new System.Windows.Forms.CheckBox();
            this.check_RemoteVideo = new System.Windows.Forms.CheckBox();
            this.check_RemoteAudio = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.check_LocalSharing = new System.Windows.Forms.CheckBox();
            this.check_LocalVideo = new System.Windows.Forms.CheckBox();
            this.check_LocalAudio = new System.Windows.Forms.CheckBox();
            this.btn_AskToShare = new System.Windows.Forms.Button();
            this.btn_UnmuteMedia = new System.Windows.Forms.Button();
            this.btn_MuteMedia = new System.Windows.Forms.Button();
            this.cb_MuteMedia = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_Subject = new System.Windows.Forms.TextBox();
            this.btn_RemoveMedia = new System.Windows.Forms.Button();
            this.btn_AddMedia = new System.Windows.Forms.Button();
            this.btn_DeclineCall = new System.Windows.Forms.Button();
            this.btn_HangUp = new System.Windows.Forms.Button();
            this.btn_MakeCall = new System.Windows.Forms.Button();
            this.cb_MakeCallMedias = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_ContactsList = new System.Windows.Forms.ComboBox();
            this.lbl_ConferencesInProgress = new System.Windows.Forms.Label();
            this.btn_StartConf = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.cb_BubblesList = new System.Windows.Forms.ComboBox();
            this.btn_JoinConf = new System.Windows.Forms.Button();
            this.btn_ManageInputStreams = new System.Windows.Forms.Button();
            this.btn_RefreshAudioOutput = new System.Windows.Forms.Button();
            this.btn_RefreshAudioInput = new System.Windows.Forms.Button();
            this.btn_LoadConfig = new System.Windows.Forms.Button();
            this.btn_OutputVideoInput = new System.Windows.Forms.Button();
            this.btn_OutputSharingInput = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_Information = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.lbl_IncomingCall = new System.Windows.Forms.Label();
            this.lbl_ConversationDetails = new System.Windows.Forms.Label();
            this.tb_ConferenceName = new System.Windows.Forms.TextBox();
            this.lbl_BubbleInfo = new System.Windows.Forms.Label();
            this.btn_AnswerCall = new System.Windows.Forms.Button();
            this.cb_AnswerCallMedias = new System.Windows.Forms.ComboBox();
            this.cb_RemoveMedia = new System.Windows.Forms.ComboBox();
            this.cb_AddMedia = new System.Windows.Forms.ComboBox();
            this.cb_UnmuteMedia = new System.Windows.Forms.ComboBox();
            this.btn_OutputRemoteVideoInput = new System.Windows.Forms.Button();
            this.btn_OutputRemoteSharingInput = new System.Windows.Forms.Button();
            this.btn_OutputLocalVideoInput = new System.Windows.Forms.Button();
            this.btn_OutputLocalSharingInput = new System.Windows.Forms.Button();
            this.btn_StopVideoInput = new System.Windows.Forms.Button();
            this.btn_StartVideoInput = new System.Windows.Forms.Button();
            this.btn_StopSharingInput = new System.Windows.Forms.Button();
            this.btn_StartSharingInput = new System.Windows.Forms.Button();
            this.btn_StopAudioInput = new System.Windows.Forms.Button();
            this.btn_StartAudioInput = new System.Windows.Forms.Button();
            this.btn_DeclineConf = new System.Windows.Forms.Button();
            this.btn_ConferenceOptions = new System.Windows.Forms.Button();
            this.lbl_ConversationInProgress = new System.Windows.Forms.Label();
            this.cb_ConferencesName = new System.Windows.Forms.ComboBox();
            this.btn_AddAsParticipant = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cb_AudioOutputs
            // 
            this.cb_AudioOutputs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AudioOutputs.FormattingEnabled = true;
            this.cb_AudioOutputs.Location = new System.Drawing.Point(101, 150);
            this.cb_AudioOutputs.Name = "cb_AudioOutputs";
            this.cb_AudioOutputs.Size = new System.Drawing.Size(260, 23);
            this.cb_AudioOutputs.TabIndex = 138;
            this.cb_AudioOutputs.SelectedIndexChanged += new System.EventHandler(this.cb_AudioOutputList_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 15);
            this.label4.TabIndex = 137;
            this.label4.Text = "Audio Output:";
            // 
            // cb_AudioInputs
            // 
            this.cb_AudioInputs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AudioInputs.FormattingEnabled = true;
            this.cb_AudioInputs.Location = new System.Drawing.Point(101, 121);
            this.cb_AudioInputs.Name = "cb_AudioInputs";
            this.cb_AudioInputs.Size = new System.Drawing.Size(260, 23);
            this.cb_AudioInputs.TabIndex = 140;
            this.cb_AudioInputs.SelectedIndexChanged += new System.EventHandler(this.cb_AudioInputList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 15);
            this.label1.TabIndex = 139;
            this.label1.Text = "Audio Input:";
            // 
            // cb_VideoInputs
            // 
            this.cb_VideoInputs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_VideoInputs.FormattingEnabled = true;
            this.cb_VideoInputs.Location = new System.Drawing.Point(101, 55);
            this.cb_VideoInputs.Name = "cb_VideoInputs";
            this.cb_VideoInputs.Size = new System.Drawing.Size(260, 23);
            this.cb_VideoInputs.TabIndex = 144;
            this.cb_VideoInputs.SelectedIndexChanged += new System.EventHandler(this.cb_VideoInputs_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 15);
            this.label2.TabIndex = 143;
            this.label2.Text = "Video Input:";
            // 
            // cb_SharingInputs
            // 
            this.cb_SharingInputs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_SharingInputs.FormattingEnabled = true;
            this.cb_SharingInputs.Location = new System.Drawing.Point(101, 84);
            this.cb_SharingInputs.Name = "cb_SharingInputs";
            this.cb_SharingInputs.Size = new System.Drawing.Size(260, 23);
            this.cb_SharingInputs.TabIndex = 147;
            this.cb_SharingInputs.SelectedIndexChanged += new System.EventHandler(this.cb_SharingInputs_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 15);
            this.label3.TabIndex = 146;
            this.label3.Text = "Sharing Input:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(153, 418);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(141, 13);
            this.label7.TabIndex = 156;
            this.label7.Text = "Medias used by remote:";
            // 
            // check_RemoteSharing
            // 
            this.check_RemoteSharing.AutoCheck = false;
            this.check_RemoteSharing.AutoSize = true;
            this.check_RemoteSharing.Location = new System.Drawing.Point(174, 481);
            this.check_RemoteSharing.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_RemoteSharing.Name = "check_RemoteSharing";
            this.check_RemoteSharing.Size = new System.Drawing.Size(66, 19);
            this.check_RemoteSharing.TabIndex = 155;
            this.check_RemoteSharing.Text = "Sharing";
            this.check_RemoteSharing.UseVisualStyleBackColor = true;
            // 
            // check_RemoteVideo
            // 
            this.check_RemoteVideo.AutoCheck = false;
            this.check_RemoteVideo.AutoSize = true;
            this.check_RemoteVideo.Location = new System.Drawing.Point(174, 458);
            this.check_RemoteVideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_RemoteVideo.Name = "check_RemoteVideo";
            this.check_RemoteVideo.Size = new System.Drawing.Size(56, 19);
            this.check_RemoteVideo.TabIndex = 154;
            this.check_RemoteVideo.Text = "Video";
            this.check_RemoteVideo.UseVisualStyleBackColor = true;
            // 
            // check_RemoteAudio
            // 
            this.check_RemoteAudio.AutoCheck = false;
            this.check_RemoteAudio.AutoSize = true;
            this.check_RemoteAudio.Location = new System.Drawing.Point(174, 434);
            this.check_RemoteAudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_RemoteAudio.Name = "check_RemoteAudio";
            this.check_RemoteAudio.Size = new System.Drawing.Size(58, 19);
            this.check_RemoteAudio.TabIndex = 153;
            this.check_RemoteAudio.Text = "Audio";
            this.check_RemoteAudio.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(24, 418);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 152;
            this.label6.Text = "Medias used:";
            // 
            // check_LocalSharing
            // 
            this.check_LocalSharing.AutoCheck = false;
            this.check_LocalSharing.AutoSize = true;
            this.check_LocalSharing.Location = new System.Drawing.Point(45, 481);
            this.check_LocalSharing.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalSharing.Name = "check_LocalSharing";
            this.check_LocalSharing.Size = new System.Drawing.Size(66, 19);
            this.check_LocalSharing.TabIndex = 151;
            this.check_LocalSharing.Text = "Sharing";
            this.check_LocalSharing.UseVisualStyleBackColor = true;
            // 
            // check_LocalVideo
            // 
            this.check_LocalVideo.AutoCheck = false;
            this.check_LocalVideo.AutoSize = true;
            this.check_LocalVideo.Location = new System.Drawing.Point(45, 458);
            this.check_LocalVideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalVideo.Name = "check_LocalVideo";
            this.check_LocalVideo.Size = new System.Drawing.Size(56, 19);
            this.check_LocalVideo.TabIndex = 150;
            this.check_LocalVideo.Text = "Video";
            this.check_LocalVideo.UseVisualStyleBackColor = true;
            // 
            // check_LocalAudio
            // 
            this.check_LocalAudio.AutoCheck = false;
            this.check_LocalAudio.AutoSize = true;
            this.check_LocalAudio.Location = new System.Drawing.Point(45, 434);
            this.check_LocalAudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.check_LocalAudio.Name = "check_LocalAudio";
            this.check_LocalAudio.Size = new System.Drawing.Size(58, 19);
            this.check_LocalAudio.TabIndex = 149;
            this.check_LocalAudio.Text = "Audio";
            this.check_LocalAudio.UseVisualStyleBackColor = true;
            // 
            // btn_AskToShare
            // 
            this.btn_AskToShare.Enabled = false;
            this.btn_AskToShare.Location = new System.Drawing.Point(237, 304);
            this.btn_AskToShare.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AskToShare.Name = "btn_AskToShare";
            this.btn_AskToShare.Size = new System.Drawing.Size(81, 23);
            this.btn_AskToShare.TabIndex = 178;
            this.btn_AskToShare.Text = "Ask to Share";
            this.btn_AskToShare.UseVisualStyleBackColor = true;
            this.btn_AskToShare.Click += new System.EventHandler(this.btn_AskToShare_Click);
            // 
            // btn_UnmuteMedia
            // 
            this.btn_UnmuteMedia.Location = new System.Drawing.Point(152, 376);
            this.btn_UnmuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_UnmuteMedia.Name = "btn_UnmuteMedia";
            this.btn_UnmuteMedia.Size = new System.Drawing.Size(81, 23);
            this.btn_UnmuteMedia.TabIndex = 177;
            this.btn_UnmuteMedia.Text = "Unmute";
            this.btn_UnmuteMedia.UseVisualStyleBackColor = true;
            this.btn_UnmuteMedia.Click += new System.EventHandler(this.btn_UnmuteMedia_Click);
            // 
            // btn_MuteMedia
            // 
            this.btn_MuteMedia.Location = new System.Drawing.Point(153, 347);
            this.btn_MuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_MuteMedia.Name = "btn_MuteMedia";
            this.btn_MuteMedia.Size = new System.Drawing.Size(81, 23);
            this.btn_MuteMedia.TabIndex = 175;
            this.btn_MuteMedia.Text = "Mute";
            this.btn_MuteMedia.UseVisualStyleBackColor = true;
            this.btn_MuteMedia.Click += new System.EventHandler(this.btn_MuteMedia_Click);
            // 
            // cb_MuteMedia
            // 
            this.cb_MuteMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_MuteMedia.FormattingEnabled = true;
            this.cb_MuteMedia.Location = new System.Drawing.Point(24, 347);
            this.cb_MuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_MuteMedia.Name = "cb_MuteMedia";
            this.cb_MuteMedia.Size = new System.Drawing.Size(123, 23);
            this.cb_MuteMedia.TabIndex = 174;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(452, 67);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(46, 13);
            this.label8.TabIndex = 173;
            this.label8.Text = "Subject:";
            // 
            // tb_Subject
            // 
            this.tb_Subject.Location = new System.Drawing.Point(499, 64);
            this.tb_Subject.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_Subject.Name = "tb_Subject";
            this.tb_Subject.Size = new System.Drawing.Size(198, 23);
            this.tb_Subject.TabIndex = 172;
            // 
            // btn_RemoveMedia
            // 
            this.btn_RemoveMedia.Location = new System.Drawing.Point(155, 275);
            this.btn_RemoveMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_RemoveMedia.Name = "btn_RemoveMedia";
            this.btn_RemoveMedia.Size = new System.Drawing.Size(79, 23);
            this.btn_RemoveMedia.TabIndex = 171;
            this.btn_RemoveMedia.Text = "Remove";
            this.btn_RemoveMedia.UseVisualStyleBackColor = true;
            this.btn_RemoveMedia.Click += new System.EventHandler(this.btn_RemoveMedia_Click);
            // 
            // btn_AddMedia
            // 
            this.btn_AddMedia.Location = new System.Drawing.Point(154, 304);
            this.btn_AddMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AddMedia.Name = "btn_AddMedia";
            this.btn_AddMedia.Size = new System.Drawing.Size(79, 23);
            this.btn_AddMedia.TabIndex = 169;
            this.btn_AddMedia.Text = "Add";
            this.btn_AddMedia.UseVisualStyleBackColor = true;
            this.btn_AddMedia.Click += new System.EventHandler(this.btn_AddMedia_Click);
            // 
            // btn_DeclineCall
            // 
            this.btn_DeclineCall.FlatAppearance.BorderSize = 0;
            this.btn_DeclineCall.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_DeclineCall.Location = new System.Drawing.Point(629, 92);
            this.btn_DeclineCall.Margin = new System.Windows.Forms.Padding(0);
            this.btn_DeclineCall.Name = "btn_DeclineCall";
            this.btn_DeclineCall.Size = new System.Drawing.Size(68, 23);
            this.btn_DeclineCall.TabIndex = 165;
            this.btn_DeclineCall.Text = "Decline";
            this.btn_DeclineCall.UseVisualStyleBackColor = true;
            this.btn_DeclineCall.Click += new System.EventHandler(this.btn_DeclineCall_Click);
            // 
            // btn_HangUp
            // 
            this.btn_HangUp.Location = new System.Drawing.Point(154, 246);
            this.btn_HangUp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_HangUp.Name = "btn_HangUp";
            this.btn_HangUp.Size = new System.Drawing.Size(81, 23);
            this.btn_HangUp.TabIndex = 164;
            this.btn_HangUp.Text = "Hang Up";
            this.btn_HangUp.UseVisualStyleBackColor = true;
            this.btn_HangUp.Click += new System.EventHandler(this.btn_HangUp_Click);
            // 
            // btn_MakeCall
            // 
            this.btn_MakeCall.Location = new System.Drawing.Point(828, 63);
            this.btn_MakeCall.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_MakeCall.Name = "btn_MakeCall";
            this.btn_MakeCall.Size = new System.Drawing.Size(82, 23);
            this.btn_MakeCall.TabIndex = 163;
            this.btn_MakeCall.Text = "Make Call";
            this.btn_MakeCall.UseVisualStyleBackColor = true;
            this.btn_MakeCall.Click += new System.EventHandler(this.btn_MakeCall_Click);
            // 
            // cb_MakeCallMedias
            // 
            this.cb_MakeCallMedias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_MakeCallMedias.FormattingEnabled = true;
            this.cb_MakeCallMedias.Location = new System.Drawing.Point(701, 63);
            this.cb_MakeCallMedias.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_MakeCallMedias.Name = "cb_MakeCallMedias";
            this.cb_MakeCallMedias.Size = new System.Drawing.Size(123, 23);
            this.cb_MakeCallMedias.TabIndex = 162;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.Location = new System.Drawing.Point(452, 19);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 161;
            this.label5.Text = "Contacts";
            // 
            // cb_ContactsList
            // 
            this.cb_ContactsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_ContactsList.FormattingEnabled = true;
            this.cb_ContactsList.Location = new System.Drawing.Point(452, 35);
            this.cb_ContactsList.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_ContactsList.Name = "cb_ContactsList";
            this.cb_ContactsList.Size = new System.Drawing.Size(245, 23);
            this.cb_ContactsList.TabIndex = 160;
            this.cb_ContactsList.SelectedIndexChanged += new System.EventHandler(this.cb_ContactsList_SelectedIndexChanged);
            // 
            // lbl_ConferencesInProgress
            // 
            this.lbl_ConferencesInProgress.AutoSize = true;
            this.lbl_ConferencesInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_ConferencesInProgress.ForeColor = System.Drawing.Color.Firebrick;
            this.lbl_ConferencesInProgress.Location = new System.Drawing.Point(452, 172);
            this.lbl_ConferencesInProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ConferencesInProgress.Name = "lbl_ConferencesInProgress";
            this.lbl_ConferencesInProgress.Size = new System.Drawing.Size(74, 13);
            this.lbl_ConferencesInProgress.TabIndex = 186;
            this.lbl_ConferencesInProgress.Text = "In progress:";
            // 
            // btn_StartConf
            // 
            this.btn_StartConf.Location = new System.Drawing.Point(701, 138);
            this.btn_StartConf.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_StartConf.Name = "btn_StartConf";
            this.btn_StartConf.Size = new System.Drawing.Size(82, 23);
            this.btn_StartConf.TabIndex = 182;
            this.btn_StartConf.Text = "Start Conf";
            this.btn_StartConf.UseVisualStyleBackColor = true;
            this.btn_StartConf.Click += new System.EventHandler(this.btn_StartConf_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label11.Location = new System.Drawing.Point(452, 122);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(52, 13);
            this.label11.TabIndex = 181;
            this.label11.Text = "Bubbles";
            // 
            // cb_BubblesList
            // 
            this.cb_BubblesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_BubblesList.FormattingEnabled = true;
            this.cb_BubblesList.Location = new System.Drawing.Point(452, 138);
            this.cb_BubblesList.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_BubblesList.Name = "cb_BubblesList";
            this.cb_BubblesList.Size = new System.Drawing.Size(245, 23);
            this.cb_BubblesList.TabIndex = 180;
            this.cb_BubblesList.SelectedIndexChanged += new System.EventHandler(this.cb_BubblesList_SelectedIndexChanged);
            // 
            // btn_JoinConf
            // 
            this.btn_JoinConf.Location = new System.Drawing.Point(701, 167);
            this.btn_JoinConf.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_JoinConf.Name = "btn_JoinConf";
            this.btn_JoinConf.Size = new System.Drawing.Size(82, 23);
            this.btn_JoinConf.TabIndex = 179;
            this.btn_JoinConf.Text = "Join Conf";
            this.btn_JoinConf.UseVisualStyleBackColor = true;
            this.btn_JoinConf.Click += new System.EventHandler(this.btn_JoinConf_Click);
            // 
            // btn_ManageInputStreams
            // 
            this.btn_ManageInputStreams.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btn_ManageInputStreams.ForeColor = System.Drawing.Color.SaddleBrown;
            this.btn_ManageInputStreams.Location = new System.Drawing.Point(12, 12);
            this.btn_ManageInputStreams.Name = "btn_ManageInputStreams";
            this.btn_ManageInputStreams.Size = new System.Drawing.Size(163, 23);
            this.btn_ManageInputStreams.TabIndex = 187;
            this.btn_ManageInputStreams.Text = "Manage Input Streams";
            this.btn_ManageInputStreams.UseVisualStyleBackColor = true;
            this.btn_ManageInputStreams.Click += new System.EventHandler(this.btn_ManageInputStreams_Click);
            // 
            // btn_RefreshAudioOutput
            // 
            this.btn_RefreshAudioOutput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_RefreshAudioOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_RefreshAudioOutput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_RefreshAudioOutput.Location = new System.Drawing.Point(362, 153);
            this.btn_RefreshAudioOutput.Name = "btn_RefreshAudioOutput";
            this.btn_RefreshAudioOutput.Size = new System.Drawing.Size(16, 16);
            this.btn_RefreshAudioOutput.TabIndex = 333;
            this.btn_RefreshAudioOutput.UseVisualStyleBackColor = true;
            this.btn_RefreshAudioOutput.Click += new System.EventHandler(this.btb_AudioOutputListRefresh_Click);
            // 
            // btn_RefreshAudioInput
            // 
            this.btn_RefreshAudioInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_RefreshAudioInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_RefreshAudioInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_RefreshAudioInput.Location = new System.Drawing.Point(401, 124);
            this.btn_RefreshAudioInput.Name = "btn_RefreshAudioInput";
            this.btn_RefreshAudioInput.Size = new System.Drawing.Size(16, 16);
            this.btn_RefreshAudioInput.TabIndex = 334;
            this.btn_RefreshAudioInput.UseVisualStyleBackColor = true;
            this.btn_RefreshAudioInput.Click += new System.EventHandler(this.btb_AudioInputListRefresh_Click);
            // 
            // btn_LoadConfig
            // 
            this.btn_LoadConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btn_LoadConfig.ForeColor = System.Drawing.Color.SaddleBrown;
            this.btn_LoadConfig.Location = new System.Drawing.Point(198, 12);
            this.btn_LoadConfig.Name = "btn_LoadConfig";
            this.btn_LoadConfig.Size = new System.Drawing.Size(163, 23);
            this.btn_LoadConfig.TabIndex = 335;
            this.btn_LoadConfig.Text = "Load Configuration";
            this.btn_LoadConfig.UseVisualStyleBackColor = true;
            this.btn_LoadConfig.Click += new System.EventHandler(this.btn_LoadConfig_Click);
            // 
            // btn_OutputVideoInput
            // 
            this.btn_OutputVideoInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_OutputVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputVideoInput.Location = new System.Drawing.Point(401, 58);
            this.btn_OutputVideoInput.Name = "btn_OutputVideoInput";
            this.btn_OutputVideoInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputVideoInput.TabIndex = 336;
            this.btn_OutputVideoInput.UseVisualStyleBackColor = true;
            this.btn_OutputVideoInput.Click += new System.EventHandler(this.btn_OutputVideoInput_Click);
            // 
            // btn_OutputSharingInput
            // 
            this.btn_OutputSharingInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_OutputSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputSharingInput.Location = new System.Drawing.Point(401, 87);
            this.btn_OutputSharingInput.Name = "btn_OutputSharingInput";
            this.btn_OutputSharingInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputSharingInput.TabIndex = 337;
            this.btn_OutputSharingInput.UseVisualStyleBackColor = true;
            this.btn_OutputSharingInput.Click += new System.EventHandler(this.btn_OutputSharingInput_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(454, 206);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 339;
            this.label9.Text = "Information:";
            // 
            // tb_Information
            // 
            this.tb_Information.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tb_Information.Location = new System.Drawing.Point(452, 222);
            this.tb_Information.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_Information.Multiline = true;
            this.tb_Information.Name = "tb_Information";
            this.tb_Information.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Information.Size = new System.Drawing.Size(480, 337);
            this.tb_Information.TabIndex = 338;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label10.Location = new System.Drawing.Point(12, 201);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(85, 13);
            this.label10.TabIndex = 340;
            this.label10.Text = "Conversation:";
            // 
            // lbl_IncomingCall
            // 
            this.lbl_IncomingCall.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_IncomingCall.ForeColor = System.Drawing.Color.Firebrick;
            this.lbl_IncomingCall.Location = new System.Drawing.Point(452, 96);
            this.lbl_IncomingCall.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_IncomingCall.Name = "lbl_IncomingCall";
            this.lbl_IncomingCall.Size = new System.Drawing.Size(175, 18);
            this.lbl_IncomingCall.TabIndex = 342;
            this.lbl_IncomingCall.Text = "Incoming call";
            this.lbl_IncomingCall.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_ConversationDetails
            // 
            this.lbl_ConversationDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_ConversationDetails.ForeColor = System.Drawing.Color.Blue;
            this.lbl_ConversationDetails.Location = new System.Drawing.Point(12, 219);
            this.lbl_ConversationDetails.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ConversationDetails.Name = "lbl_ConversationDetails";
            this.lbl_ConversationDetails.Size = new System.Drawing.Size(349, 16);
            this.lbl_ConversationDetails.TabIndex = 344;
            this.lbl_ConversationDetails.Text = "details";
            // 
            // tb_ConferenceName
            // 
            this.tb_ConferenceName.Location = new System.Drawing.Point(530, 167);
            this.tb_ConferenceName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb_ConferenceName.Name = "tb_ConferenceName";
            this.tb_ConferenceName.ReadOnly = true;
            this.tb_ConferenceName.Size = new System.Drawing.Size(167, 23);
            this.tb_ConferenceName.TabIndex = 345;
            // 
            // lbl_BubbleInfo
            // 
            this.lbl_BubbleInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_BubbleInfo.Location = new System.Drawing.Point(787, 143);
            this.lbl_BubbleInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_BubbleInfo.Name = "lbl_BubbleInfo";
            this.lbl_BubbleInfo.Size = new System.Drawing.Size(145, 17);
            this.lbl_BubbleInfo.TabIndex = 346;
            this.lbl_BubbleInfo.Text = "bubble info";
            // 
            // btn_AnswerCall
            // 
            this.btn_AnswerCall.Location = new System.Drawing.Point(828, 92);
            this.btn_AnswerCall.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AnswerCall.Name = "btn_AnswerCall";
            this.btn_AnswerCall.Size = new System.Drawing.Size(82, 23);
            this.btn_AnswerCall.TabIndex = 347;
            this.btn_AnswerCall.Text = "Answer";
            this.btn_AnswerCall.UseVisualStyleBackColor = true;
            this.btn_AnswerCall.Click += new System.EventHandler(this.btn_AnswerCall_Click);
            // 
            // cb_AnswerCallMedias
            // 
            this.cb_AnswerCallMedias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AnswerCallMedias.FormattingEnabled = true;
            this.cb_AnswerCallMedias.Location = new System.Drawing.Point(701, 92);
            this.cb_AnswerCallMedias.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AnswerCallMedias.Name = "cb_AnswerCallMedias";
            this.cb_AnswerCallMedias.Size = new System.Drawing.Size(123, 23);
            this.cb_AnswerCallMedias.TabIndex = 348;
            // 
            // cb_RemoveMedia
            // 
            this.cb_RemoveMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_RemoveMedia.FormattingEnabled = true;
            this.cb_RemoveMedia.Location = new System.Drawing.Point(24, 276);
            this.cb_RemoveMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_RemoveMedia.Name = "cb_RemoveMedia";
            this.cb_RemoveMedia.Size = new System.Drawing.Size(123, 23);
            this.cb_RemoveMedia.TabIndex = 350;
            // 
            // cb_AddMedia
            // 
            this.cb_AddMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AddMedia.FormattingEnabled = true;
            this.cb_AddMedia.Location = new System.Drawing.Point(24, 305);
            this.cb_AddMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_AddMedia.Name = "cb_AddMedia";
            this.cb_AddMedia.Size = new System.Drawing.Size(123, 23);
            this.cb_AddMedia.TabIndex = 349;
            this.cb_AddMedia.SelectedIndexChanged += new System.EventHandler(this.cb_AddMedia_SelectedIndexChanged);
            // 
            // cb_UnmuteMedia
            // 
            this.cb_UnmuteMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_UnmuteMedia.FormattingEnabled = true;
            this.cb_UnmuteMedia.Location = new System.Drawing.Point(24, 376);
            this.cb_UnmuteMedia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_UnmuteMedia.Name = "cb_UnmuteMedia";
            this.cb_UnmuteMedia.Size = new System.Drawing.Size(123, 23);
            this.cb_UnmuteMedia.TabIndex = 353;
            // 
            // btn_OutputRemoteVideoInput
            // 
            this.btn_OutputRemoteVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputRemoteVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputRemoteVideoInput.Location = new System.Drawing.Point(237, 458);
            this.btn_OutputRemoteVideoInput.Name = "btn_OutputRemoteVideoInput";
            this.btn_OutputRemoteVideoInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputRemoteVideoInput.TabIndex = 354;
            this.btn_OutputRemoteVideoInput.UseVisualStyleBackColor = true;
            this.btn_OutputRemoteVideoInput.Click += new System.EventHandler(this.btn_OutputRemoteVideoInput_Click);
            // 
            // btn_OutputRemoteSharingInput
            // 
            this.btn_OutputRemoteSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputRemoteSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputRemoteSharingInput.Location = new System.Drawing.Point(237, 482);
            this.btn_OutputRemoteSharingInput.Name = "btn_OutputRemoteSharingInput";
            this.btn_OutputRemoteSharingInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputRemoteSharingInput.TabIndex = 355;
            this.btn_OutputRemoteSharingInput.UseVisualStyleBackColor = true;
            this.btn_OutputRemoteSharingInput.Click += new System.EventHandler(this.btn_OutputRemoteSharingInput_Click);
            // 
            // btn_OutputLocalVideoInput
            // 
            this.btn_OutputLocalVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputLocalVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputLocalVideoInput.Location = new System.Drawing.Point(108, 482);
            this.btn_OutputLocalVideoInput.Name = "btn_OutputLocalVideoInput";
            this.btn_OutputLocalVideoInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputLocalVideoInput.TabIndex = 356;
            this.btn_OutputLocalVideoInput.UseVisualStyleBackColor = true;
            this.btn_OutputLocalVideoInput.Click += new System.EventHandler(this.btn_OutputLocalVideoInput_Click);
            // 
            // btn_OutputLocalSharingInput
            // 
            this.btn_OutputLocalSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OutputLocalSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_OutputLocalSharingInput.Location = new System.Drawing.Point(108, 458);
            this.btn_OutputLocalSharingInput.Name = "btn_OutputLocalSharingInput";
            this.btn_OutputLocalSharingInput.Size = new System.Drawing.Size(16, 16);
            this.btn_OutputLocalSharingInput.TabIndex = 357;
            this.btn_OutputLocalSharingInput.UseVisualStyleBackColor = true;
            this.btn_OutputLocalSharingInput.Click += new System.EventHandler(this.btn_OutputLocalSharingInput_Click);
            // 
            // btn_StopVideoInput
            // 
            this.btn_StopVideoInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StopVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StopVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StopVideoInput.Location = new System.Drawing.Point(381, 58);
            this.btn_StopVideoInput.Name = "btn_StopVideoInput";
            this.btn_StopVideoInput.Size = new System.Drawing.Size(16, 16);
            this.btn_StopVideoInput.TabIndex = 359;
            this.btn_StopVideoInput.UseVisualStyleBackColor = true;
            this.btn_StopVideoInput.Click += new System.EventHandler(this.btn_StoptMediaInput_Click);
            // 
            // btn_StartVideoInput
            // 
            this.btn_StartVideoInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StartVideoInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StartVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StartVideoInput.Location = new System.Drawing.Point(362, 58);
            this.btn_StartVideoInput.Name = "btn_StartVideoInput";
            this.btn_StartVideoInput.Size = new System.Drawing.Size(16, 16);
            this.btn_StartVideoInput.TabIndex = 358;
            this.btn_StartVideoInput.UseVisualStyleBackColor = true;
            this.btn_StartVideoInput.Click += new System.EventHandler(this.btn_StartMediaInput_Click);
            // 
            // btn_StopSharingInput
            // 
            this.btn_StopSharingInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StopSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StopSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StopSharingInput.Location = new System.Drawing.Point(381, 87);
            this.btn_StopSharingInput.Name = "btn_StopSharingInput";
            this.btn_StopSharingInput.Size = new System.Drawing.Size(16, 16);
            this.btn_StopSharingInput.TabIndex = 361;
            this.btn_StopSharingInput.UseVisualStyleBackColor = true;
            this.btn_StopSharingInput.Click += new System.EventHandler(this.btn_StoptMediaInput_Click);
            // 
            // btn_StartSharingInput
            // 
            this.btn_StartSharingInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StartSharingInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StartSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StartSharingInput.Location = new System.Drawing.Point(362, 87);
            this.btn_StartSharingInput.Name = "btn_StartSharingInput";
            this.btn_StartSharingInput.Size = new System.Drawing.Size(16, 16);
            this.btn_StartSharingInput.TabIndex = 360;
            this.btn_StartSharingInput.UseVisualStyleBackColor = true;
            this.btn_StartSharingInput.Click += new System.EventHandler(this.btn_StartMediaInput_Click);
            // 
            // btn_StopAudioInput
            // 
            this.btn_StopAudioInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StopAudioInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StopAudioInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StopAudioInput.Location = new System.Drawing.Point(381, 124);
            this.btn_StopAudioInput.Name = "btn_StopAudioInput";
            this.btn_StopAudioInput.Size = new System.Drawing.Size(16, 16);
            this.btn_StopAudioInput.TabIndex = 363;
            this.btn_StopAudioInput.UseVisualStyleBackColor = true;
            this.btn_StopAudioInput.Click += new System.EventHandler(this.btn_StoptMediaInput_Click);
            // 
            // btn_StartAudioInput
            // 
            this.btn_StartAudioInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_StartAudioInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_StartAudioInput.ForeColor = System.Drawing.SystemColors.Control;
            this.btn_StartAudioInput.Location = new System.Drawing.Point(362, 124);
            this.btn_StartAudioInput.Name = "btn_StartAudioInput";
            this.btn_StartAudioInput.Size = new System.Drawing.Size(16, 16);
            this.btn_StartAudioInput.TabIndex = 362;
            this.btn_StartAudioInput.UseVisualStyleBackColor = true;
            this.btn_StartAudioInput.Click += new System.EventHandler(this.btn_StartMediaInput_Click);
            // 
            // btn_DeclineConf
            // 
            this.btn_DeclineConf.FlatAppearance.BorderSize = 0;
            this.btn_DeclineConf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_DeclineConf.Location = new System.Drawing.Point(787, 167);
            this.btn_DeclineConf.Margin = new System.Windows.Forms.Padding(0);
            this.btn_DeclineConf.Name = "btn_DeclineConf";
            this.btn_DeclineConf.Size = new System.Drawing.Size(68, 23);
            this.btn_DeclineConf.TabIndex = 364;
            this.btn_DeclineConf.Text = "Decline";
            this.btn_DeclineConf.UseVisualStyleBackColor = true;
            this.btn_DeclineConf.Click += new System.EventHandler(this.btn_DeclineConf_Click);
            // 
            // btn_ConferenceOptions
            // 
            this.btn_ConferenceOptions.Enabled = false;
            this.btn_ConferenceOptions.Location = new System.Drawing.Point(24, 246);
            this.btn_ConferenceOptions.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_ConferenceOptions.Name = "btn_ConferenceOptions";
            this.btn_ConferenceOptions.Size = new System.Drawing.Size(123, 23);
            this.btn_ConferenceOptions.TabIndex = 365;
            this.btn_ConferenceOptions.Text = "Conf. Options ...";
            this.btn_ConferenceOptions.UseVisualStyleBackColor = true;
            this.btn_ConferenceOptions.Click += new System.EventHandler(this.btn_ConferenceOptions_Click);
            // 
            // lbl_ConversationInProgress
            // 
            this.lbl_ConversationInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_ConversationInProgress.ForeColor = System.Drawing.Color.Firebrick;
            this.lbl_ConversationInProgress.Location = new System.Drawing.Point(101, 201);
            this.lbl_ConversationInProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ConversationInProgress.Name = "lbl_ConversationInProgress";
            this.lbl_ConversationInProgress.Size = new System.Drawing.Size(82, 18);
            this.lbl_ConversationInProgress.TabIndex = 1299;
            this.lbl_ConversationInProgress.Text = "In progress";
            this.lbl_ConversationInProgress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cb_ConferencesName
            // 
            this.cb_ConferencesName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_ConferencesName.FormattingEnabled = true;
            this.cb_ConferencesName.Location = new System.Drawing.Point(530, 193);
            this.cb_ConferencesName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cb_ConferencesName.Name = "cb_ConferencesName";
            this.cb_ConferencesName.Size = new System.Drawing.Size(167, 23);
            this.cb_ConferencesName.TabIndex = 1300;
            // 
            // btn_AddAsParticipant
            // 
            this.btn_AddAsParticipant.Enabled = false;
            this.btn_AddAsParticipant.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_AddAsParticipant.Location = new System.Drawing.Point(702, 35);
            this.btn_AddAsParticipant.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btn_AddAsParticipant.Name = "btn_AddAsParticipant";
            this.btn_AddAsParticipant.Size = new System.Drawing.Size(122, 23);
            this.btn_AddAsParticipant.TabIndex = 1301;
            this.btn_AddAsParticipant.Text = "Add as participant";
            this.btn_AddAsParticipant.UseVisualStyleBackColor = true;
            // 
            // FormWebRTC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 566);
            this.Controls.Add(this.btn_AddAsParticipant);
            this.Controls.Add(this.cb_ConferencesName);
            this.Controls.Add(this.lbl_ConversationInProgress);
            this.Controls.Add(this.btn_ConferenceOptions);
            this.Controls.Add(this.btn_DeclineConf);
            this.Controls.Add(this.btn_StopAudioInput);
            this.Controls.Add(this.btn_StartAudioInput);
            this.Controls.Add(this.btn_StopSharingInput);
            this.Controls.Add(this.btn_StartSharingInput);
            this.Controls.Add(this.btn_StopVideoInput);
            this.Controls.Add(this.btn_StartVideoInput);
            this.Controls.Add(this.btn_OutputLocalSharingInput);
            this.Controls.Add(this.btn_OutputLocalVideoInput);
            this.Controls.Add(this.btn_OutputRemoteSharingInput);
            this.Controls.Add(this.btn_OutputRemoteVideoInput);
            this.Controls.Add(this.cb_UnmuteMedia);
            this.Controls.Add(this.cb_RemoveMedia);
            this.Controls.Add(this.cb_AddMedia);
            this.Controls.Add(this.cb_AnswerCallMedias);
            this.Controls.Add(this.btn_AnswerCall);
            this.Controls.Add(this.lbl_BubbleInfo);
            this.Controls.Add(this.tb_ConferenceName);
            this.Controls.Add(this.lbl_ConversationDetails);
            this.Controls.Add(this.lbl_IncomingCall);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tb_Information);
            this.Controls.Add(this.btn_OutputSharingInput);
            this.Controls.Add(this.btn_OutputVideoInput);
            this.Controls.Add(this.btn_LoadConfig);
            this.Controls.Add(this.btn_RefreshAudioInput);
            this.Controls.Add(this.btn_RefreshAudioOutput);
            this.Controls.Add(this.btn_ManageInputStreams);
            this.Controls.Add(this.lbl_ConferencesInProgress);
            this.Controls.Add(this.btn_StartConf);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.cb_BubblesList);
            this.Controls.Add(this.btn_JoinConf);
            this.Controls.Add(this.btn_AskToShare);
            this.Controls.Add(this.btn_UnmuteMedia);
            this.Controls.Add(this.btn_MuteMedia);
            this.Controls.Add(this.cb_MuteMedia);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tb_Subject);
            this.Controls.Add(this.btn_RemoveMedia);
            this.Controls.Add(this.btn_AddMedia);
            this.Controls.Add(this.btn_DeclineCall);
            this.Controls.Add(this.btn_HangUp);
            this.Controls.Add(this.btn_MakeCall);
            this.Controls.Add(this.cb_MakeCallMedias);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cb_ContactsList);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.check_RemoteSharing);
            this.Controls.Add(this.check_RemoteVideo);
            this.Controls.Add(this.check_RemoteAudio);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.check_LocalSharing);
            this.Controls.Add(this.check_LocalVideo);
            this.Controls.Add(this.check_LocalAudio);
            this.Controls.Add(this.cb_SharingInputs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cb_VideoInputs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_AudioInputs);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_AudioOutputs);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWebRTC";
            this.Text = "WebRtcForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWebRTC_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox cb_AudioOutputs;
        private Label label4;
        private ComboBox cb_AudioInputs;
        private Label label1;
        private ComboBox cb_VideoInputs;
        private Label label2;
        private ComboBox cb_SharingInputs;
        private Label label3;
        private Label label7;
        private CheckBox check_RemoteSharing;
        private CheckBox check_RemoteVideo;
        private CheckBox check_RemoteAudio;
        private Label label6;
        private CheckBox check_LocalSharing;
        private CheckBox check_LocalVideo;
        private CheckBox check_LocalAudio;
        private Button btn_AskToShare;
        private Button btn_UnmuteMedia;
        private Button btn_MuteMedia;
        private ComboBox cb_MuteMedia;
        private Label label8;
        private TextBox tb_Subject;
        private Button btn_RemoveMedia;
        private Button btn_AddMedia;
        private Button btn_DeclineCall;
        private Button btn_HangUp;
        private Button btn_MakeCall;
        private ComboBox cb_MakeCallMedias;
        private Label label5;
        private ComboBox cb_ContactsList;
        private Label lbl_ConferencesInProgress;
        private Button btn_StartConf;
        private Label label11;
        private ComboBox cb_BubblesList;
        private Button btn_JoinConf;
        private Button btn_ManageInputStreams;
        private Button btn_RefreshAudioOutput;
        private Button btn_RefreshAudioInput;
        private Button btn_LoadConfig;
        private Button btn_OutputVideoInput;
        private Button btn_OutputSharingInput;
        private Label label9;
        private TextBox tb_Information;
        private Label label10;
        private Label lbl_IncomingCall;
        private Label lbl_ConversationDetails;
        private TextBox tb_ConferenceName;
        private Label lbl_BubbleInfo;
        private Button btn_AnswerCall;
        private ComboBox cb_AnswerCallMedias;
        private ComboBox cb_RemoveMedia;
        private ComboBox cb_AddMedia;
        private ComboBox cb_UnmuteMedia;
        private Button btn_OutputRemoteVideoInput;
        private Button btn_OutputRemoteSharingInput;
        private Button btn_OutputLocalVideoInput;
        private Button btn_OutputLocalSharingInput;
        private Button btn_StopVideoInput;
        private Button btn_StartVideoInput;
        private Button btn_StopSharingInput;
        private Button btn_StartSharingInput;
        private Button btn_StopAudioInput;
        private Button btn_StartAudioInput;
        private Button btn_DeclineConf;
        private Button btn_ConferenceOptions;
        private Label lbl_ConversationInProgress;
        private ComboBox cb_ConferencesName;
        private Button btn_AddAsParticipant;
    }
}
