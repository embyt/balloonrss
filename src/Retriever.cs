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
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;
using BalloonRss.Properties;


namespace BalloonRss
{
    // the retriever class is a collection of all RSS channels
    class Retriever : Dictionary<String, RssChannel>
    {
        // the background worker to retrieve the channels
        public BackgroundWorker backgroundWorker;

        // the history of shown rss entries
        public Queue<RssItem> rssHistory;

        // priority of best channel compared to best available channel
        public double bestPriorityRatio = 1;

        // special RssItem that exists if a new update is available
        private RssUpdateItem applicationUpdateInfo = null;
        private int applicationUpdatePreScaler = 0;


        public Retriever()
        {
            // setup background worker
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.RetrieveChannels);

            // initialize history queue
            rssHistory = new Queue<RssItem>(Settings.Default.historyDepth);
        }


        // read channel config and create instances for all channels
        public bool InitializeChannels()
        {
            bool firstRun = false;

            // do some cleanup since this is executed also in case of a config file change
            this.Clear();

            // read channel configuration file
            ChannelList channelList = new ChannelList(out firstRun);

            // add the channels found
            foreach (ChannelInfo channelInfo in channelList)
            {
                this.Add(channelInfo.link, new RssChannel(channelInfo));
            }

            // setup the initial priorities
            CalculateEffectiveChannelPriorities();

            return firstRun;
        }


        // returns an array of the channels (used for channel information view)
        public RssChannel[] GetChannels()
        {
            RssChannel[] channels = new RssChannel[this.Count];
            this.Values.CopyTo(channels, 0);
            return channels;
        }


        // this is called from the background worker
        private void RetrieveChannels(object sender, DoWorkEventArgs e)
        {
            // get the news from all channels
            foreach (KeyValuePair<String,RssChannel> keyValuePair in this)
            {
                RetrieveChannel(keyValuePair.Key);

                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }
            // setup or update the best priority ratio
            UpdateBestPriorityRatio();

            // check for application update
            if (Settings.Default.checkForUpdates)
                CheckForUpdates();
        }


        // this is called from the background worker
        // it fetches the RSS file from the server
        private bool RetrieveChannel(String url)
        {
            WebResponse webResp;

            // retrieve URL
            try
            {
                WebRequest webReq = (WebRequest)WebRequest.Create(url);
                webResp = webReq.GetResponse();
            }
            catch (Exception e)
            {
                if (Settings.Default.reportNetworkErrors)
                {
                    // the report progress function is used for error signaling
                    backgroundWorker.ReportProgress(0,
                        new String[] { Resources.str_balloonErrorRetrieving + url, e.Message });
                }
                return false;
            }

            // parse rss file
            try
            {
                XmlDocument rssDocument = new XmlDocument();
                rssDocument.Load(webResp.GetResponseStream());
                UpdateChannel(url, rssDocument);
                webResp.Close();
            }
            catch (Exception e)
            {
                backgroundWorker.ReportProgress(0, 
                    new String[] { Resources.str_balloonErrorParseRss + url, e.Message });
                return false;
            }

            return true;
        }


        // this is called from the background worker
        // it invokes the corresponding channel to parse the RSS file
        private void UpdateChannel(String url, XmlNode xmlNode)
        {
            RssChannel rssChannel;

            // search for the channel
            if (this.TryGetValue(url, out rssChannel) == true)
            {
                int oldItemCount = rssChannel.Count;

                // update the channel with the new rss data
                rssChannel.UpdateChannel(xmlNode);
            }
            else
            {
                // this must not happen
                throw new Exception("Could not find channel for " + url);
            }
        }


        // this is called from the background worker
        private void CheckForUpdates()
        {
            // we don't check for it every time 
            if (applicationUpdatePreScaler > 0)
            {
                applicationUpdatePreScaler--;
                return;
            }
            applicationUpdatePreScaler = Settings.Default.updateCheckIntervall;

            // ignore all errors for this comfort feature
            try
            {
                // set current application version
                Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                int currentVersion = 100*assemblyVersion.Major + assemblyVersion.Minor;

                // get the actual version string
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(Settings.Default.updateCheckUrl + currentVersion);
                HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
                byte[] receiveBuffer = new byte[1024];
                int count = httpResp.GetResponseStream().Read(receiveBuffer, 0, receiveBuffer.Length);
                String receiveString = System.Text.Encoding.ASCII.GetString(receiveBuffer, 0, count);
                httpResp.Close();

                // interpret response
                int newVersion = Int32.Parse(receiveString);

                // is there a newer version available?
                if (newVersion > currentVersion)
                {
                    // we must not overwrite an update info which was already displayed
                    if ( (applicationUpdateInfo == null) || (applicationUpdateInfo.NewVersion < newVersion) )
                        applicationUpdateInfo = new RssUpdateItem();

                    applicationUpdateInfo.NewVersion = newVersion;
                    applicationUpdateInfo.CurrentVersion = currentVersion;
                }
            }
            catch (Exception)
            {
                // ignore this
            }
        }


