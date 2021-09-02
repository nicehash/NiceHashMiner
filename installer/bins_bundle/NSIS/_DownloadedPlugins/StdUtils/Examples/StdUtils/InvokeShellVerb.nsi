#################################################################################
# StdUtils plug-in for NSIS
# Copyright (C) 2004-2018 LoRd_MuldeR <MuldeR2@GMX.de>
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
#
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public
# License along with this library; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
#
# http://www.gnu.org/licenses/lgpl-2.1.txt
#################################################################################

Caption "StdUtils Test-Suite"

!addincludedir  "..\..\Include"

!ifdef NSIS_UNICODE
	!addplugindir "..\..\Plugins\Release_Unicode"
	OutFile "InvokeShellVerb-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "InvokeShellVerb-ANSI.exe"
!endif

!include 'StdUtils.nsh'

RequestExecutionLevel user ;no elevation needed for this test
ShowInstDetails show

Section
	IfFileExists "$SYSDIR\mspaint.exe" +3
	MessageBox MB_ICONSTOP 'File does not exist:$\n"$SYSDIR\mspaint.exe"$\n$\nExample cannot run!'
	Quit
	MessageBox MB_OK "Please make sure Paint isn't pinned to your Taskbar right now.$\nThen press 'OK' to begin test..."
SectionEnd

Section
	DetailPrint "Going to pin MSPaint..."
	
	DetailPrint  'InvokeShellVerb: "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.PinToTaskbar}'
	${StdUtils.InvokeShellVerb} $0 "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.PinToTaskbar}
	DetailPrint "Result: $0"

	StrCmp "$0" "ok" 0 +3
	MessageBox MB_TOPMOST "Paint should have been pinned to Taskbar now!"
	Goto +2
	MessageBox MB_TOPMOST "Failed to pin, see log for details!"

	DetailPrint "--------------"
SectionEnd

Section
	DetailPrint "Going to un-pin MSPaint..."
	
	DetailPrint  'InvokeShellVerb: "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.UnpinFromTaskbar}'
	${StdUtils.InvokeShellVerb} $0 "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.UnpinFromTaskbar}
	DetailPrint "Result: $0"
	
	StrCmp "$0" "ok" 0 +3
	MessageBox MB_TOPMOST "Paint should have been un-pinned from Taskbar now!"
	Goto +2
	MessageBox MB_TOPMOST "Failed to un-pin, see log for details!"

	DetailPrint "--------------"
SectionEnd

Section
	IfFileExists "$SYSDIR\mspaint.exe" +3
	MessageBox MB_ICONSTOP 'File does not exist:$\n"$SYSDIR\mspaint.exe"$\n$\nExample cannot run!'
	Quit
	MessageBox MB_OK "Please make sure Paint isn't pinned to your Startmenu right now.$\nThen press 'OK' to begin test..."
SectionEnd

Section
	DetailPrint "Going to pin MSPaint..."
	
	DetailPrint  'InvokeShellVerb: "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.PinToStart}'
	${StdUtils.InvokeShellVerb} $0 "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.PinToStart}
	DetailPrint "Result: $0"

	StrCmp "$0" "ok" 0 +3
	MessageBox MB_TOPMOST "Paint should have been pinned to Start now!"
	Goto +2
	MessageBox MB_TOPMOST "Failed to pin, see log for details!"

	DetailPrint "--------------"
SectionEnd

Section
	DetailPrint "Going to un-pin MSPaint..."
	
	DetailPrint  'InvokeShellVerb: "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.UnpinFromStart}'
	${StdUtils.InvokeShellVerb} $0 "$SYSDIR" "mspaint.exe" ${StdUtils.Const.ShellVerb.UnpinFromStart}
	DetailPrint "Result: $0"
	
	StrCmp "$0" "ok" 0 +3
	MessageBox MB_TOPMOST "Paint should have been un-pinned from Start now!"
	Goto +2
	MessageBox MB_TOPMOST "Failed to un-pin, see log for details!"

	DetailPrint "--------------"
SectionEnd
