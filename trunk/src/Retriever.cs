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
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;


namespace BalloonRss
{
    // the retriever class is a collection of all RSS channels
    class Retriever : System.Collections.Generic.Dictionary<String, RssChannel>
    {
        // the background worker to retrieve the channels
        public BackgroundWorker backgroundWorker;

        // the history of shown rss entries
        public Queue<RssItem> rssHistory;

        // total priority used for channel selection
        private int totalPriority;

        // total number of rss messages left
        private int rssCount;


        public Retriever()
        {
            // setup background worker
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.RetrieveChannels);

            // initialize history queue
            rssHistory = new Queue<RssItem>(Properties.Settings.Default.historyDepth);
        }


        public void InitializeChannels()
        {
            // do some cleanup since this is executed also in case of a config file change
            this.Clear();
            totalPriority = 0;
            rssCount = 0;

            // read channel configuration file
            // this may raise an exception in case of a fatal error dealing with the config file
            ChannelList channelList = new ChannelList(false);

            // add the channels found
            foreach (ChannelInfo channelInfo in channelList)
            {
                this.Add(channelInfo.link, new RssChannel(channelInfo));
            }
        }


        public RssChannel[] GetChannels()
        {
            RssChannel[] channels = new RssChannel[this.Count];
            this.Values.CopyTo(channels, 0);
            return channels;
        }


        public int GetQueueSize()
        {
            return rssCount;
        }


        public RssItem GetNextItem()
        {
            // first, choose the channel for this news entry

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
                    actualNumber += curChannel.channelInfo.priority;
                }

                // are we done?
                if (actualNumber > randomNumber)
                {
                    rssChannel = curChannel;
                    break;
                }
            }

            // all channels are empty
            if (rssChannel == null)
                return null;


            // now, as we have the channel, let the channel select the best news item
            RssItem rssItem = rssChannel.GetNextItem();
            rssCount--;

            // update total priority if channel is empty now
            if (rssChannel.IsItemAvailable() == false)
            {
                totalPriority -= rssChannel.channelInfo.priority;
            }


            // check whether the queue is full
            if (rssHistory.Count == Properties.Settings.Default.historyDepth)
                rssHistory.Dequeue();  // remove last item from history
            // add item to history queue
            rssHistory.Enqueue(rssItem);

            return rssItem;
        }


        // this is called from the background worker
        private void RetrieveChannels(object sender, DoWorkEventArgs e)
        {
            // get the news from all channels
            foreach (KeyValuePair<String,RssChannel> keyValuePair in this)
            {
                if (backgroundWorker.CancellationPending)
                    break;

                RetrieveChannel(keyValuePair.Key);
            }
        }


        // this is called from the background worker
        private bool RetrieveChannel(String url)
        {
            WebResponse httpResp;

            // retrieve URL
            try
            {
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
                httpResp = httpReq.GetResponse();
            }
            catch (Exception e)
            {
                // the report progress function is used for error signaling
                backgroundWorker.ReportProgress(0, 
                    new String[] { resources.str_balloonErrorRetrieving + url, e.Message });
                return false;
            }

            // parse rss file
            try
            {
                XmlDocument rssDocument = new System.Xml.XmlDocument();
                rssDocument.Load(httpResp.GetResponseStream());
                UpdateChannel(url, rssDocument);
                httpResp.Close();
            }
            catch (Exception e)
            {
                backgroundWorker.ReportProgress(0, 
                    new String[] { resources.str_balloonErrorParseRss + url, e.Message });
                return false;
            }

            return true;
        }


        // this is called from the background worker
        public void UpdateChannel(String url, XmlNode xmlNode)
        {
            RssChannel rssChannel;

            // search for the channel
            if (this.TryGetValue(url, out rssChannel) == true)
            {
                int oldItemCount = rssChannel.Count;

                // update the channel with the new rss data
                int newMessages = rssChannel.UpdateChannel(xmlNode);

                // update score and count
                rssCount += newMessages;
                if ((oldItemCount == 0) && (rssChannel.Count > 0))
                {
                    totalPriority += rssChannel.channelInfo.priority;
                }
            }
            else
            {
                // this must not happen
                throw new Exception("Could not find channel for " + url);
            }
        }
    }
}
