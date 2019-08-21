Var newStartMenuLink
Var oldStartMenuLink
Var newDesktopLink
Var oldDesktopLink
Var oldShortcutName
Var oldMenuDirectory

# INCLUDE DIRS
!addincludedir "include_3rdparty"
!addincludedir "include_common"
!addincludedir "include_uninstaller"

; This will make the uninstaller
!define BUILD_UNINSTALLER
!include "packageDefs.nsh" ; added definitions
!include "flagMacros.nsh" ; added flag macros
!include "common.nsh"
!include "MUI2.nsh"
!include "userDataCheck.nsh"
!include "allowOnlyOneInstallerInstance.nsh"

!insertmacro addVersionInfo

# we always want to execute user mode, we don't need admin/UAC privs 
RequestExecutionLevel user

SilentInstall silent

!include "assistedInstaller.nsh"
;--------------------------------
;Languages
  !include "langs.nsh"
;--------------------------------

Function .onInit
  WriteUninstaller "${UNINSTALLER_OUT_FILE}"
  !insertmacro quitSuccess
FunctionEnd

; keep this 
Section "install"
SectionEnd

!include "uninstaller.nsh"