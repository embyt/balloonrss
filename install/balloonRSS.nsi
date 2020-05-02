;
;BalloonRSS - Simple RSS news aggregator using balloon tooltips
;    Copyright (C) 2008  Roman Morawek <roman.morawek@embyt.com>
;
;    This program is free software: you can redistribute it and/or modify
;    it under the terms of the GNU General Public License as published by
;    the Free Software Foundation, either version 3 of the License, or
;    (at your option) any later version.
;
;    This program is distributed in the hope that it will be useful,
;    but WITHOUT ANY WARRANTY; without even the implied warranty of
;    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;    GNU General Public License for more details.
;
;    You should have received a copy of the GNU General Public License
;    along with this program.  If not, see <http://www.gnu.org/licenses/>.
;

;--------------------------------
;Definitions
!define APPL_NAME "BalloonRSS"
!define APPL_VERSION "2.9"  ; keep this consistent with AssemblyInfo.cs!
!define PRODUCT_PUBLISHER "Roman Morawek"
!define PRODUCT_PUBLISHER_WEB_SITE "https://www.embyt.com"
!define PRODUCT_WEB_SITE "https://embyt.github.io/balloonrss"
!define PRODUCT_DOWNLOAD_SITE "https://github.com/embyt/balloonrss/releases"

!define BASEDIR ".."


;--------------------------------
;Includes
!include dotnet.nsh
!include "MUI2.nsh"


;--------------------------------
;User defined macros
!macro CheckAppRunning APP_PROCESS_NAME
  StrCpy $0 "${APP_PROCESS_NAME}"
  DetailPrint "Searching for processes called '$0'"
  KillProc::FindProcesses
  StrCmp $1 "-1" wooops
  Goto AppRunning
 
  AppRunning:
    DetailPrint "-> Found $0 processes running"
    StrCmp $0 "0" AppNotRunning
    MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "${APP_PROCESS_NAME} is currently running. $\n$\nDo you want to close it and continue installation?" IDYES KillApp
    DetailPrint "Installation Aborted!"
    Abort
  KillApp:
    StrCpy $0 "${APP_PROCESS_NAME}"
    DetailPrint "Killing all processes called '$0'"
    KillProc::KillProcesses
    StrCmp $1 "-1" wooops
    DetailPrint "-> Killed $0 processes, failed to kill $1 processes"
    sleep 1500
    Goto AppNotRunning
  wooops:
    DetailPrint "-> Error: Something went wrong :-("
    Abort
  AppNotRunning:
    DetailPrint "No Application Instances Found"
!macroend


;--------------------------------
;General

;Name and file
Name "${APPL_NAME} ${APPL_VERSION}"
OutFile "${APPL_NAME}_${APPL_VERSION}_setup.exe"

;Default installation folder
InstallDir "$PROGRAMFILES\${APPL_NAME}"

;Get installation folder from registry if available
InstallDirRegKey HKLM "Software\${APPL_NAME}" "Install_Dir"

;Start Menu Folder Page Configuration
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "${APPL_NAME}"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\${APPL_NAME}" 
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start_Menu_Folder"

;Set installer icon
!define MUI_ICON yellow32.ico
!define MUI_UNICON yellow32.ico

;--------------------------------
;Variables

Var StartMenuFolder

;--------------------------------
;Pages

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${BASEDIR}\license.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\BalloonRss.exe"
!define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\README.txt"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_COMPONENTS
!insertmacro MUI_UNPAGE_INSTFILES


;--------------------------------
;Languages
 
!insertmacro MUI_LANGUAGE "English"


;--------------------------------
;Installer Sections

