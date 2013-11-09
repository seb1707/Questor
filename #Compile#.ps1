$global:goodbuilds=0
$global:badbuilds=0
$settingsfileloc=".\CompileSettings.xml"
[xml]$compilelocsettings = Get-Content $settingsfileloc
[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") | Out-Null
#Region Functions
function compilesolution {
  param (
        $msBuildexEcutable,
        $project,
        $msBuildOption
        )
   
  Write-Host "Staring to build project: " -NoNewline
  Write-Host "[$($project)]" -ForegroundColor DarkYellow -BackgroundColor Black
  $results=Invoke-Expression "$($msBuildexEcutable) .\$($project)\$($project).csproj /p:configuration=$($msBuildOption) /target:Clean';'Build /flp:logfile="".\DebugLogs\$($project)Compile.log"""
  if ($LastExitCode -eq 0) {
    Write-Host "Build succeeded, 0 Error(s)" -ForegroundColor DarkGreen 
    write-Host "-----------------------------------------------------------------"
    $global:goodbuilds++
    }
  else {
    Write-Host "There were ERRORS while compiling project: " -ForegroundColor Red -NoNewline
    Write-Host "[$($project)]" -NoNewline
    Write-Host " check log for errors: " -ForegroundColor Red -NoNewline 
    Write-Host "[DebugLogs\$($project)Errors.log]" -ForegroundColor Yellow 
    write-Host "-----------------------------------------------------------------"
    $global:badbuilds++
  }
  
}

#endregion

#region Settings
#settings
$msbuild=$env:SystemRoot+"\Microsoft.Net\FrameWork\v4.0.30319\msbuild.exe"
$msbuildoption = "Debug"
if ($compilelocsettings.Settings.copyToLocation -ne $null) {
    $innerspacedestdir=$compilelocsettings.Settings.copyToLocation
}
else
{
    $innerspacedestdir=".NET Programs"
}

#endregion

Write-Host "+====================================================+"
Write-Host "|                 START                              |" 
Write-Host "+====================================================+"
Write-Host ""
sleep 1
#region Prepare tasks
#Empty Output folder

Write-Host "Removing unnecessary files from .\output\ "  -NoNewline
Remove-Item .\output\* -Recurse -Exclude "Caldari L4",*.xml,skillplan.txt
sleep -Milliseconds 300
Write-Host "[" -ForegroundColor Yellow -NoNewline
Write-Host " OK " -ForegroundColor Green -NoNewline
Write-Host "]" -ForegroundColor Yellow
sleep -Milliseconds 500

#empty bin folder
Write-Host "Removing unnecessary files from .\bin\debug\ and .\bin\release\ "  -NoNewline
Remove-Item .\bin\debug\* -Recurse -ErrorAction SilentlyContinue -Force | Out-Null
Remove-Item .\bin\release\* -Recurse -ErrorAction SilentlyContinue -Force | Out-Null
sleep -Milliseconds 300
Write-Host "[" -ForegroundColor Yellow -NoNewline
Write-Host " OK " -ForegroundColor Green -NoNewline
Write-Host "]" -ForegroundColor Yellow
sleep -Milliseconds 500

#create DebugLogs folder - if there is one, we will force new one :) we are deleting results anyway :)
Write-Host "Create debug folder .\DebugLogs\ "  -NoNewline
New-Item .\DebugLogs\ -Force -ItemType directory | Out-Null
Remove-Item .\DebugLogs\* -Recurse -ErrorAction SilentlyContinue -Force | Out-Null
sleep -Milliseconds 300
Write-Host "[" -ForegroundColor Yellow -NoNewline
Write-Host " OK " -ForegroundColor Green -NoNewline
Write-Host "]" -ForegroundColor Yellow
sleep -Milliseconds 500
#endregion

#maybe we can collect this from XML, but I don't see a way for now
$arrProjects=  ("Questor.Modules",
                "Questor",
                "valuedump",
                "QuestorManager",
                "BUYLPI",
                "updateinvtypes"
               )
Write-Host ""
Write-Host "Starting to compile" -NoNewline
sleep -Milliseconds 300
Write-Host "." -NoNewline; sleep  -Milliseconds 300;Write-Host "." -NoNewline; sleep  -Milliseconds 300; Write-Host ".";sleep -Milliseconds 300
Write-Host ""
write-Host "-----------------------------------------------------------------"
$arrProjects | % {
compilesolution -msbuildexecutable $msbuild -project $_ -msbuildoption $msbuildoption
}

Write-Host "Copy files to output folder " -NoNewline


if ($msbuildoption -eq "Debug") {
 Copy-Item .\bin\$msbuildoption\* -Include *.exe,*.dll,*.pdb -Destination .\output\
}
else {
 Copy-Item .\bin\$msbuildoption\* -Include *.exe,*.dll -Destination .\output\
}
sleep -Milliseconds 300
Write-Host "[" -ForegroundColor Yellow -NoNewline
Write-Host " OK " -ForegroundColor Green -NoNewline
Write-Host "]" -ForegroundColor Yellow
sleep -Milliseconds 500

#Summary write-up
Write-Host ""
Write-Host "+====================================================+"
Write-Host "|                SUMMARY                             |" 
Write-Host "+====================================================+"
Write-Host "|" -NoNewline
if ($global:badbuilds -gt 0) {
  Write-Host "We have encountered $($global:badbuilds) errors while building solutions, please check .\DebugLogs\ !!!" -BackgroundColor Black -ForegroundColor Red -NoNewline
  Write-Host "|"
  Write-Host "+====================================================+"
  Write-Host ""
  break
}

if ($global:goodbuilds -gt 0) {
  Write-Host "We have successfully compiled $($global:goodbuilds) solutions!" -BackgroundColor Black -ForegroundColor DarkGreen -NoNewline
  Write-Host "           |"
  Write-Host "+====================================================+"
  Write-Host ""
}
if ($compilelocsettings.Settings.innerspaceLocation) {
  $innerpath=$compilelocsettings.Settings.innerspaceLocation
}
else {
  $regInner=Get-ItemProperty "hklm:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\InnerSpace.exe" -ErrorAction SilentlyContinue
  $innerpath = $regInner.Path
}
if ($compilelocsettings.Settings.unnattendedCopy -ne $True) {
Write-Host "Do you want files to be copied to production? [ $($innerpath) ]"
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
}
else {
Write-Host "You wanted unattended copy, so here we go" -ForegroundColor Yellow -NoNewline
sleep -Milliseconds 400
Write-Host "." -ForegroundColor Yellow -NoNewline; sleep  -Milliseconds 400;Write-Host "." -ForegroundColor Yellow -NoNewline; sleep  -Milliseconds 400; Write-Host "." -ForegroundColor Yellow;sleep -Milliseconds 400
Write-Host ""
$choice = "Yes"
}

if ($innerpath -eq $null) {
 Write-Host "Could not read Innerspace location, please copy manually!!!" -ForegroundColor Red
 $choice="N"
}

Write-Host "Found Innerspace path:" -NoNewline
Write-Host " [ " -ForegroundColor White -NoNewline
write-host "$($innerpath)" -NoNewline -ForegroundColor Yellow
Write-Host " ] " -ForegroundColor White -NoNewline
write-host "please check if it is ok?" 
sleep 3
switch ($choice)
    { 
        "Y" {
             if ((Test-Path "$($innerpath)\$($innerspacedestdir)")-ne $true) {
                Write-Host "Creating directory " -NoNewline
                Write-Host "["  -NoNewline -ForegroundColor white
                write-host "$($innerpath)\$($innerspacedestdir)" -NoNewline -ForegroundColor yellow
                Write-Host "]" -NoNewline -ForegroundColor white
                write-host "... " -NoNewline
                mkdir  "$($innerpath)\$($innerspacedestdir)" -Force | Out-Null
                sleep -Milliseconds 500
                Write-Host "[" -ForegroundColor yellow -NoNewline
                Write-Host " OK " -ForegroundColor Green -NoNewline
                Write-Host "]" -ForegroundColor yellow  
             }
             $iscopied = $false
             $i=0
             do {
               try{
                 Write-Host "Copy *.exe to production ... " -NoNewline
                 Copy-Item .\output\*.exe -Destination "$($innerpath)\$($innerspacedestdir)" -Force 
                 sleep -Milliseconds 300
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                 $iscopied = $true
               }
               catch {
                 $OUTPUT= [System.Windows.Forms.MessageBox]::Show("Encountered problem while copying .exe, prolly questor is running." , "Status" , 5)
                 if ($OUTPUT -eq "RETRY" ) { Write-Host "Retrying copy!!!" -ForegroundColor Yellow} 
                 else { 
                   Write-Host "Skipping copy!!!" -ForegroundColor Red
                   sleep -Seconds 2
                   break
                 }

               }
             }  while ($iscopied -eq $false);
             sleep -Milliseconds 500
             $iscopied = $false
             $i=0
             do {
               try{
                 Write-Host "Copy *.dll to production ... " -NoNewline
                 Copy-Item .\output\*.dll -Destination "$($innerpath)\$($innerspacedestdir)" -Force 
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                 $iscopied = $true
               }
               catch {
                 $OUTPUT= [System.Windows.Forms.MessageBox]::Show("Encountered problem while copying .dll, prolly questor is running." , "Status" , 5)
                 if ($OUTPUT -eq "RETRY" ) { Write-Host "Retrying copy!!!" -ForegroundColor Yellow} 
                 else { 
                   Write-Host "Skipping copy!!!" -ForegroundColor Red
                   sleep -Seconds 2
                   break
                 }
 
               }
             }  while ($iscopied -eq $false);
             sleep -Milliseconds 500
             $iscopied = $false
             $i=0
             do {
               try{
                 Write-Host "Copy *.pdb to prodcution ... " -NoNewline
                 Copy-Item .\output\*.pdb -Destination "$($innerpath)\$($innerspacedestdir)" -Force
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                 $iscopied = $true
               }
               catch {
                 $OUTPUT= [System.Windows.Forms.MessageBox]::Show("Encountered problem while copying .pdb, prolly questor is running." , "Status" , 5)
                 if ($OUTPUT -eq "RETRY" ) { Write-Host "Retrying copy!!!" -ForegroundColor Yellow} 
                 else { 
                   Write-Host "Skipping copy!!!" -ForegroundColor Red
                   sleep -Seconds 2
                   break
                 }
 
               }
             }  while ($iscopied -eq $false);

             sleep -Milliseconds 500
             Write-Host "Copy missing *.xml files to prodcution ... "
             sleep -Milliseconds 500
             Get-ChildItem .\output -Filter *.xml | % {
             if (!(Test-Path "$($innerpath)\$($innerspacedestdir)\$($_.name)")){
                Write-Host "Copying file " -NoNewline
                Write-Host "[" -ForegroundColor white -NoNewline
                write-host "$($_.name)" -NoNewline -ForegroundColor yellow
                Write-Host "]" -ForegroundColor white -NoNewline
                Write-Host "... " -NoNewline
                Copy-Item .\output\$($_.name) -Destination "$($innerpath)\$($innerspacedestdir)"
                sleep -Milliseconds 500
                Write-Host "[" -ForegroundColor yellow -NoNewline
                Write-Host " OK " -ForegroundColor Green -NoNewline
                Write-Host "]" -ForegroundColor yellow
             }
             else {
                Write-Host "File " -NoNewline
                Write-Host "["  -NoNewline -ForegroundColor white
                write-host "$($_.name)" -NoNewline -ForegroundColor yellow
                Write-Host "]" -NoNewline -ForegroundColor white
                Write-Host " found, skipping ..."
             }
            }
            }
        "yes" {
             if ((Test-Path "$($innerpath)\$($innerspacedestdir)")-ne $true) {
                Write-Host "Creating directory " -NoNewline
                Write-Host "["  -NoNewline -ForegroundColor white
                write-host "$($innerpath)\$($innerspacedestdir)" -NoNewline -ForegroundColor yellow
                Write-Host "]" -NoNewline -ForegroundColor white
                write-host "... " -NoNewline
                mkdir  "$($innerpath)\$($innerspacedestdir)" -Force | Out-Null
                sleep -Milliseconds 500
                Write-Host "[" -ForegroundColor yellow -NoNewline
                Write-Host " OK " -ForegroundColor Green -NoNewline
                Write-Host "]" -ForegroundColor yellow
             }
             $iscopied = $false
             $i=0
             do {
               try{
                 Write-Host "Copy *.exe to production ... " -NoNewline
                 Copy-Item .\output\*.exe -Destination "$($innerpath)\$($innerspacedestdir)" -Force 
                 sleep -Milliseconds 300
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                 $iscopied = $true
               }
               catch {
                 $OUTPUT= [System.Windows.Forms.MessageBox]::Show("Encountered problem while copying .exe, prolly questor is running." , "Status" , 5)
                 if ($OUTPUT -eq "RETRY" ) { Write-Host "Retrying copy!!!" -ForegroundColor Yellow} 
                 else { 
                   Write-Host "Skipping copy!!!" -ForegroundColor Red
                   sleep -Seconds 2
                   break
                 }
 
               }
             }  while ($iscopied -eq $false);
             sleep -Milliseconds 500
             $iscopied = $false
             $i=0
             do {
               try{
                 Write-Host "Copy *.dll to production ... " -NoNewline
                 Copy-Item .\output\*.dll -Destination "$($innerpath)\$($innerspacedestdir)" -Force 
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                 $iscopied = $true
               }
               catch {
                 $OUTPUT= [System.Windows.Forms.MessageBox]::Show("Encountered problem while copying .dll, prolly questor is running." , "Status" , 5)
                 if ($OUTPUT -eq "RETRY" ) { Write-Host "Retrying copy!!!" -ForegroundColor Yellow} 
                 else { 
                   Write-Host "Skipping copy!!!" -ForegroundColor Red
                   sleep -Seconds 2
                   break
                 }
 
               }
             }  while ($iscopied -eq $false);
             sleep -Milliseconds 500
             $iscopied = $false
             $i=0
             do {
               try{
                 Write-Host "Copy *.pdb to prodcution ... " -NoNewline
                 Copy-Item .\output\*.pdb -Destination "$($innerpath)\$($innerspacedestdir)" -Force
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                 $iscopied = $true
               }
               catch {
                 $OUTPUT= [System.Windows.Forms.MessageBox]::Show("Encountered problem while copying .pdb, prolly questor is running." , "Status" , 5)
                 if ($OUTPUT -eq "RETRY" ) { Write-Host "Retrying copy!!!" -ForegroundColor Yellow} 
                 else { 
                   Write-Host "Skipping copy!!!" -ForegroundColor Red
                   sleep -Seconds 2
                   break
                 }
 
               }
             }  while ($iscopied -eq $false);

             sleep -Milliseconds 500
             Write-Host "Copy missing *.xml files to prodcution ... "
             sleep -Milliseconds 500
             Get-ChildItem .\output -Filter *.xml | % {
             if (!(Test-Path "$($innerpath)\$($innerspacedestdir)\$($_.name)")){
                Write-Host "Copying file " -NoNewline
                Write-Host "[" -ForegroundColor white -NoNewline
                write-host "$($_.name)" -NoNewline -ForegroundColor yellow
                Write-Host "]" -ForegroundColor white -NoNewline
                Write-Host "... " -NoNewline
                Copy-Item .\output\$($_.name) -Destination "$($innerpath)\$($innerspacedestdir)"
                sleep -Milliseconds 500
                Write-Host "[" -ForegroundColor Yellow -NoNewline
                Write-Host " OK " -ForegroundColor Green -NoNewline
                Write-Host "]" -ForegroundColor Yellow
             }
             else {
                Write-Host "File " -NoNewline
                Write-Host "["  -NoNewline -ForegroundColor white
                write-host "$($_.name)" -NoNewline -ForegroundColor yellow
                Write-Host "]" -NoNewline -ForegroundColor white
                Write-Host " found, skipping ..."
             }
            }
            } 
        "N" {
             Write-Host "Skipping copy ... " -NoNewline
             sleep -Milliseconds 500
             Write-Host "[" -ForegroundColor Yellow -NoNewline
             Write-Host " OK " -ForegroundColor Green -NoNewline
             Write-Host "]" -ForegroundColor Yellow
             
            } 
        "No" {
             Write-Host "Skipping copy ... " -NoNewline
             sleep -Milliseconds 500
             Write-Host "[" -ForegroundColor Yellow -NoNewline
             Write-Host " OK " -ForegroundColor Green -NoNewline
             Write-Host "]" -ForegroundColor Yellow
             } 
        
        default {
                 Write-Host "Skipping copy ... " -NoNewline
                 sleep -Milliseconds 500
                 Write-Host "[" -ForegroundColor Yellow -NoNewline
                 Write-Host " OK " -ForegroundColor Green -NoNewline
                 Write-Host "]" -ForegroundColor Yellow
                }
    }
Sleep 4
Write-Host "Bye!"
sleep 2