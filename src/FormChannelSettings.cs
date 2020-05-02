/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2009  Roman Morawek <roman.morawek@embyt.com>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Forms;
using BalloonRss.Properties;

using System.Drawing;
using System.Reflection;
using System.IO;


namespace BalloonRss
{
    class FormChannelSettings : Form
    {
        private const int panelWidth = 480;
        private const int panelHeight = 145;
        private const int splitterBorder = 10;

        // GUI elements
        private ListView listView;
        private Button editButton;
        private Button deleteButton;

        // other class data
        private ChannelList channelList;


        public FormChannelSettings(String rssLink)
        {
            // initialise with current channel settings
            bool firstRun;
            channelList = new ChannelList(out firstRun);

            // setup GUI
            this.SuspendLayout();
            InitializeComponent();
            FillChannelList();
            this.ResumeLayout();

            if (rssLink != null)
                OnNew(rssLink, null);
        }


        private void InitializeComponent()
        {
            // list control
            this.listView = new ListView();
            this.listView.Dock = DockStyle.Fill;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = View.Details;
            this.listView.AllowColumnReorder = true;
            this.listView.FullRowSelect = true;
            this.listView.DoubleClick += new EventHandler(OnEdit);
            this.listView.SelectedIndexChanged += new EventHandler(listView_SelectedIndexChanged);

            // button panel
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.FlowDirection = FlowDirection.TopDown;
            flPanel.Dock = DockStyle.Fill;
            // "new" button
            Button button = new Button();
            button.Text = Resources.str_channelSettingsFormNewButton;
            button.Click += new EventHandler(this.OnNew);
            button.Anchor = AnchorStyles.Top;
            flPanel.Controls.Add(button);
            // "delete" button
            deleteButton = new Button();
            deleteButton.Text = Resources.str_channelSettingsFormDeleteButton;
            deleteButton.Click += new EventHandler(this.OnDelete);
            deleteButton.Anchor = AnchorStyles.Top;
            deleteButton.Enabled = false;
            flPanel.Controls.Add(deleteButton);
            // "edit" button
            editButton = new Button();
            editButton.Text = Resources.str_channelSettingsFormEditButton;
            editButton.Click += new EventHandler(this.OnEdit);
            editButton.Anchor = AnchorStyles.Top;
            editButton.Enabled = false;
            flPanel.Controls.Add(editButton);
            // "OK" button
            button = new Button();
            button.Text = Resources.str_settingsFormOKButton;
            button.Click += new EventHandler(this.OnOK);
            this.AcceptButton = button;
            button.Anchor = AnchorStyles.Bottom;
            flPanel.Controls.Add(button);
            // "Cancel" button
            button = new Button();
            button.Text = Resources.str_settingsFormCancelButton;
            button.Click += new EventHandler(this.OnCancel);
            this.CancelButton = button;
            button.Anchor = AnchorStyles.Bottom;
            flPanel.Controls.Add(button);

            // main split container
            SplitContainer mainContainer = new SplitContainer();
            mainContainer.FixedPanel = FixedPanel.Panel2;
            mainContainer.Panel1.Controls.Add(listView);
            mainContainer.Panel2.Controls.Add(flPanel);
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.IsSplitterFixed = true;
            mainContainer.SplitterDistance = mainContainer.Width - button.Width - splitterBorder;

            // 
            // form settings
            // 
            this.Controls.Add(mainContainer);
            this.ClientSize = new System.Drawing.Size(panelWidth, panelHeight);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Text = Resources.str_channelSettingsFormTitle;
            this.Icon = Resources.ico_yellow32;
            this.HelpButton = true;
            this.HelpRequested += new HelpEventHandler(FormChannelSettings_HelpRequested);
        }

        void FormChannelSettings_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            Help.ShowHelp(this, Settings.Default.helpFilename, HelpNavigator.Topic, Settings.Default.helpNameChannelSettings);
        }


        private void FillChannelList()
        {
            // clear any old data
            listView.Clear();

            // populate and set image list
            ImageList imageList = new ImageList();
            imageList.Images.Add(Resources.img_unlocked);
            imageList.Images.Add(Resources.img_locked);
            listView.StateImageList = imageList;

            // check whether we have only non-global channels
            bool onlyUserChannels = true;
            foreach (ChannelInfo curChannel in channelList)
            {
                if (curChannel.globalChannel)
                    onlyUserChannels = false;
            }

            // set the table headers
            // the locked image column has no header and is invisible if only user items exist
            listView.Columns.Add(null, onlyUserChannels ? 0 : -1, HorizontalAlignment.Left);
            listView.Columns.Add(Resources.str_historyHeaderId, 0, HorizontalAlignment.Left);   // hide the ID column
            listView.Columns.Add(Resources.str_channelSettingsHeaderTitle, -2, HorizontalAlignment.Left);
            listView.Columns.Add(Resources.str_channelSettingsHeaderPriority, -2, HorizontalAlignment.Left);

            // create the list items
            ListViewItem[] listItems = new ListViewItem[channelList.Count];
            for (int i = 0; i < listItems.Length; i++)
            {
                listItems[i] = new ListViewItem();
                listItems[i].SubItems.Add(i.ToString());
                listItems[i].SubItems.Add(channelList[i].link);
                listItems[i].SubItems.Add(channelList[i].priority.ToString());
                listItems[i].StateImageIndex = Convert.ToInt16(channelList[i].globalChannel);
            }

            // fill the list
            listView.Items.AddRange(listItems);
        }


