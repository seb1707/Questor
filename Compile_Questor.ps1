$global:goodbuilds=0
$global:badbuilds=0
#Region Functions
function compilesolution {
  param (
  		$msbuildexecutable,
		$project,
		$msbuildoption
		)
  Write-Host ""
  Write-Host "-----------------------------------------------------------------"		
  Write-Host "Staring to buld project: " -ForegroundColor Yellow  -BackgroundColor DarkGray -NoNewline
  Write-Host "[$($project)]" -ForegroundColor DarkYellow -BackgroundColor Black
  Write-Host "-----------------------------------------------------------------"

  $results=Invoke-Expression "$($msbuildexecutable) .\$($project)\$($project).csproj /p:configuration=$($msbuildoption) /target:Clean';'Build /flp:logfile="".\DebugLogs\$($project)Compile.log"""
  #Write-Host $results
  if ($results -contains "Build succeeded.") {
    Write-Host "Build succeeded, 0 Error(s)" -ForegroundColor Green -BackgroundColor DarkGray
	write-Host "-----------------------------------------------------------------"
    $global:goodbuilds++
  }
  else {
  	
    Write-Host "There were ERRORS while compiling project: " -ForegroundColor Red -NoNewline
	Write-Host "[$($project)]" -ForegroundColor Yellow -NoNewline -BackgroundColor DarkGray
	Write-Host " check log for errors: " -ForegroundColor Red -NoNewline 
	Write-Host "[DebugLogs\$($project)Errors.log]" -ForegroundColor Yellow -BackgroundColor DarkGray
	write-Host "-----------------------------------------------------------------"
	#Write-Host $results
	$global:badbuilds++
  }
  
}

#endregion

#region Settings
#settings
$msbuild=$env:SystemRoot+"\Microsoft.Net\FrameWork\v4.0.30319\msbuild.exe"
$msbuildoption = "Debug"
#endregion

Write-Host "+==============================================================================+"
Write-Host "|                           START                                              |"	
Write-Host "+==============================================================================+"
Write-Host ""
sleep 1
#region Prepare tasks
#Empty Output folder

Write-Host "Removing unnecessary files from .\output\" -ForegroundColor Yellow -BackgroundColor DarkGray 
Remove-Item .\output\* -Recurse -Exclude "Caldari L4",*.xml,skillplan.txt
sleep -Milliseconds 300

#empty bin folde
Write-Host "Removing unnecessary files from .\bin\debug\ and .\bin\release\" -ForegroundColor Yellow -BackgroundColor DarkGray 
Remove-Item .\bin\debug\* -Recurse -ErrorAction SilentlyContinue -Force | Out-Null
Remove-Item .\bin\release\* -Recurse -ErrorAction SilentlyContinue -Force | Out-Null
sleep -Milliseconds 300

#create DebugLogs folder - if there is one, we will force new one :) we are deleting resulults anyway :)
Write-Host "Create debug folder .\DebugLogs\" -ForegroundColor Yellow -BackgroundColor DarkGray 
New-Item .\DebugLogs\ -Force -ItemType directory | Out-Null
Remove-Item .\DebugLogs\* -Recurse -ErrorAction SilentlyContinue -Force | Out-Null
sleep -Milliseconds 300
#endregion

#maybe we can collect this from XML, but I don't se wy for now
$arrProjects=  ("Questor.Modules",
			    "Questor",
				"valuedump",
				"QuestorManager",
				"BUYLPI",
				"updateinvtypes"
			   )
Write-Host "Starting to compile ..." -ForegroundColor Yellow -BackgroundColor DarkGray 
Write-Host ""
$arrProjects | % {
compilesolution -msbuildexecutable $msbuild -project $_ -msbuildoption $msbuildoption
}

Write-Host "Copy files to ouput folder ..." -ForegroundColor Yellow -BackgroundColor DarkGray 


if ($msbuildoption -eq "Debug") {
 Copy-Item .\bin\$msbuildoption\* -Include *.exe,*.dll,*.pdb -Destination .\output\
}
else {
 Copy-Item .\bin\$msbuildoption\* -Include *.exe,*.dll -Destination .\output\
}
sleep -Milliseconds 300

