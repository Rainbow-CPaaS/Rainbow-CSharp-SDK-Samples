namespace SampleChannels
{
    partial class FormChannelItems
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
            this.tbChannelName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbInformation = new System.Windows.Forms.TextBox();
            this.cbChannels = new System.Windows.Forms.ComboBox();
            this.btnGetItemsFromCache = new System.Windows.Forms.Button();
            this.cbItemsList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbItemId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbItemTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbItemUrl = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbItemMessage = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnItemAdd = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.cbItemType = new System.Windows.Forms.ComboBox();
            this.btnItemUpdate = new System.Windows.Forms.Button();
            this.tbItemFrom = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnChannelItemAppreciationGet = new System.Windows.Forms.Button();
            this.btnGetMoreItems = new System.Windows.Forms.Button();
            this.btnItemDelete = new System.Windows.Forms.Button();
            this.lbItemModified = new System.Windows.Forms.Label();
            this.tbChannelItemAppreciations = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cbChannelItemAppreciationsList = new System.Windows.Forms.ComboBox();
            this.btnChannelItemAppreciationSet = new System.Windows.Forms.Button();
            this.tbAppreciations = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbChannelName
            // 
            this.tbChannelName.Location = new System.Drawing.Point(68, 9);
            this.tbChannelName.Name = "tbChannelName";
            this.tbChannelName.ReadOnly = true;
            this.tbChannelName.Size = new System.Drawing.Size(333, 23);
            this.tbChannelName.TabIndex = 44;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 15);
            this.label1.TabIndex = 41;
            this.label1.Text = "Channel";
            // 
            // tbInformation
            // 
            this.tbInformation.Location = new System.Drawing.Point(407, 9);
            this.tbInformation.Multiline = true;
            this.tbInformation.Name = "tbInformation";
            this.tbInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInformation.Size = new System.Drawing.Size(381, 119);
            this.tbInformation.TabIndex = 40;
            // 
            // cbChannels
            // 
            this.cbChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChannels.FormattingEnabled = true;
            this.cbChannels.Location = new System.Drawing.Point(105, -38);
            this.cbChannels.Name = "cbChannels";
            this.cbChannels.Size = new System.Drawing.Size(296, 23);
            this.cbChannels.TabIndex = 39;
            // 
            // btnGetItemsFromCache
            // 
            this.btnGetItemsFromCache.Location = new System.Drawing.Point(68, 38);
            this.btnGetItemsFromCache.Name = "btnGetItemsFromCache";
            this.btnGetItemsFromCache.Size = new System.Drawing.Size(138, 23);
            this.btnGetItemsFromCache.TabIndex = 38;
            this.btnGetItemsFromCache.Text = "Get Items from cache";
            this.btnGetItemsFromCache.UseVisualStyleBackColor = true;
            this.btnGetItemsFromCache.Click += new System.EventHandler(this.btnGetItemsFromCache_Click);
            // 
            // cbItemsList
            // 
            this.cbItemsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbItemsList.FormattingEnabled = true;
            this.cbItemsList.Location = new System.Drawing.Point(68, 67);
            this.cbItemsList.Name = "cbItemsList";
            this.cbItemsList.Size = new System.Drawing.Size(333, 23);
            this.cbItemsList.TabIndex = 45;
            this.cbItemsList.SelectionChangeCommitted += new System.EventHandler(this.cbItemsList_SelectionChangeCommitted);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 15);
            this.label2.TabIndex = 46;
            this.label2.Text = "Item";
            // 
            // tbItemId
            // 
            this.tbItemId.Location = new System.Drawing.Point(68, 96);
            this.tbItemId.Name = "tbItemId";
            this.tbItemId.ReadOnly = true;
            this.tbItemId.Size = new System.Drawing.Size(333, 23);
            this.tbItemId.TabIndex = 48;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 47;
            this.label3.Text = "Item Id";
            // 
            // tbItemTitle
            // 
            this.tbItemTitle.Location = new System.Drawing.Point(68, 154);
            this.tbItemTitle.Name = "tbItemTitle";
            this.tbItemTitle.Size = new System.Drawing.Size(333, 23);
            this.tbItemTitle.TabIndex = 50;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 15);
            this.label4.TabIndex = 49;
            this.label4.Text = "Title";
            // 
            // tbItemUrl
            // 
            this.tbItemUrl.Location = new System.Drawing.Point(68, 183);
            this.tbItemUrl.Name = "tbItemUrl";
            this.tbItemUrl.Size = new System.Drawing.Size(333, 23);
            this.tbItemUrl.TabIndex = 52;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 186);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 15);
            this.label5.TabIndex = 51;
            this.label5.Text = "Url";
            // 
            // tbItemMessage
            // 
            this.tbItemMessage.Location = new System.Drawing.Point(12, 265);
            this.tbItemMessage.Multiline = true;
            this.tbItemMessage.Name = "tbItemMessage";
            this.tbItemMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbItemMessage.Size = new System.Drawing.Size(776, 223);
            this.tbItemMessage.TabIndex = 55;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 247);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 15);
            this.label7.TabIndex = 56;
            this.label7.Text = "Message";
            // 
            // btnItemAdd
            // 
            this.btnItemAdd.Location = new System.Drawing.Point(416, 493);
            this.btnItemAdd.Name = "btnItemAdd";
            this.btnItemAdd.Size = new System.Drawing.Size(120, 23);
            this.btnItemAdd.TabIndex = 57;
            this.btnItemAdd.Text = "Add Item";
            this.btnItemAdd.UseVisualStyleBackColor = true;
            this.btnItemAdd.Click += new System.EventHandler(this.btnItemAdd_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(128, 494);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(120, 23);
            this.btnBrowse.TabIndex = 58;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // cbItemType
            // 
            this.cbItemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbItemType.FormattingEnabled = true;
            this.cbItemType.Items.AddRange(new object[] {
            "BASIC",
            "HTML",
            "DATA"});
            this.cbItemType.Location = new System.Drawing.Point(68, 212);
            this.cbItemType.Name = "cbItemType";
            this.cbItemType.Size = new System.Drawing.Size(137, 23);
            this.cbItemType.TabIndex = 59;
            // 
            // btnItemUpdate
            // 
            this.btnItemUpdate.Location = new System.Drawing.Point(542, 493);
            this.btnItemUpdate.Name = "btnItemUpdate";
            this.btnItemUpdate.Size = new System.Drawing.Size(120, 23);
            this.btnItemUpdate.TabIndex = 60;
            this.btnItemUpdate.Text = "Update Item";
            this.btnItemUpdate.UseVisualStyleBackColor = true;
            this.btnItemUpdate.Click += new System.EventHandler(this.btnItemUpdate_Click);
            // 
            // tbItemFrom
            // 
            this.tbItemFrom.Location = new System.Drawing.Point(68, 125);
            this.tbItemFrom.Name = "tbItemFrom";
            this.tbItemFrom.ReadOnly = true;
            this.tbItemFrom.Size = new System.Drawing.Size(333, 23);
            this.tbItemFrom.TabIndex = 62;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 128);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 15);
            this.label6.TabIndex = 61;
            this.label6.Text = "From";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 215);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 15);
            this.label8.TabIndex = 63;
            this.label8.Text = "Type";
            // 
            // btnChannelItemAppreciationGet
            // 
            this.btnChannelItemAppreciationGet.Location = new System.Drawing.Point(542, 138);
            this.btnChannelItemAppreciationGet.Name = "btnChannelItemAppreciationGet";
            this.btnChannelItemAppreciationGet.Size = new System.Drawing.Size(47, 23);
            this.btnChannelItemAppreciationGet.TabIndex = 65;
            this.btnChannelItemAppreciationGet.Text = "Get";
            this.btnChannelItemAppreciationGet.UseVisualStyleBackColor = true;
            this.btnChannelItemAppreciationGet.Click += new System.EventHandler(this.btnChannelItemAppreciationGet_Click);
            // 
            // btnGetMoreItems
            // 
            this.btnGetMoreItems.Location = new System.Drawing.Point(254, 38);
            this.btnGetMoreItems.Name = "btnGetMoreItems";
            this.btnGetMoreItems.Size = new System.Drawing.Size(147, 23);
            this.btnGetMoreItems.TabIndex = 66;
            this.btnGetMoreItems.Text = "Get More Items";
            this.btnGetMoreItems.UseVisualStyleBackColor = true;
            this.btnGetMoreItems.Click += new System.EventHandler(this.btnGetMoreItems_Click);
            // 
            // btnItemDelete
            // 
            this.btnItemDelete.Location = new System.Drawing.Point(668, 493);
            this.btnItemDelete.Name = "btnItemDelete";
            this.btnItemDelete.Size = new System.Drawing.Size(120, 23);
            this.btnItemDelete.TabIndex = 67;
            this.btnItemDelete.Text = "Delete Item";
            this.btnItemDelete.UseVisualStyleBackColor = true;
            this.btnItemDelete.Click += new System.EventHandler(this.btnItemDelete_Click);
            // 
            // lbItemModified
            // 
            this.lbItemModified.AutoSize = true;
            this.lbItemModified.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbItemModified.Location = new System.Drawing.Point(211, 215);
            this.lbItemModified.Name = "lbItemModified";
            this.lbItemModified.Size = new System.Drawing.Size(57, 15);
            this.lbItemModified.TabIndex = 68;
            this.lbItemModified.Text = "Modified";
            // 
            // tbChannelItemAppreciations
            // 
            this.tbChannelItemAppreciations.Location = new System.Drawing.Point(407, 163);
            this.tbChannelItemAppreciations.Multiline = true;
            this.tbChannelItemAppreciations.Name = "tbChannelItemAppreciations";
            this.tbChannelItemAppreciations.ReadOnly = true;
            this.tbChannelItemAppreciations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbChannelItemAppreciations.Size = new System.Drawing.Size(381, 96);
            this.tbChannelItemAppreciations.TabIndex = 69;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(407, 142);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(127, 15);
            this.label9.TabIndex = 70;
            this.label9.Text = "Detailled appreciations";
            // 
            // cbChannelItemAppreciationsList
            // 
            this.cbChannelItemAppreciationsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChannelItemAppreciationsList.FormattingEnabled = true;
            this.cbChannelItemAppreciationsList.Items.AddRange(new object[] {
            "applause",
            "doubt",
            "fantastic",
            "happy",
            "like",
            "none"});
            this.cbChannelItemAppreciationsList.Location = new System.Drawing.Point(651, 138);
            this.cbChannelItemAppreciationsList.Name = "cbChannelItemAppreciationsList";
            this.cbChannelItemAppreciationsList.Size = new System.Drawing.Size(137, 23);
            this.cbChannelItemAppreciationsList.TabIndex = 71;
            // 
            // btnChannelItemAppreciationSet
            // 
            this.btnChannelItemAppreciationSet.Location = new System.Drawing.Point(598, 138);
            this.btnChannelItemAppreciationSet.Name = "btnChannelItemAppreciationSet";
            this.btnChannelItemAppreciationSet.Size = new System.Drawing.Size(47, 23);
            this.btnChannelItemAppreciationSet.TabIndex = 72;
            this.btnChannelItemAppreciationSet.Text = "Set";
            this.btnChannelItemAppreciationSet.UseVisualStyleBackColor = true;
            this.btnChannelItemAppreciationSet.Click += new System.EventHandler(this.btnChannelItemAppreciationSet_Click);
            // 
            // tbAppreciations
            // 
            this.tbAppreciations.Location = new System.Drawing.Point(281, 212);
            this.tbAppreciations.Multiline = true;
            this.tbAppreciations.Name = "tbAppreciations";
            this.tbAppreciations.ReadOnly = true;
            this.tbAppreciations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbAppreciations.Size = new System.Drawing.Size(120, 50);
            this.tbAppreciations.TabIndex = 73;
            // 
            // FormChannelItems
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 529);
            this.Controls.Add(this.tbAppreciations);
            this.Controls.Add(this.btnChannelItemAppreciationSet);
            this.Controls.Add(this.cbChannelItemAppreciationsList);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbChannelItemAppreciations);
            this.Controls.Add(this.lbItemModified);
            this.Controls.Add(this.btnItemDelete);
            this.Controls.Add(this.btnGetMoreItems);
            this.Controls.Add(this.btnChannelItemAppreciationGet);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbItemFrom);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnItemUpdate);
            this.Controls.Add(this.cbItemType);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnItemAdd);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbItemMessage);
            this.Controls.Add(this.tbItemUrl);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbItemTitle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbItemId);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbItemsList);
            this.Controls.Add(this.tbChannelName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbInformation);
            this.Controls.Add(this.cbChannels);
            this.Controls.Add(this.btnGetItemsFromCache);
            this.Name = "FormChannelItems";
            this.Text = "FormChannelItems";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TextBox tbChannelName;
        private Label label1;
        private TextBox tbInformation;
        private ComboBox cbChannels;
        private Button btnGetItemsFromCache;
        private ComboBox cbItemsList;
        private Label label2;
        private TextBox tbItemId;
        private Label label3;
        private TextBox tbItemTitle;
        private Label label4;
        private TextBox tbItemUrl;
        private Label label5;
        private TextBox tbItemMessage;
        private Label label7;
        private Button btnItemAdd;
        private Button btnBrowse;
        private ComboBox cbItemType;
        private Button btnItemUpdate;
        private TextBox tbItemFrom;
        private Label label6;
        private Label label8;
        private Button btnChannelItemAppreciationGet;
        private Button btnGetMoreItems;
        private Button btnItemDelete;
        private Label lbItemModified;
        private TextBox tbChannelItemAppreciations;
        private Label label9;
        private ComboBox btnChannelItemAppreciationsList;
        private Button btnChannelItemAppreciationSet;
        private ComboBox cbChannelItemAppreciationsList;
        private TextBox tbAppreciations;
    }
}