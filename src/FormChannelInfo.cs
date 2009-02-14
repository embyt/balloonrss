/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2009  Roman Morawek <romor@users.sourceforge.net>

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


namespace BalloonRss
{
    class FormChannelInfo : Form
    {
        private const int panelWidth = 600;
        private const int panelHeight = 200;

        private RssChannel[] rssChannel;
        private ListView listView;
        private ListViewItem[] listItems;
        private ContextMenuStrip contextMenu;


        public FormChannelInfo(RssChannel[] rssChannel)
        {
            // store references to the channels
            this.rssChannel = rssChannel;

            // build GUI
            this.SuspendLayout();
            InitializeComponent();
            FillChannelList();
            this.ResumeLayout();
        }


        private void InitializeComponent()
        {
            // contextMenu
            contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Text = Resources.str_channelContextMenuMarkRead;
            menuItem.Click += new EventHandler(MiMarkAllReadClick);
            contextMenu.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem();
            menuItem.Text = Resources.str_channelContextMenuClearData;
            menuItem.Click += new EventHandler(MiClearChannelDataClick);
            contextMenu.Items.Add(menuItem);

            // listView
            this.listView = new System.Windows.Forms.ListView();
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = View.Details;
            this.listView.AllowColumnReorder = true;
            this.listView.FullRowSelect = true;
            this.listView.DoubleClick += new EventHandler(OnItemActivate);
            //this.listView.ContextMenuStrip = contextMenu;
            this.listView.MouseClick += new MouseEventHandler(ListView_MouseClick);

            // Form
            this.ClientSize = new System.Drawing.Size(panelWidth, panelHeight);
            this.Controls.Add(this.listView);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Text = Resources.str_channelFormTitle;
            this.Icon = Resources.ico_yellow32;
            this.HelpButton = true;
            this.HelpRequested += new HelpEventHandler(FormChannelInfo_HelpRequested);
        }

        void FormChannelInfo_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            Help.ShowHelp(this, Settings.Default.helpFilename, HelpNavigator.Topic, Settings.Default.helpNameChannelInfo);
        }


        private void FillChannelList()
        {
            listItems = new ListViewItem[rssChannel.Length];

            // set the table headers
            listView.Columns.Add(Resources.str_channelHeaderTitle, -2, HorizontalAlignment.Left);
            listView.Columns.Add(Resources.str_channelHeaderPrio, -2, HorizontalAlignment.Center);
            listView.Columns.Add(Resources.str_channelHeaderClickRate, -2, HorizontalAlignment.Center);
            listView.Columns.Add(Resources.str_channelHeaderEffPrio, -2, HorizontalAlignment.Center);
            listView.Columns.Add(Resources.str_channelHeaderCount, -2, HorizontalAlignment.Center);
            listView.Columns.Add(Resources.str_channelHeaderTotal, -2, HorizontalAlignment.Center);
            listView.Columns.Add(Resources.str_channelHeaderLastUpdate, -2, HorizontalAlignment.Center);

            // create the list items
            for (int i = 0; i < rssChannel.Length; i++)
            {
                listItems[i] = new ListViewItem();
                FillListSubItems(i);
            }
            listView.Items.AddRange(listItems);
        }

        // this fills the text of a list item
        // the list item has to be created before
        private void FillListSubItems(int index)
        {
            string viewCount = "-";
            if (rssChannel[index].channelViewedCount > 0)
                viewCount = "" + 100 * rssChannel[index].channelOpenedCount / rssChannel[index].channelViewedCount + " %";

            // we need to clear potential old list data for the case of a refresh
            listItems[index].SubItems.Clear();

            // add the text data
            listItems[index].Text = rssChannel[index].title;
            listItems[index].SubItems.AddRange(new string[] {
                "" + rssChannel[index].channelInfo.priority,
                viewCount,
                "" + rssChannel[index].effectivePriority,
                "" + rssChannel[index].Count,
                "" + rssChannel[index].channelMessageCount,
                "" + rssChannel[index].lastUpdate,
                "" + rssChannel[index].channelInfo.link,
            } );
        }


        // start link to channel on double click
        private void OnItemActivate(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                // start the link of the channel
                System.Diagnostics.Process.Start(item.SubItems[7].Text);
            }

            // close window
            Dispose();
        }


        // display context menu
        private void ListView_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (e.Clicks == 1))
            {
                contextMenu.Show(listView, e.Location);
            }
        }


        // mark all channel data as read for all selected list items
        private void MiMarkAllReadClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                rssChannel[item.Index].MarkAllRead();
                // also update the display
                FillListSubItems(item.Index);
            }
        }

        // clear channel data as read for all selected list items
        private void MiClearChannelDataClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                rssChannel[item.Index].ClearChannelData();
                // also update the display
                FillListSubItems(item.Index);
            }
        }
    }
}