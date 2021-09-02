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
	OutFile "TimerCreate-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "TimerCreate-ANSI.exe"
!endif

!include 'StdUtils.nsh'

RequestExecutionLevel user
ShowInstDetails show

Var TimerId
Var MyCount

Function MyCallback
	IntOp $MyCount $MyCount + 1
	DetailPrint "Timer event has been triggered! (#$MyCount)"
FunctionEnd

Function .onGUIInit
	${StdUtils.TimerCreate} $TimerId MyCallback 1500
	StrCmp $TimerId "error" 0 +2
	MessageBox MB_ICONSTOP "Failed to create timer!"
FunctionEnd

Function .onGUIEnd
	StrCmp $TimerId "error" 0 +2
	Return
	${StdUtils.TimerDestroy} $0 $TimerId
	StrCmp $0 "ok" +2
	MessageBox MB_ICONSTOP "Failed to destroy timer!"
FunctionEnd

Section
	DetailPrint "Hello, world!"
SectionEnd
