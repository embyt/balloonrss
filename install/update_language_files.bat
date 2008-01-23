@ECHO OFF

rem This batch file includes new keys in the application language files
rem Author: Roman Morawek

echo Culture:de
resxsync ..\src\Properties\Resources.resx ..\src\Resources\strings.de.resx /v /s

echo.
echo Culture:pt
resxsync ..\src\Properties\Resources.resx ..\src\Resources\strings.pt.resx /v /s

pause