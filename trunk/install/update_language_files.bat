@ECHO OFF

rem This batch file includes new keys in the application language files
rem Author: Roman Morawek
rem resxsync is a utility to synchronise resx files, you may obtain it here: http://www.screwturn.eu/ResxSync.ashx

echo Culture:de
resxsync ..\src\Properties\Resources.resx ..\src\Resources\strings.de.resx /v /l

echo.
echo Culture:pt
resxsync ..\src\Properties\Resources.resx ..\src\Resources\strings.pt.resx /v /l

pause