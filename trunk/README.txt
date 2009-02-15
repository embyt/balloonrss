                                  BalloonRSS
                               ----------------

                      Copyright (C) 2009  Roman Morawek
                         romor@users.sourceforge.net
                      http://balloonrss.sourceforge.net


Overview
--------

BalloonRSS is a simple RSS aggregator that displays incoming messages as 
balloon tooltips in the windows task bar. The news entries themselves are 
linked and read with the browser.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the license, or
(at your option) any later version.


Features
--------

- Retrieves RSS messages from various sources
- Performs priorisation of news
- Displays the messages in a balloon tooltip pop-up in the task bar
- The messages are linked and are opened within the browser if you click them
- Adaptation of channel priority according the user's click rate
- The message frequency and viewing period is configurable
- Configuration via GUI
- Message history
- Support of HTTP authentication
- Support for multi-user environments
- Multi-language support: English, German, Portuguese
...


Supported systems
-----------------

BalloonRSS is developed to be used on any Win32 system, where the .NET framework 
2.0 is available, which includes:
- Windows 98
- Windows 2000
- Windows ME
- Windows Server 2003
- Windows Vista
- Windows XP


Installation
------------

- Make sure that you have the .NET framework 2.0 installed (it is included with 
  Vista); otherwise download the Microsoft .NET Framework Version 2.0
  Redistributable Package from Microsoft and install it.
- Start BalloonRSS_xxx_setup.exe and follow the steps on the screen
- Start the application.
- Right click on the icon in the task bar and adapt the channel configuration 
  with the RSS feeds of your choice.

The base message recurrence is set to 5 minutes per default, so you have to wait
a bit until your first messages will appear.


Usage
-----

As you start the application you get a notify icon in the windows task bar (at 
the right botton, near the clock). All commands are accessed by the context
menu (right click on the icon). A single left click enters pause mode.

Color codes of the icon:
- blue: application paused
- yellow: messages pending
- orange: no messages pending

You can obtain further information on the website:
http://balloonrss.sourceforge.net