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
            components = new System.ComponentModel.Container();
            cb_AudioOutputs = new ComboBox();
            label4 = new Label();
            cb_AudioInputs = new ComboBox();
            label1 = new Label();
            cb_VideoInputs = new ComboBox();
            label2 = new Label();
            cb_SharingInputs = new ComboBox();
            label3 = new Label();
            label7 = new Label();
            check_RemoteSharing = new CheckBox();
            check_RemoteVideo = new CheckBox();
            check_RemoteAudio = new CheckBox();
            label6 = new Label();
            check_LocalSharing = new CheckBox();
            check_LocalVideo = new CheckBox();
            check_LocalAudio = new CheckBox();
            btn_AskToShare = new Button();
            btn_UnmuteMedia = new Button();
            btn_MuteMedia = new Button();
            cb_MuteMedia = new ComboBox();
            label8 = new Label();
            tb_Subject = new TextBox();
            btn_RemoveMedia = new Button();
            btn_AddMedia = new Button();
            btn_DeclineCall = new Button();
            btn_HangUp = new Button();
            btn_MakeCall = new Button();
            cb_MakeCallMedias = new ComboBox();
            label5 = new Label();
            cb_ContactsList = new ComboBox();
            lbl_ConferencesInProgress = new Label();
            btn_StartConf = new Button();
            label11 = new Label();
            cb_BubblesList = new ComboBox();
            btn_JoinConf = new Button();
            btn_ManageInputStreams = new Button();
            btn_RefreshAudioOutput = new Button();
            btn_RefreshAudioInput = new Button();
            btn_LoadConfig = new Button();
            btn_OutputVideoInput = new Button();
            btn_OutputSharingInput = new Button();
            label9 = new Label();
            tb_Information = new TextBox();
            label10 = new Label();
            lbl_IncomingCall = new Label();
            lbl_ConversationDetails = new Label();
            lbl_BubbleInfo = new Label();
            btn_AnswerCall = new Button();
            cb_AnswerCallMedias = new ComboBox();
            cb_RemoveMedia = new ComboBox();
            cb_AddMedia = new ComboBox();
            cb_UnmuteMedia = new ComboBox();
            btn_OutputLocalVideoInput = new Button();
            btn_OutputLocalSharingInput = new Button();
            btn_StopVideoInput = new Button();
            btn_StartVideoInput = new Button();
            btn_StopSharingInput = new Button();
            btn_StartSharingInput = new Button();
            btn_StopAudioInput = new Button();
            btn_StartAudioInput = new Button();
            btn_DeclineConf = new Button();
            btn_ConferenceOptions = new Button();
            lbl_ConversationInProgress = new Label();
            cb_ConferencesName = new ComboBox();
            btn_AddAsParticipant = new Button();
            btn_OutputRemoteVideoInput = new Button();
            btn_OutputRemoteSharingInput = new Button();
            toolTip1 = new ToolTip(components);
            btn_SubscribeRemoteAudioInput = new Button();
            btn_SubscribeRemoteVideoInput = new Button();
            btn_SubscribeRemoteSharingInput = new Button();
            btn_Details = new Button();
            check_LocalDataChannel = new CheckBox();
            btn_SubscribeRemoteDataChannelInput = new Button();
            check_RemoteDataChannel = new CheckBox();
            tbDataChannel = new TextBox();
            btn_DataChannelSend = new Button();
            btn_InformationClear = new Button();
            SuspendLayout();
            // 
            // cb_AudioOutputs
            // 
            cb_AudioOutputs.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_AudioOutputs.FormattingEnabled = true;
            cb_AudioOutputs.Location = new System.Drawing.Point(101, 150);
            cb_AudioOutputs.Name = "cb_AudioOutputs";
            cb_AudioOutputs.Size = new System.Drawing.Size(260, 23);
            cb_AudioOutputs.TabIndex = 138;
            cb_AudioOutputs.SelectedIndexChanged += cb_AudioOutputList_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 153);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(83, 15);
            label4.TabIndex = 137;
            label4.Text = "Audio Output:";
            // 
            // cb_AudioInputs
            // 
            cb_AudioInputs.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_AudioInputs.FormattingEnabled = true;
            cb_AudioInputs.Location = new System.Drawing.Point(101, 121);
            cb_AudioInputs.Name = "cb_AudioInputs";
            cb_AudioInputs.Size = new System.Drawing.Size(260, 23);
            cb_AudioInputs.TabIndex = 140;
            cb_AudioInputs.SelectedIndexChanged += cb_AudioInputList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 124);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(73, 15);
            label1.TabIndex = 139;
            label1.Text = "Audio Input:";
            // 
            // cb_VideoInputs
            // 
            cb_VideoInputs.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_VideoInputs.FormattingEnabled = true;
            cb_VideoInputs.Location = new System.Drawing.Point(101, 55);
            cb_VideoInputs.Name = "cb_VideoInputs";
            cb_VideoInputs.Size = new System.Drawing.Size(260, 23);
            cb_VideoInputs.TabIndex = 144;
            cb_VideoInputs.SelectedIndexChanged += cb_VideoInputs_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 58);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(71, 15);
            label2.TabIndex = 143;
            label2.Text = "Video Input:";
            // 
            // cb_SharingInputs
            // 
            cb_SharingInputs.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_SharingInputs.FormattingEnabled = true;
            cb_SharingInputs.Location = new System.Drawing.Point(101, 84);
            cb_SharingInputs.Name = "cb_SharingInputs";
            cb_SharingInputs.Size = new System.Drawing.Size(260, 23);
            cb_SharingInputs.TabIndex = 147;
            cb_SharingInputs.SelectedIndexChanged += cb_SharingInputs_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 87);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(81, 15);
            label3.TabIndex = 146;
            label3.Text = "Sharing Input:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label7.Location = new System.Drawing.Point(153, 418);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(141, 13);
            label7.TabIndex = 156;
            label7.Text = "Medias used by remote:";
            // 
            // check_RemoteSharing
            // 
            check_RemoteSharing.AutoCheck = false;
            check_RemoteSharing.AutoSize = true;
            check_RemoteSharing.Location = new System.Drawing.Point(174, 481);
            check_RemoteSharing.Margin = new Padding(2, 3, 2, 3);
            check_RemoteSharing.Name = "check_RemoteSharing";
            check_RemoteSharing.Size = new System.Drawing.Size(66, 19);
            check_RemoteSharing.TabIndex = 155;
            check_RemoteSharing.Text = "Sharing";
            check_RemoteSharing.UseVisualStyleBackColor = true;
            // 
            // check_RemoteVideo
            // 
            check_RemoteVideo.AutoCheck = false;
            check_RemoteVideo.AutoSize = true;
            check_RemoteVideo.Location = new System.Drawing.Point(174, 456);
            check_RemoteVideo.Margin = new Padding(2, 3, 2, 3);
            check_RemoteVideo.Name = "check_RemoteVideo";
            check_RemoteVideo.Size = new System.Drawing.Size(56, 19);
            check_RemoteVideo.TabIndex = 154;
            check_RemoteVideo.Text = "Video";
            check_RemoteVideo.UseVisualStyleBackColor = true;
            // 
            // check_RemoteAudio
            // 
            check_RemoteAudio.AutoCheck = false;
            check_RemoteAudio.AutoSize = true;
            check_RemoteAudio.Location = new System.Drawing.Point(174, 434);
            check_RemoteAudio.Margin = new Padding(2, 3, 2, 3);
            check_RemoteAudio.Name = "check_RemoteAudio";
            check_RemoteAudio.Size = new System.Drawing.Size(58, 19);
            check_RemoteAudio.TabIndex = 153;
            check_RemoteAudio.Text = "Audio";
            check_RemoteAudio.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label6.Location = new System.Drawing.Point(24, 418);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(82, 13);
            label6.TabIndex = 152;
            label6.Text = "Medias used:";
            // 
            // check_LocalSharing
            // 
            check_LocalSharing.AutoCheck = false;
            check_LocalSharing.AutoSize = true;
            check_LocalSharing.Location = new System.Drawing.Point(45, 481);
            check_LocalSharing.Margin = new Padding(2, 3, 2, 3);
            check_LocalSharing.Name = "check_LocalSharing";
            check_LocalSharing.Size = new System.Drawing.Size(66, 19);
            check_LocalSharing.TabIndex = 151;
            check_LocalSharing.Text = "Sharing";
            check_LocalSharing.UseVisualStyleBackColor = true;
            // 
            // check_LocalVideo
            // 
            check_LocalVideo.AutoCheck = false;
            check_LocalVideo.AutoSize = true;
            check_LocalVideo.Location = new System.Drawing.Point(45, 458);
            check_LocalVideo.Margin = new Padding(2, 3, 2, 3);
            check_LocalVideo.Name = "check_LocalVideo";
            check_LocalVideo.Size = new System.Drawing.Size(56, 19);
            check_LocalVideo.TabIndex = 150;
            check_LocalVideo.Text = "Video";
            check_LocalVideo.UseVisualStyleBackColor = true;
            // 
            // check_LocalAudio
            // 
            check_LocalAudio.AutoCheck = false;
            check_LocalAudio.AutoSize = true;
            check_LocalAudio.Location = new System.Drawing.Point(45, 434);
            check_LocalAudio.Margin = new Padding(2, 3, 2, 3);
            check_LocalAudio.Name = "check_LocalAudio";
            check_LocalAudio.Size = new System.Drawing.Size(58, 19);
            check_LocalAudio.TabIndex = 149;
            check_LocalAudio.Text = "Audio";
            check_LocalAudio.UseVisualStyleBackColor = true;
            // 
            // btn_AskToShare
            // 
            btn_AskToShare.Enabled = false;
            btn_AskToShare.Location = new System.Drawing.Point(237, 304);
            btn_AskToShare.Margin = new Padding(2, 3, 2, 3);
            btn_AskToShare.Name = "btn_AskToShare";
            btn_AskToShare.Size = new System.Drawing.Size(81, 23);
            btn_AskToShare.TabIndex = 178;
            btn_AskToShare.Text = "Ask to Share";
            btn_AskToShare.UseVisualStyleBackColor = true;
            btn_AskToShare.Click += btn_AskToShare_Click;
            // 
            // btn_UnmuteMedia
            // 
            btn_UnmuteMedia.Location = new System.Drawing.Point(152, 376);
            btn_UnmuteMedia.Margin = new Padding(2, 3, 2, 3);
            btn_UnmuteMedia.Name = "btn_UnmuteMedia";
            btn_UnmuteMedia.Size = new System.Drawing.Size(81, 23);
            btn_UnmuteMedia.TabIndex = 177;
            btn_UnmuteMedia.Text = "Unmute";
            btn_UnmuteMedia.UseVisualStyleBackColor = true;
            btn_UnmuteMedia.Click += btn_UnmuteMedia_Click;
            // 
            // btn_MuteMedia
            // 
            btn_MuteMedia.Location = new System.Drawing.Point(153, 347);
            btn_MuteMedia.Margin = new Padding(2, 3, 2, 3);
            btn_MuteMedia.Name = "btn_MuteMedia";
            btn_MuteMedia.Size = new System.Drawing.Size(81, 23);
            btn_MuteMedia.TabIndex = 175;
            btn_MuteMedia.Text = "Mute";
            btn_MuteMedia.UseVisualStyleBackColor = true;
            btn_MuteMedia.Click += btn_MuteMedia_Click;
            // 
            // cb_MuteMedia
            // 
            cb_MuteMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_MuteMedia.FormattingEnabled = true;
            cb_MuteMedia.Location = new System.Drawing.Point(24, 347);
            cb_MuteMedia.Margin = new Padding(2, 3, 2, 3);
            cb_MuteMedia.Name = "cb_MuteMedia";
            cb_MuteMedia.Size = new System.Drawing.Size(123, 23);
            cb_MuteMedia.TabIndex = 174;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label8.Location = new System.Drawing.Point(452, 67);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(46, 13);
            label8.TabIndex = 173;
            label8.Text = "Subject:";
            // 
            // tb_Subject
            // 
            tb_Subject.Location = new System.Drawing.Point(499, 64);
            tb_Subject.Margin = new Padding(2, 3, 2, 3);
            tb_Subject.Name = "tb_Subject";
            tb_Subject.Size = new System.Drawing.Size(198, 23);
            tb_Subject.TabIndex = 172;
            // 
            // btn_RemoveMedia
            // 
            btn_RemoveMedia.Location = new System.Drawing.Point(155, 275);
            btn_RemoveMedia.Margin = new Padding(2, 3, 2, 3);
            btn_RemoveMedia.Name = "btn_RemoveMedia";
            btn_RemoveMedia.Size = new System.Drawing.Size(79, 23);
            btn_RemoveMedia.TabIndex = 171;
            btn_RemoveMedia.Text = "Remove";
            btn_RemoveMedia.UseVisualStyleBackColor = true;
            btn_RemoveMedia.Click += btn_RemoveMedia_Click;
            // 
            // btn_AddMedia
            // 
            btn_AddMedia.Location = new System.Drawing.Point(154, 304);
            btn_AddMedia.Margin = new Padding(2, 3, 2, 3);
            btn_AddMedia.Name = "btn_AddMedia";
            btn_AddMedia.Size = new System.Drawing.Size(79, 23);
            btn_AddMedia.TabIndex = 169;
            btn_AddMedia.Text = "Add";
            btn_AddMedia.UseVisualStyleBackColor = true;
            btn_AddMedia.Click += btn_AddMedia_Click;
            // 
            // btn_DeclineCall
            // 
            btn_DeclineCall.FlatAppearance.BorderSize = 0;
            btn_DeclineCall.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            btn_DeclineCall.Location = new System.Drawing.Point(629, 92);
            btn_DeclineCall.Margin = new Padding(0);
            btn_DeclineCall.Name = "btn_DeclineCall";
            btn_DeclineCall.Size = new System.Drawing.Size(68, 23);
            btn_DeclineCall.TabIndex = 165;
            btn_DeclineCall.Text = "Decline";
            btn_DeclineCall.UseVisualStyleBackColor = true;
            btn_DeclineCall.Click += btn_DeclineCall_Click;
            // 
            // btn_HangUp
            // 
            btn_HangUp.Location = new System.Drawing.Point(154, 246);
            btn_HangUp.Margin = new Padding(2, 3, 2, 3);
            btn_HangUp.Name = "btn_HangUp";
            btn_HangUp.Size = new System.Drawing.Size(81, 23);
            btn_HangUp.TabIndex = 164;
            btn_HangUp.Text = "Hang Up";
            btn_HangUp.UseVisualStyleBackColor = true;
            btn_HangUp.Click += btn_HangUp_Click;
            // 
            // btn_MakeCall
            // 
            btn_MakeCall.Location = new System.Drawing.Point(828, 63);
            btn_MakeCall.Margin = new Padding(2, 3, 2, 3);
            btn_MakeCall.Name = "btn_MakeCall";
            btn_MakeCall.Size = new System.Drawing.Size(82, 23);
            btn_MakeCall.TabIndex = 163;
            btn_MakeCall.Text = "Make Call";
            btn_MakeCall.UseVisualStyleBackColor = true;
            btn_MakeCall.Click += btn_MakeCall_Click;
            // 
            // cb_MakeCallMedias
            // 
            cb_MakeCallMedias.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_MakeCallMedias.FormattingEnabled = true;
            cb_MakeCallMedias.Location = new System.Drawing.Point(701, 63);
            cb_MakeCallMedias.Margin = new Padding(2, 3, 2, 3);
            cb_MakeCallMedias.Name = "cb_MakeCallMedias";
            cb_MakeCallMedias.Size = new System.Drawing.Size(123, 23);
            cb_MakeCallMedias.TabIndex = 162;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label5.Location = new System.Drawing.Point(452, 19);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(57, 13);
            label5.TabIndex = 161;
            label5.Text = "Contacts";
            // 
            // cb_ContactsList
            // 
            cb_ContactsList.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_ContactsList.FormattingEnabled = true;
            cb_ContactsList.Location = new System.Drawing.Point(452, 35);
            cb_ContactsList.Margin = new Padding(2, 3, 2, 3);
            cb_ContactsList.Name = "cb_ContactsList";
            cb_ContactsList.Size = new System.Drawing.Size(245, 23);
            cb_ContactsList.TabIndex = 160;
            cb_ContactsList.SelectedIndexChanged += cb_ContactsList_SelectedIndexChanged;
            // 
            // lbl_ConferencesInProgress
            // 
            lbl_ConferencesInProgress.AutoSize = true;
            lbl_ConferencesInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            lbl_ConferencesInProgress.ForeColor = System.Drawing.Color.Firebrick;
            lbl_ConferencesInProgress.Location = new System.Drawing.Point(452, 172);
            lbl_ConferencesInProgress.Margin = new Padding(2, 0, 2, 0);
            lbl_ConferencesInProgress.Name = "lbl_ConferencesInProgress";
            lbl_ConferencesInProgress.Size = new System.Drawing.Size(74, 13);
            lbl_ConferencesInProgress.TabIndex = 186;
            lbl_ConferencesInProgress.Text = "In progress:";
            // 
            // btn_StartConf
            // 
            btn_StartConf.Location = new System.Drawing.Point(701, 138);
            btn_StartConf.Margin = new Padding(2, 3, 2, 3);
            btn_StartConf.Name = "btn_StartConf";
            btn_StartConf.Size = new System.Drawing.Size(82, 23);
            btn_StartConf.TabIndex = 182;
            btn_StartConf.Text = "Start Conf";
            btn_StartConf.UseVisualStyleBackColor = true;
            btn_StartConf.Click += btn_StartConf_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label11.Location = new System.Drawing.Point(452, 122);
            label11.Margin = new Padding(2, 0, 2, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(52, 13);
            label11.TabIndex = 181;
            label11.Text = "Bubbles";
            // 
            // cb_BubblesList
            // 
            cb_BubblesList.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_BubblesList.FormattingEnabled = true;
            cb_BubblesList.Location = new System.Drawing.Point(452, 138);
            cb_BubblesList.Margin = new Padding(2, 3, 2, 3);
            cb_BubblesList.Name = "cb_BubblesList";
            cb_BubblesList.Size = new System.Drawing.Size(245, 23);
            cb_BubblesList.TabIndex = 180;
            cb_BubblesList.SelectedIndexChanged += cb_BubblesList_SelectedIndexChanged;
            // 
            // btn_JoinConf
            // 
            btn_JoinConf.Location = new System.Drawing.Point(701, 167);
            btn_JoinConf.Margin = new Padding(2, 3, 2, 3);
            btn_JoinConf.Name = "btn_JoinConf";
            btn_JoinConf.Size = new System.Drawing.Size(82, 23);
            btn_JoinConf.TabIndex = 179;
            btn_JoinConf.Text = "Join Conf";
            btn_JoinConf.UseVisualStyleBackColor = true;
            btn_JoinConf.Click += btn_JoinConf_Click;
            // 
            // btn_ManageInputStreams
            // 
            btn_ManageInputStreams.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btn_ManageInputStreams.ForeColor = System.Drawing.Color.SaddleBrown;
            btn_ManageInputStreams.Location = new System.Drawing.Point(12, 12);
            btn_ManageInputStreams.Name = "btn_ManageInputStreams";
            btn_ManageInputStreams.Size = new System.Drawing.Size(163, 23);
            btn_ManageInputStreams.TabIndex = 187;
            btn_ManageInputStreams.Text = "Manage Input Streams";
            btn_ManageInputStreams.UseVisualStyleBackColor = true;
            btn_ManageInputStreams.Click += btn_ManageInputStreams_Click;
            // 
            // btn_RefreshAudioOutput
            // 
            btn_RefreshAudioOutput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_RefreshAudioOutput.FlatStyle = FlatStyle.Flat;
            btn_RefreshAudioOutput.ForeColor = System.Drawing.SystemColors.Control;
            btn_RefreshAudioOutput.Location = new System.Drawing.Point(362, 153);
            btn_RefreshAudioOutput.Name = "btn_RefreshAudioOutput";
            btn_RefreshAudioOutput.Size = new System.Drawing.Size(16, 16);
            btn_RefreshAudioOutput.TabIndex = 333;
            btn_RefreshAudioOutput.UseVisualStyleBackColor = true;
            btn_RefreshAudioOutput.Click += btb_AudioOutputListRefresh_Click;
            // 
            // btn_RefreshAudioInput
            // 
            btn_RefreshAudioInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_RefreshAudioInput.FlatStyle = FlatStyle.Flat;
            btn_RefreshAudioInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_RefreshAudioInput.Location = new System.Drawing.Point(401, 124);
            btn_RefreshAudioInput.Name = "btn_RefreshAudioInput";
            btn_RefreshAudioInput.Size = new System.Drawing.Size(16, 16);
            btn_RefreshAudioInput.TabIndex = 334;
            btn_RefreshAudioInput.UseVisualStyleBackColor = true;
            btn_RefreshAudioInput.Click += btb_AudioInputListRefresh_Click;
            // 
            // btn_LoadConfig
            // 
            btn_LoadConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btn_LoadConfig.ForeColor = System.Drawing.Color.SaddleBrown;
            btn_LoadConfig.Location = new System.Drawing.Point(198, 12);
            btn_LoadConfig.Name = "btn_LoadConfig";
            btn_LoadConfig.Size = new System.Drawing.Size(163, 23);
            btn_LoadConfig.TabIndex = 335;
            btn_LoadConfig.Text = "Load Configuration";
            btn_LoadConfig.UseVisualStyleBackColor = true;
            btn_LoadConfig.Click += btn_LoadConfig_Click;
            // 
            // btn_OutputVideoInput
            // 
            btn_OutputVideoInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_OutputVideoInput.FlatStyle = FlatStyle.Flat;
            btn_OutputVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputVideoInput.Location = new System.Drawing.Point(401, 58);
            btn_OutputVideoInput.Name = "btn_OutputVideoInput";
            btn_OutputVideoInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputVideoInput.TabIndex = 336;
            btn_OutputVideoInput.UseVisualStyleBackColor = true;
            btn_OutputVideoInput.Click += btn_OutputVideoInput_Click;
            // 
            // btn_OutputSharingInput
            // 
            btn_OutputSharingInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_OutputSharingInput.FlatStyle = FlatStyle.Flat;
            btn_OutputSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputSharingInput.Location = new System.Drawing.Point(401, 87);
            btn_OutputSharingInput.Name = "btn_OutputSharingInput";
            btn_OutputSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputSharingInput.TabIndex = 337;
            btn_OutputSharingInput.UseVisualStyleBackColor = true;
            btn_OutputSharingInput.Click += btn_OutputSharingInput_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label9.Location = new System.Drawing.Point(454, 206);
            label9.Margin = new Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(74, 13);
            label9.TabIndex = 339;
            label9.Text = "Information:";
            // 
            // tb_Information
            // 
            tb_Information.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            tb_Information.Location = new System.Drawing.Point(452, 222);
            tb_Information.Margin = new Padding(2, 3, 2, 3);
            tb_Information.Multiline = true;
            tb_Information.Name = "tb_Information";
            tb_Information.ScrollBars = ScrollBars.Vertical;
            tb_Information.Size = new System.Drawing.Size(480, 337);
            tb_Information.TabIndex = 338;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            label10.Location = new System.Drawing.Point(12, 201);
            label10.Margin = new Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(85, 13);
            label10.TabIndex = 340;
            label10.Text = "Conversation:";
            // 
            // lbl_IncomingCall
            // 
            lbl_IncomingCall.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            lbl_IncomingCall.ForeColor = System.Drawing.Color.Firebrick;
            lbl_IncomingCall.Location = new System.Drawing.Point(452, 96);
            lbl_IncomingCall.Margin = new Padding(2, 0, 2, 0);
            lbl_IncomingCall.Name = "lbl_IncomingCall";
            lbl_IncomingCall.Size = new System.Drawing.Size(175, 18);
            lbl_IncomingCall.TabIndex = 342;
            lbl_IncomingCall.Text = "Incoming call";
            lbl_IncomingCall.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_ConversationDetails
            // 
            lbl_ConversationDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            lbl_ConversationDetails.ForeColor = System.Drawing.Color.Blue;
            lbl_ConversationDetails.Location = new System.Drawing.Point(12, 219);
            lbl_ConversationDetails.Margin = new Padding(2, 0, 2, 0);
            lbl_ConversationDetails.Name = "lbl_ConversationDetails";
            lbl_ConversationDetails.Size = new System.Drawing.Size(349, 16);
            lbl_ConversationDetails.TabIndex = 344;
            lbl_ConversationDetails.Text = "details";
            // 
            // lbl_BubbleInfo
            // 
            lbl_BubbleInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            lbl_BubbleInfo.Location = new System.Drawing.Point(787, 143);
            lbl_BubbleInfo.Margin = new Padding(2, 0, 2, 0);
            lbl_BubbleInfo.Name = "lbl_BubbleInfo";
            lbl_BubbleInfo.Size = new System.Drawing.Size(145, 17);
            lbl_BubbleInfo.TabIndex = 346;
            lbl_BubbleInfo.Text = "bubble info";
            // 
            // btn_AnswerCall
            // 
            btn_AnswerCall.Location = new System.Drawing.Point(828, 92);
            btn_AnswerCall.Margin = new Padding(2, 3, 2, 3);
            btn_AnswerCall.Name = "btn_AnswerCall";
            btn_AnswerCall.Size = new System.Drawing.Size(82, 23);
            btn_AnswerCall.TabIndex = 347;
            btn_AnswerCall.Text = "Answer";
            btn_AnswerCall.UseVisualStyleBackColor = true;
            btn_AnswerCall.Click += btn_AnswerCall_Click;
            // 
            // cb_AnswerCallMedias
            // 
            cb_AnswerCallMedias.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_AnswerCallMedias.FormattingEnabled = true;
            cb_AnswerCallMedias.Location = new System.Drawing.Point(701, 92);
            cb_AnswerCallMedias.Margin = new Padding(2, 3, 2, 3);
            cb_AnswerCallMedias.Name = "cb_AnswerCallMedias";
            cb_AnswerCallMedias.Size = new System.Drawing.Size(123, 23);
            cb_AnswerCallMedias.TabIndex = 348;
            // 
            // cb_RemoveMedia
            // 
            cb_RemoveMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_RemoveMedia.FormattingEnabled = true;
            cb_RemoveMedia.Location = new System.Drawing.Point(24, 276);
            cb_RemoveMedia.Margin = new Padding(2, 3, 2, 3);
            cb_RemoveMedia.Name = "cb_RemoveMedia";
            cb_RemoveMedia.Size = new System.Drawing.Size(123, 23);
            cb_RemoveMedia.TabIndex = 350;
            cb_RemoveMedia.SelectedIndexChanged += cb_RemoveMedia_SelectedIndexChanged;
            // 
            // cb_AddMedia
            // 
            cb_AddMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_AddMedia.FormattingEnabled = true;
            cb_AddMedia.Location = new System.Drawing.Point(24, 305);
            cb_AddMedia.Margin = new Padding(2, 3, 2, 3);
            cb_AddMedia.Name = "cb_AddMedia";
            cb_AddMedia.Size = new System.Drawing.Size(123, 23);
            cb_AddMedia.TabIndex = 349;
            cb_AddMedia.SelectedIndexChanged += cb_AddMedia_SelectedIndexChanged;
            // 
            // cb_UnmuteMedia
            // 
            cb_UnmuteMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_UnmuteMedia.FormattingEnabled = true;
            cb_UnmuteMedia.Location = new System.Drawing.Point(24, 376);
            cb_UnmuteMedia.Margin = new Padding(2, 3, 2, 3);
            cb_UnmuteMedia.Name = "cb_UnmuteMedia";
            cb_UnmuteMedia.Size = new System.Drawing.Size(123, 23);
            cb_UnmuteMedia.TabIndex = 353;
            // 
            // btn_OutputLocalVideoInput
            // 
            btn_OutputLocalVideoInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_OutputLocalVideoInput.FlatStyle = FlatStyle.Flat;
            btn_OutputLocalVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputLocalVideoInput.Location = new System.Drawing.Point(106, 459);
            btn_OutputLocalVideoInput.Name = "btn_OutputLocalVideoInput";
            btn_OutputLocalVideoInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputLocalVideoInput.TabIndex = 356;
            btn_OutputLocalVideoInput.UseVisualStyleBackColor = true;
            btn_OutputLocalVideoInput.Click += btn_OutputLocalVideoInput_Click;
            // 
            // btn_OutputLocalSharingInput
            // 
            btn_OutputLocalSharingInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_OutputLocalSharingInput.FlatStyle = FlatStyle.Flat;
            btn_OutputLocalSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputLocalSharingInput.Location = new System.Drawing.Point(107, 482);
            btn_OutputLocalSharingInput.Name = "btn_OutputLocalSharingInput";
            btn_OutputLocalSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputLocalSharingInput.TabIndex = 357;
            btn_OutputLocalSharingInput.UseVisualStyleBackColor = true;
            btn_OutputLocalSharingInput.Click += btn_OutputLocalSharingInput_Click;
            // 
            // btn_StopVideoInput
            // 
            btn_StopVideoInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_StopVideoInput.FlatStyle = FlatStyle.Flat;
            btn_StopVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_StopVideoInput.Location = new System.Drawing.Point(381, 58);
            btn_StopVideoInput.Name = "btn_StopVideoInput";
            btn_StopVideoInput.Size = new System.Drawing.Size(16, 16);
            btn_StopVideoInput.TabIndex = 359;
            btn_StopVideoInput.UseVisualStyleBackColor = true;
            btn_StopVideoInput.Click += btn_StoptMediaInput_Click;
            // 
            // btn_StartVideoInput
            // 
            btn_StartVideoInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_StartVideoInput.FlatStyle = FlatStyle.Flat;
            btn_StartVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_StartVideoInput.Location = new System.Drawing.Point(362, 58);
            btn_StartVideoInput.Name = "btn_StartVideoInput";
            btn_StartVideoInput.Size = new System.Drawing.Size(16, 16);
            btn_StartVideoInput.TabIndex = 358;
            btn_StartVideoInput.UseVisualStyleBackColor = true;
            btn_StartVideoInput.Click += btn_StartMediaInput_Click;
            // 
            // btn_StopSharingInput
            // 
            btn_StopSharingInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_StopSharingInput.FlatStyle = FlatStyle.Flat;
            btn_StopSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_StopSharingInput.Location = new System.Drawing.Point(381, 87);
            btn_StopSharingInput.Name = "btn_StopSharingInput";
            btn_StopSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_StopSharingInput.TabIndex = 361;
            btn_StopSharingInput.UseVisualStyleBackColor = true;
            btn_StopSharingInput.Click += btn_StoptMediaInput_Click;
            // 
            // btn_StartSharingInput
            // 
            btn_StartSharingInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_StartSharingInput.FlatStyle = FlatStyle.Flat;
            btn_StartSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_StartSharingInput.Location = new System.Drawing.Point(362, 87);
            btn_StartSharingInput.Name = "btn_StartSharingInput";
            btn_StartSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_StartSharingInput.TabIndex = 360;
            btn_StartSharingInput.UseVisualStyleBackColor = true;
            btn_StartSharingInput.Click += btn_StartMediaInput_Click;
            // 
            // btn_StopAudioInput
            // 
            btn_StopAudioInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_StopAudioInput.FlatStyle = FlatStyle.Flat;
            btn_StopAudioInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_StopAudioInput.Location = new System.Drawing.Point(381, 124);
            btn_StopAudioInput.Name = "btn_StopAudioInput";
            btn_StopAudioInput.Size = new System.Drawing.Size(16, 16);
            btn_StopAudioInput.TabIndex = 363;
            btn_StopAudioInput.UseVisualStyleBackColor = true;
            btn_StopAudioInput.Click += btn_StoptMediaInput_Click;
            // 
            // btn_StartAudioInput
            // 
            btn_StartAudioInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_StartAudioInput.FlatStyle = FlatStyle.Flat;
            btn_StartAudioInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_StartAudioInput.Location = new System.Drawing.Point(362, 124);
            btn_StartAudioInput.Name = "btn_StartAudioInput";
            btn_StartAudioInput.Size = new System.Drawing.Size(16, 16);
            btn_StartAudioInput.TabIndex = 362;
            btn_StartAudioInput.UseVisualStyleBackColor = true;
            btn_StartAudioInput.Click += btn_StartMediaInput_Click;
            // 
            // btn_DeclineConf
            // 
            btn_DeclineConf.FlatAppearance.BorderSize = 0;
            btn_DeclineConf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            btn_DeclineConf.Location = new System.Drawing.Point(787, 167);
            btn_DeclineConf.Margin = new Padding(0);
            btn_DeclineConf.Name = "btn_DeclineConf";
            btn_DeclineConf.Size = new System.Drawing.Size(68, 23);
            btn_DeclineConf.TabIndex = 364;
            btn_DeclineConf.Text = "Decline";
            btn_DeclineConf.UseVisualStyleBackColor = true;
            btn_DeclineConf.Click += btn_DeclineConf_Click;
            // 
            // btn_ConferenceOptions
            // 
            btn_ConferenceOptions.Enabled = false;
            btn_ConferenceOptions.Location = new System.Drawing.Point(173, 531);
            btn_ConferenceOptions.Margin = new Padding(2, 3, 2, 3);
            btn_ConferenceOptions.Name = "btn_ConferenceOptions";
            btn_ConferenceOptions.Size = new System.Drawing.Size(144, 23);
            btn_ConferenceOptions.TabIndex = 365;
            btn_ConferenceOptions.Text = "Conference options ...";
            btn_ConferenceOptions.UseVisualStyleBackColor = true;
            btn_ConferenceOptions.Click += btn_ConferenceOptions_Click;
            // 
            // lbl_ConversationInProgress
            // 
            lbl_ConversationInProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            lbl_ConversationInProgress.ForeColor = System.Drawing.Color.Firebrick;
            lbl_ConversationInProgress.Location = new System.Drawing.Point(101, 201);
            lbl_ConversationInProgress.Margin = new Padding(2, 0, 2, 0);
            lbl_ConversationInProgress.Name = "lbl_ConversationInProgress";
            lbl_ConversationInProgress.Size = new System.Drawing.Size(82, 18);
            lbl_ConversationInProgress.TabIndex = 1299;
            lbl_ConversationInProgress.Text = "In progress";
            lbl_ConversationInProgress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cb_ConferencesName
            // 
            cb_ConferencesName.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_ConferencesName.FormattingEnabled = true;
            cb_ConferencesName.Location = new System.Drawing.Point(530, 168);
            cb_ConferencesName.Margin = new Padding(2, 3, 2, 3);
            cb_ConferencesName.Name = "cb_ConferencesName";
            cb_ConferencesName.Size = new System.Drawing.Size(167, 23);
            cb_ConferencesName.TabIndex = 1300;
            // 
            // btn_AddAsParticipant
            // 
            btn_AddAsParticipant.Enabled = false;
            btn_AddAsParticipant.ForeColor = System.Drawing.SystemColors.ControlText;
            btn_AddAsParticipant.Location = new System.Drawing.Point(702, 35);
            btn_AddAsParticipant.Margin = new Padding(2, 3, 2, 3);
            btn_AddAsParticipant.Name = "btn_AddAsParticipant";
            btn_AddAsParticipant.Size = new System.Drawing.Size(122, 23);
            btn_AddAsParticipant.TabIndex = 1301;
            btn_AddAsParticipant.Text = "Add as participant";
            btn_AddAsParticipant.UseVisualStyleBackColor = true;
            // 
            // btn_OutputRemoteVideoInput
            // 
            btn_OutputRemoteVideoInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_OutputRemoteVideoInput.FlatStyle = FlatStyle.Flat;
            btn_OutputRemoteVideoInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputRemoteVideoInput.Location = new System.Drawing.Point(342, 459);
            btn_OutputRemoteVideoInput.Name = "btn_OutputRemoteVideoInput";
            btn_OutputRemoteVideoInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputRemoteVideoInput.TabIndex = 1302;
            btn_OutputRemoteVideoInput.UseVisualStyleBackColor = true;
            btn_OutputRemoteVideoInput.Click += btn_OutputRemoteVideoInput_Click;
            // 
            // btn_OutputRemoteSharingInput
            // 
            btn_OutputRemoteSharingInput.BackgroundImageLayout = ImageLayout.Stretch;
            btn_OutputRemoteSharingInput.FlatStyle = FlatStyle.Flat;
            btn_OutputRemoteSharingInput.ForeColor = System.Drawing.SystemColors.Control;
            btn_OutputRemoteSharingInput.Location = new System.Drawing.Point(342, 482);
            btn_OutputRemoteSharingInput.Name = "btn_OutputRemoteSharingInput";
            btn_OutputRemoteSharingInput.Size = new System.Drawing.Size(16, 16);
            btn_OutputRemoteSharingInput.TabIndex = 1303;
            btn_OutputRemoteSharingInput.UseVisualStyleBackColor = true;
            btn_OutputRemoteSharingInput.Click += btn_OutputRemoteSharingInput_Click;
            // 
            // btn_SubscribeRemoteAudioInput
            // 
            btn_SubscribeRemoteAudioInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteAudioInput.ForeColor = System.Drawing.Color.DarkGreen;
            btn_SubscribeRemoteAudioInput.Location = new System.Drawing.Point(266, 433);
            btn_SubscribeRemoteAudioInput.Margin = new Padding(4, 3, 4, 3);
            btn_SubscribeRemoteAudioInput.Name = "btn_SubscribeRemoteAudioInput";
            btn_SubscribeRemoteAudioInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteAudioInput.TabIndex = 1305;
            btn_SubscribeRemoteAudioInput.Text = "Unsubscribe";
            btn_SubscribeRemoteAudioInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteAudioInput.Click += btn_SubscribeRemoteAudioInput_Click;
            // 
            // btn_SubscribeRemoteVideoInput
            // 
            btn_SubscribeRemoteVideoInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteVideoInput.Location = new System.Drawing.Point(266, 457);
            btn_SubscribeRemoteVideoInput.Margin = new Padding(4, 3, 4, 3);
            btn_SubscribeRemoteVideoInput.Name = "btn_SubscribeRemoteVideoInput";
            btn_SubscribeRemoteVideoInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteVideoInput.TabIndex = 1306;
            btn_SubscribeRemoteVideoInput.Text = "Unsubscribe";
            btn_SubscribeRemoteVideoInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteVideoInput.Click += btn_SubscribeRemoteVideoInput_Click;
            // 
            // btn_SubscribeRemoteSharingInput
            // 
            btn_SubscribeRemoteSharingInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteSharingInput.Location = new System.Drawing.Point(266, 480);
            btn_SubscribeRemoteSharingInput.Margin = new Padding(4, 3, 4, 3);
            btn_SubscribeRemoteSharingInput.Name = "btn_SubscribeRemoteSharingInput";
            btn_SubscribeRemoteSharingInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteSharingInput.TabIndex = 1307;
            btn_SubscribeRemoteSharingInput.Text = "Unsubscribe";
            btn_SubscribeRemoteSharingInput.UseVisualStyleBackColor = true;
            btn_SubscribeRemoteSharingInput.Click += btn_SubscribeRemoteSharingInput_Click;
            // 
            // btn_Details
            // 
            btn_Details.Location = new System.Drawing.Point(280, 196);
            btn_Details.Margin = new Padding(2, 3, 2, 3);
            btn_Details.Name = "btn_Details";
            btn_Details.Size = new System.Drawing.Size(81, 23);
            btn_Details.TabIndex = 1308;
            btn_Details.Text = "Get details";
            btn_Details.UseVisualStyleBackColor = true;
            btn_Details.Click += btn_Details_Click;
            // 
            // check_LocalDataChannel
            // 
            check_LocalDataChannel.AutoCheck = false;
            check_LocalDataChannel.AutoSize = true;
            check_LocalDataChannel.Location = new System.Drawing.Point(45, 506);
            check_LocalDataChannel.Margin = new Padding(2, 3, 2, 3);
            check_LocalDataChannel.Name = "check_LocalDataChannel";
            check_LocalDataChannel.Size = new System.Drawing.Size(94, 19);
            check_LocalDataChannel.TabIndex = 1309;
            check_LocalDataChannel.Text = "DataChannel";
            check_LocalDataChannel.UseVisualStyleBackColor = true;
            // 
            // btn_SubscribeRemoteDataChannelInput
            // 
            btn_SubscribeRemoteDataChannelInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_SubscribeRemoteDataChannelInput.Location = new System.Drawing.Point(266, 503);
            btn_SubscribeRemoteDataChannelInput.Margin = new Padding(4, 3, 4, 3);
            btn_SubscribeRemoteDataChannelInput.Name = "btn_SubscribeRemoteDataChannelInput";
            btn_SubscribeRemoteDataChannelInput.Size = new System.Drawing.Size(73, 20);
            btn_SubscribeRemoteDataChannelInput.TabIndex = 1311;
            btn_SubscribeRemoteDataChannelInput.Text = "Unsubscribe";
            btn_SubscribeRemoteDataChannelInput.UseVisualStyleBackColor = true;
            // 
            // check_RemoteDataChannel
            // 
            check_RemoteDataChannel.AutoCheck = false;
            check_RemoteDataChannel.AutoSize = true;
            check_RemoteDataChannel.Location = new System.Drawing.Point(174, 506);
            check_RemoteDataChannel.Margin = new Padding(2, 3, 2, 3);
            check_RemoteDataChannel.Name = "check_RemoteDataChannel";
            check_RemoteDataChannel.Size = new System.Drawing.Size(94, 19);
            check_RemoteDataChannel.TabIndex = 1310;
            check_RemoteDataChannel.Text = "DataChannel";
            check_RemoteDataChannel.UseVisualStyleBackColor = true;
            // 
            // tbDataChannel
            // 
            tbDataChannel.Location = new System.Drawing.Point(237, 276);
            tbDataChannel.Margin = new Padding(4, 3, 4, 3);
            tbDataChannel.Name = "tbDataChannel";
            tbDataChannel.Size = new System.Drawing.Size(160, 23);
            tbDataChannel.TabIndex = 1312;
            // 
            // btn_DataChannelSend
            // 
            btn_DataChannelSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_DataChannelSend.Location = new System.Drawing.Point(405, 278);
            btn_DataChannelSend.Margin = new Padding(4, 3, 4, 3);
            btn_DataChannelSend.Name = "btn_DataChannelSend";
            btn_DataChannelSend.Size = new System.Drawing.Size(41, 20);
            btn_DataChannelSend.TabIndex = 1313;
            btn_DataChannelSend.Text = "Send";
            btn_DataChannelSend.UseVisualStyleBackColor = true;
            btn_DataChannelSend.Click += btn_DataChannelSend_Click;
            // 
            // btn_InformationClear
            // 
            btn_InformationClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            btn_InformationClear.Location = new System.Drawing.Point(534, 201);
            btn_InformationClear.Margin = new Padding(4, 3, 4, 3);
            btn_InformationClear.Name = "btn_InformationClear";
            btn_InformationClear.Size = new System.Drawing.Size(39, 20);
            btn_InformationClear.TabIndex = 1314;
            btn_InformationClear.Text = "Clear";
            btn_InformationClear.UseVisualStyleBackColor = true;
            btn_InformationClear.Click += btn_InformationClear_Click;
            // 
            // FormWebRTC
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(943, 566);
            Controls.Add(btn_InformationClear);
            Controls.Add(btn_DataChannelSend);
            Controls.Add(tbDataChannel);
            Controls.Add(btn_SubscribeRemoteDataChannelInput);
            Controls.Add(check_RemoteDataChannel);
            Controls.Add(check_LocalDataChannel);
            Controls.Add(btn_Details);
            Controls.Add(btn_SubscribeRemoteSharingInput);
            Controls.Add(btn_SubscribeRemoteVideoInput);
            Controls.Add(btn_SubscribeRemoteAudioInput);
            Controls.Add(btn_OutputRemoteSharingInput);
            Controls.Add(btn_OutputRemoteVideoInput);
            Controls.Add(btn_AddAsParticipant);
            Controls.Add(cb_ConferencesName);
            Controls.Add(lbl_ConversationInProgress);
            Controls.Add(btn_ConferenceOptions);
            Controls.Add(btn_DeclineConf);
            Controls.Add(btn_StopAudioInput);
            Controls.Add(btn_StartAudioInput);
            Controls.Add(btn_StopSharingInput);
            Controls.Add(btn_StartSharingInput);
            Controls.Add(btn_StopVideoInput);
            Controls.Add(btn_StartVideoInput);
            Controls.Add(btn_OutputLocalSharingInput);
            Controls.Add(btn_OutputLocalVideoInput);
            Controls.Add(cb_UnmuteMedia);
            Controls.Add(cb_RemoveMedia);
            Controls.Add(cb_AddMedia);
            Controls.Add(cb_AnswerCallMedias);
            Controls.Add(btn_AnswerCall);
            Controls.Add(lbl_BubbleInfo);
            Controls.Add(lbl_ConversationDetails);
            Controls.Add(lbl_IncomingCall);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(tb_Information);
            Controls.Add(btn_OutputSharingInput);
            Controls.Add(btn_OutputVideoInput);
            Controls.Add(btn_LoadConfig);
            Controls.Add(btn_RefreshAudioInput);
            Controls.Add(btn_RefreshAudioOutput);
            Controls.Add(btn_ManageInputStreams);
            Controls.Add(lbl_ConferencesInProgress);
            Controls.Add(btn_StartConf);
            Controls.Add(label11);
            Controls.Add(cb_BubblesList);
            Controls.Add(btn_JoinConf);
            Controls.Add(btn_AskToShare);
            Controls.Add(btn_UnmuteMedia);
            Controls.Add(btn_MuteMedia);
            Controls.Add(cb_MuteMedia);
            Controls.Add(label8);
            Controls.Add(tb_Subject);
            Controls.Add(btn_RemoveMedia);
            Controls.Add(btn_AddMedia);
            Controls.Add(btn_DeclineCall);
            Controls.Add(btn_HangUp);
            Controls.Add(btn_MakeCall);
            Controls.Add(cb_MakeCallMedias);
            Controls.Add(label5);
            Controls.Add(cb_ContactsList);
            Controls.Add(label7);
            Controls.Add(check_RemoteSharing);
            Controls.Add(check_RemoteVideo);
            Controls.Add(check_RemoteAudio);
            Controls.Add(label6);
            Controls.Add(check_LocalSharing);
            Controls.Add(check_LocalVideo);
            Controls.Add(check_LocalAudio);
            Controls.Add(cb_SharingInputs);
            Controls.Add(label3);
            Controls.Add(cb_VideoInputs);
            Controls.Add(label2);
            Controls.Add(cb_AudioInputs);
            Controls.Add(label1);
            Controls.Add(cb_AudioOutputs);
            Controls.Add(label4);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormWebRTC";
            Text = "WebRtcForm";
            FormClosing += FormWebRTC_FormClosing;
            ResumeLayout(false);
            PerformLayout();
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
        private Label lbl_BubbleInfo;
        private Button btn_AnswerCall;
        private ComboBox cb_AnswerCallMedias;
        private ComboBox cb_RemoveMedia;
        private ComboBox cb_AddMedia;
        private ComboBox cb_UnmuteMedia;
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
        private Button btn_OutputRemoteVideoInput;
        private Button btn_OutputRemoteSharingInput;
        private ToolTip toolTip1;
        private Button btn_SubscribeRemoteAudioInput;
        private Button btn_SubscribeRemoteVideoInput;
        private Button btn_SubscribeRemoteSharingInput;
        private Button btn_Test;
        private CheckBox check_LocalDataChannel;
        private Button btn_SubscribeRemoteDataChannelInput;
        private CheckBox check_RemoteDataChannel;
        private TextBox tbDataChannel;
        private Button btn_DataChannelSend;
        private Button btn_InformationClear;
        private Button btn_Details;
    }
}
