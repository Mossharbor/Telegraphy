[CmdletBinding()] # allows passing standard PS parameters such as Verbose etc.
 
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:TARGET_ENVIRONMENT_STAGE = "dev" # tests are be pointed to local service instance
$env:TARGET_ENVIRONMENT_REGION = "westus" # tests are be pointed to local service instance
$env:ROOT = Split-Path $MyInvocation.MyCommand.Path
$env:SLN_NAME = "Telegraphy.sln"
$env:SLN_RUNSETTINGS_NAME = "Telegraphy.runsettings"
$env:NUGETVERSIONPROPSPATH = $env:Root
$env:NUGETVERSIONPROPSPATH += "\.build\dependency.version.props"
$env:VERSIONFILE = ${env:ROOT}
$env:VERSIONFILE +=  "\Telegraphy.Net\Package.nuspec"
$testConfigFile = "${env:ROOT}\UnitTests\app.config"
 
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
Write-Host "";
Write-Host "Commands:";
Write-Host "   opensln";
Write-Host "   runtests";
Write-Host "   azsetuptestenv";
Write-Host "   rerunfailedtests"; 
Write-Host "   validate";
Write-Host "   verifypackagereferences";
Write-Host "   fixpackagereferences";
Write-Host "   listunusedpackagereferences";
Write-Host "   getpackageversion";
Write-Host "   updatepackageversion";
Write-Host "   publishnugetpackages";
 
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

function setuptestconfig{

	
    $testconfigContents =@"
    <?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Azure.Amqp" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-2.2.0.0" newVersion="2.2.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-10.0.0.0" newVersion="10.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.Serialization.Primitives" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.1.2.0" newVersion="4.1.2.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.WindowsAzure.Storage" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-9.1.1.0" newVersion="9.1.1.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
  <appSettings>
    <!-- NOTE these are set by the init script running the azsetuptestenv <resource group name> command in powershell -->
    <add key="EventHubConnectionString" value="%EventHubConnectionString%" />
    <add key="StorageConnectionString" value="%StorageConnectionString%" />
    <add key="ServiceBusConnectionString" value="%ServiceBusConnectionString%" />
    <add key="RelayConnectionString" value="%RelayConnectionString%" />
    <add key="WcfRelayConnectionString" value="%WcfRelayConnectionString%" />
    <add key="EmailAccount" value="%EmailAccount%" />
    <add key="EmailAccountPassword" value="%EmailAccountPassword%" />
  </appSettings>
</configuration>
"@

    if (-not(Test-Path -Path $testConfigFile -PathType Leaf)) {
        #
        #  Setup Test Config
        #
        Set-Content -path $testConfigFile -Value $testconfigContents
    }

}

