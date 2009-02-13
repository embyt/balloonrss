/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2009  Roman Morawek <romor@users.sourceforge.net>

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
        private const String xmlAuthUserName = "httpAuthUsername";
        private const String xmlAuthPwdName = "httpAuthPassword";
        private const String xmlMarkAsReadName = "markAsReadAtStartup";

        public String link;
        public byte priority;
        public bool globalChannel = false;
        public String httpAuthUsername = null;
        public String httpAuthPassword = null;
        public bool markAsReadAtStartup = false;


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
            httpAuthUsername = templateChannel.httpAuthUsername;
            httpAuthPassword = templateChannel.httpAuthPassword;
            markAsReadAtStartup = templateChannel.markAsReadAtStartup;
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
                String curTag = xmlChild.Name.Trim();

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

                    // the optional http auth user tag
                    case xmlAuthUserName:
                        httpAuthUsername = xmlChild.InnerText;
                        break;

                    // the optional http auth pwd tag
                    case xmlAuthPwdName:
                        httpAuthPassword = xmlChild.InnerText;
                        break;

                    case xmlMarkAsReadName:
                        // if this raises an exception, that's fine
                        markAsReadAtStartup = Convert.ToBoolean(xmlChild.InnerText);
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

            // save "mark as read" tag
            XmlElement xmlMarkAsRead = channelFile.CreateElement(xmlMarkAsReadName);
            xmlMarkAsRead.InnerText = this.markAsReadAtStartup.ToString();
            xmlChannelInfo.AppendChild(xmlMarkAsRead);

            // save http auth user tag
            if (httpAuthUsername != null)
            {
                XmlElement xmlAuthUser = channelFile.CreateElement(xmlAuthUserName);
                xmlAuthUser.InnerText = this.httpAuthUsername.ToString();
                xmlChannelInfo.AppendChild(xmlAuthUser);
            }
            // save http auth pwd tag
            if (httpAuthPassword != null)
            {
                XmlElement xmlAuthPwd = channelFile.CreateElement(xmlAuthPwdName);
                xmlAuthPwd.InnerText = this.httpAuthPassword.ToString();
                xmlChannelInfo.AppendChild(xmlAuthPwd);
            }
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
