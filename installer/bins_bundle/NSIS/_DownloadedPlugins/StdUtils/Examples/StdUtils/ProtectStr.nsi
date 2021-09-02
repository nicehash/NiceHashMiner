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
	OutFile "ProtectStr-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "ProtectStr-ANSI.exe"
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
# ProtectStr
# -----------------------------------------

Section
	ClearErrors

	${StdUtils.ProtectStr} $1 "CU" "" ""
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "" "$1"
	DetailPrint 'UnprotectedStr: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	${StdUtils.ProtectStr} $1 "CU" "" "Five quacking Zephyrs jolt my wax bed."
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "" "$1"
	DetailPrint 'UnprotectedStr: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	${StdUtils.ProtectStr} $1 "CU" "a7gzLbdwdbk4" "Five quacking Zephyrs jolt my wax bed."
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "a7gzLbdwdbk4" "$1"
	DetailPrint 'UnprotectedStr: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort
	
	${StdUtils.ProtectStr} $1 "CU" "a7gzLbdwdbk4" "Five quacking Zephyrs jolt my wax bed."
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "" "$1"
	DetailPrint 'UnprotectedStr: "$2" (Expected to fail!)'
	${StdUtils.UnprotectStr} $2 0 "HMJjUaUV4p9W" "$1"
	DetailPrint 'UnprotectedStr: "$2" (Expected to fail!)'
	IfErrors +3
	DetailPrint "Whoops, succeeded unexpectedly!"
	Abort
	
	DetailPrint "--------------"
SectionEnd

Section
	ClearErrors

	${StdUtils.ProtectStr} $1 "LM" "" ""
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "" "$1"
	DetailPrint 'UnprotectedStr: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	${StdUtils.ProtectStr} $1 "LM" "" "Five quacking Zephyrs jolt my wax bed."
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "" "$1"
	DetailPrint 'UnprotectedStr: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort

	${StdUtils.ProtectStr} $1 "LM" "a7gzLbdwdbk4" "Five quacking Zephyrs jolt my wax bed."
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "a7gzLbdwdbk4" "$1"
	DetailPrint 'UnprotectedStr: "$2"'
	IfErrors 0 +3
	DetailPrint "Whoops, failed unexpectedly!"
	Abort
	
	${StdUtils.ProtectStr} $1 "LM" "a7gzLbdwdbk4" "Five quacking Zephyrs jolt my wax bed."
	DetailPrint 'ProtectedStr: "$1"'
	${StdUtils.UnprotectStr} $2 0 "" "$1"
	DetailPrint 'UnprotectedStr: "$2" (Expected to fail!)'
	${StdUtils.UnprotectStr} $2 0 "HMJjUaUV4p9W" "$1"
	DetailPrint 'UnprotectedStr: "$2" (Expected to fail!)'
	IfErrors +3
	DetailPrint "Whoops, succeeded unexpectedly!"
	Abort
	
	DetailPrint "--------------"
SectionEnd

Section
	ClearErrors
	StrCpy $9 0

TheLoop:
	DetailPrint "[$9]"
	${StdUtils.RandMinMax} $0 13 30
	DetailPrint 'RandomLength: $0'
	${StdUtils.RandBytes} $1 $0
	DetailPrint 'OriginalStr: "$1"'
	
	${StdUtils.ProtectStr} $2 "CU" "" "$1"
	DetailPrint 'ProtectedStr: "$2"'
	${StdUtils.UnprotectStr} $3 0 "" "$2"
	DetailPrint 'UnprotectedStr: "$3"'
	IfErrors Failure
	StrCmp $1 $3 +3
	DetailPrint "Miscompare !!!"
	Abort
	
	${StdUtils.ProtectStr} $2 "LM" "" "$1"
	DetailPrint 'ProtectedStr: "$2"'
	${StdUtils.UnprotectStr} $3 0 "" "$2"
	DetailPrint 'UnprotectedStr: "$3"'
	StrCmp $1 $3 Success
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
