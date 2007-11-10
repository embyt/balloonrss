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
    class ChannelList : System.Collections.Generic.List<ChannelInfo>
    {
        private const String xmlRootNodeName = "channels";
        private const String xmlItemName = "item";


        public ChannelList(String channelConfigFilename)
        {
            ParseChannelConfigFile(channelConfigFilename);
        }


        private void ParseChannelConfigFile(String configFilename)
        {
            bool gotChannelTag = false;

            // open xml configuration file
            XmlDocument configFile = new XmlDocument();

            configFile.Load(configFilename);

            // parse configuration file
            foreach (XmlNode rootNode in configFile)
            {
                // search for "channels" tag
                if (rootNode.Name.Trim().ToLower() == xmlRootNodeName)
                {
                    gotChannelTag = true;
                    ParseRootNode(rootNode);
                }
            }

            if (!gotChannelTag)
            {
                throw new FormatException(resources.str_balloonErrorConfigChannelTag);
            }
        }


        private void ParseRootNode(XmlNode channelsNode)
        {
            foreach (XmlNode xmlNode in channelsNode)
            {
                // search for "item" tag
                if (xmlNode.Name.Trim().ToLower() == xmlItemName)
                {
                    // create new channel with this information
                    ChannelInfo newChannel = new ChannelInfo(xmlNode);
                    this.Add(newChannel);
                }
            }
            // configuration read finished
        }


        public void SaveToFile(String channelConfigFilename)
        {
            // write this information in the channel file
            XmlDocument channelFile = new XmlDocument();
            XmlNode xmlRoot;

            // create root node
            xmlRoot = channelFile.CreateElement(xmlRootNodeName);
            channelFile.AppendChild(xmlRoot);

            // add content
            foreach (ChannelInfo curChannelInfo in this)
            {
                XmlElement xmlChannelInfo = channelFile.CreateElement(xmlItemName);
                curChannelInfo.DumpChannelInfo(channelFile, xmlChannelInfo);
                xmlRoot.AppendChild(xmlChannelInfo);
            }

            channelFile.Save(channelConfigFilename);
        }
    }
}
