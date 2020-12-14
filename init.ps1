[CmdletBinding()] # allows passing standard PS parameters such as Verbose etc.
 
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:TARGET_ENVIRONMENT_STAGE = "dev" # tests are be pointed to local service instance
$env:TARGET_ENVIRONMENT_REGION = "westus" # tests are be pointed to local service instance
$env:ROOT = Split-Path $MyInvocation.MyCommand.Path
$env:SLN_NAME = "Telegraphy.sln"
$env:SLN_RUNSETTINGS_NAME = "Telegraphy.runsettings"
 
if (-not [Environment]::Is64BitProcess) {
    write-host "Please run this command window in AMD64 processor architecture.`r`n"
    write-host "This could be because cmd.exe, powershell.exe, ConEmu.exe is a 32-bit process`r`n"
    Pause; exit;
}
 
Write-Host "Restoring Nuget Packages for ${env:SLN_NAME}"  -ForegroundColor Cyan;
dotnet.exe restore ${env:SLN_NAME} --verbosity quiet
 
Write-Host "";
Write-Host (Get-Content -Raw "${env:ROOT}\.build\welcome.txt")
Write-Host "";
Write-Host "Please use the" -NoNewline; Write-Host " validate" -ForegroundColor Yellow -NoNewline; Write-Host " command to run tests before pushing a pull requests"
 
#############################################################################################
#
# Functionality to development easier/faster
#
#############################################################################################
 
function opensln {
    Start-Process "${env:ROOT}\${env:SLN_NAME}";
}
 
#############################################################################################
#
# Functionality to make running pre-checkin validation easier
#   validate == runs full validation suite
#   rerunfailetests == reruns all of the failed tests from the last run
#   runtests  test_xyz == runs a specific tests (NOTE: test_xyz is a wildcard match)
#
#############################################################################################
 
function __executetest {
    param(
        [String] $sln,
        [String] $trxDir,
        [String] $testName
    )
    Process {
        Write-Host "*****************************************"
        Write-Host "Executing: " -NoNewline
        Write-Host $testName -ForegroundColor Cyan
        #Write-Host "dotnet.exe test $sln --no-build --no-restore -s ${slnDir}${env:SLN_RUNSETTINGS_NAME} --filter $testName --list-tests --nologo"
        dotnet.exe test $sln --no-build --no-restore -s ${slnDir}${env:SLN_RUNSETTINGS_NAME} --filter $testName --list-tests --nologo
 
        #Write-Host "dotnet.exe test $sln --no-build --no-restore -s ${slnDir}${env:SLN_RUNSETTINGS_NAME} --filter $testName -v m --logger trx --results-directory $trxDir --collect:'XPlat Code Coverage'"
        dotnet.exe test $sln --no-build --no-restore -s ${slnDir}${env:SLN_RUNSETTINGS_NAME} --filter $testName -v m --logger trx --results-directory $trxDir --nologo --collect:"XPlat Code Coverage"
    }
}
 
function __printTestResult {
    param(
        [int] $exitCode,
        [String] $testName
    )
              
    Process {
        if ($exitCode -eq 0) {
            Write-Host " Passed ==> " -ForegroundColor Green -NoNewline
        }
        else {
            Write-Host " Failed ==> " -ForegroundColor Red -NoNewline
        }
                             
        Write-Host $testName
    }
}
 
