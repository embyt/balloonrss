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
using System.Text;
using System.Xml;
using System.IO;


namespace BalloonRss
{
    public class RssChannel : List<RssItem>
    {
        private const String xmlRootNodeName = "ChannelData";
        private const String xmlItemName = "RssItem";

        public ChannelInfo channelInfo;
        public String title;
        public String description = null;
        public DateTime lastUpdate = DateTime.MinValue;
        public int channelMessageCount;
        public int channelViewedCount;
        public int channelOpenedCount;
        public int effectivePriority = 0;


        private static String rssFeedDirName = "" + Path.DirectorySeparatorChar + "BalloonRSS" + Path.DirectorySeparatorChar + "rssFeeds";
        private static String rssViewedFilename = "viewed_";
        private static String rssOpenedFilename = "opened_";


        public RssChannel(ChannelInfo channelInfo)
        {
            this.channelInfo = channelInfo;

            // as the default title, we take the link
            title = channelInfo.link;

            GetMessageCounters();
        }


        private void GetMessageCounters()
        {
            // right now there are no messages queued
            channelMessageCount = 0;

            // get the number of messages already viewed
            try
            {
                // load channel file
                XmlDocument channelFile = new XmlDocument();
                channelFile.Load(GetRssViewedFilename(channelInfo.link));

                // get the items and count them
                channelViewedCount = channelFile.GetElementsByTagName(xmlItemName).Count;
            }
            catch (Exception)
            {
                // if we cannot find a history file, treat the item as new
                channelViewedCount = 0;
            }

            // get the number of messages displayed in browser
            try
            {
                // load channel file
                XmlDocument channelFile = new XmlDocument();
                channelFile.Load(GetRssOpenedFilename(channelInfo.link));

                // get the items and count them
                channelOpenedCount = channelFile.GetElementsByTagName(xmlItemName).Count;
            }
            catch (Exception)
            {
                // if we cannot find a history file, treat the item as new
                channelOpenedCount = 0;
            }
        }


        public int UpdateChannel(XmlNode xmlNode)
        {
            int messageCount = 0;
            int newMessages = 0;

            foreach (XmlNode xmlChild in xmlNode)
            {
                String curTag = xmlChild.Name.Trim().ToLower();

                switch (curTag)
                {
                    // umbrella tags that needs to be resolved into deeper level
                    case "rss":
                    case "channel":
                    case "rdf:rdf":
                        newMessages = UpdateChannel(xmlChild);
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
                        RssItem rssItem;
                        try
                        {
                            rssItem = new RssItem(xmlChild, this);
                        }
                        catch (FormatException)
                        {
                            // should we report this error? no.
                            break;
                        }

                        // check whether we really want to add this item
                        if (IsItemNew(rssItem))
                        {
                            // add it
                            this.Add(rssItem);

                            // increment counter of new messages
                            newMessages++;
                        }
                        messageCount++; // we count also the known messages

                        break;

                    default:
                        // skip this unknown tag
                        break;
                }
            }

            // we have to check messageCount because this function is also used 
            // for the outer rss/channel/rdf:rdf tag...
            if (messageCount > 0)
            {
                this.channelMessageCount = messageCount;

                // update last update timestamp
                lastUpdate = DateTime.Now;
            }

            return newMessages;
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

            // set display timestamp
            rssItem.dispDate = DateTime.Now;

            return rssItem;
        }


        private void RemoveItem(RssItem rssItem)
        {
            // remove the item from this list
            this.Remove(rssItem);

            // update counter
            channelViewedCount++;

            // write this information in the channel file
            XmlDocument channelFile = new XmlDocument();
            XmlNode xmlRoot;

            // open file and find root node
            try
            {
                channelFile.Load(GetRssViewedFilename(channelInfo.link));
                xmlRoot = channelFile.DocumentElement;
                if (channelFile.DocumentElement.Name != xmlRootNodeName)
                    throw new Exception("Xml Root Element not found.");
            }
            catch (Exception)
            {
                channelFile = new XmlDocument();
                xmlRoot = channelFile.CreateElement(xmlRootNodeName);
                channelFile.AppendChild(xmlRoot);
            }

            // add the current item
            XmlElement xmlRssItem = channelFile.CreateElement(xmlItemName);
            xmlRssItem.InnerText = rssItem.link;
            xmlRssItem.SetAttribute("dispDate", DateTime.Now.ToString());
            xmlRoot.AppendChild(xmlRssItem);

            try
            {
                channelFile.Save(GetRssViewedFilename(channelInfo.link));
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(GetRssFeedFolder());
                channelFile.Save(GetRssViewedFilename(channelInfo.link));
            }
        }


