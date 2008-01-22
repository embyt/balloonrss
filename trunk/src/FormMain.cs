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
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;


namespace BalloonRss
{
    public class FormMain : Form
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

        // application state variables
        private bool isPaused = false;
        private bool isRssViewed = false;


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
                    MessageBox.Show(Properties.Resources.str_balloonErrorDuplicateInstanceBody,
                        Properties.Resources.str_balloonErrorDuplicateInstanceHeader,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }


        public FormMain()
        {
            // setup GUI
            InitializeComponent();

            // setup display Timer
            dispTimer = new Timer();
            dispTimer.Tick += new EventHandler(OnDispTimerTick);
            dispTimer.Enabled = false;
            dispTimer.Interval = Properties.Settings.Default.displayIntervall * 1000; // intervall in seconds

            // setup retrieve Timer
            retrieveTimer = new Timer();
            retrieveTimer.Tick += new EventHandler(OnRetrieverTimerTick);
            retrieveTimer.Enabled = false;
            retrieveTimer.Interval = Properties.Settings.Default.retrieveIntervall * 1000; // intervall in seconds

            // setup and start the background worker
            retriever = new Retriever();
            retriever.backgroundWorker.RunWorkerCompleted += 
                new System.ComponentModel.RunWorkerCompletedEventHandler(this.RetrieveCompleted);
            retriever.backgroundWorker.ProgressChanged += 
                new System.ComponentModel.ProgressChangedEventHandler(this.RetrieverProgressError);

            // set initial icon
            UpdateIcon();

            // read channel settings
            try
            {
                // this may raise an exception in case of a fatal error dealing with the config file
                retriever.InitializeChannels();
            }
            catch (Exception)
            {
                // at the first exception, the channel config file was created, the second try should work
                try
                {
                    retriever.InitializeChannels();
                }
                catch (Exception)
                {
                    // if also this try failed, we probably could not find the defaultChannels.xml file
                    // display error message
                    notifyIcon.ShowBalloonTip(
                        Properties.Settings.Default.balloonTimespan * 1000,
                        Properties.Resources.str_balloonErrorChannelsHeader,
                        Properties.Resources.str_balloonErrorChannelsBody,
                        ToolTipIcon.Error);

                    // we have to return here to skip welcome message and to not start retrieving
                    return;
                }

                // display welcome message
                notifyIcon.ShowBalloonTip(
                    Properties.Settings.Default.balloonTimespan * 1000,
                    Properties.Resources.str_balloonWelcomeHeader,
                    Properties.Resources.str_balloonWelcomeBody,
                    ToolTipIcon.Info);
            }

            // load initial channels
            retriever.backgroundWorker.RunWorkerAsync();
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Icon = BalloonRss.Properties.Resources.ico_yellow32;
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
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipClicked += new EventHandler(OnBalloonTipClicked);
            notifyIcon.MouseClick += new MouseEventHandler(OnIconClicked);

            this.ResumeLayout(false);
        }


        private ContextMenu CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            // menuItem Exit
            MenuItem mi_exit = new System.Windows.Forms.MenuItem();
            mi_exit.Text = Properties.Resources.str_contextMenuExit;
            mi_exit.Click += new System.EventHandler(this.MiExitClick);

            // menuItem Settings
            MenuItem mi_settings = new System.Windows.Forms.MenuItem();
            mi_settings.Text = Properties.Resources.str_contextMenuSettings;
            mi_settings.Click += new System.EventHandler(this.MiSettingsClick);
            mi_settings.Enabled = true;

            // menuItem Channel Settings
            MenuItem mi_channelSettings = new System.Windows.Forms.MenuItem();
            mi_channelSettings.Text = Properties.Resources.str_contextMenuChannelSettings;
            mi_channelSettings.Click += new System.EventHandler(this.MiChannelSettingsClick);
            mi_channelSettings.Enabled = true;

            // menuItem Channel Info
            MenuItem mi_channelInfo = new System.Windows.Forms.MenuItem();
            mi_channelInfo.Text = Properties.Resources.str_contextMenuChannelInfo;
            mi_channelInfo.Click += new System.EventHandler(this.MiChannelInfoClick);
            mi_channelInfo.Enabled = true;

            // menuItem Get Channels
            MenuItem mi_getChannels = new System.Windows.Forms.MenuItem();
            mi_getChannels.Text = Properties.Resources.str_contextMenuGetChannels;
            mi_getChannels.Click += new System.EventHandler(this.MiGetChannelsClick);
            mi_getChannels.Enabled = true;

            // menuItem History
            mi_history = new System.Windows.Forms.MenuItem();
            mi_history.Text = Properties.Resources.str_contextMenuHistory;
            mi_history.Click += new System.EventHandler(this.MiHistoryClick);
            mi_history.Enabled = false;

