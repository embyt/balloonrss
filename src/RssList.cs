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
using System.Xml;


namespace BalloonRss
{
    class RssList : System.Collections.Generic.Dictionary<String, RssChannel>
    {
        private int totalPriority;
        private int rssCount;

        public int RssCount
        {
            get { return rssCount; }
        }

        public void ReadConfigFile(XmlNode channelsNode)
        {
            // parse configuration file
            totalPriority = 0;
            rssCount = 0;

            foreach (XmlNode xmlNode in channelsNode)
            {
                // search for "item" tag
                if (xmlNode.Name.Trim().ToLower() == "item")
                {
                    // create new channel with this information
                    RssChannel newChannel = new RssChannel(xmlNode);
                    this.Add(newChannel.link, newChannel);
                }
            }
            // configuration read finished
        }


        public void UpdateChannel(String url, XmlNode xmlNode)
        {
            RssChannel rssChannel;

            if (this.TryGetValue(url, out rssChannel) == true)
            {
                int oldItemCount = rssChannel.Count;

                // we already know this channel
                rssChannel.UpdateChannel(xmlNode);

                // update score and count
                rssCount += rssChannel.Count - oldItemCount;
                if ( (oldItemCount == 0) && (rssChannel.Count > 0) )
                {
                    totalPriority += rssChannel.priority;
                }
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
            rssCount--;

            // update total priority if channel is empty now
            if (rssChannel.IsItemAvailable() == false)
            {
                totalPriority -= rssChannel.priority;
            }

            return rssItem;
        }
    }
}
