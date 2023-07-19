using Microsoft.Extensions.Logging;
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
    public partial class FormChannelItems : Form
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<FormChannelItems>(); // Good way to log with SDK 2.6.0 and more
        //private static readonly Logger log = LogConfigurator.GetLogger(typeof(FormChannelItems)); // Old way to log before SDK 2.6.0

        // Define SDK Objects
        Rainbow.Application? rbApplication = null;
        Rainbow.Contacts? rbContacts = null;
        Rainbow.Channels? rbChannels = null;

        String? currentContactId = null;
        List<Rainbow.Model.Contact>? contactsList = null;

        String? currentChannelId = null;
        Rainbow.Model.Channel? currentChannel = null;

        String? currentChannelItemId = null;
        Rainbow.Model.ChannelItem? currentChannelItem = null;
        List<Rainbow.Model.ChannelItem>? currentChannelItemsList = null;
        List<Rainbow.Model.ChannelUserAppreciation>? currentUserAppreciationsList = null;

        public FormChannelItems()
        {
            InitializeComponent();
        }

        public void Initialize(Rainbow.Application application, String channelId)
        {
            if ((application != null) && (rbApplication != application))
            {
                // Remove previous event
                if (rbApplication != null)
                {

                }

                rbApplication = application;

                rbContacts = rbApplication.GetContacts();
                rbChannels = rbApplication.GetChannels();

                currentContactId = rbContacts.GetCurrentContactId();
                contactsList = rbContacts.GetAllContactsFromCache();

                // Define events we want to manage
                rbChannels.ChannelItemUpdated += RbChannels_ChannelItemUpdated;
                rbChannels.ChannelItemAppreciationsUpdated += RbChannels_ChannelItemAppreciationsUpdated;
            }

            currentChannelId = channelId;
            currentChannel = rbChannels.GetChannelFromCache(currentChannelId);
            currentChannelItem = null;

            UpdateFullUI();
        }

        private void UpdateFullUI()
        {
            if (currentChannel != null)
            {
                tbChannelName.Text = currentChannel.Name;

                btnGetItemsFromCache.Enabled = true;
                btnGetMoreItems.Enabled = true;
            }
            else
            {
                tbChannelName.Text = "";

                btnGetItemsFromCache.Enabled = false;
                btnGetMoreItems.Enabled = false;
            }

            cbChannelItemAppreciationsList.SelectedIndex = 0;

            UpdateUIChannelItemsList();
            UpdateUIChannelItem();
        }

        private void UpdateUIChannelItemsList()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIChannelItemsList);
                this.Invoke(d, new object[] { });
            }
            else
            {
                cbItemsList.Items.Clear();
                
                if (currentChannelItemsList?.Count > 0)
                {
                    ListItem item;
                    int index = 0;
                    int selectedIndex = 0;
                    foreach (var channelItem in currentChannelItemsList)
                    {
                        item = new ListItem(channelItem.Id, channelItem.Title);
                        cbItemsList.Items.Add(item);

                        if (channelItem.Id == currentChannelItemId)
                            selectedIndex = index;

                        index++;
                    }

                    cbItemsList.SelectedIndex = selectedIndex;
                    currentChannelItem = currentChannelItemsList[selectedIndex];
                    currentChannelItemId = currentChannelItem.Id;
                }
                else
                {
                    currentChannelItem = null;
                    currentChannelItemId = null;
                }
                UpdateUIChannelItem();
            }
        }

        private void UpdateUIChannelItem()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIChannelItem);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lbItemModified.Visible = false;

                if (currentChannelItem != null)
                {
                    tbItemId.Text = currentChannelItem.Id;
                    tbItemFrom.Text = GetDisplayNameFromJid(currentChannelItem.From);
                    tbItemTitle.Text = currentChannelItem.Title;
                    tbItemUrl.Text = currentChannelItem.Url;
                    tbItemMessage.Text = currentChannelItem.Message;

                    btnItemUpdate.Enabled = true;

                    String type = currentChannelItem.Type.ToString();
                    if (type == "BASIC")
                        cbItemType.SelectedIndex = 0;
                    else if (type == "HTML")
                        cbItemType.SelectedIndex = 1;
                    else if (type == "DATA")
                        cbItemType.SelectedIndex = 2;


                    if ((currentChannelItem.Timestamp != DateTime.MinValue)
                        && (currentChannelItem.Timestamp != currentChannelItem.Creation))
                        lbItemModified.Visible = true;

                    String str = "";
                    if(currentChannelItem.Appreciations != null)
                    {
                        if (currentChannelItem.Appreciations.Applause > 0)
                            str += $"Applause: {currentChannelItem.Appreciations.Applause}\r\n";
                        if (currentChannelItem.Appreciations.Doubt > 0)
                            str += $"Doubt: {currentChannelItem.Appreciations.Doubt}\r\n";
                        if (currentChannelItem.Appreciations.Fantastic > 0)
                            str += $"Fantastic: {currentChannelItem.Appreciations.Fantastic}\r\n";
                        if (currentChannelItem.Appreciations.Happy > 0)
                            str += $"Happy: {currentChannelItem.Appreciations.Happy}\r\n";
                        if (currentChannelItem.Appreciations.Like > 0)
                            str += $"Like: {currentChannelItem.Appreciations.Like}\r\n";
                    }
                    if (str == "")
                        str = "No appreciation";
                    tbAppreciations.Text = str;
                }
                else
                {
                    tbItemId.Text = "";
                    tbItemFrom.Text = "";
                    tbItemTitle.Text = "";
                    tbItemUrl.Text = "";
                    tbItemMessage.Text = "";

                    tbAppreciations.Text = "";

                    btnItemUpdate.Enabled = false;
                }
            }
        }

        private void UpdateUIAppreciationsList()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIAppreciationsList);
                this.Invoke(d, new object[] { });
            }
            else
            {
                String str = "";
                if (currentUserAppreciationsList != null)
                {
                    foreach (var userAppreciation in currentUserAppreciationsList)
                    {
                        str += $"{GetDisplayNameFromId(userAppreciation.UserId)} [{userAppreciation.Appreciation}]\r\n";
                    }
                }
                tbChannelItemAppreciations.Text = str;
            }
        }

        private String GetDisplayNameFromJid(String jid)
        {
            String result = "";
            if (jid != null)
            {
                var contact = rbContacts.GetContactFromContactJid(jid);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
                else
                    result = jid;
            }

            return result;
        }

        private String GetDisplayNameFromId(String id)
        {
            String result = "";
            if (id != null)
            {
                var contact = rbContacts.GetContactFromContactId(id);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
                else
                    result = id;
            }

            return result;
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

    #region EVENTS FROM Rainbow Sdk objects

        private void RbChannels_ChannelItemUpdated(object? sender, Rainbow.Events.ChannelItemEventArgs e)
        {
            if (e.ChannelId == currentChannelId)
                AddInformation($"A Channel Item has been updated on the current selected Channel - Action:[{e.Action}] - ItemId:[{e.ItemId}]");
            else
                AddInformation($"A Channel Item has been updated on another Channel:[{e.ChannelId}] - Action:[{e.Action}]");
        }

        private void RbChannels_ChannelItemAppreciationsUpdated(object? sender, Rainbow.Events.ChannelItemEventArgs e)
        {
            if (e.ChannelId == currentChannelId)
                AddInformation($"An appreciation on a Channel Item has been updated on the current selected Channel - Action:[{e.Action}] - ItemId:[{e.ItemId}]");
            else
                AddInformation($"An appreciation on a Channel Item has been updated on another Channel:[{e.ChannelId}] - Action:[{e.Action}]");
        }

    #endregion EVENTS FROM Rainbow Sdk objects

    #region EVENTS FROM Form Elements

        private void btnGetItemsFromCache_Click(object sender, EventArgs e)
        {
            if(currentChannelId != null)
            {
                currentChannelItemsList = rbChannels.GetItemsFromCache(currentChannelId);

                if(currentChannelItemsList == null)
                    AddInformation($"Nb items retrieved from cache:[{0}]");
                else
                    AddInformation($"Nb items retrieved from cache:[{currentChannelItemsList?.Count}]");

                UpdateUIChannelItemsList();
            }
        }

        private void btnGetMoreItems_Click(object sender, EventArgs e)
        {
            if (currentChannelId != null)
            {
                rbChannels.GetItems(currentChannelId, 20, callback =>
                {
                    if (callback.Result.Success)
                    {
                        var moreItems = callback.Data;
                        AddInformation($"Nb items retrieved:[{moreItems.Count}]");
                        
                        if (currentChannelItemsList == null)
                            currentChannelItemsList = new List<Rainbow.Model.ChannelItem>();
                        currentChannelItemsList.AddRange(moreItems);
                        
                        UpdateUIChannelItemsList();
                    }
                    else
                        AddInformation($"Cannot get more items:[{callback.Result}]");
                });
            }
        }

        private void cbItemsList_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if(cbItemsList.SelectedItem is ListItem item)
            {
                currentChannelItemId = item.Id;
                currentChannelItem = rbChannels.GetItemFromCache(currentChannelId, currentChannelItemId);
                
                UpdateUIChannelItem();

                currentUserAppreciationsList = null;
                UpdateUIAppreciationsList();
            }
        }

        private void btnItemDelete_Click(object sender, EventArgs e)
        {
            if(currentChannelItemId != null)
            {
                rbChannels.DeleteItem(currentChannelId, currentChannelItemId, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddInformation($"Items deleted");

                        if (currentChannelItemsList != null)
                        { 
                            int index = currentChannelItemsList.FindIndex(item => item.Id == currentChannelItemId);
                            currentChannelItemsList.RemoveAt(index);
                        }

                        currentChannelItemId = null;
                        UpdateUIChannelItemsList();
                    }
                    else
                        AddInformation($"Cannot delete item:[{callback.Result}]");
                });
            }
        }

        private void btnItemUpdate_Click(object sender, EventArgs e)
        {
            if (currentChannelItemId != null)
            {
                var itemUpdated = rbChannels.GetItemFromCache(currentChannelId, currentChannelItemId);
                itemUpdated.Title = tbItemTitle.Text;
                itemUpdated.Message = tbItemMessage.Text;
                itemUpdated.Url = tbItemUrl.Text;

                String type = cbItemType.SelectedItem.ToString();
                if (type == "BASIC")
                    itemUpdated.Type = Rainbow.Model.ChannelItem.ItemType.BASIC;
                else if (type == "HTML")
                    itemUpdated.Type = Rainbow.Model.ChannelItem.ItemType.HTML;
                else if (type == "DATA")
                    itemUpdated.Type = Rainbow.Model.ChannelItem.ItemType.DATA;


                rbChannels.UpdateItem(currentChannelId, itemUpdated, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddInformation($"Items updated");
                        UpdateUIChannelItemsList();
                    }
                    else
                        AddInformation($"Cannot update item:[{callback.Result}]");
                });
            }
        }

        private void btnItemAdd_Click(object sender, EventArgs e)
        {
            if (currentChannelId != null)
            {
                Rainbow.Model.ChannelItem itemAdded = new Rainbow.Model.ChannelItem();
                itemAdded.Title = tbItemTitle.Text;
                itemAdded.Message = tbItemMessage.Text;
                itemAdded.Url = tbItemUrl.Text;

                String type = cbItemType.SelectedItem.ToString();
                if (type == "BASIC")
                    itemAdded.Type = Rainbow.Model.ChannelItem.ItemType.BASIC;
                else if (type == "HTML")
                    itemAdded.Type = Rainbow.Model.ChannelItem.ItemType.HTML;
                else if (type == "DATA")
                    itemAdded.Type = Rainbow.Model.ChannelItem.ItemType.DATA;


                rbChannels.AddItem(currentChannelId, itemAdded, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddInformation($"Items added");

                        var newItem = callback.Data;

                        if (currentChannelItemsList == null)
                            currentChannelItemsList = new List<Rainbow.Model.ChannelItem>();
                        
                        currentChannelItemsList.Insert(0, newItem);
                        currentChannelItemId = newItem.Id;

                        UpdateUIChannelItemsList();
                    }
                    else
                        AddInformation($"Cannot add item:[{callback.Result}]");
                });
            }
        }

        private void btnChannelItemAppreciationGet_Click(object sender, EventArgs e)
        {
            if (currentChannelItemId != null)
            {
                rbChannels.GetDetailedAppreciations(currentChannelId, currentChannelItemId, callback =>
                {
                    if(callback.Result.Success)
                    {
                        currentUserAppreciationsList = callback.Data;
                        if(currentUserAppreciationsList == null)
                            AddInformation($"Bad result:[currentUserAppreciationsList is NULL]");
                        else
                            AddInformation($"Nb Detailed Appreciations:[{currentUserAppreciationsList.Count}]");
                        UpdateUIAppreciationsList();
                    }
                    else
                        AddInformation($"Cannot get Detailed Appreciations:[{callback.Result}]");
                });
            }
        }

        private void btnChannelItemAppreciationSet_Click(object sender, EventArgs e)
        {
            if ( (currentChannelItemId != null) && (cbChannelItemAppreciationsList.SelectedItem != null) )
            {
                String appreciation = "";
                appreciation = cbChannelItemAppreciationsList.SelectedItem as String;
                rbChannels.LikeItem(currentChannelId, currentChannelItemId, appreciation, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddInformation($"Item liked");
                        btnChannelItemAppreciationGet_Click(this, null);
                    }
                    else
                        AddInformation($"Cannot get like item:[{callback.Result}]");
                });
            }
        }

    #endregion EVENTS FROM Form Elements


    }
}