        void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // get selected enty
            if (listView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem curItem in listView.SelectedItems)
                {
                    int selectedChannel = Int32.Parse(curItem.SubItems[1].Text);

                    editButton.Enabled = !channelList[selectedChannel].globalChannel;
                    deleteButton.Enabled = !channelList[selectedChannel].globalChannel;

                    // we just use the first selection
                    break;
                }
            }
            else
            {
                editButton.Enabled = false;
                deleteButton.Enabled = false;
            }
        }


        // open a child window to create a new channel
        private void OnNew(object sender, EventArgs e)
        {
            // create new enty
            ChannelInfo channelInfo = new ChannelInfo();

            // set link if specified
            if (sender is String)
                channelInfo.link = sender as String;

            // diaplay edit box
            FormChannelSettingsEdit channelEdit = new FormChannelSettingsEdit(channelInfo, true);
            DialogResult dialogResult = channelEdit.ShowDialog(this);

            // if edit is confirmed, store the entry
            if (dialogResult == DialogResult.OK)
            {
                // check for duplicate channel
                if (channelList.IsNewChannel(channelInfo, null))
                {
                    // OK, let's add it
                    channelList.Add(channelInfo);

                    ListViewItem listItem = new ListViewItem();
                    listItem.SubItems.Add(listView.Items.Count.ToString());
                    listItem.SubItems.Add(channelInfo.link);
                    listItem.SubItems.Add(channelInfo.priority.ToString());
                    listItem.StateImageIndex = Convert.ToInt16(channelInfo.globalChannel);
                    listView.Items.Add(listItem);
                }
                else
                {
                    MessageBox.Show(
                        Resources.str_channelSettingsDuplicateLink,
                        Resources.str_settingsFormErrorHeader,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            // we need to reselect an entry after this operation
            editButton.Enabled = false;
            deleteButton.Enabled = false;
        }

        // open child window to edit channel
        private void OnEdit(object sender, EventArgs e)
        {
            // get selected enty
            foreach (ListViewItem curItem in listView.SelectedItems)
            {
                int selectedChannel = Int32.Parse(curItem.SubItems[1].Text);

                // do not edit global items (this may happen at double click)
                if (channelList[selectedChannel].globalChannel)
                    break;

                // diaplay edit box
                ChannelInfo channelInfo = new ChannelInfo(channelList[selectedChannel]);
                FormChannelSettingsEdit channelEdit = new FormChannelSettingsEdit(channelInfo, false);
                DialogResult dialogResult = channelEdit.ShowDialog(this);

                // update list view display in case of pressing OK
                if (dialogResult == DialogResult.OK)
                {
                    // check for duplicate channel
                    if (channelList.IsNewChannel(channelInfo, channelList[selectedChannel]))
                    {
                        // override the stored channel
                        channelList[selectedChannel] = channelInfo;
                        curItem.SubItems[2].Text = channelList[selectedChannel].link;
                        curItem.SubItems[3].Text = channelList[selectedChannel].priority.ToString();
                    }
                    else
                    {
                        MessageBox.Show(
                            Resources.str_channelSettingsDuplicateLink,
                            Resources.str_settingsFormErrorHeader,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }

                // we just edit the first selection and skip the remaining ones
                break;
            }

            // we need to reselect an entry after this operation
            editButton.Enabled = false;
            deleteButton.Enabled = false;
        }

        private void OnDelete(object sender, EventArgs e)
        {
            bool deleteConfirmed = false;

            // just delete the first selected entry
            foreach (ListViewItem curItem in listView.SelectedItems)
            {
                int selectedChannel = Int32.Parse(curItem.SubItems[1].Text);

                // ask for confirmation just once
                if (!deleteConfirmed)
                {
                    DialogResult dialogResult = MessageBox.Show(
                        Properties.Resources.str_channelSettingsConfirmDeleteText + channelList[selectedChannel].link + "?",
                        Properties.Resources.str_channelSettingsConfirmDeleteTitel,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                        deleteConfirmed = true; // also for all succeeding entries
                    else
                        break;  // also for all succeeding entries
                }

                if (deleteConfirmed)
                {
                    // OK, we really want to delete it
                    channelList.RemoveAt(selectedChannel);
                    // rebuild list view
                    FillChannelList();
                }

                // skip other selected entries
                break;  // otherwise you need to take care on (*) index changes (*) confirm dialog text
            }

            // we need to reselect an entry after this operation
            editButton.Enabled = false;
            deleteButton.Enabled = false;
        }


        private void OnOK(object sender, EventArgs e)
        {
            // store data
            channelList.SaveToFile();

            // close window
            this.DialogResult = DialogResult.OK;
            Dispose();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            // even if we press cancel we need to reload the channels if the user cleared the collected channel data
            this.DialogResult = DialogResult.Cancel;

            // close window
            Dispose();
        }
    }
}