Section "General" SecGeneral

  SectionIn RO

  ; Kill application if it already runs
  !insertmacro CheckAppRunning "BalloonRss.exe"
  
  ; Set output path to the installation directory.
  SetOutPath "$INSTDIR"
  
  File "${BASEDIR}\bin\BalloonRss.exe"
  File "${BASEDIR}\README.txt"
  File "${BASEDIR}\help\balloonrss.chm"
  
  ;Create shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
    CreateShortCut "$SMPROGRAMS\$StartMenuFolder\${APPL_NAME}.lnk" "$INSTDIR\BalloonRss.exe"
  !insertmacro MUI_STARTMENU_WRITE_END

  ;Store installation folder
  WriteRegStr HKLM "Software\${APPL_NAME}" "Install_Dir" $INSTDIR
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "DisplayName" "${APPL_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "NoRepair" 1
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "Publisher" "${PRODUCT_PUBLISHER}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "URLInfoAbout" "${PRODUCT_PUBLISHER_WEB_SITE}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "DisplayVersion" "${APPL_VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "HelpLink" "${PRODUCT_WEB_SITE}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}" "URLUpdateInfo" "${PRODUCT_DOWNLOAD_SITE}"
  ;also available: ReadMe, Comments

  ; Create uninstaller
  WriteUninstaller "uninstall.exe"

SectionEnd


;--------------------------------
; Optional section (can be disabled by the user)

Section "Language Files" SecLanguage
  File /r /x .svn "${BASEDIR}\bin\de"
  File /r /x .svn "${BASEDIR}\bin\pt"
SectionEnd

Section "Source Code" SecSource
  File "${BASEDIR}\license.txt"
  File "${BASEDIR}\balloonRss.sln"
  File /r /x obj /x .svn /x BalloonRss.csproj.user "${BASEDIR}\src"
SectionEnd

Section "Autostart Program" SecAutostart
  CreateShortCut "$SMSTARTUP\${APPL_NAME}.lnk" "$INSTDIR\BalloonRss.exe" "" "$INSTDIR\BalloonRss.exe" 0
SectionEnd


;--------------------------------
;Uninstaller Sections

Section "un.General" SecUnGeneral

  ; Kill application if it already runs
  !insertmacro CheckAppRunning "BalloonRss.exe"
  
  ; Remove files and uninstaller
  Delete /REBOOTOK "$INSTDIR\BalloonRss.exe"
  RMDir /r "$INSTDIR"

  ; Remove shortcuts, if any
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
  Delete "$SMPROGRAMS\$StartMenuFolder\${APPL_NAME}.lnk"
  RMDir "$SMPROGRAMS\$StartMenuFolder"  ; this will just remove the dir if it is emty
  Delete "$SMSTARTUP\${APPL_NAME}.lnk"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPL_NAME}"
  DeleteRegKey HKLM "SOFTWARE\${APPL_NAME}"
  DeleteRegKey HKCU "Software\${APPL_NAME}"

SectionEnd


Section "un.Data" SecUnData
  RMDir /r "$APPDATA\${APPL_NAME}"
SectionEnd


;--------------------------------
;Descriptions

; Language strings
LangString DESC_SecGeneral ${LANG_ENGLISH} "Installs the required application files."
LangString DESC_SecDotNet ${LANG_ENGLISH} "Installs the Microsoft .NET framework, which is required for this application. This will be skipped if the .NET framework is already present."
LangString DESC_SecLanguage ${LANG_ENGLISH} "Installs language files for German and Portuguese."
LangString DESC_SecSource ${LANG_ENGLISH} "Installs the source code."
LangString DESC_SecAutostart ${LANG_ENGLISH} "Launch ${APPL_NAME} on start-up."

LangString DESC_SecUnGeneral ${LANG_ENGLISH} "Uninstalls the application."
LangString DESC_SecUnData ${LANG_ENGLISH} "Removes application specific settings and data."


;Assign language strings to sections
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGeneral} $(DESC_SecGeneral)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDotNet} $(DESC_SecDotNet)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecLanguage} $(DESC_SecLanguage)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecSource} $(DESC_SecSource)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecAutostart} $(DESC_SecAutostart)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

!insertmacro MUI_UNFUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecUnGeneral} $(DESC_SecUnGeneral)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecUnData} $(DESC_SecUnData)
!insertmacro MUI_UNFUNCTION_DESCRIPTION_END
