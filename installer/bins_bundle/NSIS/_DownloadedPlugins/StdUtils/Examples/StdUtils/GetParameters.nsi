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
	OutFile "GetParameters-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "GetParameters-ANSI.exe"
!endif

!include 'StdUtils.nsh'

RequestExecutionLevel user
ShowInstDetails show

Section
	${StdUtils.TestParameter} $R0 "?"
	StrCmp "$R0" "true" 0 +3
	DetailPrint 'Command-line parameter /? is specified!'
	Goto +2
	DetailPrint 'Command-line parameter /? is *not* specified!'
	
	${StdUtils.GetParameter} $R0 "?" "<MyDefault>"
	DetailPrint 'Value of command-line parameter /? is: "$R0"'
	
	DetailPrint "----"
SectionEnd

Section
	StrCpy $R0 0                                    #Init counter to zero
	${StdUtils.ParameterCnt} $R1                    #Get number of command-line tokens
	IntCmp $R1 0 0 0 LoopNext                       #Any tokens available?
	DetailPrint 'No command-line tokens!'           #Print some info
	Goto LoopExit                                   #Exit
LoopNext:
	${StdUtils.ParameterStr} $R2 $R0                #Read next command-line token
	DetailPrint 'Command-line token #$R0 is "$R2"'  #Print command-line token
	IntOp $R0 $R0 + 1                               #counter += 1
	IntCmp $R0 $R1 0 LoopNext                       #Loop while more tokens available
LoopExit:
	DetailPrint "----"
SectionEnd

Section
	${StdUtils.GetAllParameters} $R0 0
	DetailPrint "Complete command-line: '$R0'"

	${StdUtils.GetAllParameters} $R0 1
	DetailPrint "Truncated command-line: '$R0'"
	
	DetailPrint "----"
SectionEnd