function __getxmlfiles {
    param(
        [String] $rootDir,
        [String] $filter = "*.xml",
        [Bool] $recurse = $false
    )
    Process {
        $Result = @();
 
        $folder = $rootDir;
        if ($recurse)
        {
            $files = Get-ChildItem -Path $folder -Filter $filter -Recurse
        }
        else
        {
            $files = Get-ChildItem -Path $folder -Filter $filter
        }
 
        foreach ($file in $files) {
            $o = New-Object "PSObject";
                             
            $fullPath = $file.fullname
 
            $exists = $true;
            Add-Member -InputObject $o -NotePropertyName "Exists" -NotePropertyValue $exists;
 
            $valid = $false;
            $xml = $null;
 
            try {
                $xml = [Xml] (Get-Content -literalpath $fullPath);
                $valid = $true;
            }
            catch {
            }
 
            Add-Member -InputObject $o -NotePropertyName "Path" -NotePropertyValue $fullPath;
            Add-Member -InputObject $o -NotePropertyName "Valid" -NotePropertyValue $valid;
            Add-Member -InputObject $o -NotePropertyName "XmlObject" -NotePropertyValue $xml;
 
            $Result += $o;
        }
        return , $Result;
    }
}
 
function __gettrxsummary {
    param(
        [String] $trxDir
    )
    Process {
        $Result = @();
 
        $trxFiles = __getxmlfiles $trxDir "*.trx"
 
        foreach ($o in $trxFiles) {
            $o2 = New-Object "PSObject";
 
            # Map across original properties
            #Add-Member -InputObject $o2 -NotePropertyName "Path" -NotePropertyValue $o.Path;
            #Add-Member -InputObject $o2 -NotePropertyName "Valid" -NotePropertyValue $o.Valid;
            #Add-Member -InputObject $o2 -NotePropertyName "XmlObject" -NotePropertyValue $o.XmlObject;
            #Add-Member -InputObject $o2 -NotePropertyName "Exists" -NotePropertyValue $o.Exists;
           
            if ($o.Exists -and $o.Valid -and ($null -ne $o.XmlObject)) {
                $x = $o.XmlObject;
 
                $outcome = $x.TestRun.ResultSummary.Outcome;
                $allAssemblies = $x.TestRun.TestDefinitions.UnitTest.TestMethod.codeBase;
                $assemblyName = "unknown"
                $fileName = Split-Path $o.Path -leaf
                $fileName = "..." + $fileName.substring($fileName.length - 9)
                if ($allAssemblies) {
                    if ($allAssemblies -is [array]) {
                        $testCategoryArr = $allAssemblies[0];
                    }
                    else {
                        $testCategoryArr = $allAssemblies;
                    }
                    $assemblyName = Split-Path $testCategoryArr -leaf 
                    $assemblyName = $assemblyName -replace "Microsoft.CognitiveServices.Orchestration", ".."
                }
                else {
                    continue;
                }
 
                # Outcome 
                Add-Member -InputObject $o2 -NotePropertyName "Outcome" -NotePropertyValue "$outcome";
 
                # Counters
                Add-Member -InputObject $o2 -NotePropertyName "Total"               -NotePropertyValue $x.TestRun.ResultSummary.Counters.Total
                Add-Member -InputObject $o2 -NotePropertyName "Executed"            -NotePropertyValue $x.TestRun.ResultSummary.Counters.Executed
                Add-Member -InputObject $o2 -NotePropertyName "Passed"              -NotePropertyValue $x.TestRun.ResultSummary.Counters.Passed
                Add-Member -InputObject $o2 -NotePropertyName "Error"               -NotePropertyValue $x.TestRun.ResultSummary.Counters.Error
                Add-Member -InputObject $o2 -NotePropertyName "Failed"              -NotePropertyValue $x.TestRun.ResultSummary.Counters.Failed
                Add-Member -InputObject $o2 -NotePropertyName "Timeout"             -NotePropertyValue $x.TestRun.ResultSummary.Counters.Timeout
                Add-Member -InputObject $o2 -NotePropertyName "Aborted"             -NotePropertyValue $x.TestRun.ResultSummary.Counters.Aborted
                Add-Member -InputObject $o2 -NotePropertyName "Inconclusive"        -NotePropertyValue $x.TestRun.ResultSummary.Counters.Inconclusive
                Add-Member -InputObject $o2 -NotePropertyName "PassedButRunAborted" -NotePropertyValue $x.TestRun.ResultSummary.Counters.PassedButRunAborted
                Add-Member -InputObject $o2 -NotePropertyName "NotRunnable"         -NotePropertyValue $x.TestRun.ResultSummary.Counters.NotRunnable
                Add-Member -InputObject $o2 -NotePropertyName "NotExecuted"         -NotePropertyValue $x.TestRun.ResultSummary.Counters.NotExecuted
                Add-Member -InputObject $o2 -NotePropertyName "Disconnected"        -NotePropertyValue $x.TestRun.ResultSummary.Counters.Disconnected
                Add-Member -InputObject $o2 -NotePropertyName "Warning"             -NotePropertyValue $x.TestRun.ResultSummary.Counters.Warning
                Add-Member -InputObject $o2 -NotePropertyName "Completed"           -NotePropertyValue $x.TestRun.ResultSummary.Counters.Completed
                Add-Member -InputObject $o2 -NotePropertyName "InProgress"          -NotePropertyValue $x.TestRun.ResultSummary.Counters.InProgress
                Add-Member -InputObject $o2 -NotePropertyName "Assembly"            -NotePropertyValue "$assemblyName"
                Add-Member -InputObject $o2 -NotePropertyName "ResultFile"            -NotePropertyValue "$fileName"
 
                $Result += $o2;
            }
        }
        return , $Result;
    }
}
 
