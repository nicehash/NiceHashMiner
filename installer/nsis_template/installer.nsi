Var newStartMenuLink
Var oldStartMenuLink
Var newDesktopLink
Var oldDesktopLink
Var oldShortcutName
Var oldMenuDirectory

# INCLUDE DIRS
!addincludedir "include_3rdparty"
!addincludedir "include_common"
!addincludedir "include_installer"

!include "packageDefs.nsh" ; added definitions
!include "flagMacros.nsh" ; added flag macros
!include "common.nsh"
!include "MUI2.nsh"
!include "userDataCheck.nsh"
!include "allowOnlyOneInstallerInstance.nsh"

!insertmacro addVersionInfo

# we always want to execute user mode, we don't need admin/UAC privs 
RequestExecutionLevel user

Var appExe
Var launchLink

!include "assistedInstaller.nsh"
;--------------------------------
;Languages
  !include "langs.nsh"
;--------------------------------

Function .onInit
  !insertmacro check64BitAndSetRegView

  ${IfNot} ${UAC_IsInnerInstance}
    !insertmacro ALLOW_ONLY_ONE_INSTALLER_INSTANCE
  ${EndIf}

  !insertmacro initUserDataCheck
FunctionEnd

!include "installUtil.nsh"
Section "install"
  !include "installSection.nsh"
SectionEnd
