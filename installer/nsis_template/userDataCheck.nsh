!include FileFunc.nsh
!include UAC.nsh

!define FOLDERID_UserProgramFiles {5CD7AEE2-2219-4A67-B85D-6C9CE15660CB}
!define KF_FLAG_CREATE 0x00008000

!define INSTALL_REGISTRY_KEY "Software\${APP_GUID}"
!define UNINSTALL_REGISTRY_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_GUID}"

Var showCleanUninstallInstallPage
!macro setShowCleanUninstallInstallPage
  StrCpy $showCleanUninstallInstallPage "0"
  !ifndef BUILD_UNINSTALLER
    # check if we have general_settings with installer
    IfFileExists "$LOCALAPPDATA\Programs\${BASE_NAME}\configs" 0 +2
    StrCpy $showCleanUninstallInstallPage "1"
  !else
    # check if we have settings folder with uninstaller
    IfFileExists "$LOCALAPPDATA\Programs\${BASE_NAME}\*" 0 +2
    StrCpy $showCleanUninstallInstallPage "1"
  !endif
!macroend


## TODO cleanup user vs all install
# current Install Mode ("all" or "CurrentUser")
Var installMode

Var perUserInstallationFolder

!macro setInstallModePerUser
  StrCpy $installMode CurrentUser
  SetShellVarContext current

  # сhecks registry for previous installation path
  ReadRegStr $perUserInstallationFolder HKCU "${INSTALL_REGISTRY_KEY}" InstallLocation
  ${if} $perUserInstallationFolder != ""
    StrCpy $INSTDIR $perUserInstallationFolder
  ${else}
    StrCpy $0 "$LOCALAPPDATA\Programs"
    # Win7 has a per-user programfiles known folder and this can be a non-default location
    System::Call 'Shell32::SHGetKnownFolderPath(g "${FOLDERID_UserProgramFiles}",i ${KF_FLAG_CREATE},i0,*i.r2)i.r1'
    ${If} $1 == 0
      System::Call '*$2(&w${NSIS_MAX_STRLEN} .r1)'
      StrCpy $0 $1
      System::Call 'Ole32::CoTaskMemFree(ir2)'
    ${endif}
    StrCpy $INSTDIR "$0\${APP_FILENAME}"
  ${endif}
!macroend