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
        public RssChannel channel;


        protected RssItem()
        {
            pubDate = DateTime.MinValue;
            creationDate = DateTime.Now;
            channel = null;
            dispDate = DateTime.MinValue;
        }

        public RssItem(XmlNode xmlNode, RssChannel channel)
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
                        this.title = StripHtml(curXmlNode.InnerText);
                        break;
                    case "link":
                        if (channel.channelType != "feed")
                        {
                            this.link = curXmlNode.InnerText;
                        }
                        else
                        {
                            this.link = curXmlNode.Attributes.GetNamedItem("href").Value;
                        }
                        break;
                    case "description": // for rss and rdf feeds
                    case "summary":     // for atom feeds
                        this.description = StripHtml(curXmlNode.InnerText);
                        break;

                    // optional attributes
                    case "author":
                        this.author = curXmlNode.InnerText;
                        break;
                    case "pubdate":     // for rss and rdf feeds
                    case "updated":     // for atom feeds
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

            // nevertheless, some feeds use empty fields which is not acceptable
            if (title == "") 
                title = " ";
            if (description == "")
                description = " ";
        }


        private String StripHtml(String text)
        {
            String escapedText = null;
            // strip all html tags like <b> and translate special chars like &amp;
            /* this would strip everything but it yields an exception in case of 
             * an already escaped string like "harry & sally" */
            try
            {
                XmlNode xmlNode = new XmlDocument().CreateElement("stripEntity");
                xmlNode.InnerXml = text;
                escapedText = xmlNode.InnerText;
            }
            catch (Exception)
            {
                // it seems that the text was already escaped
                // should we still remove the tags like <b>?
                // we could do it by the regexp "<^[>]+>"

                // fixme: we just go with the text and assume it is already proper shaped
                escapedText = text;
            }

            return escapedText;
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
