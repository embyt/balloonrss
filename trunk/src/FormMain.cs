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
    public class FormMain : System.Windows.Forms.Form
    {
        // some important GUI elements
        private NotifyIcon notifyIcon;

        // context menu items
        private MenuItem mi_history;
        private MenuItem mi_nextMessage;
        private MenuItem mi_lastMessage;

        // the "working horse"
        private Retriever retriever;

        // the timers to display and to retrieve the balloon tooltips
        private Timer dispTimer;
        private Timer retrieveTimer;

        // some dll calls needed to hide the icon in the ALT+TAB bar
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr window, int index, int value);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr window, int index);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_APPWINDOW = 0x00040000;



        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormMain rssForm = new FormMain();
            Application.Run(rssForm);
        }


        public FormMain()
        {
            // setup GUI
            InitializeComponent();

            // setup display Timer
            dispTimer = new Timer();
            dispTimer.Interval = Properties.Settings.Default.displayIntervall;
            dispTimer.Tick += new EventHandler(OnDispTimerTick);
            dispTimer.Enabled = false;

            // setup retrieve Timer
            retrieveTimer = new Timer();
            retrieveTimer.Interval = Properties.Settings.Default.retrieveIntervall;
            retrieveTimer.Tick += new EventHandler(OnRetrieverTimerTick);
            retrieveTimer.Enabled = false;
            
            // setup and start the background worker
            retriever = new Retriever();
            retriever.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.RetrieveCompleted);
            retriever.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.RetrieverProgressChanged);

            // read channel settings
            retriever.Initialize(Properties.Settings.Default.channelConfigFileName);

            // load initial channels
            retriever.RunWorkerAsync();
        }


        private ContextMenu CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            // menuItem Exit
            MenuItem mi_exit = new System.Windows.Forms.MenuItem();
            mi_exit.Text = resources.str_contextMenuExit;
            mi_exit.Click += new System.EventHandler(this.MiExitClick);

            // menuItem Settings
            MenuItem mi_settings = new System.Windows.Forms.MenuItem();
            mi_settings.Text = resources.str_contextMenuSettings;
            mi_settings.Click += new System.EventHandler(this.MiSettingsClick);
            mi_settings.Enabled = false;

            // menuItem Channel Info
            MenuItem mi_channelInfo = new System.Windows.Forms.MenuItem();
            mi_channelInfo.Text = resources.str_contextMenuChannelInfo;
            mi_channelInfo.Click += new System.EventHandler(this.MiChannelInfoClick);
            mi_channelInfo.Enabled = true;

            // menuItem History
            mi_history = new System.Windows.Forms.MenuItem();
            mi_history.Text = resources.str_contextMenuHistory;
            mi_history.Click += new System.EventHandler(this.MiHistoryClick);
            mi_history.Enabled = false;

            // menuItem Next Message
            mi_nextMessage = new System.Windows.Forms.MenuItem();
            mi_nextMessage.Text = resources.str_contextMenuNextMessage;
            mi_nextMessage.Click += new System.EventHandler(this.MiNextMessageClick);
            mi_nextMessage.Enabled = false;

            // menuItem Last Message
            mi_lastMessage = new System.Windows.Forms.MenuItem();
            mi_lastMessage.Text = resources.str_contextMenuLastMessage;
            mi_lastMessage.Click += new System.EventHandler(this.OnBalloonTipClicked);
            mi_lastMessage.Enabled = false;

            // Initialize contextMenu
            contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] 
            { 
                mi_settings,
                mi_channelInfo,
                new System.Windows.Forms.MenuItem("-"),
                mi_history,
                mi_lastMessage,
                mi_nextMessage,
                new System.Windows.Forms.MenuItem("-"),
                mi_exit,
            });

            return contextMenu;
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Icon = BalloonRss.resources.ico_yellow32;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.Text = "BalloonRss";
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;
            // remove appliation from the ALT+TAB menu
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);

            // Create the NotifyIcon.
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.ContextMenu = CreateContextMenu();
            notifyIcon.Icon = BalloonRss.resources.ico_blue16;
            notifyIcon.Text = resources.str_iconInfoInit;
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipClicked += new EventHandler(OnBalloonTipClicked);
            // notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_BalloonTipClicked);

            this.ResumeLayout(false);
        }
 

        private void MiSettingsClick(object sender, EventArgs e)
        {
            // cancel background worker operation
            retriever.CancelAsync();

            // Show the settings form

        }

        private void MiChannelInfoClick(object sender, EventArgs e)
        {
            dispTimer.Stop();

            FormChannelInfo formChannelInfo = new FormChannelInfo(retriever.GetChannels());
            formChannelInfo.ShowDialog();

            dispTimer.Start();
        }

        private void MiExitClick(object sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void MiHistoryClick(object sender, EventArgs e)
        {
            dispTimer.Stop();

            FormHistory formHistory = new FormHistory(retriever.rssHistory);
            formHistory.ShowDialog();

            dispTimer.Start();
        }

        private void MiNextMessageClick(object sender, EventArgs e)
        {
            // raise the display timer event
            OnDispTimerTick(this, EventArgs.Empty);
        }
        

        private void OnBalloonTipClicked(object sender, EventArgs e)
        {
            // display full rss entry
            if (retriever.rssHistory.Count > 0)
            {
                // get most recent item of queue
                RssItem rssItem = retriever.rssHistory.ToArray()[retriever.rssHistory.Count - 1];
                // start browser
                System.Diagnostics.Process.Start(rssItem.link);
            }
            else
                notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, resources.str_balloonWarningNoEntryHeader, resources.str_balloonWarningNoEntryBody, ToolTipIcon.Warning);
        }


        private void RetrieveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // update icon
            if (retriever.GetQueueSize() > 0)
            {
                // start the display timer (it may be already running)
                dispTimer.Start();
                mi_nextMessage.Enabled = true;
                UpdateCount(retriever.GetQueueSize());
            }

            // start the retriever timer
            retrieveTimer.Start();
        }


        private void OnDispTimerTick(object source, EventArgs e)
        {
            // to avoid parallel access, we skip the RSS display access as the retriever is working
            if (retriever.IsBusy)
                return;     // wait for next timer tick...

            // display next item
            RssItem rssItem = retriever.GetNextItem();

            // update text
            UpdateCount(retriever.GetQueueSize());

            if (rssItem != null)
            {
                // display the news
                if (Properties.Settings.Default.channelAsTitle)
                {
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, rssItem.channel, rssItem.title, ToolTipIcon.None);
                }
                else
                {
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, rssItem.title, rssItem.description, ToolTipIcon.None);
                }

                // enable the message history (might be already enabled)
                mi_history.Enabled = true;
                mi_lastMessage.Enabled = true;
            }
            else
            {
                dispTimer.Stop();
                mi_nextMessage.Enabled = false;
            }
        }


        private void OnRetrieverTimerTick(object source, EventArgs e)
        {
            // stop the timer, it is started again as the retrieving is completed
            retrieveTimer.Stop();

            // start background worker thread to retrieve channels
            retriever.RunWorkerAsync();
        }
        

        private void UpdateCount(int rssCount)
        {
            // update icon
            if (rssCount > 0)
                notifyIcon.Icon = resources.ico_yellow16;
            else
                notifyIcon.Icon = resources.ico_orange16;

            // update info text
            notifyIcon.Text = rssCount + resources.str_iconInfoNewsCount;
        }


        private void RetrieverProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan, resources.str_balloonErrorHeader, e.UserState as string, ToolTipIcon.Error);
        }
   }
}