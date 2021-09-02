Name "Example"
OutFile "Example.exe"

RequestExecutionLevel User

Page Custom PagePre PageLeave
Page InstFiles

ShowInstDetails Show
ShowUnInstDetails Show

Function .OnInit
  InitPluginsDir
  File /oname=$PLUGINSDIR\page.ini "Page.ini"
FunctionEnd

Function PagePre
  InstallOptions::dialog /NOUNLOAD "$PLUGINSDIR\page.ini"
FunctionEnd

Function PageLeave
  ReadINIStr $R0 "$PLUGINSDIR\page.ini" "Field 2" "State"
  StrCmp $R0 1 +1 +3
  StrCpy $R0 "/TL"
  GoTo next
  ReadINIStr $R0 "$PLUGINSDIR\page.ini" "Field 3" "State"
  StrCmp $R0 1 +1 +3
  StrCpy $R0 "/TR"
  GoTo next
  ReadINIStr $R0 "$PLUGINSDIR\page.ini" "Field 4" "State"
  StrCmp $R0 1 +1 +3
  StrCpy $R0 "/BL"
  GoTo next
  ReadINIStr $R0 "$PLUGINSDIR\page.ini" "Field 5" "State"
  StrCmp $R0 1 +1 +3
  StrCpy $R0 "/BR"
  GoTo next
  ReadINIStr $R0 "$PLUGINSDIR\page.ini" "Field 6" "State"
  StrCmp $R0 1 +1 +2
  StrCpy $R0 "/CENTER"

next:

  ReadINIStr $R1 "$PLUGINSDIR\page.ini" "Field 7" "State"

  ; Modern tag.
  ReadINIStr $R2 "$PLUGINSDIR\page.ini" "Field 9" "State"
  StrCmp $R2 0 +2
  StrCpy $R2 "/MODERN"

FunctionEnd

Section
  SpiderBanner::Show $R0 $R1 $R2
  DetailPrint "Waiting..."
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  DetailPrint "Hello World!"
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  DetailPrint "This is a demonstration."
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  DetailPrint "This is one stupidly, crazy, over the top, once in a blue moon, insanely long detail text that should never have been written into this installer."
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  SpiderBanner::Destroy
  SpiderBanner::ShowPBOnly
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  Sleep 100
  DetailPrint "Now we are finished."
  Sleep 100
  SetAutoClose False
SectionEnd