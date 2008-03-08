@ECHO OFF

rem This batch file creates the localization dlls to support different application languages
rem Author: Roman Morawek

echo Preparing resource files
resgen ..\src\Resources\strings.de.resx BalloonRss.Properties.Resources.de.resources
resgen ..\src\Resources\strings.pt.resx BalloonRss.Properties.Resources.pt.resources
echo.

echo Creating dll for culture de
al /t:lib /embed:BalloonRss.Properties.Resources.de.resources /culture:de /out:BalloonRss.resources.dll
del BalloonRss.Properties.Resources.de.resources
move BalloonRss.resources.dll ..\bin\de

echo Creating dll for culture pt
al /t:lib /embed:BalloonRss.Properties.Resources.pt.resources /culture:pt /out:BalloonRss.resources.dll
del BalloonRss.Properties.Resources.pt.resources
move BalloonRss.resources.dll ..\bin\pt

pause