function __gettrxtestresults {
    param(
        [String] $trxDir,
        [bool]$includePassed = $false,
        [bool]$includeNotRun = $false
    )
    Process {
        $Result = @();
 
        $trxFiles = __getxmlfiles $trxDir "*.trx"
 
        foreach ($o in $trxFiles) {
            if ($o.Exists -and $o.Valid -and ($null -ne $o.XmlObject)) {
                $x = $o.XmlObject;
 
                $results = $x.TestRun.Results.UnitTestResult;
                foreach ($testresult in $results) {
                    $o2 = New-Object "PSObject";
                    $outcome = $testresult.outcome;
                    $testName = $testresult.testName;
                                                                        
                    if ($outcome -eq "Passed" -and $includePassed -eq $false) {
                        continue;
                    }
                                                                        
                    if ($outcome -eq "NotExecuted" -and $includeNotRun -eq $false) {
                        continue;
                    }
                                                                        
                    Add-Member -InputObject $o2 -NotePropertyName "Outcome" -NotePropertyValue "$outcome";
                    Add-Member -InputObject $o2 -NotePropertyName "Test" -NotePropertyValue "$testName" ;
                                                          
                    $Result += $o2;
                }
            }
        }
        return , $Result;
    }
}
 
function __getcodecoveragesummary {
    param(
        [String] $trxDir
    )
    Process {
        $Result = @();
 
        $trxFiles = __getxmlfiles $trxDir "coverage.cobertura.xml" $true
        $alreadyProcessed = @('NA');
 
        foreach ($o in $trxFiles) {
            $o2 = New-Object "PSObject";
 
            if ($o.Exists -and $o.Valid -and ($null -ne $o.XmlObject)) {
                $x = $o.XmlObject;
                $fileName = Split-Path $o.Path -leaf
 
                if ($alreadyProcessed -contains $x.coverage.sources.source)
                {
                    continue;
                }
 
                $coverageRate = [double]$x.coverage.'line-rate' * 100
 
                # Counters
                Add-Member -InputObject $o2 -NotePropertyName "TotalLines"          -NotePropertyValue $x.coverage.'lines-valid'
                Add-Member -InputObject $o2 -NotePropertyName "LinesCovered"        -NotePropertyValue $x.coverage.'lines-covered'
                Add-Member -InputObject $o2 -NotePropertyName "LineCoverageRate"    -NotePropertyValue $coverageRate
                Add-Member -InputObject $o2 -NotePropertyName "Source"              -NotePropertyValue $x.coverage.sources.source;
                Add-Member -InputObject $o2 -NotePropertyName "ResultFile"          -NotePropertyValue "$fileName"
 
                $alreadyProcessed += $x.coverage.sources.source;
 
                $Result += $o2;
            }
        }
        return , $Result;
    }
}
 