        private static String GetRssFeedFolder()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + rssFeedDirName;
        }

        private static String GetRssViewedFilename(String url)
        {
            return GetRssFeedFolder() + Path.DirectorySeparatorChar + MakeSafeFilename(rssViewedFilename + url);
        }

        private static String GetRssOpenedFilename(String url)
        {
            return GetRssFeedFolder() + Path.DirectorySeparatorChar + MakeSafeFilename(rssOpenedFilename + url);
        }


        private static String MakeSafeFilename(String url)
        {
            String safe = url.ToLower();

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
            // check for plausible parameter
            if (item == null)
                return false;

            // first, check whether this item is already in the channel queue
            foreach (RssItem curItem in this)
            {
                if (item.link == curItem.link)
                    return false;
            }

            // second, check whether we already displayed this item

            // load channel file
            XmlDocument channelFile = new XmlDocument();

            // open file
            try
            {
                channelFile.Load(GetRssViewedFilename(channelInfo.link));
            }
            catch (Exception)
            {
                // if we cannot find a history file, treat the item as new
                return true;
            }

            // check root tag
            if (channelFile.DocumentElement.Name != xmlRootNodeName)
            {
                // if we face an illegal history file, treat the item as new
                return true;
            }

            // get the items
            XmlNodeList itemList = channelFile.GetElementsByTagName(xmlItemName);

            // search whether the item is already known
            foreach (XmlNode curNode in itemList)
            {
                if (item.link == curNode.InnerText)
                    return false;   // we found the item
            }

            return true;
        }


        public void ActivateItem(RssItem rssItem)
        {
            // store activation
            // write this information in the file
            XmlDocument channelFile = new XmlDocument();
            XmlNode xmlRoot;

            // open file and find root node
            try
            {
                channelFile.Load(GetRssOpenedFilename(rssItem.channel.channelInfo.link));
                xmlRoot = channelFile.DocumentElement;
                if (channelFile.DocumentElement.Name != xmlRootNodeName)
                    throw new Exception("Xml Opened Root Element not found.");
            }
            catch (Exception)
            {
                channelFile = new XmlDocument();
                xmlRoot = channelFile.CreateElement(xmlRootNodeName);
                channelFile.AppendChild(xmlRoot);
            }

            // add the current item
            XmlElement xmlRssItem = channelFile.CreateElement(xmlItemName);
            xmlRssItem.InnerText = rssItem.link;
            xmlRssItem.SetAttribute("openedDate", DateTime.Now.ToString());
            xmlRoot.AppendChild(xmlRssItem);

            channelFile.Save(GetRssOpenedFilename(rssItem.channel.channelInfo.link));

            // update counter
            channelOpenedCount++;

            // open browser
            System.Diagnostics.Process.Start(rssItem.link);
        }


        public void ClearChannelData()
        {
            try
            {
                File.Delete(GetRssViewedFilename(channelInfo.link));
                File.Delete(GetRssOpenedFilename(channelInfo.link));
            }
            catch (DirectoryNotFoundException)
            {
                // skip this exception, the file does not exist anyway
            }
        }


        public void MarkAllRead()
        {
            // write this information in the channel file
            XmlDocument channelFile = new XmlDocument();
            XmlNode xmlRoot;

            // open file and find root node
            try
            {
                channelFile.Load(GetRssViewedFilename(channelInfo.link));
                xmlRoot = channelFile.DocumentElement;
                if (channelFile.DocumentElement.Name != xmlRootNodeName)
                    throw new Exception("Xml Root Element not found.");
            }
            catch (Exception)
            {
                channelFile = new XmlDocument();
                xmlRoot = channelFile.CreateElement(xmlRootNodeName);
                channelFile.AppendChild(xmlRoot);
            }

            // loop over all items
            foreach (RssItem rssItem in this)
            {
                // add the current item
                XmlElement xmlRssItem = channelFile.CreateElement(xmlItemName);
                xmlRssItem.InnerText = rssItem.link;
                xmlRssItem.SetAttribute("dispDate", "never");
                xmlRoot.AppendChild(xmlRssItem);
            }

            try
            {
                channelFile.Save(GetRssViewedFilename(channelInfo.link));
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(GetRssFeedFolder());
                channelFile.Save(GetRssViewedFilename(channelInfo.link));
            }

            // remove all items from this list
            this.Clear();
        }
    }
}
