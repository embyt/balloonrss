using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


namespace BalloonRss
{
    class RssList : System.Collections.Generic.Dictionary<String, RssChannel>
    {
        private int totalPriority;


        public void ReadConfigFile(XmlNode channelsNode)
        {
            // parse configuration file
            totalPriority = 0;

            foreach (XmlNode xmlNode in channelsNode)
            {
                // search for "item" tag
                if (xmlNode.Name.Trim().ToLower() == "item")
                {
                    // create new channel with this information
                    RssChannel newChannel = new RssChannel(xmlNode);
                    this.Add(newChannel.link, newChannel);
                    totalPriority += newChannel.priority;
                }
            }
            // configuration read finished
        }

            
        public void GetInitialChannels(Retriever retriever)
        {
            // now get the initial news from all channels
            foreach (KeyValuePair<string,RssChannel> keyValuePair in this)
            {
                retriever.GetChannel(keyValuePair.Key);
            }
        }

        public void GetNextChannel(Retriever retriever)
        {
        }

        
        public void UpdateChannel(String url, XmlNode xmlNode)
        {
            RssChannel rssChannel;

            if (this.TryGetValue(url, out rssChannel) == true)
            {
                // we already know this channel
                rssChannel.UpdateChannel(xmlNode);
            }
            else
            {
                // this must not happen
                throw new Exception("Could not find channel for " + url);
            }
        }


        public RssItem GetNextItem()
        {
            // first, choose the channel for this news entry

            // for this I use a random number in the range of the total priority
            int randomNumber = new Random().Next(totalPriority);

            // now walk through the channel list until we found the number
            int actualNumber = 0;
            RssChannel rssChannel = null;
            foreach (KeyValuePair<string, RssChannel> keyValuePair in this)
            {
                RssChannel curChannel = keyValuePair.Value;
                if (curChannel.IsItemAvailable())
                {
                    actualNumber += curChannel.priority;
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

            // update total priority if channel is empty now
            if (rssChannel.IsItemAvailable() == false)
            {
                totalPriority -= rssChannel.priority;
            }

            return rssItem;
        }
    }
}
