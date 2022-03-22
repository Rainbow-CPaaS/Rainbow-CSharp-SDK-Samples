using Microsoft.Extensions.Logging;
using Rainbow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleChannels
{
    public partial class FormChannel : Form
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<FormChannel>(); // Good way to log with SDK 2.6.0 and more
        //private static readonly Logger log = LogConfigurator.GetLogger(typeof(FormChannel)); // Old way to log before SDK 2.6.0

        // Define SDK Objects
        Rainbow.Application? rbApplication = null;
        Rainbow.Contacts? rbContacts = null;
        Rainbow.Channels? rbChannels = null;

        String? currentContactId = null;

        List<Rainbow.Model.Channel>? channelsList = null;
        List<Rainbow.Model.Channel.Member>? channelMembersList = null;
        List<Rainbow.Model.Contact>? contactsList = null;
        String? channelIdSelected;
        String? avatarIdSelected;

        Random random = new Random();

        FormChannelItems formChannelItems;

        public FormChannel()
        {
            InitializeComponent();
        }

        public void Initialize(Rainbow.Application application)
        {
            if ( (application != null) && (rbApplication != application) )
            {
                // REmove previous event
                if(rbApplication != null)
                {
                    rbChannels.ChannelAvatarUpdated -= RbChannels_ChannelAvatarUpdated;
                    rbChannels.ChannelCreated -= RbChannels_ChannelCreated;
                    rbChannels.ChannelDeleted -= RbChannels_ChannelDeleted;
                    rbChannels.ChannelInfoUpdated -= RbChannels_ChannelInfoUpdated;
                    rbChannels.MemberUpdated -= RbChannels_MemberUpdated;
                }

                rbApplication = application;

                rbContacts = rbApplication.GetContacts();
                rbChannels = rbApplication.GetChannels();

                currentContactId = rbContacts.GetCurrentContactId();

                // Define events we want to manage
                rbChannels.ChannelAvatarUpdated += RbChannels_ChannelAvatarUpdated;
                rbChannels.ChannelCreated += RbChannels_ChannelCreated;
                rbChannels.ChannelDeleted += RbChannels_ChannelDeleted;
                rbChannels.ChannelInfoUpdated += RbChannels_ChannelInfoUpdated;
                rbChannels.MemberUpdated += RbChannels_MemberUpdated;
            }

            ResetData();

            UpdateFullUI();
        }

        private void ResetData()
        {
            channelsList = null;
            channelIdSelected = null;
            avatarIdSelected = null;
        }

        private void UpdateFullUI()
        {
            UpdateUIChannelsList();

            UpdateMembersList();

            UpdateContactsList();
        }

        private void UpdateUIChannelsList(Boolean newChannelCreatedOrDeleted = false)
        {
            if (this.InvokeRequired)
            {
                FormLogin.BoolArgReturningVoidDelegate d = new FormLogin.BoolArgReturningVoidDelegate(UpdateUIChannelsList);
                this.Invoke(d, new object[] { newChannelCreatedOrDeleted  });
            }
            else
            {
                if (newChannelCreatedOrDeleted)
                {
                    lbMembers.Items.Clear();

                    cbChannelInvited.Checked = false;
                    GetChannels();
                    return;
                }

                cbChannels.Items.Clear();

                if (channelsList?.Count > 0)
                {
                    ListItem item;
                    int index = 0;
                    int selectedIndex = -1;
                    foreach (var channel in channelsList)
                    {
                        item = new ListItem(channel.Id, channel.Name);
                        cbChannels.Items.Add(item);
                        if (channel.Id == channelIdSelected)
                            selectedIndex = index;
                        index++;
                    }
                    if(selectedIndex != -1)
                        cbChannels.SelectedIndex = selectedIndex;
                    else
                        cbChannels.SelectedIndex = 0;
                }
                UpdateUIChannelSelected();
            }
        }

        private void UpdateUIChannelSelected(Boolean userSelection = false)
        {
            if (this.InvokeRequired)
            {
                FormLogin.BoolArgReturningVoidDelegate d = new FormLogin.BoolArgReturningVoidDelegate(UpdateUIChannelSelected);
                this.Invoke(d, new object[] { userSelection });
            }
            else
            {
                if (userSelection)
                {
                    // We need to clear Members List since we changed selection
                    lbMembers.Items.Clear();
                }

                if (cbChannels.SelectedIndex > -1)
                {
                    var item = cbChannels.SelectedItem as ListItem;
                    channelIdSelected = item.Id;

                    var channel = rbChannels.GetChannelFromCache(channelIdSelected);
                    if (channel != null)
                    {
                        tbChannelId.Text = channel.Id;
                        tbChannelName.Text = channel.Name;
                        tbChannelCategory.Text = channel.Category;  
                        tbChannelTopic.Text = channel.Topic;

                        var contact = rbContacts.GetContactFromContactId(channel.CreatorId);
                        if (contact != null)
                            tbChannelCreator.Text = Rainbow.Util.GetContactDisplayName(contact);
                        else
                            tbChannelCreator.Text = channel.CreatorId;

                        tbChannelMode.Text = channel.Mode.ToString();

                        tbChannelNbUsers.Text = channel.UsersCount.ToString();
                        tbChannelNbSubscribers.Text = channel.SubscribersCount.ToString();
                        cbChannelMuted.Checked = channel.Mute;
                    }

                    gbAvatar.Enabled = true;
                    if (avatarIdSelected != channelIdSelected)
                        SetAvatarChannel(null);

                    btnAcceptInvitation.Enabled = (!channel.Subscribed) && channel.Invited;
                    btnDeclineInvitation.Enabled = (!channel.Subscribed) && channel.Invited;

                    if (channel.CreatorId == currentContactId)
                    {
                        btnUpdateChannel.Enabled = true;
                        btnDeleteChannel.Enabled = true;
                        btnUnsubscribe.Enabled = false;
                    }
                    else
                    {
                        btnUpdateChannel.Enabled = false;
                        btnDeleteChannel.Enabled = false;
                        btnUnsubscribe.Enabled = channel.Subscribed; 
                    }

                    btnChannelMute.Enabled = true;
                    btnManageItems.Enabled = true;

                    btnGetMembers.Enabled = true;
                    btnAddMember.Enabled = true;
                }
                else
                {
                    channelIdSelected = null;


                    tbChannelId.Text = "";
                    tbChannelName.Text = "";
                    tbChannelCategory.Text = "";
                    tbChannelTopic.Text = "";
                    tbChannelCreator.Text = "";
                    tbChannelMode.Text = "";
                    tbChannelNbUsers.Text = "";
                    tbChannelNbSubscribers.Text = "";
                    cbChannelMuted.Checked = false;


                    avatarIdSelected = null;
                    gbAvatar.Enabled = false;
                    SetAvatarChannel(null);

                    btnAcceptInvitation.Enabled = false;
                    btnDeclineInvitation.Enabled = false;

                    btnUnsubscribe.Enabled = false;

                    btnUpdateChannel.Enabled = false;
                    btnDeleteChannel.Enabled = false;

                    btnChannelMute.Enabled = false;
                    btnManageItems.Enabled = false;

                    btnGetMembers.Enabled = false;
                    btnAddMember.Enabled = false;

                }
            }
        }

        private void UpdateMembersList(Boolean memberAddodOrRemoved = false)
        {
            if (this.InvokeRequired)
            {
                FormLogin.BoolArgReturningVoidDelegate d = new FormLogin.BoolArgReturningVoidDelegate(UpdateMembersList);
                this.Invoke(d, new object[] { memberAddodOrRemoved });
            }
            else
            {
                lbMembers.Items.Clear();

                if (channelMembersList?.Count > 0)
                {
                    ListItem item;
                    Rainbow.Model.Contact contact;
                    String displayName;
                    foreach (var member in channelMembersList)
                    {
                        displayName = GetMemberDisplayName(member);

                        item = new ListItem(member.Id, displayName);
                        lbMembers.Items.Add(item);
                    }
                }
            }
        }

        private String GetMemberDisplayName(Rainbow.Model.Channel.Member member)
        {
            string displayName;
            var contact = rbContacts.GetContactFromContactId(member.Id);
            if (contact != null)
                displayName = Rainbow.Util.GetContactDisplayName(contact);
            else
                displayName = member.Id;

            displayName += $" [{member.Type}]";
            if ((!member.Subscribed) && member.Invited)
                displayName += $" [Invited]";

            return displayName;
        }

        private void UpdateContactsList()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateContactsList);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                lbContacts.Items.Clear();

                if (contactsList?.Count > 0)
                {
                    ListItem item;
                    String displayName;
                    foreach (var contact in contactsList)
                    {
                        if (contact.Id != currentContactId) // Avoid to display current user in the list
                        {
                            displayName = Rainbow.Util.GetContactDisplayName(contact);
                            item = new ListItem(contact.Id, displayName);
                            lbContacts.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void SetAvatarChannel(System.Drawing.Image? value)
        {
            if (this.InvokeRequired)
            {
                FormLogin.ImageArgReturningVoidDelegate d = new FormLogin.ImageArgReturningVoidDelegate(SetAvatarChannel);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                pbAvatar.Image = value;
                btnDeleteAvatar.Enabled = (value == null) ? false : true;
            }
        }

        private void AddInformation(String info)
        {
            if (this.InvokeRequired)
            {
                FormLogin.StringArgReturningVoidDelegate d = new FormLogin.StringArgReturningVoidDelegate(AddInformation);
                this.Invoke(d, new object[] { info });
            }
            else
            {
                if (!String.IsNullOrEmpty(info))
                {
                    log.LogDebug(info);
                    //log.Debug(info); // To log entry in the old way

                    if (!info.StartsWith("\r\n"))
                        info = "\r\n" + info;

                    tbInformation.Text += "\n" + info;

                    // Auto scrool to the end
                    tbInformation.SelectionStart = tbInformation.Text.Length;
                    tbInformation.ScrollToCaret();
                }
            }
        }

        private Bitmap? GetBitmapFromBytes(ref byte[] data)
        {
            Bitmap? result = null;
            try
            {
                using (var ms = new MemoryStream(data))
                    result = new Bitmap(System.Drawing.Image.FromStream(ms));
            }
            catch (Exception e)
            {
                AddInformation($"[GetImageFromBytes] Exception:[{Rainbow.Util.SerializeException(e)}]");
            }
            return result;
        }

    #region EVENTS FROM Rainbow Sdk objects

        private void RbChannels_MemberUpdated(object? sender, Rainbow.Events.ChannelMemberEventArgs e)
        {
            String displayName = GetMemberDisplayName(e.Member);
            AddInformation($"Event received - Member Updated:{displayName}");
        }

        private void RbChannels_ChannelDeleted(object? sender, Rainbow.Events.IdEventArgs e)
        {
            AddInformation($"Event received - Channel Deleted:{e.Id}");
        }

        private void RbChannels_ChannelInfoUpdated(object? sender, Rainbow.Events.IdEventArgs e)
        {
            AddInformation($"Event received - Channel Info Updated:{e.Id}");
        }

        private void RbChannels_ChannelCreated(object? sender, Rainbow.Events.IdEventArgs e)
        {
            AddInformation($"Event received - Channel Created:{e.Id}");
        }

        private void RbChannels_ChannelAvatarUpdated(object? sender, Rainbow.Events.ChannelAvatarEventArgs e)
        {
            AddInformation($"Event received - Avatar Updated:{e.ChannelId} - Status:{e.Status}");
        }

    #endregion EVENTS FROM Rainbow Sdk objects

    #region EVENTS FROM Form Elements
        
        public void GetChannels()
        {
            if (cbChannelInvited.Checked)
            {
                rbChannels.GetInvitedChannels(callback =>
                {
                    String output;
                    if (callback.Result.Success)
                    {
                        channelsList = callback.Data;
                        output = $"Nb Invited channels found :[{channelsList.Count}]";
                        AddInformation(output);

                        UpdateUIChannelsList();
                    }
                    else
                        AddInformation($"Pb to get Invited channels: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                });
            }
            else
            {
                rbChannels.GetSubscribedChannels(callback =>
                {
                    String output;
                    if (callback.Result.Success)
                    {
                        channelsList = callback.Data;
                        output = $"Nb Subscribed channels found :[{channelsList.Count}]";
                        AddInformation(output);

                        UpdateUIChannelsList();
                    }
                    else
                        AddInformation($"Pb to get Subscribed channels: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                });
            }
        }

        private void btnGetChannels_Click(object sender, EventArgs e)
        {
            // Since we ask a new list, we reset the selected Channel Id 
            channelIdSelected = null;

            GetChannels();
        }

        private void btnSearchChannels_Click(object sender, EventArgs e)
        {
            AddInformation("Not explained in this sample yet but available in the SDK");
        }
        
        private void cbChannels_SelectionChangeCommitted(object sender, EventArgs e)
        {
            UpdateUIChannelSelected(true);
        }

        private void btnAcceptInvitation_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.AcceptInvitation(channelIdSelected, callback =>
                {
                    if (callback.Result.Success)
                    {
                        channelIdSelected = null;
                        AddInformation("Channel invitation accepted succesfully");

                        UpdateUIChannelsList(true);
                    }
                    else
                    {
                        AddInformation($"Pb to accept invitation channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                    }

                });
            }
        }

        private void btnDeclineInvitation_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.DeclineInvitation(channelIdSelected, callback =>
                {
                    if (callback.Result.Success)
                    {
                        channelIdSelected = null;
                        AddInformation("Channel invitation declined succesfully");

                        UpdateUIChannelsList(true);
                    }
                    else
                    {
                        AddInformation($"Pb to decline invitation channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                    }

                });
            }
        }

        private void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.Unsubscribe(channelIdSelected, callback =>
                {
                    if (callback.Result.Success)
                    {
                        channelIdSelected = null;
                        AddInformation("Channel Unsubscribeed succesfully");

                        UpdateUIChannelsList(true);
                    }
                    else
                    {
                        AddInformation($"Pb to unsubscribe channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                    }

                });
            }
        }

        private void btnUpdateChannel_Click(object sender, EventArgs e)
        {
            Rainbow.Model.Channel channel = rbChannels.GetChannelFromCache(channelIdSelected);

            channel.Id = tbChannelId.Text;
            channel.Name = tbChannelName.Text;
            channel.Topic = tbChannelTopic.Text;
            channel.Category = tbChannelCategory.Text;
            channel.MaxItems = 80;
            channel.MaxPayloadSize = 44000;

            rbChannels.UpdateChannel(channel, callback =>
            {
                if (callback.Result.Success)
                {
                    var channel = callback.Data;
                    channelIdSelected = channel.Id;
                    AddInformation($"Channel updated succesfully - Id:[{channelIdSelected}]");

                    UpdateUIChannelsList(true);
                }
                else
                {
                    AddInformation($"Pb to update channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                }
            });
        }

        private void btnDeleteChannel_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.DeleteChannel(channelIdSelected, callback =>
                {
                    if (callback.Result.Success)
                    {
                        channelIdSelected = null;
                        AddInformation("Channel deleted succesfully");

                        UpdateUIChannelsList(true);
                    }
                    else
                    {
                        AddInformation($"Pb to delete channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                    }

                });
            }
        }

        private void btnCreateChannel_Click(object sender, EventArgs e)
        {
            Rainbow.Model.Channel channel = new Rainbow.Model.Channel();

            channel.Name = tbChannelName.Text;
            channel.Topic = tbChannelTopic.Text;
            channel.Category = tbChannelCategory.Text;
            channel.MaxItems = 80;
            channel.MaxPayloadSize = 44000;

            if (rbCompanyPublic.Checked)
                channel.Mode = Rainbow.Model.Channel.ChannelMode.COMPANY_PUBLIC;
            else if (rbCompanyPrivate.Checked)
                channel.Mode = Rainbow.Model.Channel.ChannelMode.COMPANY_PRIVATE;
            else
                channel.Mode = Rainbow.Model.Channel.ChannelMode.COMPANY_CLOSED;

            rbChannels.CreateChannel(channel, callback =>
            {
                if (callback.Result.Success)
                {
                    var channel = callback.Data;
                    channelIdSelected = channel.Id;
                    AddInformation($"Channel created succesfully - Id:[{channelIdSelected}]");

                    UpdateUIChannelsList(true);
                }
                else
                    AddInformation($"Pb to create channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
            });
        }

        private void btnChannelMute_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.MuteOrUnmute(channelIdSelected, !cbChannelMuted.Checked, callback =>
                {
                    if (callback.Result.Success)
                    {
                        channelIdSelected = null;
                        AddInformation("Channel muted / unmuted succesfully");

                        UpdateUIChannelsList(true);
                    }
                    else
                    {
                        AddInformation($"Pb to mute / unmute channel: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                    }

                });
            }
        }

        private void btnGetAvatar_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.GetAvatarFromChannelId(channelIdSelected, 80, callback =>
                {
                    if (callback.Result.Success)
                    {
                        byte[] data = callback.Data;
                        if (data?.Length > 0)
                        {
                            SetAvatarChannel(GetBitmapFromBytes(ref data));
                            AddInformation($"Get avatar succesfully");
                        }
                        else
                        {
                            SetAvatarChannel(null);
                            AddInformation($"Get avatar succesfully - but there is no avatar specified for this channel");
                        }
                    }
                    else
                        AddInformation($"Pb to get avatar: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                });
            }
        }

        private void btnDeleteAvatar_Click(object sender, EventArgs e)
        {
            if (channelIdSelected != null)
            {
                rbChannels.DeleteAvatarFromChannelId(channelIdSelected, callback =>
                {
                    if (callback.Result.Success)
                    {
                        SetAvatarChannel(null);
                        AddInformation($"Delete avatar succesfully");
                    }
                    else
                        AddInformation($"Pb to delete avatar: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                });
            }
        }

        private void btnUpdateAvatar_Click(object sender, EventArgs e)
        {
            if (channelIdSelected == null)
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image PNG (*.png)|*.png|Image JPG (*.jpg)|*.jpg";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string extension;
                    if (filePath != null)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            extension = Path.GetExtension(filePath).ToUpper();
                            extension = extension.Substring(1, extension.Length - 1);
                            if ((extension == "PNG") || (extension == "JPG"))
                            {
                                byte[] data = System.IO.File.ReadAllBytes(filePath);

                                rbChannels.UpdateAvatarFromChannelId(channelIdSelected, ref data, extension, callback =>
                                {
                                    if (callback.Result.Success)
                                    {
                                        AddInformation($"Avatar has been updated");
                                        btnGetAvatar_Click(this, null);
                                    }
                                    else
                                        AddInformation($"Not possible to update avatar: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                                });
                            }
                            else
                                AddInformation($"Image file can only be PNG or JPG.");
                        }
                        else
                            AddInformation($"Avatar can't be updated - file not exists");
                    }
                }
            }
        }

        private void btnGetContacts_Click(object sender, EventArgs e)
        {
            contactsList = rbContacts.GetAllContactsFromCache();
            AddInformation($"Contacts has been retrieved - nb:[{contactsList.Count}]");
            UpdateContactsList();
        }

        private void btnGetMembers_Click(object sender, EventArgs e)
        {
            if (channelIdSelected == null)
                return;

            //lbMembers.Items.Clear();
            rbChannels.GetAllMembers(channelIdSelected, callback =>
            {
                if (callback.Result.Success)
                {
                    channelMembersList = callback.Data;
                    AddInformation($"Members has been retrieved - nb:[{channelMembersList.Count}]");
                    UpdateMembersList();
                }
                else
                    AddInformation($"Not possible to get members: {Rainbow.Util.SerializeSdkError(callback.Result)}");
            });
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            if (channelIdSelected == null)
                return;

            if (lbContacts.SelectedIndex == -1)
                return;

            var contactItem = lbContacts.SelectedItem as ListItem;

            List<Rainbow.Model.Channel.Member> members = new List<Rainbow.Model.Channel.Member>();
            Rainbow.Model.Channel.Member member = new Rainbow.Model.Channel.Member()
            {
                Id = contactItem.Id,
                Type = cbMemberAsPublisher.Checked ? Rainbow.Model.Channel.MemberType.PUBLISHER : Rainbow.Model.Channel.MemberType.MEMBER
            };
            members.Add(member);

            rbChannels.SetMembersRole(channelIdSelected, members, callback =>
            {
                if (callback.Result.Success)
                {
                    AddInformation($"Member has been added");
                    btnGetMembers_Click(this, null);
                    GetChannels();
                }
                else
                    AddInformation($"Member cannot be added: {Rainbow.Util.SerializeSdkError(callback.Result)}");
            });
        }

        private void btnRemoveMember_Click(object sender, EventArgs e)
        {
            if (channelIdSelected == null)
                return;

            if (lbMembers.SelectedIndex == -1)
                return;

            var memberItem = lbMembers.SelectedItem as ListItem;

            List<Rainbow.Model.Channel.Member> members = new List<Rainbow.Model.Channel.Member>();
            Rainbow.Model.Channel.Member member = new Rainbow.Model.Channel.Member()
            {
                Id = memberItem.Id,
                Type = Rainbow.Model.Channel.MemberType.NONE
            };
            members.Add(member);

            rbChannels.SetMembersRole(channelIdSelected, members, callback =>
            {
                if (callback.Result.Success)
                {
                    AddInformation($"Member has been removed");
                    btnGetMembers_Click(this, null);
                    GetChannels();
                }
                else
                    AddInformation($"Member cannot be removed: {Rainbow.Util.SerializeSdkError(callback.Result)}");
            });
        }

        private void btnManageItems_Click(object sender, EventArgs e)
        {
            AddInformation("Not explained in this sample yet but available in the SDK");

            if ((formChannelItems == null) || (formChannelItems?.IsDisposed == true))
            {
                formChannelItems = new FormChannelItems();
                formChannelItems.Initialize(rbApplication, channelIdSelected);
            }

            if (!formChannelItems.Visible)
                formChannelItems.Show(this);
        }

        private void tbMassProOk_Click(object sender, EventArgs e)
        {
            int nbChannelCreated = 0;
            int nbPublishersAdded = 0;

            int nbChannelToCreate;
            int maxNbPublishersToAdd;

            List<Rainbow.Model.Channel.Member> membersFullList;
            Rainbow.Model.Channel.Member member;

            if (!int.TryParse(tbMassProNbPublishers.Text, out maxNbPublishersToAdd))
            {
                AddInformation($"Cannot get integer from 'Max Nb Publihsers' field");
                return;
            }

            if (!int.TryParse(tbMassProNbChannels.Text, out nbChannelToCreate))
            {
                AddInformation($"Cannot get integer from 'Nb Channels' field");
                return;
            }

            var contacts = rbContacts.GetAllContactsFromCache();
            if(contacts.Count <= maxNbPublishersToAdd)
            {
                AddInformation($"Current user has not enough contacts according the 'Max Nb Publihsers' field");
                return;
            }
            else
            {
                membersFullList = new List<Rainbow.Model.Channel.Member>();
                foreach(var contact in contacts)
                {
                    if(contact.Id != currentContactId)
                    {
                        member = new Rainbow.Model.Channel.Member()
                        {
                            Id = contact.Id,
                            Type = Rainbow.Model.Channel.MemberType.PUBLISHER
                            
                        };
                        membersFullList.Add(member);
                        if (membersFullList.Count == maxNbPublishersToAdd)
                            break;
                    }
                }
            }


            Task task = new Task(() =>
            {
                AddInformation($"Masspro - START - Date:[{DateTime.Now.ToString("o")}] -Nb Channels to create:[{nbChannelToCreate}] - Max Publisher by Channel:[{maxNbPublishersToAdd}]");
                do
                {
                    ManualResetEvent manualResetEventCreateChannel = new ManualResetEvent(false);
                    ManualResetEvent manualResetEventAddPublisher = new ManualResetEvent(false);
                    String channelIdCreated;

                    Rainbow.Model.Channel channel = new Rainbow.Model.Channel()
                    {
                        Name = "Masspro_" + Rainbow.Util.GetGUID(),
                        Topic = "vACD Communication",
                        Category = "Telephony",
                        MaxItems = 10,
                        MaxPayloadSize = 30000,
                        Mode = Rainbow.Model.Channel.ChannelMode.COMPANY_PUBLIC
                    };

                    AddInformation($"Masspro - channel creation + member roles - START - Date:[{DateTime.Now.ToString("o")}]");
                    rbChannels.CreateChannel(channel, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            channelIdCreated = callback.Data.Id;
                            AddInformation($"Masspro - channel create - Id:[{channelIdCreated}]");
                            nbChannelCreated++;

                            Boolean notDone = true;
                            var membersList = membersFullList.Take(random.Next(1, maxNbPublishersToAdd)).ToList();
                            do
                            {
                                AddInformation($"Masspro - Set Member roles - START - Date:[{DateTime.Now.ToString("o")}]");
                                manualResetEventAddPublisher.Reset();
                                rbChannels.SetMembersRole(channelIdCreated, membersList, callbackMembers =>
                                { 
                                    if(callbackMembers.Result.Success)
                                    {
                                        notDone = false;
                                        AddInformation($"Masspro - Set Member roles done");
                                    }
                                    else
                                        AddInformation($"Masspro - Failed to set member roles:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");

                                    manualResetEventCreateChannel.Set();
                                });
                                manualResetEventCreateChannel.WaitOne();
                            }
                            while (notDone);
                            
                        }
                        else
                            AddInformation($"Masspro - failed to create channel:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        AddInformation($"Masspro - channel creation + member roles - END - Date:[{DateTime.Now.ToString("o")}]");
                        manualResetEventCreateChannel.Set();
                    });
                    manualResetEventCreateChannel.WaitOne();

                }
                while (nbChannelCreated < nbChannelToCreate);
                AddInformation($"Masspro - START - End:[{DateTime.Now.ToString("o")}]");
            });
            task.Start();
        }

        private void tbMassProDelete_Click(object sender, EventArgs e)
        {
            rbChannels.GetSubscribedChannels(callback =>
            {
                if(callback.Result.Success)
                {
                    var channels = callback.Data;

                    // We want only channels create from MassPro
                    channels = channels.FindAll(x => x.Name.StartsWith("Masspro_"));

                    while (channels.Count > 0)
                    {
                        ManualResetEvent manualResetEventDeleteChannel = new ManualResetEvent(false);

                        AddInformation($"Masspro - START - Date:[{DateTime.Now.ToString("o")}] - Nb Channels to delete:[{channels.Count}]");
                        rbChannels.DeleteChannel(channels[0].Id, callbackDelete =>
                        {
                            if (callbackDelete.Result.Success)
                            {
                                AddInformation($"Channel deleted :[{channels[0].Id}]");
                                channels.RemoveAt(0);
                            }
                            else
                                AddInformation($"Channel cannot be deleted:[{Rainbow.Util.SerializeSdkError(callbackDelete.Result)}]");

                            manualResetEventDeleteChannel.Set();
                        });
                        manualResetEventDeleteChannel.WaitOne();
                    }
                    AddInformation($"Masspro - END - Date:[{DateTime.Now.ToString("o")}]");
                }
            });
        }

    #endregion EVENTS FROM Form Elements

        
    }
}