            // menuItem Next Message
            mi_nextMessage = new System.Windows.Forms.MenuItem();
            mi_nextMessage.Text = Properties.Resources.str_contextMenuNextMessage;
            mi_nextMessage.Click += new System.EventHandler(this.MiNextMessageClick);
            mi_nextMessage.Enabled = false;

            // menuItem Last Message
            mi_lastMessage = new System.Windows.Forms.MenuItem();
            mi_lastMessage.Text = Properties.Resources.str_contextMenuLastMessage;
            mi_lastMessage.Click += new System.EventHandler(this.OnBalloonTipClicked);
            mi_lastMessage.Enabled = false;

            // Initialize contextMenu
            contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] 
            { 
                mi_channelSettings,
                mi_settings,
                new System.Windows.Forms.MenuItem("-"),
                mi_channelInfo,
                mi_getChannels,
                new System.Windows.Forms.MenuItem("-"),
                mi_history,
                mi_lastMessage,
                mi_nextMessage,
                new System.Windows.Forms.MenuItem("-"),
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
                mi_nextMessage.Enabled = false;
            }

            // update timer values
            dispTimer.Interval = Convert.ToInt32(Properties.Settings.Default.displayIntervall * 1000 *
                retriever.bestPriorityRatio);
            retrieveTimer.Interval = Properties.Settings.Default.retrieveIntervall * 1000; // intervall in seconds

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
            retriever.backgroundWorker.RunWorkerAsync();
        }

        private void MiChannelInfoClick(object sender, EventArgs e)
        {
            dispTimer.Stop();

            // update the channel effective priorities and best priority ratio
            retriever.CalculateEffectiveChannelPriorities();
            retriever.UpdateBestPriorityRatio();

            FormChannelInfo formChannelInfo = new FormChannelInfo(retriever.GetChannels());
            formChannelInfo.ShowDialog();

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
                    rssItem.channel.ActivateItem(rssItem);
                }
                else
                {
                    isRssViewed = false;
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan * 1000, Properties.Resources.str_balloonWarningNoEntryHeader, Properties.Resources.str_balloonWarningNoEntryBody, ToolTipIcon.Warning);
                }
            }
            // if no rss item was viewed, we skip the click
        }


        private void RetrieveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // update icon
            if (retriever.GetQueueSize() > 0)
            {
                // start the display timer (it may be already running)
                dispTimer.Start();
                // update timer value according actual priority, if this time is shorter
                if (dispTimer.Interval > Properties.Settings.Default.displayIntervall * 1000 * retriever.bestPriorityRatio)
                    dispTimer.Interval = Convert.ToInt32(Properties.Settings.Default.displayIntervall * 1000 * retriever.bestPriorityRatio);
                mi_nextMessage.Enabled = true;
            }
            UpdateIcon();

            // start the retriever timer
            retrieveTimer.Start();
        }


        private void OnDispTimerTick(object source, EventArgs e)
        {
            // set default timer for the case of early function return
            dispTimer.Interval = Properties.Settings.Default.displayIntervall * 1000;

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
                if (Properties.Settings.Default.channelAsTitle)
                {
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan * 1000, rssItem.channel.channelInfo.link, rssItem.title, ToolTipIcon.None);
                }
                else
                {
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan*1000, rssItem.title, rssItem.description, ToolTipIcon.None);
                }

                // enable the message history (might be already enabled)
                mi_history.Enabled = true;
                mi_lastMessage.Enabled = true;

                // get the timer intervall for the next message
                dispTimer.Interval = Convert.ToInt32(Properties.Settings.Default.displayIntervall * 1000 * retriever.bestPriorityRatio);
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
            retriever.backgroundWorker.RunWorkerAsync();
        }
        

        private void UpdateIcon()
        {
            if (!isPaused)
            {
                int rssCount = retriever.GetQueueSize();

                // update icon
                if (rssCount > 0)
                    notifyIcon.Icon = Properties.Resources.ico_yellow16;
                else
                    notifyIcon.Icon = Properties.Resources.ico_orange16;

                // update info text
                if (rssCount != 1)
                    notifyIcon.Text = rssCount + Properties.Resources.str_iconInfoNewsCount;
                else
                    notifyIcon.Text = Properties.Resources.str_iconInfoNewsCount1;
            }
            else
            {
                // update icon and text
                notifyIcon.Icon = Properties.Resources.ico_blue16;
                notifyIcon.Text = Properties.Resources.str_iconInfoPause;
            }
        }


        private void RetrieverProgressError(object sender, ProgressChangedEventArgs e)
        {
            // report error message via pop-up
            String[] message = e.UserState as String[];
            isRssViewed = false;
            notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan*1000, message[0], message[1], ToolTipIcon.Error);
        }
    }
}