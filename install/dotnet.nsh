;
;BalloonRSS - Simple RSS news aggregator using balloon tooltips
;    Copyright (C) 2007  Roman Morawek <roman.morawek@embyt.com>
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
  ; URL to the .NET Framework 2.0 download
  ; No different locales needed for it is multilingual
  !define URL_DOTNET "http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe"


;--------------------------------
; Declaring variables
  Var "DOTNET_RETURN_CODE"


;--------------------------------
; Section for .NET Framework
; This downloads and installs Miscrosoft .NET Framework 2.0
; if not found on the system

  Section $(SEC_DOTNET) SecDotNet
    Call IsDotNETInstalled
    Pop $R3
    ; IsDotNETInstalled returns 1 for yes and 0 for no
    StrCmp $R3 "1" lbl_isinstalled lbl_notinstalled

    lbl_notinstalled:

    SectionIn RO
    ; the following Goto and Label is for consistencey.
    Goto lbl_DownloadRequired

    lbl_DownloadRequired:
      MessageBox MB_ICONEXCLAMATION|MB_YESNO|MB_DEFBUTTON2 "$(DESC_DOTNET_DECISION)" /SD IDNO \
        IDYES +2 IDNO lbl_Done
      DetailPrint "$(DESC_DOWNLOADING1) $(DESC_SHORTDOTNET)..."
      ; "Downloading Microsoft .Net Framework"
      ;AddSize 286720	;already included above
      nsisdl::download /TRANSLATE "$(DESC_DOWNLOADING)" "$(DESC_CONNECTING)" \
         "$(DESC_SECOND)" "$(DESC_MINUTE)" "$(DESC_HOUR)" "$(DESC_PLURAL)" \
         "$(DESC_PROGRESS)" "$(DESC_REMAINING)" \
         /TIMEOUT=30000 "${URL_DOTNET}" "$PLUGINSDIR\dotnetfx.exe"
      Pop $0
      StrCmp "$0" "success" lbl_continue
      DetailPrint "$(DESC_DOWNLOADFAILED) $0"
      Abort

    ; start installation of .NET framework
    lbl_continue:
      DetailPrint "$(DESC_INSTALLING) $(DESC_SHORTDOTNET)..."
      Banner::show /NOUNLOAD "$(DESC_INSTALLING) $(DESC_SHORTDOTNET)..."
      nsExec::ExecToStack '"$PLUGINSDIR\dotnetfx.exe" /q /c:"install.exe /noaspupgrade /q"'
      pop $DOTNET_RETURN_CODE
      Banner::destroy
      SetRebootFlag false
      ; silence the compiler
      Goto lbl_NoDownloadRequired

    lbl_NoDownloadRequired:

      ; obtain any error code and inform the user ($DOTNET_RETURN_CODE)
      ; If nsExec is unable to execute the process,
      ; it will return "error"
      ; If the process timed out it will return "timeout"
      ; else it will return the return code from the executed process.
      StrCmp "$DOTNET_RETURN_CODE" "" lbl_NoError
      StrCmp "$DOTNET_RETURN_CODE" "0" lbl_NoError
      StrCmp "$DOTNET_RETURN_CODE" "3010" lbl_NoError
      StrCmp "$DOTNET_RETURN_CODE" "8192" lbl_NoError
      StrCmp "$DOTNET_RETURN_CODE" "error" lbl_Error
      StrCmp "$DOTNET_RETURN_CODE" "timeout" lbl_TimeOut
      ; It's a .Net Error
      StrCmp "$DOTNET_RETURN_CODE" "4101" lbl_Error_DuplicateInstance
      StrCmp "$DOTNET_RETURN_CODE" "4097" lbl_Error_NotAdministrator
      StrCmp "$DOTNET_RETURN_CODE" "1633" lbl_Error_InvalidPlatform lbl_FatalError
      ; all others are fatal

    lbl_Error_DuplicateInstance:
      DetailPrint "$(ERROR_DUPLICATE_INSTANCE)"
      GoTo lbl_Done

    lbl_Error_NotAdministrator:
      DetailPrint "$(ERROR_NOT_ADMINISTRATOR)"
      GoTo lbl_Done

    lbl_Error_InvalidPlatform:
      DetailPrint "$(ERROR_INVALID_PLATFORM)"
      GoTo lbl_Done

    lbl_TimeOut:
      DetailPrint "$(DESC_DOTNET_TIMEOUT)"
      GoTo lbl_Done

    lbl_Error:
      DetailPrint "$(ERROR_DOTNET_INVALID_PATH)"
      GoTo lbl_Done

    lbl_FatalError:
      DetailPrint "$(ERROR_DOTNET_FATAL)[$DOTNET_RETURN_CODE]"
      GoTo lbl_Done

    lbl_Done:
      DetailPrint "$(DESC_LONGDOTNET) $(NOT_INSTALLED)"
      MessageBox MB_ICONEXCLAMATION|MB_YESNO|MB_DEFBUTTON2 "$(FAILED_DOTNET_INSTALL)" /SD IDNO \
      IDYES +2 IDNO 0
      DetailPrint "${APPL_NAME} $(NOT_INSTALLED)"
      Abort

    lbl_NoError:

    lbl_isinstalled:
  SectionEnd