function azsetuptestenv {
	param(
	    [Parameter(Mandatory = $true)]
        [String] $resourceGroup
    )

	$location = "East US"
    $sblocation = "eastus"
    $userName = $env:USERNAME.substring(0, [System.Math]::Min(7, $env:USERNAME.Length)).ToLower()
	$eventHubName = ($userName+$resourceGroup+"eventhub").ToLower();
    $eventHubName = $eventHubName.substring(0, [System.Math]::Min(24, $eventHubName.Length))
	$storageAccountName = ($userName+$resourceGroup+"storage").ToLower();
    $storageAccountName = $storageAccountName.substring(0, [System.Math]::Min(24, $storageAccountName.Length))
	$serviceBusNamespaceName = ($userName+$resourceGroup+"servicebus").ToLower();
    $serviceBusNamespaceName = $serviceBusNamespaceName.substring(0, [System.Math]::Min(24, $serviceBusNamespaceName.Length))
	$serviceBusQueueName = ($userName+$resourceGroup+"servicebusqueue").ToLower();
    $serviceBusQueueName = $serviceBusQueueName.substring(0, [System.Math]::Min(24, $serviceBusQueueName.Length))
	$relayNamespaceName = ($userName+$resourceGroup+"relay").ToLower();
    $relayNamespaceName = $relayNamespaceName.substring(0, [System.Math]::Min(24, $relayNamespaceName.Length))
	$wcfRelayNamespaceName = ($userName+$resourceGroup+"wcfrelay").ToLower();
    $wcfRelayNamespaceName = $wcfRelayNamespaceName.substring(0, [System.Math]::Min(24, $wcfRelayNamespaceName.Length))
	$resourceGroupName =($userName+"-"+$resourceGroup).ToLower()
    $resourceGroupName = $resourceGroupName.substring(0, [System.Math]::Min(24, $resourceGroupName.Length))
	 

	New-AzResourceGroup -Name $resourceGroupName -Location $location -ErrorAction Stop
	
    #
    #  Setup Storage Testing Account
    #
	#DefaultEndpointsProtocol=[http|https];AccountName=myAccountName;AccountKey=myAccountKey
	Write-Host "Creating resources for azure storage testing"
	New-AzStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName -Location $location -SkuName Standard_RAGRS -Kind StorageV2 -ErrorAction Stop
    $sa = Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -AccountName $storageAccountName
	$saKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName)[0].Value 
	$storageConnectionString='DefaultEndpointsProtocol=https;AccountName=' + $storageAccountName + ';AccountKey=' + $saKey + ';EndpointSuffix=core.windows.net'

    $storageContext = New-AzStorageContext -ConnectionString $storageConnectionString
    New-AzStorageContainer -Name "telegraphytest" -Context $storageContext
    New-AzStorageContainer -Name "telegraphytesteventhub" -Context $storageContext
    $storageAccount = Get-AzStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName	

    $testConfigFileContent = (Get-Content -path $testConfigFile -Raw) -replace '%StorageConnectionString%', $storageConnectionString
	
    #
    #  Setup EventHub Testing Account
    #
	New-AzEventHubNamespace -ResourceGroupName $resourceGroupName -NamespaceName $eventHubName -Location $location -ErrorAction Stop
	$createdEventHub = New-AzEventHub -ResourceGroupName $resourceGroupName -NamespaceName $eventHubName -EventHubName $eventHubName -MessageRetentionInDays 3 -PartitionCount 1

    #$createdEventHub = Get-AzEventHub -ResourceGroupName $resourceGroupName -Namespace $eventHubName -Name $eventHubName
    $createdEventHub.CaptureDescription = New-Object -TypeName Microsoft.Azure.Commands.EventHub.Models.PSCaptureDescriptionAttributes
    $createdEventHub.CaptureDescription.Enabled = $true
    $createdEventHub.CaptureDescription.IntervalInSeconds  = 120
    $createdEventHub.CaptureDescription.Encoding  = "Avro"
    $createdEventHub.CaptureDescription.SizeLimitInBytes = 10485763
    $createdEventHub.CaptureDescription.Destination.Name = "EventHubArchive.AzureBlockBlob"
    $createdEventHub.CaptureDescription.Destination.BlobContainer = "telegraphytesteventhub"
    $createdEventHub.CaptureDescription.Destination.ArchiveNameFormat = "{Namespace}/{EventHub}/{PartitionId}/{Year}/{Month}/{Day}/{Hour}/{Minute}/{Second}"
    $createdEventHub.CaptureDescription.Destination.StorageAccountResourceId = $storageAccount.Id
    Set-AzEventHub -ResourceGroupName $resourceGroupName -Namespace $eventHubName -Name $eventHubName -InputObject $createdEventHub
	$evKey = Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $eventHubName -AuthorizationRuleName RootManageSharedAccessKey

    $testConfigFileContent = $testConfigFileContent -replace '%EventHubConnectionString%', $evKey.PrimaryConnectionString

    #
    #  Setup ServiceHub Testing Account
    #
    New-AzServiceBusNamespace -ResourceGroupName $resourceGroupName -Name $serviceBusNamespaceName -Location $location
    $sbKey = Get-AzServiceBusKey -ResourceGroupName $resourceGroupName -Namespace $serviceBusNamespaceName -Name RootManageSharedAccessKey

    $testConfigFileContent = $testConfigFileContent -replace '%ServiceBusConnectionString%', $sbKey.PrimaryConnectionString

    #
    #  Setup Relay Testing Account
    #
    New-AzRelayNamespace -ResourceGroupName $resourceGroupName -Name $relayNamespaceName -Location $location
    $relayKey = Get-AzRelayKey -ResourceGroupName $resourceGroupName -Namespace $relayNamespaceName -Name RootManageSharedAccessKey

    $testConfigFileContent = $testConfigFileContent -replace '%RelayConnectionString%', $relayKey.PrimaryConnectionString

    New-AzWcfRelay -ResourceGroupName $resourceGroupName -Namespace $relayNamespaceName -Name "TestWCFRelay" -WcfRelayType "Http"
    $wcfRelyKey = Get-AzRelayKey -ResourceGroupName $resourceGroupName -Namespace $relayNamespaceName -Name RootManageSharedAccessKey

    $testConfigFileContent = $testConfigFileContent -replace '%WcfRelayConnectionString%', $wcfRelayKey.PrimaryConnectionString

    $testConfigFileContent

	# TODO create the telegraphytest container
	# todo create table telegraphytesttable
	
	# New-AzServiceBusNamespace -ResourceGroupName $resourceGroupName -Name $serviceBusNamespaceName -Location $location
	# New-AzServiceBusQueue -ResourceGroupName $resourceGroupName  -NamespaceName $serviceBusNamespaceName -Name $serviceBusQueueName 
	# $sbKey = Get-AzServiceBusKey -ResourceGroup $resourceGroupName -Namespace $serviceBusNamespaceName -Name $serviceBusQueueName
	
	Write-Host $storageConnectionString
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