function runtests {
    param(
        [String] $filter
    )
    Process {
        $slnDir = "${env:ROOT}\";
        $trxDir = "${env:ROOT}\.trx";
        $sln = "${env:SLN_NAME}";
                             
        Push-Location $slnDir
        try {
            if ([System.IO.Directory]::Exists($trxDir)) {
                Get-ChildItem -Path $trxDir -Recurse | Remove-Item -force -recurse
            }
                             
            __executetest $sln $trxDir $filter
            $exitCode = $LASTEXITCODE
                                           
            __gettrxsummary $trxDir | Format-Table -Property Assembly, ResultFile, Total, Executed, Passed, Error, Failed, Timeout, Aborted -Autosize
                                           
            __printTestResult $exitCode $filter
                                           
            __gettrxtestresults $trxDir $true $true | Format-Table -Property Outcome, Test -Autosize
        }
        finally {
            Pop-Location
        }
    }
}
 
function rerunfailedtests {
    Process {
        $slnDir = "${env:ROOT}\";
        $trxDir = "${env:ROOT}\.trx";
        $sln = "${env:SLN_NAME}";
                             
        Push-Location $slnDir
        try {
            Write-Host "Gathering failed tests from directory '$trxDir'" -ForegroundColor Yellow
            $failedTests = __gettrxtestresults $trxDir $false $false
                                           
            $failedTests | Format-Table -Property Outcome, Test -Autosize
            
            Write-Host "Cleaning test results directory '$trxDir'" -ForegroundColor Yellow
            if ([System.IO.Directory]::Exists($trxDir)) {
                Get-ChildItem -Path $trxDir -Recurse | Remove-Item -force -recurse
            }
                                           
            foreach ($o in $failedTests) {
                if ($o.Outcome -eq "Failed") {
                    $testName = $o.Test;
                    Write-Host "Rerunning '$testName'" -ForegroundColor Yellow
                    __executetest $sln $trxDir $testName
                }
            }
                                           
            Write-Host "Summary" -ForegroundColor Yellow
            Write-Host "-------"
            __gettrxsummary $trxDir | Format-Table -Property Assembly, ResultFile, Executed, Passed, Error, Failed, Timeout, Aborted -Autosize
                             
            Write-Host ""
            __gettrxtestresults $trxDir $true $false | Format-Table -Property Outcome, Test -Autosize
        }
        finally {
            Pop-Location
        }
    }
}
 
function validate {
    $slnDir = "${env:ROOT}\";
    $trxDir = "${env:ROOT}\.trx";
    $sln = "${env:SLN_NAME}";
              
    Push-Location $slnDir
    try {
        if ([System.IO.Directory]::Exists($trxDir)) {
            Get-ChildItem -Path $trxDir -Recurse | Remove-Item -force -recurse
        }
 
        __executetest $sln $trxDir "UnitTests"
        $unitTestExitCode = $LASTEXITCODE
                             
        Write-Output ""
        Write-Host "Code Coverage Results" -ForegroundColor Cyan
        __getcodecoveragesummary $trxDir  | Format-Table -Property Source, TotalLines, LinesCovered, LineCoverageRate
 
        Write-Output ""
        Write-Host "Test Results" -ForegroundColor Cyan
        __gettrxsummary $trxDir | Format-Table -Property Assembly, ResultFile, Total, Executed, Passed, Error, Failed, Timeout, Aborted -Autosize
 
        Write-Output ""
        __printTestResult $unitTestExitCode "Unit Tests"
                             
        Write-Output ""
        Write-Host "Failed Tests" -ForegroundColor Red
        __gettrxtestresults $trxDir $false $false | Format-Table -Property Outcome, Test -Autosize
    }
    finally {
        Pop-Location
    }
}
