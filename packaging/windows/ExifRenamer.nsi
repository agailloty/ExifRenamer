Unicode True

!include "MUI2.nsh"

!ifndef APP_VERSION
  !define APP_VERSION "0.0.0"
!endif
!ifndef PUBLISH_DIR
  !error "PUBLISH_DIR must point to the dotnet publish directory"
!endif
!ifndef OUTPUT_FILE
  !define OUTPUT_FILE "ExifRenamer-Setup.exe"
!endif
!ifndef REPOSITORY_ROOT
  !define REPOSITORY_ROOT "..\.."
!endif

!define APP_NAME "ExifRenamer"
!define APP_PUBLISHER "7echnet"
!define APP_EXE "ExifRenamer.exe"
!define APP_REGISTRY_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\ExifRenamer"

Name "${APP_NAME} ${APP_VERSION}"
OutFile "${OUTPUT_FILE}"
InstallDir "$LOCALAPPDATA\Programs\${APP_NAME}"
InstallDirRegKey HKCU "${APP_REGISTRY_KEY}" "InstallLocation"
RequestExecutionLevel user
SetCompressor /SOLID lzma
BrandingText "${APP_NAME}"

VIProductVersion "${APP_VERSION}.0"
VIAddVersionKey /LANG=1033 "ProductName" "${APP_NAME}"
VIAddVersionKey /LANG=1033 "CompanyName" "${APP_PUBLISHER}"
VIAddVersionKey /LANG=1033 "FileDescription" "${APP_NAME} installer"
VIAddVersionKey /LANG=1033 "FileVersion" "${APP_VERSION}"

!define MUI_ABORTWARNING
!define MUI_ICON "${REPOSITORY_ROOT}\ExifRenamer\Assets\ExifRenamer.ico"
!define MUI_UNICON "${REPOSITORY_ROOT}\ExifRenamer\Assets\ExifRenamer.ico"
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${REPOSITORY_ROOT}\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"

Section "ExifRenamer" SEC_MAIN
  SetOutPath "$INSTDIR"
  File /r "${PUBLISH_DIR}\*"

  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateDirectory "$SMPROGRAMS\${APP_NAME}"
  CreateShortcut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}"
  CreateShortcut "$SMPROGRAMS\${APP_NAME}\Uninstall ${APP_NAME}.lnk" "$INSTDIR\Uninstall.exe"

  WriteRegStr HKCU "${APP_REGISTRY_KEY}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKCU "${APP_REGISTRY_KEY}" "DisplayVersion" "${APP_VERSION}"
  WriteRegStr HKCU "${APP_REGISTRY_KEY}" "Publisher" "${APP_PUBLISHER}"
  WriteRegStr HKCU "${APP_REGISTRY_KEY}" "DisplayIcon" "$INSTDIR\${APP_EXE}"
  WriteRegStr HKCU "${APP_REGISTRY_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "${APP_REGISTRY_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegDWORD HKCU "${APP_REGISTRY_KEY}" "NoModify" 1
  WriteRegDWORD HKCU "${APP_REGISTRY_KEY}" "NoRepair" 1
SectionEnd

Section "Uninstall"
  Delete "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk"
  Delete "$SMPROGRAMS\${APP_NAME}\Uninstall ${APP_NAME}.lnk"
  RMDir "$SMPROGRAMS\${APP_NAME}"
  DeleteRegKey HKCU "${APP_REGISTRY_KEY}"
  RMDir /r "$INSTDIR"
SectionEnd
