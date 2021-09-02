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
	OutFile "ShellExecAsUser-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "ShellExecAsUser-ANSI.exe"
!endif

!include 'StdUtils.nsh'

RequestExecutionLevel admin ;make sure our installer will get elevated on Vista+ with UAC enabled
ShowInstDetails show

Section
	DetailPrint 'ExecShell: "$SYSDIR\mspaint.exe"'
	ExecShell "open" "$SYSDIR\mspaint.exe" ;this instance of MS Paint will be elevated too!
	MessageBox MB_TOPMOST "Close Paint and click 'OK' to continue..."
SectionEnd

Section
	DetailPrint 'ExecShellAsUser: "$SYSDIR\mspaint666.exe"'
	Sleep 1000
	${StdUtils.ExecShellAsUser} $0 "$SYSDIR\mspaint666.exe" "open" "" ;launch a *non-elevated* instance of MS Paint
	DetailPrint "Result: $0" ;expected result is "ok" on UAC-enabled systems or "fallback" otherwise. Failure indicated by "error" or "timeout".
SectionEnd

Section
	DetailPrint 'ExecShellAsUser: "$SYSDIR\mspaint.exe"'
	Sleep 1000
	${StdUtils.ExecShellAsUser} $0 "$SYSDIR\mspaint.exe" "open" "" ;launch a *non-elevated* instance of MS Paint
	DetailPrint "Result: $0" ;expected result is "ok" on UAC-enabled systems or "fallback" otherwise. Failure indicated by "error" or "timeout".
SectionEnd
