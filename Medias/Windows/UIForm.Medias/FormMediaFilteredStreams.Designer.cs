using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    partial class FormMediaFilteredStreams
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("txt");
            this.label1 = new System.Windows.Forms.Label();
            this.cb_ListOfInputFilteredStreams = new System.Windows.Forms.ComboBox();
            this.lv_InputStreams = new System.Windows.Forms.ListView();
            this.btn_AddInputStreamInFilter = new System.Windows.Forms.Button();
            this.btn_RemoveInputStreamInFilter = new System.Windows.Forms.Button();
            this.lv_InputStreamsInFilter = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_NameOfNewInputFilteredStream = new System.Windows.Forms.TextBox();
            this.btn_SaveInputFilteredStream = new System.Windows.Forms.Button();
            this.btn_RemoveInputFilteredStream = new System.Windows.Forms.Button();
            this.lbl_InfoAboutInputStream = new System.Windows.Forms.Label();
            this.lbl_InfoAboutInputStreamInFilter = new System.Windows.Forms.Label();
            this.btn_AddNewInputFilteredStream = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(233, 15);
            this.label1.TabIndex = 201;
            this.label1.Text = "List Media Input Video Stream:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cb_ListOfInputFilteredStreams
            // 
            this.cb_ListOfInputFilteredStreams.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_ListOfInputFilteredStreams.FormattingEnabled = true;
            this.cb_ListOfInputFilteredStreams.Location = new System.Drawing.Point(192, 12);
            this.cb_ListOfInputFilteredStreams.Name = "cb_ListOfInputFilteredStreams";
            this.cb_ListOfInputFilteredStreams.Size = new System.Drawing.Size(189, 23);
            this.cb_ListOfInputFilteredStreams.TabIndex = 200;
            this.cb_ListOfInputFilteredStreams.SelectedIndexChanged += new System.EventHandler(this.cb_ListOfInputFilteredStreams_SelectedIndexChanged);
            // 
            // lv_InputStreams
            // 
            this.lv_InputStreams.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.lv_InputStreams.Location = new System.Drawing.Point(15, 95);
            this.lv_InputStreams.MultiSelect = false;
            this.lv_InputStreams.Name = "lv_InputStreams";
            this.lv_InputStreams.ShowGroups = false;
            this.lv_InputStreams.Size = new System.Drawing.Size(250, 245);
            this.lv_InputStreams.TabIndex = 203;
            this.lv_InputStreams.UseCompatibleStateImageBehavior = false;
            this.lv_InputStreams.View = System.Windows.Forms.View.List;
            this.lv_InputStreams.SelectedIndexChanged += new System.EventHandler(this.lv_InputStreams_SelectedIndexChanged);
            this.lv_InputStreams.DoubleClick += new System.EventHandler(this.lv_InputStreams_DoubleClick);
            // 
            // btn_AddInputStreamInFilter
            // 
            this.btn_AddInputStreamInFilter.Location = new System.Drawing.Point(274, 187);
            this.btn_AddInputStreamInFilter.Name = "btn_AddInputStreamInFilter";
            this.btn_AddInputStreamInFilter.Size = new System.Drawing.Size(107, 23);
            this.btn_AddInputStreamInFilter.TabIndex = 319;
            this.btn_AddInputStreamInFilter.Text = "Add   >>>";
            this.btn_AddInputStreamInFilter.UseVisualStyleBackColor = true;
            this.btn_AddInputStreamInFilter.Click += new System.EventHandler(this.btn_AddInputStreamInFilter_Click);
            // 
            // btn_RemoveInputStreamInFilter
            // 
            this.btn_RemoveInputStreamInFilter.Location = new System.Drawing.Point(274, 216);
            this.btn_RemoveInputStreamInFilter.Name = "btn_RemoveInputStreamInFilter";
            this.btn_RemoveInputStreamInFilter.Size = new System.Drawing.Size(107, 23);
            this.btn_RemoveInputStreamInFilter.TabIndex = 320;
            this.btn_RemoveInputStreamInFilter.Text = "<<<   Remove";
            this.btn_RemoveInputStreamInFilter.UseVisualStyleBackColor = true;
            this.btn_RemoveInputStreamInFilter.Click += new System.EventHandler(this.btn_RemoveInputStreamInFilter_Click);
            // 
            // lv_InputStreamsInFilter
            // 
            this.lv_InputStreamsInFilter.Location = new System.Drawing.Point(387, 95);
            this.lv_InputStreamsInFilter.Name = "lv_InputStreamsInFilter";
            this.lv_InputStreamsInFilter.Size = new System.Drawing.Size(250, 245);
            this.lv_InputStreamsInFilter.TabIndex = 321;
            this.lv_InputStreamsInFilter.UseCompatibleStateImageBehavior = false;
            this.lv_InputStreamsInFilter.View = System.Windows.Forms.View.List;
            this.lv_InputStreamsInFilter.SelectedIndexChanged += new System.EventHandler(this.lv_InputStreamsInFilter_SelectedIndexChanged);
            this.lv_InputStreamsInFilter.DoubleClick += new System.EventHandler(this.lv_InputStreamsInFilter_DoubleClick);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(367, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(233, 15);
            this.label2.TabIndex = 322;
            this.label2.Text = "Media Input Video Stream Used:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(147, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 15);
            this.label3.TabIndex = 323;
            this.label3.Text = "List:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(147, 43);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 15);
            this.label6.TabIndex = 325;
            this.label6.Text = "Name:";
            // 
            // tb_NameOfNewInputFilteredStream
            // 
            this.tb_NameOfNewInputFilteredStream.Location = new System.Drawing.Point(192, 40);
            this.tb_NameOfNewInputFilteredStream.Name = "tb_NameOfNewInputFilteredStream";
            this.tb_NameOfNewInputFilteredStream.Size = new System.Drawing.Size(189, 23);
            this.tb_NameOfNewInputFilteredStream.TabIndex = 324;
            // 
            // btn_SaveInputFilteredStream
            // 
            this.btn_SaveInputFilteredStream.Location = new System.Drawing.Point(500, 40);
            this.btn_SaveInputFilteredStream.Name = "btn_SaveInputFilteredStream";
            this.btn_SaveInputFilteredStream.Size = new System.Drawing.Size(107, 23);
            this.btn_SaveInputFilteredStream.TabIndex = 326;
            this.btn_SaveInputFilteredStream.Text = "Save";
            this.btn_SaveInputFilteredStream.UseVisualStyleBackColor = true;
            this.btn_SaveInputFilteredStream.Click += new System.EventHandler(this.btn_SaveInputFilteredStream_Click);
            // 
            // btn_RemoveInputFilteredStream
            // 
            this.btn_RemoveInputFilteredStream.Location = new System.Drawing.Point(387, 11);
            this.btn_RemoveInputFilteredStream.Name = "btn_RemoveInputFilteredStream";
            this.btn_RemoveInputFilteredStream.Size = new System.Drawing.Size(107, 23);
            this.btn_RemoveInputFilteredStream.TabIndex = 327;
            this.btn_RemoveInputFilteredStream.Text = "Remove";
            this.btn_RemoveInputFilteredStream.UseVisualStyleBackColor = true;
            this.btn_RemoveInputFilteredStream.Click += new System.EventHandler(this.btn_RemoveInputFilteredStream_Click);
            // 
            // lbl_InfoAboutInputStream
            // 
            this.lbl_InfoAboutInputStream.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_InfoAboutInputStream.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lbl_InfoAboutInputStream.Location = new System.Drawing.Point(15, 343);
            this.lbl_InfoAboutInputStream.Name = "lbl_InfoAboutInputStream";
            this.lbl_InfoAboutInputStream.Size = new System.Drawing.Size(250, 39);
            this.lbl_InfoAboutInputStream.TabIndex = 328;
            this.lbl_InfoAboutInputStream.Text = "Info about input stream\r\nL2\r\nL3";
            this.lbl_InfoAboutInputStream.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_InfoAboutInputStreamInFilter
            // 
            this.lbl_InfoAboutInputStreamInFilter.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_InfoAboutInputStreamInFilter.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lbl_InfoAboutInputStreamInFilter.Location = new System.Drawing.Point(387, 343);
            this.lbl_InfoAboutInputStreamInFilter.Name = "lbl_InfoAboutInputStreamInFilter";
            this.lbl_InfoAboutInputStreamInFilter.Size = new System.Drawing.Size(250, 39);
            this.lbl_InfoAboutInputStreamInFilter.TabIndex = 329;
            this.lbl_InfoAboutInputStreamInFilter.Text = "Info about input stream\r\nL2\r\nL3";
            this.lbl_InfoAboutInputStreamInFilter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_AddNewInputFilteredStream
            // 
            this.btn_AddNewInputFilteredStream.Location = new System.Drawing.Point(387, 40);
            this.btn_AddNewInputFilteredStream.Name = "btn_AddNewInputFilteredStream";
            this.btn_AddNewInputFilteredStream.Size = new System.Drawing.Size(107, 23);
            this.btn_AddNewInputFilteredStream.TabIndex = 330;
            this.btn_AddNewInputFilteredStream.Text = "Add";
            this.btn_AddNewInputFilteredStream.UseVisualStyleBackColor = true;
            this.btn_AddNewInputFilteredStream.Click += new System.EventHandler(this.btn_AddNewInputFilteredStream_Click);
            // 
            // FormMediaFilteredStreams
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 394);
            this.Controls.Add(this.btn_AddNewInputFilteredStream);
            this.Controls.Add(this.lbl_InfoAboutInputStreamInFilter);
            this.Controls.Add(this.lbl_InfoAboutInputStream);
            this.Controls.Add(this.btn_RemoveInputFilteredStream);
            this.Controls.Add(this.btn_SaveInputFilteredStream);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_NameOfNewInputFilteredStream);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lv_InputStreamsInFilter);
            this.Controls.Add(this.btn_RemoveInputStreamInFilter);
            this.Controls.Add(this.btn_AddInputStreamInFilter);
            this.Controls.Add(this.lv_InputStreams);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_ListOfInputFilteredStreams);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMediaFilteredStreams";
            this.Text = "FormMediaFilteredStreams";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMediaFilteredStreams_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private ComboBox cb_ListOfInputFilteredStreams;
        private ListView lv_InputStreams;
        private Button btn_AddInputStreamInFilter;
        private Button btn_RemoveInputStreamInFilter;
        private ListView lv_InputStreamsInFilter;
        private Label label2;
        private Label label3;
        private Label label6;
        private TextBox tb_NameOfNewInputFilteredStream;
        private Button btn_SaveInputFilteredStream;
        private Button btn_RemoveInputFilteredStream;
        private Label lbl_InfoAboutInputStream;
        private Label lbl_InfoAboutInputStreamInFilter;
        private Button btn_AddNewInputFilteredStream;
    }
}