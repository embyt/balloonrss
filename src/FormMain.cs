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
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using BalloonRss.Properties;


namespace BalloonRss
{
    public class FormMain : Form
    {
        // some most important GUI element
        private NotifyIcon applicationIcon;

        // some context menu items
        private ToolStripMenuItem mi_history;
        private ToolStripMenuItem mi_nextMessage;
        private ToolStripMenuItem mi_lastMessage;

        // the "working horse"
        private Retriever retriever;

        // the timers to display and to retrieve the balloon tooltips
        private Timer dispTimer;
        private Timer retrieveTimer;

        // application state variables
        private bool isPaused;
        private bool isRssViewed = false;
        private DateTime lastRetrieval = DateTime.MinValue;


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
            // make mutex name which is bound to application and user
            // could also use Environment.UserName instead
            String mutexId = "BalloonRSS_" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).GetHashCode().ToString("x4");
            System.Threading.Mutex singleInstanceMutex = new System.Threading.Mutex(false, mutexId);

            using (singleInstanceMutex)
            {
                // check whether the application is already running
                if (singleInstanceMutex.WaitOne(0, false))
                {
                    // start application
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    FormMain rssForm = new FormMain();
                    Application.Run(rssForm);
                }
                else
                {
                    // display error message
                    MessageBox.Show(Resources.str_balloonErrorDuplicateInstanceBody,
                        Resources.str_balloonErrorDuplicateInstanceHeader,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }


        // constructor of main form
        public FormMain()
        {
            // setup GUI
            InitializeComponent();

            // setup display Timer
            dispTimer = new Timer();
            dispTimer.Tick += new EventHandler(OnDispTimerTick);
            dispTimer.Enabled = false;
            dispTimer.Interval = Settings.Default.displayIntervall * 60 * 1000; // intervall in min

            // setup retrieve Timer
            retrieveTimer = new Timer();
            retrieveTimer.Tick += new EventHandler(OnRetrieverTimerTick);
            retrieveTimer.Enabled = false;
            retrieveTimer.Interval = Settings.Default.retrieveIntervall * 60 * 1000; // intervall in min

            // setup and start the background worker
            retriever = new Retriever();
            retriever.backgroundWorker.RunWorkerCompleted += 
                new System.ComponentModel.RunWorkerCompletedEventHandler(this.RetrieveCompleted);
            retriever.backgroundWorker.ProgressChanged += 
                new System.ComponentModel.ProgressChangedEventHandler(this.RetrieverProgressError);

            // set initial status
            isPaused = Settings.Default.startPaused;

            // set icon and tooltip text
            UpdateIcon();

            // read channel settings
            bool firstStart = retriever.InitializeChannels();

            if (firstStart)
            {
                // display welcome message
                applicationIcon.ShowBalloonTip(
                    Settings.Default.balloonTimespan * 1000,
                    Resources.str_balloonWelcomeHeader,
                    Resources.str_balloonWelcomeBody,
                    ToolTipIcon.Info);
            }

            // load initial channels in background task
            if (!isPaused)
                retriever.backgroundWorker.RunWorkerAsync();
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Icon = Resources.ico_yellow32;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.Text = "BalloonRss";   // no need to take a resource, this name is nowhere shown
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;
            // remove appliation from the ALT+TAB menu
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);

            // Create the NotifyIcon
            this.applicationIcon = new NotifyIcon();
            applicationIcon.ContextMenuStrip = CreateContextMenu();
            applicationIcon.Visible = true;
            applicationIcon.BalloonTipClicked += new EventHandler(OnBalloonTipClicked);
            applicationIcon.MouseClick += new MouseEventHandler(OnIconClicked);

            this.ResumeLayout(false);
        }


        private ContextMenuStrip CreateContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // menuItem Exit
            ToolStripMenuItem mi_exit = new ToolStripMenuItem();
            mi_exit.Text = Resources.str_contextMenuExit;
            mi_exit.Click += new EventHandler(this.MiExitClick);

            // menuItem Settings
            ToolStripMenuItem mi_settings = new ToolStripMenuItem();
            mi_settings.Text = Resources.str_contextMenuSettings;
            mi_settings.Click += new EventHandler(this.MiSettingsClick);
            mi_settings.Enabled = true;

            // menuItem Channel Settings
            ToolStripMenuItem mi_channelSettings = new ToolStripMenuItem();
            mi_channelSettings.Text = Resources.str_contextMenuChannelSettings;
            mi_channelSettings.Click += new EventHandler(this.MiChannelSettingsClick);
            mi_channelSettings.Enabled = true;

            // menuItem Channel Info
            ToolStripMenuItem mi_channelInfo = new ToolStripMenuItem();
            mi_channelInfo.Text = Resources.str_contextMenuChannelInfo;
            mi_channelInfo.Click += new EventHandler(this.MiChannelInfoClick);
            mi_channelInfo.Enabled = true;

            // menuItem Get Channels
            ToolStripMenuItem mi_getChannels = new ToolStripMenuItem();
            mi_getChannels.Text = Resources.str_contextMenuGetChannels;
            mi_getChannels.Click += new EventHandler(this.MiGetChannelsClick);
            mi_getChannels.Enabled = true;

            // menuItem History
            mi_history = new ToolStripMenuItem();
            mi_history.Text = Resources.str_contextMenuHistory;
            mi_history.Click += new EventHandler(this.MiHistoryClick);
            mi_history.Enabled = false;

            // menuItem Next Message
            mi_nextMessage = new ToolStripMenuItem();
            mi_nextMessage.Text = Resources.str_contextMenuNextMessage;
            mi_nextMessage.Click += new EventHandler(this.MiNextMessageClick);
            mi_nextMessage.Enabled = false;

            // menuItem Last Message
            mi_lastMessage = new ToolStripMenuItem();
            mi_lastMessage.Text = Resources.str_contextMenuLastMessage;
            mi_lastMessage.Click += new EventHandler(this.OnBalloonTipClicked);
            mi_lastMessage.Enabled = false;

            // Initialize contextMenu
            contextMenu.Items.AddRange(new ToolStripItem[] 
            { 
                mi_channelSettings,
                mi_settings,
                new ToolStripSeparator(),
                mi_channelInfo,
                mi_getChannels,
                new ToolStripSeparator(),
                mi_history,
                mi_lastMessage,
                mi_nextMessage,
                new ToolStripSeparator(),
                mi_exit,
            });

            return contextMenu;
        }


