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
    public class RssItem
    {
        public String title = null;
        public String description = null;
        public String author = null;
        public String link = null;
        public DateTime pubDate = DateTime.MinValue;

        public DateTime creationDate = DateTime.MinValue;
        public DateTime dispDate = DateTime.MinValue;
        public String channel;


        public RssItem(XmlNode xmlNode, String channel)
        {
            this.creationDate = DateTime.Now;
            this.channel = channel;

            // parse it
            foreach (XmlNode curXmlNode in xmlNode.ChildNodes)
            {
                String curTag = curXmlNode.Name.Trim().ToLower();

                switch (curTag)
                {
                    // mandatory attributes
                    case "title":
                        this.title = curXmlNode.InnerText;
                        break;
                    case "link":
                        this.link = curXmlNode.InnerText;
                        break;
                    case "description":
                        this.description = curXmlNode.InnerText;
                        break;

                    // optional attributes
                    case "author":
                        this.author = curXmlNode.InnerText;
                        break;
                    case "pubdate":
                        this.pubDate = ParseDate(curXmlNode.InnerText);
                        break;

                    default:
                        // skip all other tags
                        break;
                }
            }

            // the title and description fields are mandatory
            if ((title == null) || (description == null))
                throw new FormatException("Could not find title and/or description field in RSS item");
        }


        private DateTime ParseDate(String text)
        {
            DateTime dateTime = DateTime.MinValue;

            try
            {
                dateTime = DateTime.Parse(text);
            }
            catch (FormatException)
            {
                // we might get an parse exception in case of US non-standard time, like "Thu, 05 May 2005 14:50:52 EDT"
                // truncate until last blank then and retry
                text = text.Substring(0, text.LastIndexOf(" "));
                try
                {
                    dateTime = DateTime.Parse(text);
                }
                catch (FormatException)
                {
                }
            }

            return dateTime;
        }


        public DateTime GetDate()
        {
            if (pubDate != DateTime.MinValue)
                return pubDate;
            else
                return creationDate;
        }
    }
}