;--------------------------------
;Descriptions

  ; Language strings


  ; strings needed for .NET install
  LangString DESC_REMAINING ${LANG_ENGLISH} " (%d %s%s remaining)"
  LangString DESC_PROGRESS ${LANG_ENGLISH} "%dkB of %dkB @ %d.%01dkB/s"
  LangString DESC_PLURAL ${LANG_ENGLISH} "s"
  LangString DESC_HOUR ${LANG_ENGLISH} "hour"
  LangString DESC_MINUTE ${LANG_ENGLISH} "minute"
  LangString DESC_SECOND ${LANG_ENGLISH} "second"
  LangString DESC_CONNECTING ${LANG_ENGLISH} "Connecting..."
  LangString DESC_DOWNLOADING ${LANG_ENGLISH} "Downloading %s"
  LangString DESC_LONGDOTNET ${LANG_ENGLISH} "Microsoft .Net Framework 2.0"
  LangString DESC_SHORTDOTNET ${LANG_ENGLISH} "Microsoft .Net Framework 2.0"
  LangString DESC_DOTNET_DECISION ${LANG_ENGLISH} "$(DESC_SHORTDOTNET) was not \
    found on your system. \
    $\nIt is strongly advised that you install $(DESC_SHORTDOTNET) before continuing. \
    $\nIf you choose to continue, you will need to connect to the Internet \
    $\nbefore proceeding. \
    $\n$\nShould $(DESC_SHORTDOTNET) be installed now?"
  LangString SEC_DOTNET ${LANG_ENGLISH} "$(DESC_SHORTDOTNET)"
  LangString DESC_INSTALLING ${LANG_ENGLISH} "Installing"
  LangString DESC_DOWNLOADING1 ${LANG_ENGLISH} "Downloading"
  LangString DESC_DOWNLOADFAILED ${LANG_ENGLISH} "Download Failed:"
  LangString ERROR_DUPLICATE_INSTANCE ${LANG_ENGLISH} "The $(DESC_SHORTDOTNET) Installer is already running."
  LangString ERROR_NOT_ADMINISTRATOR ${LANG_ENGLISH} "You are not administrator."
  LangString ERROR_INVALID_PLATFORM ${LANG_ENGLISH} "OS not supported."
  LangString DESC_DOTNET_TIMEOUT ${LANG_ENGLISH} "The installation of the $(DESC_SHORTDOTNET) has timed out."
  LangString ERROR_DOTNET_INVALID_PATH ${LANG_ENGLISH} "The $(DESC_SHORTDOTNET) Installation $\n was not found in the following location:$\n"
  LangString ERROR_DOTNET_FATAL ${LANG_ENGLISH} "A fatal error occurred during the installation $\nof $(DESC_SHORTDOTNET)."
  LangString FAILED_DOTNET_INSTALL ${LANG_ENGLISH} "$(DESC_LONGDOTNET) was not installed. \
    $\n${APPL_NAME} may not function properly until $(DESC_LONGDOTNET) is installed. \
    $\n$\nDo you still want to install ${APPL_NAME}?"
  LangString NOT_INSTALLED ${LANG_ENGLISH} "was not installed."


;--------------------------------
; diverse functions

;--------------------------------
; Check for .NET Framework install

  Function IsDotNETInstalled
    Push $0
    Push $1
    Push $2
    Push $3
    Push $4

    ReadRegStr $4 HKEY_LOCAL_MACHINE \
      "Software\Microsoft\.NETFramework" "InstallRoot"
    # remove trailing back slash
    Push $4
    Exch $EXEDIR
    Exch $EXEDIR
    Pop $4
    # if the root directory doesn't exist .NET is not installed
    IfFileExists $4 0 noDotNET

    StrCpy $0 0

    EnumStart:
      EnumRegKey $2 HKEY_LOCAL_MACHINE \
        "Software\Microsoft\.NETFramework\Policy"  $0
      IntOp $0 $0 + 1
      StrCmp $2 "" noDotNET

      StrCpy $1 0

    EnumPolicy:
      EnumRegValue $3 HKEY_LOCAL_MACHINE \
        "Software\Microsoft\.NETFramework\Policy\$2" $1
      IntOp $1 $1 + 1
      StrCmp $3 "" EnumStart
      IfFileExists "$4\$2.$3" foundDotNET EnumPolicy

    noDotNET:
      StrCpy $0 0
      Goto done

    foundDotNET:
      StrCpy $0 1

    done:
      Pop $4
      Pop $3
      Pop $2
      Pop $1
      Exch $0
  FunctionEnd