function VerifyPackageReferences
{
	$VersionPropsPath = ${env:NUGETVERSIONPROPSPATH};
    $RootPath = (Resolve-Path -Path ${env:ROOT}).Path
    Invoke-PrintMessage "Checking PackageReferences"

    $count = 0
    Get-ChildItem -Path $RootPath -Recurse -Include *.csproj | ForEach-Object {
        $project = $_
        
        Write-Host
        Write-Host "Checking" $project.Name

        # get content of each project into an object 
        [xml]$data = Get-Content -Path $project.FullName

        # Issue 1: ensure that packages.config is not being used by the given project
        if ((Test-Path ($project.DirectoryName + "\packages.config")) -eq 1)
        {   
            $count++     
            Write-Host "##vso[task.logissue type=error] Found 'packages.config'!"
            Write-Error "**** Please convert project to use <PackageReferences>!" 
        }

        # Issue 3: ensure projects do not use hard-coded dependencies (versions) and match a format of '$(*Version)' from 'dependency.version.props'
        $data.Project.ItemGroup.PackageReference | Where-Object { $_ -ne $null } | ForEach-Object {
            $reference = $_

            if ($reference.Version -notmatch "\$\([A-Za-z0-9-_]+Version\)")
            {
                $count++ 
                Write-Host "##vso[task.logissue type=error] Found project using hard-coded or malformed version dependency macro '$($reference.Version)'!"
                Write-Error "**** Please update dependency to use a version from 'dependency.version.props' and ensure that is has the following format '`$`(xxxVersion`)'!"
            }
        }
    }

    return $count
}

#
# Invoke-FixPackageReferences
#
function FixPackageReferences
{
    Invoke-PrintMessage "Checking PackageReferences for Hard-coded Versions"

	$VersionPropsPath = ${env:NUGETVERSIONPROPSPATH};
    $versionPropsFile = (Resolve-Path -Path $VersionPropsPath).Path
    $RootPath = (Resolve-Path -Path ${env:ROOT}).Path
    Write-Host $versionPropsFile
    Write-Host $
    [xml]$dependencyVersionPropsXml = Get-Content -Path $versionPropsFile

    Get-ChildItem -Path $RootPath -Recurse -Include *.csproj | ForEach-Object {
        $project = $_

        Write-Host "Checking" $project.Name

        # get content of each project into an object 
        [xml]$data = Get-Content -Path $project.FullName

        # Ensure projects do not use hard-coded dependencies (versions) and match a format of '$(*Version)' from 'dependency.version.props'
        $updateProject = $false
        $data.Project.ItemGroup.PackageReference | Where-Object { $_ -ne $null } | ForEach-Object {
            $reference = $_
            $id = $reference.Include

            $versionString = $id.Replace('.','').Trim() + 'Version'
            $version = $reference.Version
            if ($version -notmatch "\$\([A-Za-z0-9-_]+Version\)")
            {
                Write-Host "$id does not have version set with the correct format."

                $updateProject = $true
                $updatePackageRefEntry = $false

                $reference.Version = '$(' + $versionString + ')'
                $dependencyVersionPropsXml.GetElementsByTagName($versionString) | Where-Object { $_ -ne $null } | ForEach-Object {
                    $updatePackageRefEntry = $true
                    $_.InnerXml = $version
                }

                if (-not $updatePackageRefEntry)
                {
                    $element = $dependencyVersionPropsXml.CreateElement($versionString)
                    $element.InnerXml = $version
                    $dependencyVersionPropsXml.Project.FirstChild.AppendChild($element)
                    $updatePackageRefEntry = $true
                }
            }
        }

        if ($updateProject -eq $true)
        {
            $data.Save($project.FullName)
            $dependencyVersionPropsXml.Save($versionPropsFile)
        }
    }

    # Remove any unused references
    $unusedPackages = ListUnusedPackageReferences -RootPath $RootPath -VersionPropsPath $versionPropsFile
    $unusedPackages | Where-Object { $_ -ne $null } | ForEach-Object {
        $nodeToRemove = $dependencyVersionPropsXml.GetElementsByTagName($_)[0]
        Write-Host "Removing unused package " $nodeToRemove.Name
        $dependencyVersionPropsXml.Project.FirstChild.RemoveChild($nodeToRemove)    
    }

    # Sort the elements in dependency.version.props file
    $propertyGroup = $dependencyVersionPropsXml.Project.FirstChild
    $packageCollection = $propertyGroup.ChildNodes | Sort-Object Name
    $propertyGroup.RemoveAll()

    $packageCollection | ForEach-Object { $propertyGroup.AppendChild($_) } | Out-Null
    $dependencyVersionPropsXml.Save($versionPropsFile)
}

