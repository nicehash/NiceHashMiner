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
	OutFile "SHFileOperation-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "SHFileOperation-ANSI.exe"
!endif

!macro NextTest
	Section
		DetailPrint "--------------"
	SectionEnd
!macroend

!include 'StdUtils.nsh'

RequestExecutionLevel user
ShowInstDetails show

# -----------------------------------------
# SHFileCopy/SHFileMove function
# -----------------------------------------

Section
	InitPluginsDir
	SetOutPath "$PLUGINSDIR\TestDirA"
	File "${NSISDIR}\Contrib\Graphics\Checks\*.*"
	SetOutPath "$PLUGINSDIR\TestDirA\SubDir"
	File "${NSISDIR}\Contrib\Graphics\Header\*.*"
	CreateDirectory "$PLUGINSDIR\SubDirX"
	CreateDirectory "$PLUGINSDIR\SubDirY"
	
	${StdUtils.SHFileCopy} $0 "$PLUGINSDIR\TestDirA" "$PLUGINSDIR\SubDirX\TestDirB" $HWNDPARENT
	DetailPrint "SHFileCopy: $0"
	${StdUtils.SHFileMove} $0 "$PLUGINSDIR\TestDirA" "$PLUGINSDIR\SubDirY\TestDirC" $HWNDPARENT
	DetailPrint "SHFileMove: $0"
	ExecShell "explore" "$PLUGINSDIR"
SectionEnd

!insertmacro NextTest

Section
	MessageBox MB_ICONINFORMATION "The next three operations are going to fail!$\nBut only one will be verbose..."

	${StdUtils.SHFileCopy} $0 "$PLUGINSDIR\TestDirXYZ" "$PLUGINSDIR\SubDirX\TestDirZ" $HWNDPARENT
	DetailPrint "SHFileCopy: $0"
	
	${StdUtils.SetVerbose} 1
	${StdUtils.SHFileCopy} $0 "$PLUGINSDIR\TestDirXYZ" "$PLUGINSDIR\SubDirX\TestDirZ" $HWNDPARENT
	DetailPrint "SHFileCopy: $0"
	
	${StdUtils.SetVerbose} 0
	${StdUtils.SHFileCopy} $0 "$PLUGINSDIR\TestDirXYZ" "$PLUGINSDIR\SubDirX\TestDirZ" $HWNDPARENT
	DetailPrint "SHFileCopy: $0"
SectionEnd

!insertmacro NextTest
