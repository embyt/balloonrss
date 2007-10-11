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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;


namespace BalloonRss
{
    public class MainForm : System.Windows.Forms.Form
    {
        // some important GUI elements
        private NotifyIcon notifyIcon;
        private IContainer components;

        // the "working horse"
        private Retriever retriever;

        // used to signal application exit
        private bool exitFlag = false;

        // the history of shown rss entries
        Queue<RssItem> rssHistory;


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm rssForm = new MainForm();
            Application.Run(rssForm);
            rssForm.Hide();
        }


        public MainForm()
        {
            // setup GUI
            InitializeComponent();

            // initialise variable
            rssHistory = new Queue<RssItem>(Properties.Settings.Default.historyDepth);

            // setup and start the background worker
            retriever = new Retriever();
            retriever.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.RetrieverCompleted);
            retriever.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.RetrieverProgressChanged);
            retriever.RunWorkerAsync();
        }


        private ContextMenu CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            // menuItem exit
            MenuItem mi_exit = new System.Windows.Forms.MenuItem();
            mi_exit.Text = "E&xit";
            mi_exit.Click += new System.EventHandler(this.MiExitClick);

            // menuItem settings
            MenuItem mi_settings = new System.Windows.Forms.MenuItem();
            mi_settings.Text = "&Settings";
            mi_settings.Click += new System.EventHandler(this.MiSettingsClick);
            mi_settings.Enabled = false;

            // menuItem toolTip
            MenuItem mi_history = new System.Windows.Forms.MenuItem();
            mi_history.Text = "&History";
            mi_history.Click += new System.EventHandler(this.MiHistoryClick);
            mi_history.Enabled = false;

            // Initialize contextMenu
            contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] 
            { 
                mi_history,
                mi_settings,
                mi_exit,
            });

            return contextMenu;
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.components = new System.ComponentModel.Container();

            // Set up how the form should be displayed.
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Text = "BalloonRss Settings";
            this.Name = "RssForm";
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;

            // Create the NotifyIcon.
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            notifyIcon.ContextMenu = CreateContextMenu();
            notifyIcon.Icon = BalloonRss.resources.ico_blue16;
            notifyIcon.Text = resources.str_iconInfoInit;
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipClicked += new EventHandler(notifyIcon_BalloonTipClicked);
            notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_BalloonTipClicked);

            this.ResumeLayout(false);
        }
 
        protected override void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Determine if text has changed in the textbox by comparing to original text.
            if (!exitFlag)
            {
                // Cancel close operation.
                e.Cancel = true;

                // just minimize the form
                this.Hide();
            }

            base.OnClosing(e);
        }


        private void MiSettingsClick(object sender, EventArgs e)
        {
            // cancel background worker operation
            retriever.CancelAsync();

            // Show the form

            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
        }

        private void MiExitClick(object sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.exitFlag = true;
            this.Close();
        }

        private void MiHistoryClick(object sender, EventArgs e)
        {
            // ToDo
        }

        private void notifyIcon_BalloonTipClicked(object sender, MouseEventArgs e)
        {
            // check whether we really clicked the Icon or the balloon popup
            if ((e.Button != MouseButtons.Left) || (e.Clicks != 0))
                return;

            // just forward this to the general event handler, omitting the event argument
            notifyIcon_BalloonTipClicked(sender, null as EventArgs);
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // display full rss entry
            if (rssHistory.Count > 0)
            {
                // get most recent item of queue
                RssItem rssItem = rssHistory.ToArray()[rssHistory.Count - 1];
                // start browser
                System.Diagnostics.Process.Start(rssItem.link);
            }
            else
                notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, "Display full RSS entry", "No RSS entry in queue yet.", ToolTipIcon.Warning);
        }


        private void RetrieverCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }


        private void RetrieverProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case Retriever.PROGRESS_NEWRSS:
                    // get item and store it in queue
                    RssItem rssItem = e.UserState as RssItem;

                    // check whether the queue is full
                    if (rssHistory.Count == Properties.Settings.Default.historyDepth)
                        rssHistory.Dequeue();  // remove last item from history
                    rssHistory.Enqueue(rssItem);

                    // show the pop-up
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, rssItem.title, rssItem.description, ToolTipIcon.None);
                    break;

                case Retriever.PROGRESS_ICON:
                    int rssCount = (int)e.UserState;

                    // update icon
                    if (rssCount > 0)
                        notifyIcon.Icon = resources.ico_yellow16;
                    else
                        notifyIcon.Icon = resources.ico_orange16;

                    // update info text
                    notifyIcon.Text = rssCount + resources.str_iconInfoNewsCount;
                    break;

                case Retriever.PROGRESS_ERROR:
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, "Error message", e.UserState as string, ToolTipIcon.Error);
                    break;

                default:
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, "Unexpected notification message.", e.UserState as string, ToolTipIcon.Error);
                    break;
            }
        }
   }
}