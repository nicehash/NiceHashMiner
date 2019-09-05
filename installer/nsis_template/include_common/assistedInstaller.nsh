!include UAC.nsh

!ifndef UPDATER
  !include "userDataCheckGUI.nsh"
!endif

!ifndef BUILD_UNINSTALLER
  !ifndef UPDATER
    Function StartApp
      ${if} ${isUpdated}
        ${StdUtils.ExecShellAsUser} $0 "$launchLink" "open" "--updated"
      ${else}
        ${StdUtils.ExecShellAsUser} $0 "$launchLink" "open" ""
      ${endif}
    FunctionEnd
    !define MUI_FINISHPAGE_TEXT_LARGE
    !define MUI_FINISHPAGE_TEXT "${APP_FILENAME} has been installed on your computer.$\r$\nClick Finish to close Setup.$\r$\nWARNING: Miner software is recognized as malicious by Anti-Virus software, use it at your own risk.$\r$\nWhite-list '$INSTDIR' to ensure binaries don't get deleted."
    !define MUI_FINISHPAGE_RUN
    !define MUI_FINISHPAGE_RUN_FUNCTION "StartApp"
    !insertmacro MUI_PAGE_LICENSE "assets\license.rtf"
    !insertmacro PAGE_INSTALL_MODE
  !endif
  
  !ifdef allowToChangeInstallationDirectory
    !include StrContains.nsh

    !insertmacro MUI_PAGE_DIRECTORY

    # pageDirectory leave doesn't work (it seems because $INSTDIR is set after custom leave function)
    # so, we yse instfiles pre
    !define MUI_PAGE_CUSTOMFUNCTION_PRE instFilesPre

    # Sanitize the MUI_PAGE_DIRECTORY result to make sure it has a application name sub-folder
    Function instFilesPre
      ${If} ${FileExists} "$INSTDIR\*"
        ${StrContains} $0 "${APP_FILENAME}" $INSTDIR
        ${If} $0 == ""
          StrCpy $INSTDIR "$INSTDIR\${APP_FILENAME}"
        ${endIf}
      ${endIf}
    FunctionEnd
  !endif

  !insertmacro MUI_PAGE_INSTFILES
  !ifndef UPDATER
    !insertmacro MUI_PAGE_FINISH
  !endif
    
  !else
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro PAGE_INSTALL_MODE
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH
!endif

!macro initUserDataCheck
  !insertmacro setShowCleanUninstallInstallPage  
  # always user mode install
  !insertmacro setInstallModePerUser
!macroend
