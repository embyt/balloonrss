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


namespace BalloonRss
{
    class FormChannelInfo : Form
    {
        private RssChannel[] rssChannel;
        private ListView listView;


        public FormChannelInfo(RssChannel[] rssChannel)
        {
            this.rssChannel = rssChannel;

            this.SuspendLayout();

            InitializeComponent();
            FillChannelList();

            this.ResumeLayout();
        }


        private void InitializeComponent()
        {
            this.listView = new System.Windows.Forms.ListView();
            // 
            // listHistory
            // 
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = View.Details;
            this.listView.AllowColumnReorder = true;
            this.listView.FullRowSelect = true;
            this.listView.DoubleClick += new EventHandler(OnItemActivate);

            // 
            // Form
            // 
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.listView);
            this.MinimizeBox = false;
            this.Text = resources.str_channelFormTitle;
            this.Icon = BalloonRss.resources.ico_yellow32;
        }


        private void FillChannelList()
        {
            ListViewItem[] listItems = new ListViewItem[rssChannel.Length];

            // create the list items
            for (int i = 0; i < rssChannel.Length; i++)
            {
                listItems[i] = new ListViewItem(rssChannel[i].title);
                listItems[i].SubItems.Add("" + rssChannel[i].Count);
                listItems[i].SubItems.Add("" + rssChannel[i].messageCount);
                listItems[i].SubItems.Add("" + rssChannel[i].link);
            }

            // set the table headers
            listView.Columns.Add(resources.str_channelHeaderTitle, -2, HorizontalAlignment.Left);
            listView.Columns.Add(resources.str_channelHeaderCount, -2, HorizontalAlignment.Center);
            listView.Columns.Add(resources.str_channelHeaderTotal, -2, HorizontalAlignment.Center);
            listView.Items.AddRange(listItems);
        }


        private void OnItemActivate(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                // start the link of the channel
                System.Diagnostics.Process.Start(item.SubItems[3].Text);
            }

            // close window
            Dispose();
        }
    }
}