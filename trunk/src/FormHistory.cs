/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2008  Roman Morawek <romor@users.sourceforge.net>

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
    class FormHistory : Form
    {
        private int panelWidth = 400;
        private int panelHeight = 200;

        private RssItem[] rssHistory;
        private ListView listView;


        public FormHistory(Queue<RssItem> rssHistory)
        {
            this.rssHistory = rssHistory.ToArray();

            this.SuspendLayout();

            InitializeComponent();
            FillHistoryList();

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
            // FormHistory
            // 
            this.ClientSize = new System.Drawing.Size(panelWidth, panelHeight);
            this.Controls.Add(this.listView);
            this.MinimizeBox = false;
            this.Text = Properties.Resources.str_historyFormTitle;
            this.Icon = BalloonRss.Properties.Resources.ico_yellow32;
        }


        private void FillHistoryList()
        {
            ListViewItem[] listItems = new ListViewItem[rssHistory.Length];

            // create the list items
            for (int i = 0; i < rssHistory.Length; i++)
            {
                listItems[i] = new ListViewItem("" + (i+1));
                listItems[i].SubItems.Add(rssHistory[rssHistory.Length - 1 - i].title);
                if (rssHistory[rssHistory.Length - 1 - i].GetType() != typeof(RssUpdateItem))
                    listItems[i].SubItems.Add(rssHistory[rssHistory.Length - 1 - i].channel.channelInfo.link);
                else
                    listItems[i].SubItems.Add("");
                listItems[i].SubItems.Add(rssHistory[rssHistory.Length - 1 - i].dispDate.ToShortTimeString());
                listItems[i].SubItems.Add(rssHistory[rssHistory.Length - 1 - i].link);
            }

            // set the table headers
            listView.Columns.Add(Properties.Resources.str_historyHeaderId, -2, HorizontalAlignment.Center);
            listView.Columns.Add(Properties.Resources.str_historyHeaderTitle, -2, HorizontalAlignment.Left);
            listView.Columns.Add(Properties.Resources.str_historyHeaderChannel, -2, HorizontalAlignment.Left);
            listView.Columns.Add(Properties.Resources.str_historyHeaderTimestamp, -2, HorizontalAlignment.Left);
            listView.Items.AddRange(listItems);
        }


        private void OnItemActivate(object sender, EventArgs e)
        {
            foreach (ListViewItem listItem in listView.SelectedItems)
            {
                // start the link of the RSS item
                RssItem rssItem = rssHistory[rssHistory.Length - 1 - listItem.Index];
                if (rssItem.GetType() != typeof(RssUpdateItem))
                {
                    rssItem.channel.ActivateItem(rssItem);
                }
                else
                {
                    // this is an RSS update information
                    System.Diagnostics.Process.Start(rssItem.link);
                }
            }

            // close window
            Dispose();
        }
    }
}