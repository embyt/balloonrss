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
    class ChannelList : List<ChannelInfo>
    {
        public const String configFilename = "channelConfig.xml";
        public const String defaultConfigFilename = "defaultChannels.xml";

        private const String xmlRootNodeName = "channels";
        private const String xmlItemName = "item";


        public ChannelList(bool skipErrors)
        {
            if (!skipErrors)
            {
                // this is the normal usage:
                // we try to read the config file, if it does not exist, we copy the default and throw an exception
                try
                {
                    ParseChannelConfigFile();
                }
                catch (DirectoryNotFoundException e)
                {
                    // create settings directory
                    Directory.CreateDirectory(Path.GetDirectoryName(GetChannelConfigFilename()));

                    // copy default configuration
                    File.Copy(GetDefaultChannelsFilename(), GetChannelConfigFilename());

                    // we have to re-throw the exception to signal this event to the calling application
                    throw e;
                }
                catch (Exception e)
                {
                    // most probably this is a FileNotFoundException which means we have to copy the default config file
                    // but it might be another exception like an illegal config file; also copy the default file then

                    // copy default configuration
                    File.Copy(GetDefaultChannelsFilename(), GetChannelConfigFilename());

                    // we have to re-throw the exception to signal this event to the calling application
                    throw e;
                }
            }
            else
            {
                // if loading the channel config file results in an error, we continue with the best possible
                try
                {
                    ParseChannelConfigFile();
                }
                catch (Exception)
                {
                }
            }
        }


        private String GetChannelConfigFilename()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                + Path.DirectorySeparatorChar + "BalloonRSS"
                + Path.DirectorySeparatorChar + configFilename;
        }

        private String GetDefaultChannelsFilename()
        {
            return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)
                + Path.DirectorySeparatorChar + defaultConfigFilename;
        }

        private void ParseChannelConfigFile()
        {
            bool gotChannelTag = false;

            // open xml configuration file
            XmlDocument configFile = new XmlDocument();

            configFile.Load(GetChannelConfigFilename());

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
                throw new FormatException(Properties.Resources.str_balloonErrorConfigChannelTag);
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


        public void SaveToFile()
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

            channelFile.Save(GetChannelConfigFilename());
        }
    }
}
