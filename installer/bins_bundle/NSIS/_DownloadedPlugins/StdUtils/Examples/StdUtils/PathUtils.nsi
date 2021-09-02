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
	OutFile "PathUtils-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "PathUtils-ANSI.exe"
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
# NormalizePath
# -----------------------------------------

!macro TestNormalizePath input
	${StdUtils.NormalizePath} $0 '${input}'
	DetailPrint 'Normalize: "${input}" -> "$0"'
!macroend

Section
	!insertmacro TestNormalizePath "C:\"
	!insertmacro TestNormalizePath "C:\Test"
	!insertmacro TestNormalizePath "C:\Test\"
	!insertmacro TestNormalizePath "C:\Test\."
	!insertmacro TestNormalizePath "C:\Test\Foo"
	!insertmacro TestNormalizePath "C:\Test\\\Foo"
	!insertmacro TestNormalizePath "C:\Test\.\Foo"
	!insertmacro TestNormalizePath "C:\Test\Foo\.."
	!insertmacro TestNormalizePath "C:\Test\Foo\..\Bar\"
	!insertmacro TestNormalizePath "C:\Test\.\Foo\.\Bar\Example"
	!insertmacro TestNormalizePath "C:\Test\/\Foo\/\Bar\Example"
	!insertmacro TestNormalizePath "C:\Test/Foo\\Bar"
	!insertmacro TestNormalizePath "C:\Foo\\Bar/Test\..\Test\."
	
	DetailPrint "--------------"
SectionEnd

# -----------------------------------------
# GetParentPath
# -----------------------------------------

!macro TestGetParentPath input
	${StdUtils.GetParentPath} $0 '${input}'
	DetailPrint 'GetParent: "${input}" -> "$0"'
!macroend

Section
	!insertmacro TestGetParentPath "C:\"
	!insertmacro TestGetParentPath "C:\Test"
	!insertmacro TestGetParentPath "C:\Test\"
	!insertmacro TestGetParentPath "C:\Test\."
	!insertmacro TestGetParentPath "C:\Test\Foo"
	!insertmacro TestGetParentPath "C:\Test\\\Foo"
	!insertmacro TestGetParentPath "C:\Test\.\Foo"
	!insertmacro TestGetParentPath "C:\Test\Foo\.."
	!insertmacro TestGetParentPath "C:\Test\Foo\..\Bar\"
	!insertmacro TestGetParentPath "C:\Test\.\Foo\.\Bar\Example"
	!insertmacro TestGetParentPath "C:\Test\/\Foo\/\Bar\Example"
	!insertmacro TestGetParentPath "C:\Test/Foo\\Bar"
	
	DetailPrint "--------------"
SectionEnd

Section
	StrCpy $0 "C:\Foo\\Bar/Test\.\..\Test\."
	${StdUtils.NormalizePath} $1 "$0"
	DetailPrint 'NormalizePath: "$0" -> "$1"'
SectionEnd

Section
	StrCpy $0 "C:\Foo\Bar\Test\Honk\Sponk"
Loop:
	${StdUtils.GetParentPath} $1 "$0"
	DetailPrint 'GetParentPath: "$0" -> "$1"'
	StrCpy $0 "$1"
	StrCmp "$0" "" 0 Loop
	
	DetailPrint "--------------"
SectionEnd

# -----------------------------------------
# SplitPath
# -----------------------------------------

Section
	StrCpy $0 "C:\Windows\System32\kernel32.dll"
	${StdUtils.SplitPath} $1 $2 $3 $4 "$0"
	DetailPrint 'SplitPath [1]: "$0" -> "$1"'
	DetailPrint 'SplitPath [2]: "$0" -> "$2"'
	DetailPrint 'SplitPath [3]: "$0" -> "$3"'
	DetailPrint 'SplitPath [4]: "$0" -> "$4"'
	
	DetailPrint "--------------"
SectionEnd

# -----------------------------------------
# GetFoobarPart
# -----------------------------------------

Section
	StrCpy $0 "C:\Windows\System32\kernel32.dll"
	${StdUtils.GetDrivePart} $1 "$0"
	DetailPrint 'GetDrivePart: "$0" -> "$1"'
	
	StrCpy $0 "C:\Windows\System32\kernel32.dll"
	${StdUtils.GetDirectoryPart} $1 "$0"
	DetailPrint 'GetDirectoryPart: "$0" -> "$1"'
	
	StrCpy $0 "C:\Windows\System32\kernel32.dll"
	${StdUtils.GetFileNamePart} $1 "$0"
	DetailPrint 'GetFileNamePart: "$0" -> "$1"'
	
	StrCpy $0 "C:\Windows\System32\kernel32.dll"
	${StdUtils.GetExtensionPart} $1 "$0"
	DetailPrint 'GetExtensionPart: "$0" -> "$1"'
	
	DetailPrint "--------------"
SectionEnd
