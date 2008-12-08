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
using System.Text;
using System.Xml;
using BalloonRss.Properties;


namespace BalloonRss
{
    public class ChannelInfo
    {
        private const String xmlLinkName = "link";
        private const String xmlPriorityName = "priority";

        public String link;
        public byte priority;
        public bool globalChannel = false;


        // the empty constructor is used as a new channel is entered
        public ChannelInfo()
        {
            link = Resources.str_channelSettingsDefault1Link;
            priority = Settings.Default.defaultChannelPriority;
        }


        // this constructor is used as the default channels are initialized
        public ChannelInfo(string link, string priority)
        {
            // check and save link
            this.link = link;
            if (!IsValidLink(link))
            {
                throw new FormatException();
            }

            // check and save priority
            try
            {
                this.priority = Convert.ToByte(priority);
            }
            catch (Exception)
            {
                // if the priority is illegal, use the default without comment
                this.priority = Settings.Default.defaultChannelPriority;
            }
        }


        // the clone constructor is used for temporary editing channels
        public ChannelInfo(ChannelInfo templateChannel)
        {
            link = templateChannel.link;
            priority = templateChannel.priority;
            globalChannel = templateChannel.globalChannel;
        }


        // this constructor is used by reading the config file
        public ChannelInfo(XmlNode itemNode)
        {
            // default settings
            link = null;
            priority = Settings.Default.defaultChannelPriority;

            // the xml node we get is already a channel node
            foreach (XmlNode xmlChild in itemNode)
            {
                String curTag = xmlChild.Name.Trim().ToLower();

                // what xml tag did we find?
                switch (curTag)
                {
                    // the mandatory link tag
                    case xmlLinkName:
                        link = xmlChild.InnerText;
                        if (!IsValidLink(link))
                            throw new FormatException();
                        break;

                    // the optional priority tag
                    case xmlPriorityName:
                        // if this raises an exception, that's fine
                        priority = Convert.ToByte(xmlChild.InnerText);
                        break;

                    // skip all unknown tags without comment
                    default:
                        break;
                }
            }

            // the link field is mandatory
            if (link == null)
                throw new FormatException(Resources.str_balloonErrorChannelLinkTag);
        }


        // this is used as the channel settings are written to the xml file
        public void DumpChannelInfo(XmlDocument channelFile, XmlElement xmlChannelInfo)
        {
            // save link tag
            XmlElement xmlLink = channelFile.CreateElement(xmlLinkName);
            xmlLink.InnerText = this.link;
            xmlChannelInfo.AppendChild(xmlLink);
            // save priority tag
            XmlElement xmlPriority = channelFile.CreateElement(xmlPriorityName);
            xmlPriority.InnerText = this.priority.ToString();
            xmlChannelInfo.AppendChild(xmlPriority);
        }

    
        public static bool IsValidLink(String link)
        {
            if (Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute)
                && (link.Trim().Length > 0))
                return true;
            else
                return false;
        }
    }
}
