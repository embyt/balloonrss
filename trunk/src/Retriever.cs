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

        public string configFileName = "config.xml";

        // default settings
        public int baseRecurrence = 30000;    // ms
        public int retrieveIntervall = 30;    // baseReccurences

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
            catch (FileNotFoundException e)
            {
                ReportProgress(Retriever.PROGRESS_ERROR, e.Message);
                CreateDefaultConfigFile();
                return;
            }
            catch (Exception e)
            {
                ReportProgress(Retriever.PROGRESS_ERROR, e.Message);
                return;
            }

            // parse configuration file
            foreach (XmlNode rootNode in configFile)
            {
                // search for "settings" tag
                if (rootNode.Name.Trim().ToLower() == "settings")
                {
                    foreach (XmlNode mainNode in rootNode)
                    {
                        // search for "channels" tag
                        switch (mainNode.Name.Trim().ToLower())
                        {
                            case "channels":
                                rssList.ReadConfigFile(mainNode);
                                break;
                            case "program":
                                ReadProgramOptions(mainNode);
                                break;
                        }
                    }
                }
            }
        }

        public void ReadProgramOptions(XmlNode optionsNode)
        {
            // parse configuration file
            foreach (XmlNode xmlNode in optionsNode)
            {
                // search for "item" tag
                switch (xmlNode.Name.Trim().ToLower())
                {
                    case "baserecurrence":
                        baseRecurrence = Convert.ToInt32(xmlNode.InnerText);
                        break;
                    case "retrieveintervall":
                        retrieveIntervall = Convert.ToInt32(xmlNode.InnerText);
                        break;
                }
            }
            // configuration read finished
        }

        private void CreateDefaultConfigFile()
        {
        }


        private void StartWork(object sender, DoWorkEventArgs e)
        {
            // read settings
            ReadConfigFile(configFileName);

            // setup channel system and get initial data
            rssList.GetInitialChannels(this);

            // perform main endless loop
            int retrieveDivider = 0;
            while (!CancellationPending)
            {
                // either we read a channel or we display an item
                if ((retrieveDivider % retrieveIntervall) == 0)
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
                Thread.Sleep(baseRecurrence);

                // update scheduler index
                retrieveDivider++;
                if (retrieveDivider == retrieveIntervall)
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
