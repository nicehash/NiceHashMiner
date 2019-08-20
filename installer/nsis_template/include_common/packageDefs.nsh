# include generated file from 'tools/InstallerHelper/bin/Release/_files_to_pack/packageDefsGenerated.nsh' script
!include "packageDefsGenerated.nsh" ; added definitions

!define APP_FILENAME "${BASE_NAME}"
!define PRODUCT_NAME "${BASE_NAME}"
# restore to "${BASE_NAME}" after old platform is dead
!define PRODUCT_FILENAME "NiceHashMiner"

!define REQUEST_EXECUTION_LEVEL "user"
!define COMPRESS "auto"

!define APP_64_PACKAGE_NAME "nhm.7z"
!define APP_64_PACKAGE_PATH "dist\${APP_64_PACKAGE_NAME}"
!define APP_64 "dist\${APP_64_PACKAGE_NAME}"

!define SHORTCUT_NAME "${BASE_NAME}"
!define DELETE_APP_DATA_ON_UNINSTALL
!define UNINSTALL_DISPLAY_NAME "${BASE_BRAND_NAME} ${VERSION}"
!define UNINSTALLER_OUT_FILE "dist\__uninstaller-nsis-nhm.exe"

; MUI Icons and bitmaps
!define MUI_WELCOMEFINISHPAGE_BITMAP "assets\installerSidebar.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "assets\uninstallerSidebar.bmp"
!define MUI_ICON "assets\installerIcon.ico"
!define MUI_UNICON "assets\installerIcon.ico"

; installer settings unicode
Unicode true
Icon "assets\installerIcon.ico"

; ; uncomment/comment to enable/disable this deature
; !define allowToChangeInstallationDirectory