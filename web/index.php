<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
 <title>BalloonRSS - A simple RSS aggregator using balloon tooltips</title>
 <meta http-equiv="content-type" content="text/html; charset=ISO-8859-1">
 <meta http-equiv="content-language" content="en">
 <meta name="author" content="Roman Morawek">
 <meta name="description" content="BalloonRSS is a simple RSS aggregator that displays incoming messages as balloon tooltips in the windows task bar. The news entries themselves are linked and read with the browser.">
 <meta name="keywords" content="RSS, aggregator, news reader, RSS reader, balloon tooltip, pop-up, balloon, browser, Win32, WinXP, GPL, .net, priority">
 <meta name="date" content="2009-01-05T11:06:13+01:00">
 <link rel=stylesheet type="text/css" href="/default.css">
 <link rel="shortcut icon" href="favicon.ico" type="image/vnd.microsoft.icon">
</head>

<body>

<div class="Titel"><p>BalloonRSS - A Simple RSS Aggregator using Balloon Tooltips</p></div>

<p align="center">Copyright (C) 2009, Roman Morawek<br>
romor@users.sourceforge.net<br>
<a href="http://balloonrss.sourceforge.net">http://balloonrss.sourceforge.net</a><br>
<a href="http://sourceforge.net/projects/balloonrss">http://sourceforge.net/projects/balloonrss</a><br>
License: <a href="http://www.gnu.org/licenses/gpl.html">GNU General Public License, Version 3</a></p>


<p>&nbsp;</p>
<div class="Beschriftung">
<p><img src="screenshots/balloonRss_1.png" width="469" height="173" border=0 alt="Screenshot of BalloonRSS"></p>
Screenshot of BalloonRSS, showing an RSS item as a balloon pop-up.
</div>
<p>&nbsp;</p>

<p>
<a href="#sec_overview">Overview</a><br>
<a href="#sec_features">Features</a><br>
<a href="#sec_systems">Supported Systems</a><br>
<a href="#sec_install">Download &amp; Installation</a><br>
<a href="#sec_usage">Usage</a><br>
<a href="#sec_screenshots">Screenshots</a><br>
<a href="#sec_support">Support &amp; Feedback</a><br>
</p>

<h1><a name="sec_overview">Overview</a></h1>

<p>BalloonRSS is a simple RSS reader that displays incoming messages as balloon 
tooltips in the windows task bar. The news entries themselves are linked to be 
read with the browser.</p>

<p>The messages are retrieved from a set of configurable RSS feed sites. The news entry to view next is selected by a pre-defined priority which is dynamically adapted according the user's interest, which is determined by the user's message click rate.</p>

<p>This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License (GPL) as published by
the Free Software Foundation; either version 3 of the license, or
(at your option) any later version.</p>


<h1><a name="sec_features">Features</a></h1>
<ul>
<li>retrieves RSS messages from various sources
<li>performs priorisation of news
<li>displays the messages in a balloon tooltip pop-up in the task bar
<li>the messages are linked and opened within the browser as you click them
<li>adaptation of channel priority according the user's click rate
<li>the message frequency and viewing period is configurable
<li>configuration via GUI or using .xml files
<li>message history
<li>multi-language support: English, German, Portuguese
<li>...
</ul>


<h1><a name="sec_systems">Supported Systems</a></h1>

<p>BalloonRSS is developed to be used on any Win32 system, where the .NET framework 2.0 is available, which includes: </p>
<ul>
<li>Windows 2000
<li>Windows 98
<li>Windows ME
<li>Windows Server 2003
<li>Windows Vista
<li>Windows XP
</ul>

<p>It is tested on WinXP and Vista.</p>


<h1><a name="sec_install">Download &amp; Installation</a></h1>

<p>The source code as well as the released packages can be downloaded from the <a href="https://sourceforge.net/project/showfiles.php?group_id=206266">sourceforge project download page</a>.</p>

<p>Installation procedure, using the installer:</p>
<ol>
<li>Make sure that you have the .NET framework 2.0 installed (it is already included in Vista); otherwise download the <a href="http://msdn2.microsoft.com/en-us/netframework/aa731542.aspx">Microsoft .NET Framework Version 2.0 Redistributable Package</a> from Microsoft and install it.
<li>Start BalloonRSS_xxx_setup.exe and follow the steps on the screen.
<li>Start the application and adapt the channel configuration with the RSS feeds of your choice.
</ol>

<p>The base recurrence is set to 90 seconds per default, so you have to wait a bit until your messages will appear.</p>


<h1><a name="sec_usage">Usage</a></h1>

<p>As you start the application you get a notify icon in the windows task bar (at the right botton, near the clock). All commands are accessed by the context
menu (right click on the icon).</p>

<p>The first thing you might want to do is add your favourite RSS channels to be displayed. For this you need to select <i>RSS Channel Settings</i> and add them there. If you read multiple channels you may want to assign different priorities. Messages from higher priority RSS channels are more likely to be displayed. The effective priority is calculated by the pre-selected priority of the channel settings and the user's click rate. Thus, if you click more messages from one channel, this increases its effective priority and display probability.</p>