#Summary write-up
Write-Host ""
Write-Host "+==============================================================================+"
Write-Host "|                           SUMMARY                                            |"	
Write-Host "+==============================================================================+"
if ($global:badbuilds -gt 0) {
  Write-Host "We have encountered $($global:badbuilds) errors while building solutions, please check .\DebugLogs\ !!!" -BackgroundColor Black -ForegroundColor Red
  Write-Host "+==============================================================================+"
  break
}

if ($global:goodbuilds -gt 0) {
  Write-Host "We have succesfully compiled $($global:goodbuilds) solutions!" -BackgroundColor Black -ForegroundColor DarkGreen
  Write-Host "+==============================================================================+"
}


Write-Host "Do you want files to be copied to production? [ $($regInner.Path) ]"
Write-Host "[" -ForegroundColor White -NoNewline
Write-Host "Y" -ForegroundColor Yellow -NoNewline
Write-Host "]" -ForegroundColor White -NoNewline
Write-Host "es - Normal Copy [*.dll, *.exe, *.pdb]"
Write-Host "[" -ForegroundColor White -NoNewline
Write-Host "N" -ForegroundColor Yellow -NoNewline
Write-Host "]" -ForegroundColor White -NoNewline
Write-Host "o - do not copy, "
Write-Host "[" -ForegroundColor White -NoNewline
Write-Host "Any key" -ForegroundColor Yellow -NoNewline
Write-Host "]" -ForegroundColor White -NoNewline
Write-Host " means no copy!"

$choice = Read-Host -Prompt "Please select one"
$regInner=Get-ItemProperty "hklm:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\InnerSpace.exe" -ErrorAction SilentlyContinue
if ($regInner.Path -eq $null) {
 Write-Host "Could not read Innerspace location, please copy manually!!!" -ForegroundColor Red
 $choice="N"
}

Write-Host "Found Innerspace path: $($regInner.Path) please check if it is ok?" -ForegroundColor Yellow
sleep 3

switch ($choice)
    { 
        "Y" {
		     Write-Host "Copy *.exe to prodcution ... " -BackgroundColor DarkGray -ForegroundColor Yellow
			 Copy-Item .\output\*.exe -Destination "$($regInner.Path)\.Net Programs" -Force 
             Write-Host "Copy *.dll to prodcution ... " -BackgroundColor DarkGray -ForegroundColor Yellow
			 Copy-Item .\output\*.dll -Destination "$($regInner.Path)\.Net Programs" -Force 
             Write-Host "Copy *.pdb to prodcution ... " -BackgroundColor DarkGray -ForegroundColor Yellow
			 Copy-Item .\output\*.pdb -Destination "$($regInner.Path)\.Net Programs" -Force
			} 
        "yes" {
			   Write-Host "Copy *.exe to prodcution ... " -BackgroundColor DarkGray -ForegroundColor Yellow
			   Copy-Item .\output\*.exe -Destination "$($regInner.Path)\.Net Programs" -Force 
               Write-Host "Copy *.dll to prodcution ... " -BackgroundColor DarkGray -ForegroundColor Yellow
			   Copy-Item .\output\*.dll -Destination "$($regInner.Path)\.Net Programs" -Force 
               Write-Host "Copy *.pdb to prodcution ... " -BackgroundColor DarkGray -ForegroundColor Yellow
			   Copy-Item .\output\*.pdb -Destination "$($regInner.Path)\.Net Programs" -Force
		      } 
        "N" {
			 Write-Host "Skipping copy ... " -BackgroundColor DarkGray -ForegroundColor Yellow
		    } 
        "No" {
		   	  Write-Host "Skipping copy ... " -BackgroundColor DarkGray -ForegroundColor Yellow
		     } 
		
        default {
				 Write-Host "Skipping copy ... " -BackgroundColor DarkGray -ForegroundColor Yellow
				}
    }
Sleep 5