function ListUnusedPackageReferences
{
	$VersionPropsPath = ${env:NUGETVERSIONPROPSPATH};
    Invoke-PrintMessage "Checking for Unused PackageReferences"

    #list of unused packages to return
    $unusedPackages = @()

    # collection of central packages
    $versionPropsFile = (Resolve-Path -Path $VersionPropsPath).Path
    [xml]$data0 = Get-Content -Path $versionPropsFile

    # collection of csproj projects from root path
    $projects = Get-ChildItem -Path ${env:ROOT} -Recurse -Include *.csproj

    # search projects existence of each central package
    $data0.Project.FirstChild.ChildNodes | Where-Object { $_ -ne $null } | ForEach-Object {
        $package = "`$(" + $_.Name + ")"

        $found = $false
        foreach ($project in $projects)
        {
            [xml]$data1 = Get-Content -Path $project.FullName

            $references = $data1.Project.ItemGroup.PackageReference | Where-Object { $_ -ne $null }
            foreach ($reference in $references)
            {
                if ($package -eq $reference.Version)
                {
                    $found = $true
                    break
                }
            }

            if ($found -eq $true)
            {
                break
            }
        }

        if ($found -eq $false)
        {
            Write-Warning "The package '$package' is not used!"
            $unusedPackages += $_.Name
        }
    }

    return $unusedPackages
}

function Invoke-PrintMessage
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    Write-Host | Out-Default
    Write-Host "*****" | Out-Default
    Write-Host "***** $Message" | Out-Default
    Write-Host "*****" | Out-Default
    Write-Host | Out-Default
}

function GetPackageVersion
{
	[xml]$data0 = Get-Content -Path $env:VERSIONFILE
	$version = $data0.package.metadata.version;
	Write-Host $version
}

function UpdatePackageVersion
{
	[CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $VersionString
    )
	[xml]$data0 = Get-Content -Path $env:VERSIONFILE
	$oldversion = $data0.package.metadata.version;
	$oldVersionString = "<version>"+$oldversion+"</version>"
	$newVersionSting = "<version>"+$VersionString+"</version>"
    $nuspecFiles = Get-ChildItem $env:Root *.nuspec -Recurse
    foreach($nuspecFile in $nuspecFiles)
    {
        (Get-Content $nuspecFile.PSPath) | 
        Foreach-Object { $_ -Replace $oldVersionString, $newVersionSting} |
        Set-Content $nuspecFile.PSPath
    }
}

function PublishNugetPackages
{
	[CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $nugetApiKey
    )
	
	[xml]$data0 = Get-Content -Path $env:VERSIONFILE
	$currentVersion = $data0.package.metadata.version;
	
    $nuspecFiles = Get-ChildItem $env:Root -recurse ("*"+$currentVersion + ".nupkg")
	foreach($nupkgFile in $nuspecFiles)
    {
		$localPath = Convert-Path $nupkgFile.PSPath
		Write-Host $localPath
		dotnet nuget push $localPath --api-key $nugetApiKey --source https://api.nuget.org/v3/index.json
	}
}

setuptestconfig
