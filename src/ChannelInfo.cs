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


namespace BalloonRss
{
    public class ChannelInfo
    {
        private const String xmlLinkName = "link";
        private const String xmlPriorityName = "priority";

        public String link = null;
        public byte priority = 0;


        // the empty constructor is used as a new channel is entered
        public ChannelInfo()
        {
            link = Properties.Resources.str_channelSettingsDefaultLink;
            priority = 5;
        }


        // this constructor is used by reading the config file
        public ChannelInfo(XmlNode itemNode)
        {
            foreach (XmlNode xmlChild in itemNode)
            {
                String curTag = xmlChild.Name.Trim().ToLower();

                switch (curTag)
                {
                    case xmlLinkName:
                        link = xmlChild.InnerText;
                        break;
                    case xmlPriorityName:
                        priority = Convert.ToByte(xmlChild.InnerText);
                        break;
                    default:
                        // skip this unknown tag
                        break;
                }
            }

            // the link field is mandatory
            if (link == null)
                throw new FormatException(Properties.Resources.str_balloonErrorChannelLinkTag);
        }


        public void DumpChannelInfo(XmlDocument channelFile, XmlElement xmlChannelInfo)
        {
            XmlElement xmlLink = channelFile.CreateElement(xmlLinkName);
            xmlLink.InnerText = this.link;
            xmlChannelInfo.AppendChild(xmlLink);
            XmlElement xmlPriority = channelFile.CreateElement(xmlPriorityName);
            xmlPriority.InnerText = this.priority.ToString();
            xmlChannelInfo.AppendChild(xmlPriority);
        }
    }
}
