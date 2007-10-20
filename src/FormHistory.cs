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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace BalloonRss
{
    public class FormHistory : Form
    {
        RssItem[] rssHistory;
        private ListView listHistory;
        private ListViewItem[] listItems;


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
            this.listHistory = new System.Windows.Forms.ListView();
            // 
            // listHistory
            // 
            this.listHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listHistory.Location = new System.Drawing.Point(0, 0);
            this.listHistory.Size = new System.Drawing.Size(292, 200);
            this.listHistory.UseCompatibleStateImageBehavior = false;
            this.listHistory.View = View.Details;
            this.listHistory.AllowColumnReorder = true;
            this.listHistory.FullRowSelect = true;
            this.listHistory.DoubleClick += new EventHandler(OnItemActivate);

            // 
            // FormHistory
            // 
            this.ClientSize = new System.Drawing.Size(292, 200);
            this.Controls.Add(this.listHistory);
            this.MinimizeBox = false;
            this.Text = resources.str_historyFormTitle;
        }


        private void FillHistoryList()
        {
            listItems = new ListViewItem[rssHistory.Length];

            for(int i = 0; i < rssHistory.Length; i++)
            {
                listItems[i] = new ListViewItem("" + (i+1));
                listItems[i].SubItems.Add(rssHistory[i].title);
                listItems[i].SubItems.Add(rssHistory[i].link);
            }

            listHistory.Columns.Add(resources.str_historyHeaderId, -2, HorizontalAlignment.Center);
            listHistory.Columns.Add(resources.str_historyHeaderTitle, -2, HorizontalAlignment.Left);
            listHistory.Items.AddRange(listItems);
        }


        private void OnItemActivate(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listHistory.SelectedItems)
            {
                System.Diagnostics.Process.Start(item.SubItems[2].Text);
            }

            // quit history window
            Dispose();
        }
    }
}