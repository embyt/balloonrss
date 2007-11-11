/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2007  Roman Morawek <romor@users.sourceforge.net>

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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;


namespace BalloonRss
{
    class FormChannelSettings : Form
    {
        private int panelWidth = 480;
        private int panelHeight = 145;
        private int splitterBorder = 10;

        private ListView listView;
        ChannelList channelList;


        public FormChannelSettings()
        {
            try
            {
                // initialise with current channel settings
                channelList = new ChannelList(Properties.Settings.Default.channelConfigFileName);
            }
            catch (Exception e)
            {
                // illegal current channel settings
                throw new Exception("xxx");
            }

            // setup GUI
            this.SuspendLayout();
            InitializeComponent();
            FillChannelList();
            this.ResumeLayout();
        }


        private void InitializeComponent()
        {
            // list control
            this.listView = new System.Windows.Forms.ListView();
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = View.Details;
            this.listView.AllowColumnReorder = true;
            this.listView.FullRowSelect = true;
            this.listView.DoubleClick += new EventHandler(OnEdit);

            // button panel
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.FlowDirection = FlowDirection.TopDown;
            flPanel.Dock = DockStyle.Fill;
            // "new" button
            Button button = new Button();
            button.Text = resources.str_channelSettingsFormNewButton;
            button.Click += new System.EventHandler(this.OnNew);
            button.Anchor = AnchorStyles.Top;
            flPanel.Controls.Add(button);
            // "delete" button
            button = new Button();
            button.Text = resources.str_channelSettingsFormDeleteButton;
            button.Click += new System.EventHandler(this.OnDelete);
            button.Anchor = AnchorStyles.Top;
            flPanel.Controls.Add(button);
            // "edit" button
            button = new Button();
            button.Text = resources.str_channelSettingsFormEditButton;
            button.Click += new System.EventHandler(this.OnEdit);
            button.Anchor = AnchorStyles.Top;
            flPanel.Controls.Add(button);
            // "OK" button
            button = new Button();
            button.Text = resources.str_settingsFormOKButton;
            button.Click += new System.EventHandler(this.OnOK);
            this.AcceptButton = button;
            button.Anchor = AnchorStyles.Bottom;
            flPanel.Controls.Add(button);
            // "Cancel" button
            button = new Button();
            button.Text = resources.str_settingsFormCancelButton;
            button.Click += new System.EventHandler(this.OnCancel);
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
            this.Text = resources.str_channelSettingsFormTitle;
            this.Icon = BalloonRss.resources.ico_yellow32;
        }


        private void FillChannelList()
        {
            // clear any old data
            listView.Clear();

            // create the list items
            ListViewItem[] listItems = new ListViewItem[channelList.Count];
            for (int i = 0; i < listItems.Length; i++)
            {
                listItems[i] = new ListViewItem(i.ToString());
                listItems[i].SubItems.Add(channelList[i].link);
                listItems[i].SubItems.Add(channelList[i].priority.ToString());
            }

            // set the table headers
            listView.Columns.Add(resources.str_historyHeaderId, -2, HorizontalAlignment.Left);
            listView.Columns[0].Width = 0;  // hide the ID column
            listView.Columns.Add(resources.str_channelSettingsHeaderTitle, -2, HorizontalAlignment.Left);
            listView.Columns.Add(resources.str_channelSettingsHeaderPriority, -2, HorizontalAlignment.Left);

            // fill the list
            listView.Items.AddRange(listItems);
        }


        private void OnNew(object sender, EventArgs e)
        {
            // create new enty
            ChannelInfo channelInfo = new ChannelInfo();

            // diaplay edit box
            FormChannelSettingsEdit channelEdit = new FormChannelSettingsEdit(channelInfo);
            DialogResult dialogResult = channelEdit.ShowDialog(this);

            // if edit is confirmed, store the entry
            if (dialogResult == DialogResult.OK)
            {
                channelList.Add(channelInfo);

                ListViewItem listItem = new ListViewItem(listView.Items.Count.ToString());
                listItem.SubItems.Add(channelInfo.link);
                listItem.SubItems.Add(channelInfo.priority.ToString());
                listView.Items.Add(listItem);
            }
        }

        private void OnEdit(object sender, EventArgs e)
        {
            // get selected enty
            foreach (ListViewItem curItem in listView.SelectedItems)
            {
                int selectedChannel = Int32.Parse(curItem.Text);

                // diaplay edit box
                FormChannelSettingsEdit channelEdit = new FormChannelSettingsEdit(channelList[selectedChannel]);
                DialogResult dialogResult = channelEdit.ShowDialog(this);

                // update list view display
                if (dialogResult == DialogResult.OK)
                {
                    curItem.SubItems[1].Text = channelList[selectedChannel].link;
                    curItem.SubItems[2].Text = channelList[selectedChannel].priority.ToString();
                }

                // we just edit the first selection and skip the remaining ones
                break;
            }
        }

        private void OnDelete(object sender, EventArgs e)
        {
            bool deleteConfirmed = false;

            // just delete the first selected entry
            foreach (ListViewItem curItem in listView.SelectedItems)
            {
                int selectedChannel = Int32.Parse(curItem.Text);

                // ask for confirmation just once
                if (!deleteConfirmed)
                {
                    DialogResult dialogResult = MessageBox.Show(
                        resources.str_channelSettingsConfirmDeleteText + channelList[selectedChannel].link + "?",
                        resources.str_channelSettingsConfirmDeleteTitel,
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
        }

        private void OnOK(object sender, EventArgs e)
        {
            // store data
            channelList.SaveToFile(Properties.Settings.Default.channelConfigFileName);

            // close window
            this.DialogResult = DialogResult.OK;
            Dispose();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            // close window
            this.DialogResult = DialogResult.Cancel;
            Dispose();
        }
    }
}