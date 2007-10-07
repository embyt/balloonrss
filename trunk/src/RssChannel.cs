using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;


namespace BalloonRss
{
    class RssChannel : List<RssItem>
    {
        public string title = null;
        public string link = null;
        public string description = null;
        public byte priority = 0;

        private const string rssFeedDirName = "rssFeeds";


        public RssChannel(XmlNode configFile)
        {
            foreach (XmlNode xmlChild in configFile)
            {
                string curTag = xmlChild.Name.Trim().ToLower();

                switch (curTag)
                {
                    case "link":
                        link = xmlChild.InnerText;
                        break;
                    case "priority":
                        priority = Convert.ToByte(xmlChild.InnerText);
                        break;
                    default:
                        // skip this unknown tag
                        break;
                }
            }
        }


        public void UpdateChannel(XmlNode xmlNode)
        {
            foreach (XmlNode xmlChild in xmlNode)
            {
                string curTag = xmlChild.Name.Trim().ToLower();

                switch (curTag)
                {
                    // umbrella tags that needs to be resolved into deeper level
                    case "rss":
                    case "channel":
                    case "rdf:rdf":
                        UpdateChannel(xmlChild);
                        break;

                    // the actual channel information
                    case "title":
                        title = xmlChild.InnerText;
                        break;
                    case "link":
                        // we must not update the link - it's given by the config file
                        // link = xmlChild.InnerText;
                        break;
                    case "description":
                        description = xmlChild.InnerText;
                        break;

                    // the news items within this channel
                    case "item":
                        RssItem rssItem = new RssItem(xmlChild);

                        // check whether we really want to add this item
                        if (IsItemNew(rssItem))
                            this.Add(rssItem);

                        break;

                    default:
                        // skip this unknown tag
                        break;
                }
            }
        }


        public bool IsItemAvailable()
        {
            if (this.Count > 0)
                return true;
            else
                return false;
        }


        public RssItem GetNextItem()
        {
            RssItem rssItem = this[0];

            // get newest item in this channel
            foreach(RssItem curItem in this)
            {
                if (curItem.GetDate() > rssItem.GetDate())
                {
                    rssItem = curItem;
                }
            }

            // remove it
            RemoveItem(rssItem);

            return rssItem;
        }


        private void RemoveItem(RssItem rssItem)
        {
            // remove the item from this list
            this.Remove(rssItem);

            // write this information in the channel file
            XmlDocument channelFile = new XmlDocument();
            XmlNode xmlRoot;

            // open file and find root node
            try
            {
                channelFile.Load(GetRssFeedFilename(link));
                xmlRoot = channelFile.GetElementsByTagName("ChannelData")[0];
                if (xmlRoot == null)
                    throw new Exception("Xml Root Element not found.");
            }
            catch (Exception)
            {
                channelFile = new XmlDocument();
                xmlRoot = channelFile.CreateElement("ChannelData");
                channelFile.AppendChild(xmlRoot);
            }

            // add the current item
            XmlElement xmlRssItem = channelFile.CreateElement("RssItem");
            xmlRssItem.InnerText = rssItem.link;
            xmlRssItem.SetAttribute("dispDate", DateTime.Now.ToString());
            xmlRoot.AppendChild(xmlRssItem);

            try
            {
                channelFile.Save(GetRssFeedFilename(link));
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(rssFeedDirName);
                channelFile.Save(GetRssFeedFilename(link));
            }
        }


        private string GetRssFeedFilename(string url)
        {
            return rssFeedDirName + "\\" + MakeSafeFilename(url);
        }


        private string MakeSafeFilename(string url)
        {
            string safe = url.ToLower();

            foreach (char lDisallowed in System.IO.Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(lDisallowed.ToString(), "~");
            }
            foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars())
            {
                safe = safe.Replace(lDisallowed.ToString(), "~");
            }
            safe += ".xml";

            return safe;
        }


        private bool IsItemNew(RssItem item)
        {
            // write this information in the channel file
            XmlDocument channelFile = new XmlDocument();

            // open file
            try
            {
                channelFile.Load(GetRssFeedFilename(link));
            }
            catch (Exception)
            {
                return true;
            }

            // get the items
            foreach (XmlNode curNode in channelFile.GetElementsByTagName("RssItem"))
            {
                if (item.link == curNode.InnerText)
                    return false;   // we found the item
            }

            return true;
        }
    }
}
