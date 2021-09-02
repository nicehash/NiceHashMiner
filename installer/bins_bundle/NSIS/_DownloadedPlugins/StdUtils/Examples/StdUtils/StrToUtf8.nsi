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

Caption "StdUtils PathUtils"

!addincludedir  "..\..\Include"

!ifdef NSIS_UNICODE
	!addplugindir "..\..\Plugins\Release_Unicode"
	OutFile "StrToUtf8-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "StrToUtf8-ANSI.exe"
!endif

!include 'StdUtils.nsh'

RequestExecutionLevel user
ShowInstDetails show

# -----------------------------------------
# GetLibVersion
# -----------------------------------------

Section
	${StdUtils.GetLibVersion} $1 $2
	DetailPrint "Testing StdUtils library v$1"
	DetailPrint "Library built: $2"
	
	DetailPrint "--------------"
SectionEnd

# -----------------------------------------
# StrToUtf8
# -----------------------------------------

Section
	ClearErrors

	${StdUtils.StrToUtf8} $1 ""
	DetailPrint 'UTF-8: "$1"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort
	
	${StdUtils.StrFromUtf8} $2 0 "$1"
	DetailPrint 'Plain: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	DetailPrint "--------------"
SectionEnd

Section
	ClearErrors

	${StdUtils.StrToUtf8} $1 "!"
	DetailPrint 'UTF-8: "$1"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort
	
	${StdUtils.StrFromUtf8} $2 0 "$1"
	DetailPrint 'Plain: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	DetailPrint "--------------"
SectionEnd

Section
	ClearErrors

	${StdUtils.StrToUtf8} $1 "The five boxing wizards jump quickly."
	DetailPrint 'UTF-8: "$1"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort
	
	${StdUtils.StrFromUtf8} $2 0 "$1"
	DetailPrint 'Plain: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	DetailPrint "--------------"
SectionEnd

Section
	ClearErrors
	StrCpy $9 0

TheLoop:
	DetailPrint "[$9]"
	StrCpy $0 "Lorem ipsum dolor sit amet $9, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
	${StdUtils.StrToUtf8} $1 $0
	DetailPrint 'UTF-8: "$1"'
	IfErrors Failure
	
	${StdUtils.StrFromUtf8} $2 0 "$1"
	DetailPrint 'Plain: "$2"'
	IfErrors Failure
	StrCmp $0 $2 Success
	DetailPrint "Miscompare !!!"
	Abort
	
Failure:
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

Success:
	DetailPrint "~Okay~"
	IntOp $9 $9 + 1
	IntCmp $9 3989 TheLoop TheLoop

	DetailPrint "--------------"
SectionEnd