        private void MiSettingsClick(object sender, EventArgs e)
        {
            // disable timers
            retrieveTimer.Stop();
            dispTimer.Stop();

            // cancel potential background worker operation
            retriever.backgroundWorker.CancelAsync();

            // show the settings form
            FormSettings formSettings = new FormSettings();
            DialogResult result = formSettings.ShowDialog();

            if (result == DialogResult.OK)
            {
                // restore icon properties
                UpdateIcon();
            }

            // update timer values
            dispTimer.Interval = Convert.ToInt32(Settings.Default.displayIntervall * 60 * 1000 *
                retriever.bestPriorityRatio);
            retrieveTimer.Interval = Settings.Default.retrieveIntervall * 60 * 1000; // intervall in seconds

            // restart timer
            retrieveTimer.Start();
        }

        private void MiChannelSettingsClick(object sender, EventArgs e)
        {
            // disable timers
            retrieveTimer.Stop();
            dispTimer.Stop();

            // cancel potential background worker operation
            retriever.backgroundWorker.CancelAsync();

            // show the settings form
            FormChannelSettings formChannelSettings = 
                new FormChannelSettings();
            DialogResult result = formChannelSettings.ShowDialog();

            if (result == DialogResult.OK)
            {
                // restore icon properties
                UpdateIcon();

                // setup new rss list
                // since we modified the channel settings file, there must not be an exception
                retriever.InitializeChannels();
            }

            // start background worker thread to retrieve channels
            if (!isPaused)
            {
                // we might get a race condition here, if the worker thread is still not cancelled!
                if (retriever.backgroundWorker.IsBusy == false)
                {
                    // worker already finished, everything fine
                    retriever.backgroundWorker.RunWorkerAsync();
                }
                else
                {
                    // work still pending, we cannot restart right now

                    // in this case, we start the retriever timer and 
                    // hope that the worker is finished before the timer expires!
                    retrieveTimer.Start();
                }
            }
        }

        private void MiChannelInfoClick(object sender, EventArgs e)
        {
            dispTimer.Stop();

            // update the channel effective priorities and best priority ratio
            retriever.CalculateEffectiveChannelPriorities();
            retriever.UpdateBestPriorityRatio();

            FormChannelInfo formChannelInfo = new FormChannelInfo(retriever.GetChannels());
            formChannelInfo.ShowDialog();

            // restore icon properties, the channels may be cleared
            UpdateIcon();

            dispTimer.Start();
        }

        private void MiGetChannelsClick(object sender, EventArgs e)
        {
            // disable timers
            retrieveTimer.Stop();
            if (retriever.backgroundWorker.IsBusy)
            {
                // abort, we are already about retrieving
                // also no need to restart timer, this will be done as it is finished
                return;
            }

            // start background worker thread to retrieve channels
            retriever.backgroundWorker.RunWorkerAsync();
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


        private void OnIconClicked(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (e.Clicks == 0))
            {
                // toggle pause mode
                isPaused = !isPaused;

                if (isPaused)
                {
                    // enter pause mode
                    // disable timers
                    retrieveTimer.Stop();
                    dispTimer.Stop();
                }
                else
                {
                    // re-enable the application
                    // re-enable timers
                    if ( (DateTime.Now - lastRetrieval).TotalSeconds > Settings.Default.retrieveIntervall * 60)
                    {
                        // the last retrieval is long time ago (or never happened)
                        // start background worker thread to retrieve channels
                        retriever.backgroundWorker.RunWorkerAsync();
                    }
                    else
                        retrieveTimer.Start();

                    dispTimer.Start();
                }

                // update icon and text
                UpdateIcon();
            }
        }


