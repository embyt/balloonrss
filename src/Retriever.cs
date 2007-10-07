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
