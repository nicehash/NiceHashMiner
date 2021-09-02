!include nsDialogs.nsh

Var RadioButtonLabel1

!macro PAGE_INSTALL_MODE
  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MULTIUSER_${MUI_PAGE_UNINSTALLER_PREFIX}INSTALLMODEPAGE ""
  Var MultiUser.InstallModePage
  Var MultiUser.InstallModePage.Text
  Var MultiUser.InstallModePage.KeepNonGeneralSettings
  Var MultiUser.InstallModePage.ClearAllUserData
  Var MultiUser.InstallModePage.ReturnValue

  !ifndef BUILD_UNINSTALLER
    !insertmacro FUNCTION_INSTALL_MODE_PAGE_FUNCTION MultiUser.InstallModePre_${MUI_UNIQUEID} MultiUser.InstallModeLeave_${MUI_UNIQUEID} ""
    PageEx custom
      PageCallbacks MultiUser.InstallModePre_${MUI_UNIQUEID} MultiUser.InstallModeLeave_${MUI_UNIQUEID}
      Caption " "
    PageExEnd
  !else
    !insertmacro FUNCTION_INSTALL_MODE_PAGE_FUNCTION MultiUser.InstallModePre_${MUI_UNIQUEID} MultiUser.InstallModeLeave_${MUI_UNIQUEID} un.
    UninstPage custom un.multiUser.InstallModePre_${MUI_UNIQUEID} un.MultiUser.InstallModeLeave_${MUI_UNIQUEID}
  !endif
!macroend

!macro FUNCTION_INSTALL_MODE_PAGE_FUNCTION PRE LEAVE UNINSTALLER_FUNCPREFIX
	Function "${UNINSTALLER_FUNCPREFIX}${PRE}"
		!ifdef BUILD_UNINSTALLER
      !insertmacro MUI_HEADER_TEXT "$(chooseUninstallationOptions)" "$(performCleanUninstall)"
		!else
      !insertmacro MUI_HEADER_TEXT "$(chooseInstallationOptions)" "$(performUpgradeOrCleanInstall)"
		!endif

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
		# check if we are already installed and if we are show
		${if} $showCleanUninstallInstallPage == "1"	
			nsDialogs::Create 1018
			Pop $MultiUser.InstallModePage

			!ifndef BUILD_UNINSTALLER
				${NSD_CreateLabel} 0u 0u 300u 20u "$(cleanInstall)"
				StrCpy $8 "$(cleanInstallKeep)"
				StrCpy $9 "$(cleanInstallCleanup)"
			!else
				${NSD_CreateLabel} 0u 0u 300u 20u "$(cleanUninstall)"
				StrCpy $8 "$(cleanUninstallKeep)"
				StrCpy $9 "$(cleanUninstallCleanup)"
			!endif
			Pop $MultiUser.InstallModePage.Text

			${NSD_CreateRadioButton} 10u 30u 280u 20u "$8"
			Pop $MultiUser.InstallModePage.KeepNonGeneralSettings

			System::Call "advapi32::GetUserName(t.r0,*i${NSIS_MAX_STRLEN})i"
			${NSD_CreateRadioButton} 10u 50u 280u 20u "$9"# "$9 ($0)" TODO figure out this macro that has $0 as system user name
			Pop $MultiUser.InstallModePage.ClearAllUserData

			nsDialogs::SetUserData $MultiUser.InstallModePage.KeepNonGeneralSettings 1 
			nsDialogs::SetUserData $MultiUser.InstallModePage.ClearAllUserData 0	

			; bind to radiobutton change
			${NSD_OnClick} $MultiUser.InstallModePage.ClearAllUserData ${UNINSTALLER_FUNCPREFIX}InstModeChange
			${NSD_OnClick} $MultiUser.InstallModePage.KeepNonGeneralSettings ${UNINSTALLER_FUNCPREFIX}InstModeChange

			${NSD_CreateLabel} 0u 110u 280u 50u ""
			Pop $RadioButtonLabel1

			# If already installed set to keep value by default
			SendMessage $MultiUser.InstallModePage.KeepNonGeneralSettings ${BM_SETCHECK} ${BST_CHECKED} 0 ; set as default
			SendMessage $MultiUser.InstallModePage.KeepNonGeneralSettings ${BM_CLICK} 0 0 ; trigger click event
			!insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW
			nsDialogs::Show
		${else}
			# install 
			!insertmacro setInstallModePerUser
		${endif}
	FunctionEnd

	Function "${UNINSTALLER_FUNCPREFIX}${LEAVE}"
		SendMessage $MultiUser.InstallModePage.KeepNonGeneralSettings ${BM_GETCHECK} 0 0 $MultiUser.InstallModePage.ReturnValue

		${if} $MultiUser.InstallModePage.ReturnValue = ${BST_CHECKED}
			# delete data
			;!ifndef BUILD_UNINSTALLER
				!insertmacro deleteUserData_KeepNonGeneralSettings
			;!endif
			!insertmacro setInstallModePerUser
		${else}
			!insertmacro deleteUserData_All
			!insertmacro setInstallModePerUser
		${endif}

		!insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE
	FunctionEnd

	# TODO maybe fix up the detail message for each case  
	Function "${UNINSTALLER_FUNCPREFIX}InstModeChange"
		pop $1
		nsDialogs::GetUserData $1
		pop $1

		StrCpy $7 ""
		${if} "$1" == "0" ; KEEP DATA
			!ifndef BUILD_UNINSTALLER
				; StrCpy $7 "KEEP DATA INSTALL"
			!else
				; StrCpy $7 "KEEP DATA UNINSTALL"
			!endif
		${else} ; REMOVE ALL DATA
			!ifndef BUILD_UNINSTALLER
				; StrCpy $7 "REMOVE ALL INSTALL"
			!else
				; StrCpy $7 "REMOVE ALL UNINSTALL"
			!endif
		${endif}
		SendMessage $RadioButtonLabel1 ${WM_SETTEXT} 0 "STR:$7"
	FunctionEnd
!macroend
