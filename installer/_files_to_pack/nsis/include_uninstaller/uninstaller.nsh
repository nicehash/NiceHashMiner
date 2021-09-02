Function un.onInit
  !insertmacro check64BitAndSetRegView
  !insertmacro initUserDataCheck
FunctionEnd

Section "un.install"
  # for assisted installer we check it here to show progress
  !insertmacro CHECK_APP_RUNNING

  !insertmacro setLinkVars

  ${ifNot} ${isKeepShortcuts}
    WinShell::UninstAppUserModelId "${APP_ID}"

    !ifndef DO_NOT_CREATE_DESKTOP_SHORTCUT
      WinShell::UninstShortcut "$oldDesktopLink"
      Delete "$oldDesktopLink"
    !endif

    WinShell::UninstShortcut "$oldStartMenuLink"

    ReadRegStr $R1 SHELL_CONTEXT "${INSTALL_REGISTRY_KEY}" MenuDirectory
    ${if} $R1 == ""
      Delete "$oldStartMenuLink"
    ${else}
      RMDir /r "$SMPROGRAMS\$R1"
    ${endIf}
  ${endIf}

  # refresh the desktop
  System::Call 'shell32::SHChangeNotify(i, i, i, i) v (0x08000000, 0, 0, 0)'

  ClearErrors

  DeleteRegKey SHCTX "${UNINSTALL_REGISTRY_KEY}"
  DeleteRegKey SHCTX "${INSTALL_REGISTRY_KEY}"
SectionEnd
