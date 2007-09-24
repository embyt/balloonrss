using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


namespace BalloonRss
{
    public class RssItem
    {
        public string title = null;
        public string author = null;
        public string description = null;
        public string link = null;
        public DateTime pubDate = DateTime.MinValue;
        public DateTime creationDate = DateTime.MinValue;


        public RssItem(XmlNode xmlNode)
        {
            this.creationDate = DateTime.Now;

            // parse it
            foreach (XmlNode curXmlNode in xmlNode.ChildNodes)
            {
                string curTag = curXmlNode.Name.Trim().ToLower();

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
                        this.pubDate = DateTime.Parse(curXmlNode.InnerText);
                        break;

                    default:
                        // skip all other tags
                        break;
                }
            }
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