        public int GetQueueSize()
        {
            int rssCount = 0;

            foreach (KeyValuePair<String, RssChannel> keyValuePair in this)
            {
                rssCount += keyValuePair.Value.Count;
            }
            return rssCount;
        }


        public RssItem GetNextItem()
        {
            RssItem rssItem = null;

            // shall we display update info instead of a news message?
            if ( Settings.Default.checkForUpdates && (applicationUpdateInfo != null) &&
                ((DateTime.Now - applicationUpdateInfo.dispDate) > TimeSpan.FromHours(Settings.Default.updateDisplayIntervall)) )
            {
                // OK, we take this one
                rssItem = applicationUpdateInfo;
                // set display timestamp
                rssItem.dispDate = DateTime.Now;
            }
            else
            {
                // normal news message

                // choose the channel for this news entry
                RssChannel rssChannel = GetNextChannel();

                // all channels are empty
                if (rssChannel == null)
                    return null;

                // now, as we have the channel, let the channel select the best news item
                rssItem = rssChannel.GetNextItem();

                // update the best priority ratio
                // we need to do this only if the channel got empty
                if (!rssChannel.IsItemAvailable())
                    UpdateBestPriorityRatio();
            }

            // check whether the queue is full
            if (rssHistory.Count == Settings.Default.historyDepth)
                rssHistory.Dequeue();  // remove last item from history
            // add item to history queue
            rssHistory.Enqueue(rssItem);

            return rssItem;
        }


        private RssChannel GetNextChannel()
        {
            int totalPriority = CalculateEffectiveChannelPriorities();

            // for this I use a random number in the range of the total priority
            int randomNumber = new Random().Next(totalPriority);

            // now walk through the channel list until we found the number
            int actualNumber = 0;
            RssChannel rssChannel = null;
            foreach (KeyValuePair<String, RssChannel> keyValuePair in this)
            {
                RssChannel curChannel = keyValuePair.Value;
                if (curChannel.IsItemAvailable())
                {
                    actualNumber += curChannel.effectivePriority;
                }

                // are we done?
                if (actualNumber > randomNumber)
                {
                    rssChannel = curChannel;
                    break;
                }
            }

            return rssChannel;
        }


        public int CalculateEffectiveChannelPriorities()
        {
            int prioritySum = 0;
            int viewedSum = 0;
            int openedSum = 0;

            // determine the sum of all viewed and opened items
            foreach (KeyValuePair<String, RssChannel> keyValuePair in this)
            {
                viewedSum += keyValuePair.Value.channelViewedCount;
                openedSum += keyValuePair.Value.channelOpenedCount;
            }

            // determine the effective priority of each channel
            foreach (KeyValuePair<String, RssChannel> keyValuePair in this)
            {
                RssChannel curChannel = keyValuePair.Value;

                // take care on div by zero
                try
                {
                    curChannel.effectivePriority = (int)Math.Round(
                        curChannel.channelInfo.priority 
                            * (10 - Settings.Default.clickInfluence) +
                        (((float)curChannel.channelOpenedCount / openedSum) / ((float)curChannel.channelViewedCount / viewedSum)) 
                            * Settings.Default.clickInfluence );
                }
                catch (ArithmeticException)
                {
                    // take the default priority if we did not open or view any item yet
                    curChannel.effectivePriority = 
                        curChannel.channelInfo.priority 
                            * (10 - Settings.Default.clickInfluence) + 
                        1
                            * Settings.Default.clickInfluence;
                }

                // if the channel is non-empty, add it to the total priority
                if (curChannel.IsItemAvailable())
                    prioritySum += keyValuePair.Value.effectivePriority;
            }

            return prioritySum;
        }


        public void UpdateBestPriorityRatio()
        {
            int bestPriority = 0;
            int bestAvailablePriority = 0;

            // determine the best and best available effective priority
            foreach (KeyValuePair<String, RssChannel> keyValuePair in this)
            {
                RssChannel curChannel = keyValuePair.Value;

                bestPriority = Math.Max(bestPriority, curChannel.effectivePriority);

                if (curChannel.IsItemAvailable())
                    bestAvailablePriority = Math.Max(bestAvailablePriority, curChannel.effectivePriority);
            }

            // determine best priority ratio
            if (bestAvailablePriority > 0)
                bestPriorityRatio = bestPriority / (double)bestAvailablePriority;
            else
                bestPriorityRatio = 1;  // if there is no RSS entry available we don't care for this value
        }
    }
}
