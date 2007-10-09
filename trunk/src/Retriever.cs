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
    public class Retriever : BackgroundWorker
    {
        private RssList rssList;

        public const int PROGRESS_ERROR = 0;
        public const int PROGRESS_NEWRSS = 10;


        public Retriever()
            : base()
        {
            // setup background worker
            WorkerSupportsCancellation = true;
            WorkerReportsProgress = true;
            DoWork += new System.ComponentModel.DoWorkEventHandler(this.StartWork);

            // initialise rssList
            rssList = new RssList();
        }


        private void ReadConfigFile(string configFileName)
        {
            // open xml configuration file
            XmlDocument configFile = new XmlDocument();

            try
            {
                configFile.Load(configFileName);
            }
            catch (Exception e)
            {
                ReportProgress(Retriever.PROGRESS_ERROR, e.Message);
                return;
            }

            // parse configuration file
            foreach (XmlNode rootNode in configFile)
            {
                // search for "channels" tag
                if (rootNode.Name.Trim().ToLower() == "channels")
                {
                    rssList.ReadConfigFile(rootNode);
                }
            }
        }


        private void StartWork(object sender, DoWorkEventArgs e)
        {
            // read channel settings
            ReadConfigFile(Properties.Settings.Default.channelConfigFileName);

            // setup channel system and get initial data
            rssList.GetInitialChannels(this);

            // perform main endless loop
            int retrieveDivider = 0;
            while (!CancellationPending)
            {
                // either we read a channel or we display an item
                if ((retrieveDivider % Properties.Settings.Default.retrieveIntervall) == 0)
                {
                    // retrieve and update next channel
                    rssList.GetNextChannel(this);
                }
                else
                {
                    // display next item
                    RssItem rssItem = rssList.GetNextItem();

                    if (rssItem != null)
                    {
                        // display the news
                        ReportProgress(Retriever.PROGRESS_NEWRSS, rssItem);
                    }
                }

                // wait befor next message
                Thread.Sleep(Properties.Settings.Default.baseRecurrence);

                // update scheduler index
                retrieveDivider++;
                if (retrieveDivider == Properties.Settings.Default.retrieveIntervall)
                    retrieveDivider = 0;
            }
        }


        public bool GetChannel(String url)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse httpResp = httpReq.GetResponse();
            System.Xml.XmlDocument rssDocument = new System.Xml.XmlDocument();
            rssDocument.Load(httpResp.GetResponseStream());
            rssList.UpdateChannel(url, rssDocument);

            return true;
        }
    }
}
