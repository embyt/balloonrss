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
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;


namespace BalloonRss
{
    class Retriever : BackgroundWorker
    {
        // this holds all the RSS items
        private RssList rssList;

        // the history of shown rss entries
        public Queue<RssItem> rssHistory;


        public Retriever()
            : base()
        {
            // setup background worker
            WorkerSupportsCancellation = true;
            WorkerReportsProgress = true;
            DoWork += new System.ComponentModel.DoWorkEventHandler(this.RetrieveChannels);

            // initialise rssList
            rssList = new RssList();

            // initialize history queue
            rssHistory = new Queue<RssItem>(Properties.Settings.Default.historyDepth);
        }


        public void Initialize(string configFileName)
        {
            bool gotChannelTag = false;

            // open xml configuration file
            XmlDocument configFile = new XmlDocument();

            try
            {
                configFile.Load(configFileName);

                // parse configuration file
                foreach (XmlNode rootNode in configFile)
                {
                    // search for "channels" tag
                    if (rootNode.Name.Trim().ToLower() == "channels")
                    {
                        gotChannelTag = true;
                        rssList.ReadConfigFile(rootNode);
                    }
                }
            }
            catch (Exception e)
            {
                ReportProgress(0, new String[] { resources.str_balloonErrorConfigFile, e.Message });
                return;
            }

            if (!gotChannelTag)
            {
                ReportProgress(0, new String[] { resources.str_balloonErrorConfigFile, resources.str_balloonErrorConfigChannelTag });
            }
        }


        public RssChannel[] GetChannels()
        {
            RssChannel[] channels = new RssChannel[rssList.Count];
            rssList.Values.CopyTo(channels, 0);
            return channels;
        }


        public int GetQueueSize()
        {
            return rssList.RssCount;
        }


        public RssItem GetNextItem()
        {
            RssItem rssItem = rssList.GetNextItem();

            // store the item in the history queue
            if (rssItem != null)
            {
                // check whether the queue is full
                if (rssHistory.Count == Properties.Settings.Default.historyDepth)
                    rssHistory.Dequeue();  // remove last item from history
                rssHistory.Enqueue(rssItem);
            }

            return rssItem;
        }


        private void RetrieveChannels(object sender, DoWorkEventArgs e)
        {
            // get the news from all channels
            foreach (KeyValuePair<string,RssChannel> keyValuePair in rssList)
            {
                if (CancellationPending)
                    break;

                RetrieveChannel(keyValuePair.Key);
            }
        }


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
                ReportProgress(0, new String[] { resources.str_balloonErrorRetrieving + url, e.Message });
                return false;
            }

            // parse rss file
            try
            {
                XmlDocument rssDocument = new System.Xml.XmlDocument();
                rssDocument.Load(httpResp.GetResponseStream());
                rssList.UpdateChannel(url, rssDocument);
            }
            catch (Exception e)
            {
                ReportProgress(0, new String[] { resources.str_balloonErrorParseRss + url, e.Message });
                return false;
            }

            return true;
        }
    }
}
