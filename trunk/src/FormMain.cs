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

        // the application may be paused
        private bool isPaused = false;

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
            using (System.Threading.Mutex singleInstanceMutex =
                new System.Threading.Mutex(false, "BalloonRSS_single_instance_mutex"))
            {
                // check whether the application is already running
                if (!singleInstanceMutex.WaitOne(0, false))
                {
                    MessageBox.Show(resources.str_balloonErrorDuplicateInstanceBody, 
                        resources.str_balloonErrorDuplicateInstanceHeader, 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    return;
                }

                // start application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                FormMain rssForm = new FormMain();
                Application.Run(rssForm);
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

            // setup retrieve Timer
            retrieveTimer = new Timer();
            retrieveTimer.Tick += new EventHandler(OnRetrieverTimerTick);
            retrieveTimer.Enabled = false;
            
            // setup and start the background worker
            retriever = new Retriever();
            retriever.backgroundWorker.RunWorkerCompleted += 
                new System.ComponentModel.RunWorkerCompletedEventHandler(this.RetrieveCompleted);
            retriever.backgroundWorker.ProgressChanged += 
                new System.ComponentModel.ProgressChangedEventHandler(this.RetrieverProgressError);

            // set initial icon
            UpdateIcon();

            // start the action...
            StartRetriever();
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
            mi_exit.Text = resources.str_contextMenuExit;
            mi_exit.Click += new System.EventHandler(this.MiExitClick);

            // menuItem Settings
            MenuItem mi_settings = new System.Windows.Forms.MenuItem();
            mi_settings.Text = resources.str_contextMenuSettings;
            mi_settings.Click += new System.EventHandler(this.MiSettingsClick);
            mi_settings.Enabled = true;

            // menuItem Channel Settings
            MenuItem mi_channelSettings = new System.Windows.Forms.MenuItem();
            mi_channelSettings.Text = resources.str_contextMenuChannelSettings;
            mi_channelSettings.Click += new System.EventHandler(this.MiChannelSettingsClick);
            mi_channelSettings.Enabled = true;

            // menuItem Channel Info
            MenuItem mi_channelInfo = new System.Windows.Forms.MenuItem();
            mi_channelInfo.Text = resources.str_contextMenuChannelInfo;
            mi_channelInfo.Click += new System.EventHandler(this.MiChannelInfoClick);
            mi_channelInfo.Enabled = true;

            // menuItem Get Channels
            MenuItem mi_getChannels = new System.Windows.Forms.MenuItem();
            mi_getChannels.Text = resources.str_contextMenuGetChannels;
            mi_getChannels.Click += new System.EventHandler(this.MiGetChannelsClick);
            mi_getChannels.Enabled = true;

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
                mi_channelSettings,
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

            // setup new rss list
            StartRetriever();
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
            }

            // setup new rss list
            StartRetriever();
        }

        private void MiChannelInfoClick(object sender, EventArgs e)
        {
            dispTimer.Stop();

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
            // display full rss entry
            if (retriever.rssHistory.Count > 0)
            {
                // get most recent item of queue
                RssItem rssItem = retriever.rssHistory.ToArray()[retriever.rssHistory.Count - 1];
                // start browser
                System.Diagnostics.Process.Start(rssItem.link);
            }
            else
                notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan*1000, resources.str_balloonWarningNoEntryHeader, resources.str_balloonWarningNoEntryBody, ToolTipIcon.Warning);
        }


        private void StartRetriever()
        {
            // setup the timer intervalls (this is called also after settings change...)
            dispTimer.Interval = Properties.Settings.Default.displayIntervall * 1000; // intervall in seconds
            retrieveTimer.Interval = Properties.Settings.Default.retrieveIntervall * 1000; // intervall in seconds

            // read channel settings
            try
            {
                // this may raise an exception in case of a fatal error dealing with the config file
                retriever.InitializeChannels(Properties.Settings.Default.channelConfigFileName);

                // init successful, load initial channels
                retriever.backgroundWorker.RunWorkerAsync();
            }
            catch (Exception e)
            {
                // display error message
                notifyIcon.ShowBalloonTip(
                    Properties.Settings.Default.balloonTimespan * 1000,
                    resources.str_balloonErrorConfigFile,
                    e.Message,
                    ToolTipIcon.Error);

                // wait until the user changes the settings
            }
        }


        private void RetrieveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // update icon
            if (retriever.GetQueueSize() > 0)
            {
                // start the display timer (it may be already running)
                dispTimer.Start();
                mi_nextMessage.Enabled = true;
            }
            UpdateIcon();

            // start the retriever timer
            retrieveTimer.Start();
        }


        private void OnDispTimerTick(object source, EventArgs e)
        {
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
                if (Properties.Settings.Default.channelAsTitle)
                {
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan*1000, rssItem.channel, rssItem.title, ToolTipIcon.None);
                }
                else
                {
                    notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan*1000, rssItem.title, rssItem.description, ToolTipIcon.None);
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
            retriever.backgroundWorker.RunWorkerAsync();
        }
        

        private void UpdateIcon()
        {
            if (!isPaused)
            {
                int rssCount = retriever.GetQueueSize();

                // update icon
                if (rssCount > 0)
                    notifyIcon.Icon = resources.ico_yellow16;
                else
                    notifyIcon.Icon = resources.ico_orange16;

                // update info text
                if (rssCount != 1)
                    notifyIcon.Text = rssCount + resources.str_iconInfoNewsCount;
                else
                    notifyIcon.Text = resources.str_iconInfoNewsCount1;
            }
            else
            {
                // update icon and text
                notifyIcon.Icon = resources.ico_blue16;
                notifyIcon.Text = resources.str_iconInfoPause;
            }
        }


        private void RetrieverProgressError(object sender, ProgressChangedEventArgs e)
        {
            String[] message = e.UserState as String[];
            notifyIcon.ShowBalloonTip(Properties.Settings.Default.balloonTimespan*1000, message[0], message[1], ToolTipIcon.Error);
        }
    }
}