        private void OnBalloonTipClicked(object sender, EventArgs e)
        {
            // open rss item in browser
            if (isRssViewed)
            {
                if (retriever.rssHistory.Count > 0)
                {
                    // get most recent item of queue
                    RssItem rssItem = retriever.rssHistory.ToArray()[retriever.rssHistory.Count - 1];

                    // start browser
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
                else
                {
                    isRssViewed = false;
                    applicationIcon.ShowBalloonTip(Settings.Default.balloonTimespan * 1000, Resources.str_balloonWarningNoEntryHeader, Resources.str_balloonWarningNoEntryBody, ToolTipIcon.Warning);
                }
            }
            // if no rss item was viewed, we skip the click
        }


        private void RetrieveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lastRetrieval = DateTime.Now;

            // update icon
            bool messagesAvailable = UpdateIcon();

            if (messagesAvailable)
            {
                // start the display timer (it may be already running)
                if (!isPaused)
                    dispTimer.Start();

                // update timer value according actual priority, if this time is shorter
                if (dispTimer.Interval > Settings.Default.displayIntervall * 60 * 1000 * retriever.bestPriorityRatio)
                    dispTimer.Interval = Convert.ToInt32(Settings.Default.displayIntervall * 60 * 1000 * retriever.bestPriorityRatio);
            }

            // start the retriever timer
            if (!isPaused)
                retrieveTimer.Start();
        }


        private void OnDispTimerTick(object source, EventArgs e)
        {
            // set default timer for the case of early function return
            dispTimer.Interval = Settings.Default.displayIntervall * 60 * 1000;

            // to avoid parallel access, we skip the RSS display access as the retriever is working
            if (retriever.backgroundWorker.IsBusy)
                return;     // wait for next timer tick...

            // display next item
            RssItem rssItem = retriever.GetNextItem();

            // update text
            UpdateIcon();

            if (rssItem != null)
            {
                // display the news
                isRssViewed = true;

                // determine prefered display mode
                // take care: an RssUpdateItem cannot be displayed with the channel as title!
                if (Settings.Default.channelAsTitle && (rssItem.channel != null))
                {
                    applicationIcon.ShowBalloonTip(Settings.Default.balloonTimespan*1000, rssItem.channel.channelInfo.link, rssItem.title, ToolTipIcon.None);
                }
                else
                {
                    applicationIcon.ShowBalloonTip(Settings.Default.balloonTimespan*1000, rssItem.title, rssItem.description, ToolTipIcon.None);
                }

                // enable the message history (might be already enabled)
                mi_history.Enabled = true;
                mi_lastMessage.Enabled = true;

                // get the timer intervall for the next message
                dispTimer.Interval = Convert.ToInt32(Settings.Default.displayIntervall * 60 * 1000 * retriever.bestPriorityRatio);
            }
            else
            {
                dispTimer.Stop();
            }
        }


        private void OnRetrieverTimerTick(object source, EventArgs e)
        {
            // stop the timer, it is started again as the retrieving is completed
            retrieveTimer.Stop();

            // start background worker thread to retrieve channels
            retriever.backgroundWorker.RunWorkerAsync();
        }
        

        // updates the application icon and its mouse-over-text
        private bool UpdateIcon()
        {
            int rssCount = retriever.GetQueueSize();

            if (!isPaused)
            {
                // update icon
                if (rssCount > 0)
                    applicationIcon.Icon = Resources.ico_yellow16;
                else
                    applicationIcon.Icon = Resources.ico_orange16;

                // update info text
                if (rssCount != 1)
                    applicationIcon.Text = rssCount + Resources.str_iconInfoNewsCount;
                else
                    applicationIcon.Text = Resources.str_iconInfoNewsCount1;
            }
            else
            {
                // update icon and text
                applicationIcon.Icon = Resources.ico_blue16;
                applicationIcon.Text = Resources.str_iconInfoPause;
            }

            if (rssCount != 0)
                mi_nextMessage.Enabled = true;
            else
                mi_nextMessage.Enabled = false;

            return (rssCount > 0);
        }


        private void RetrieverProgressError(object sender, ProgressChangedEventArgs e)
        {
            // report error message via pop-up
            String[] message = e.UserState as String[];
            isRssViewed = false;
            applicationIcon.ShowBalloonTip(Settings.Default.balloonTimespan*1000, message[0], message[1], ToolTipIcon.Error);
        }
    }
}