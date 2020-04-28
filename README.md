# BalloonRSS #

## Overview ##

BalloonRSS is a simple RSS aggregator that displays incoming messages as balloon tooltips in the windows task bar. The news entries themselves are linked and read with the browser.

## Features ##

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
 - Automatically check for program updates (if enabled)
 - Multi-language support: English, German, Portuguese
 - ...

## Supported systems ##

BalloonRSS is developed to be used on any Win32 system, where the .NET framework 
is available, which includes:
- Windows 7
- Windows 8
- Windows 10

## Installation ##

 - Start BalloonRSS_xxx_setup.exe and follow the steps on the screen
 - Start the application. Notice that the only entry point to the application is the icon in the notification area (usually on the bottom right).
 - Right click on the icon in the task bar and adapt the channel configuration with the RSS feeds of your choice.
 - You may want to configure that the icon in the notification area is always displayed

The base message recurrence is set to 5 minutes per default, so you have to wait a bit until your first messages will appear.

## Usage ##

As you start the application you get a notify icon in the windows task bar (at the right botton, near the clock). All commands are accessed by the context menu (right click on the icon). Make sure that the icon is always shown and not hidden by configuring this in windows.

A single left click enters pause mode. Double clicking the icon may also have functionality if this is configured in the application settings.

Color codes of the icon:
 - blue: application paused
 - yellow: messages pending
 - orange: no messages pending

You can obtain further information from the application's help file or on the 
website:
http://balloonrss.sourceforge.net
