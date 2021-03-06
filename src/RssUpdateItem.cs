/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2008  Roman Morawek <roman.morawek@embyt.com>

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
using BalloonRss.Properties;


namespace BalloonRss
{
    public class RssUpdateItem : RssItem
    {
        private int newVersion;
        public int NewVersion
        {
            get
            {
                return newVersion;
            }
            set
            {
                newVersion = value;
                title = Resources.str_updateTitle + (value/100) + "." + (value%100);
            }
        }

        private int currentVersion;
        public int CurrentVersion
        {
            get
            {
                return currentVersion;
            }
            set
            {
                currentVersion = value;
                link = Settings.Default.updateInfoUrl + value;
            }
        }


        public RssUpdateItem()
        {
            title = Resources.str_updateTitle;
            description = Resources.str_updateBody;
            author = "Roman Morawek";
            link = Settings.Default.updateInfoUrl;        
        }

    }
}