<p>A single left click on the application icon in the task bar enters pause mode. This is indicated with the color of the icon. No messages will be displayed in this status.</p>

<p>Color codes of the icon:</p>
<ul style="list-style-type:none">
<li><img src="icon/blue16.png" width="16" height="16" alt="blue icon"> blue: application paused
<li><img src="icon/yellow16.png" width="16" height="16" alt="yellow icon"> yellow: messages pending
<li><img src="icon/orange16.png" width="16" height="16" alt="orange icon"> orange: no messages pending
</ul>


<h2>Advanced Usage</h2>

<h3>Channel Settings in Multi-User Environment</h3>
<p>BalloonRSS uses 2 channel configuration files:</p>
<ol>
<li>A user specific channel configuration (located in the user settings directory, e.g. C:\Users\xxx\AppData\Roaming\BalloonRSS)
<li>A computer specific channel configuration (located in the program directory, e.g. C:\Program Files\BalloonRSS).
</ol>

<p>Using the configuration dialogs in the GUI, you always modify user's channel entries. The global channel settings can only be changed by directly editing the configuration .xml file.</p>

<p>The easiest way to configure the global channel settings is to configure the desired global channels as a user and then move the channelConfig.xml file from the user settings directory to the program directory. Afterwards you may add additional user channels.</p>

<h3>Application Settings in Multi-User Environment</h3>
<p>You may pre-define application settings in a multi-user environment by putting a configuration file <i>BalloonRss.exe.config</i> in the program directory. You need to manually edit this XML file. You start by obtaining the default settings file from the <a href="http://balloonrss.svn.sourceforge.net/">SVN repository</a> at <a href="http://balloonrss.svn.sourceforge.net/viewvc/balloonrss/trunk/bin/BalloonRss.exe.config">trunk/bin/BalloonRss.exe.config</a> and modify it according your needs.</p>

<p>Once a user modifys its settings using the application configuration dialog, BalloonRss will create a user specific setting file in the user directory. This file then overrides the application settings.</p>

<h3>Alternative Manual Installation</h3>
<p>If you do not want to use the installer to setup BalloonRSS, you can just obtain the appliction .exe file from the SVN repository and copy it to your desired directory.</p>

<p>The steps in detail:</p>
<ol>
<li>Make sure that you have the .NET framework 2.0 installed (it is already included in Vista); otherwise download the <a href="http://msdn2.microsoft.com/en-us/netframework/aa731542.aspx">Microsoft .NET Framework Version 2.0 Redistributable Package</a> from Microsoft and install it.
<li>Copy the executable "balloonrss.exe" (available using the <a href="http://balloonrss.svn.sourceforge.net/">subversion access</a>) in any directory of your choice.
<li>Start the application and adapt the channel configuration with the RSS feeds of your choice.
<li>You might want to link it in the autostart folder.
</ol>



<h1><a name="sec_screenshots">Screenshots</a></h1>

<p><img src="screenshots/balloonRss_1.png" width="469" height="173" border=0 alt="Screenshot of BalloonRSS"><br>
Screenshot: BalloonRSS, showing an RSS item as a balloon pop-up.</p>

<p><img src="screenshots/balloonRss_2.png" width="327" height="249" border=0 alt="Screenshot of BalloonRSS"><br>
Screenshot: Main menu of application.</p>

<p><img src="screenshots/balloonRss_3.png" width="596" height="125" border=0 alt="Screenshot of BalloonRSS"><br>
Screenshot: Information on all subscribed RSS channels.</p>

<p><img src="screenshots/balloonRss_4.png" width="458" height="214" border=0 alt="Screenshot of BalloonRSS"><br>
Screenshot: History of last shown RSS messages.</p>

<p><img src="screenshots/balloonRss_5.png" width="260" height="129" border=0 alt="Screenshot of BalloonRSS"><br>
Screenshot: Status display if you place the mouse cursor over the application item.</p>


<h1><a name="sec_support">Support &amp; Feedback</a></h1>
<p>Please use the <a href="http://sourceforge.net/tracker/?group_id=206266">tracker system</a> for bug, support or feature requests.</p>

<p>For general discussions, you might want to use the <a href="http://sourceforge.net/forum/?group_id=206266">project forums</a>.</p>

<p>You also might want to look into the <a href="releasenotes.html">release notes</a>.</p>

<hr>
<table width="100%"><tr>
<td><i>last update: <?php echo date ("r", filemtime($_SERVER["SCRIPT_FILENAME"]))?><br>
Roman Morawek</i></td>
<td align="right"><table align="right"><tr><td><a href="http://sourceforge.net"><img src="http://sflogo.sourceforge.net/sflogo.php?group_id=206266&amp;type=2" width="125" height="37" border="0" alt="SourceForge.net Logo"></a></td><td><a href="http://jigsaw.w3.org/css-validator/check/referer"><img border=0 width=88 height=31 SRC="http://jigsaw.w3.org/css-validator/images/vcss.gif" ALT="Valid CSS!"></a></td>
<td><a href="http://validator.w3.org/check/referer"><img border=0 src="http://validator.w3.org/images/vh40" alt="Valid HTML 4.0!" height=31 width=88></a></td></tr></table></td>
</tr></table>
</body>
</html>
