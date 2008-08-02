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
using BalloonRss.Properties;


namespace BalloonRss
{
    class ChannelList : List<ChannelInfo>
    {
        private const String xmlRootNodeName = "channels";
        private const String xmlItemName = "item";


        // the constructor loads the channel config file
        public ChannelList(out bool fallbackToDefaultChannels)
        {
            fallbackToDefaultChannels = false;

            // we try to read the config file; if it does not exist we use default settings
            try
            {
                ParseChannelConfigFile();
            }
            catch (DirectoryNotFoundException)
            {
                // create settings directory
                Directory.CreateDirectory(Path.GetDirectoryName(GetChannelConfigFilename()));

                // we have to use the default channels
                LoadDefaultChannelSettings();
                fallbackToDefaultChannels = true;
            }
            catch (Exception)
            {
                // this may be a FileNotFoundException or another exception in case of an illegal config file
                // we have to load default channels
                LoadDefaultChannelSettings();
                fallbackToDefaultChannels = true;
            }
        }


        private static String GetChannelConfigFilename()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                + Path.DirectorySeparatorChar + "BalloonRSS"
                + Path.DirectorySeparatorChar + Settings.Default.configFilename;
        }


        private void LoadDefaultChannelSettings()
        {
            // there might be channels left from a config file read attempt; clear them
            this.Clear();

            // create up to 4 default channels
            try
            {
                ChannelInfo newChannel = new ChannelInfo(
                    Properties.Resources.str_channelSettingsDefault1Link,
                    Properties.Resources.str_channelSettingsDefault1Priority);
                this.Add(newChannel);
            }
            catch (Exception) { };

            try
            {
                ChannelInfo newChannel = new ChannelInfo(
                    Properties.Resources.str_channelSettingsDefault2Link,
                    Properties.Resources.str_channelSettingsDefault2Priority);
                this.Add(newChannel);
            }
            catch (Exception) { };

            try
            {
                ChannelInfo newChannel = new ChannelInfo(
                    Properties.Resources.str_channelSettingsDefault3Link,
                    Properties.Resources.str_channelSettingsDefault3Priority);
                this.Add(newChannel);
            }
            catch (Exception) { };

            try
            {
                ChannelInfo newChannel = new ChannelInfo(
                    Properties.Resources.str_channelSettingsDefault4Link,
                    Properties.Resources.str_channelSettingsDefault4Priority);
                this.Add(newChannel);
            }
            catch (Exception) { };

            // as the last step, dump the settings file
            SaveToFile();
        }


        // this parses and loads the channel config file
        // in case of any error an exception shall be thrown
        private void ParseChannelConfigFile()
        {
            // open xml configuration file
            XmlDocument configFile = new XmlDocument();
            configFile.Load(GetChannelConfigFilename());

            // check root tag
            if (configFile.DocumentElement.Name != xmlRootNodeName)
                throw new FormatException(Resources.str_balloonErrorConfigChannelTag);

            // loop over all channel entries
            XmlNodeList itemList = configFile.GetElementsByTagName(xmlItemName);
            foreach (XmlNode xmlNode in itemList)
            {
                // create new channel with this information
                ChannelInfo newChannel = new ChannelInfo(xmlNode);

                // search for duplicate node
                foreach (ChannelInfo curChannel in this)
                {
                    if (curChannel.link == newChannel.link)
                        throw new FormatException("duplicate link found");
                }

                this.Add(newChannel);
            }
            // configuration read finished
        }


        // dump the current channel list to the xml file
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

            // save the file
            channelFile.Save(GetChannelConfigFilename());
        }
    }
}