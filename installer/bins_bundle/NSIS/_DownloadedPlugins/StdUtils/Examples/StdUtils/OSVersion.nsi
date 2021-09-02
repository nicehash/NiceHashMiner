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

Caption "StdUtils GetRealOSVersion"

!addincludedir  "..\..\Include"

!ifdef NSIS_UNICODE
	!addplugindir "..\..\Plugins\Release_Unicode"
	OutFile "OSVersion-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "OSVersion-ANSI.exe"
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
# GetRealOSVersion
# -----------------------------------------

Section
	${StdUtils.GetRealOSVersion} $1 $2 $3
	${StdUtils.GetRealOSBuildNo} $4
	DetailPrint "Windows NT v$1.$2, ServicePack $3, Build $4"
	
	${StdUtils.GetOSEdition} $1
	DetailPrint "Edition: $1"
	
	${StdUtils.GetOSReleaseId} $1
	DetailPrint "Release Id: v$1"
	
	${StdUtils.GetRealOSName} $1
	DetailPrint "Friendly name: $\"$1$\""
	
	DetailPrint "--------------"
SectionEnd
