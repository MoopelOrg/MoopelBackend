#Add all test failures as issues
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)]
    [string]$ReportPath,

    [string]$Repository,

    [string[]]$Label = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:IssueTitlePrefix = 'Test failure: '
$script:MaximumFailureDetailsLength = 10000
$script:TestFailureLabel = 'test-failure'
$script:TestFailureLabelColor = 'D73A4A'
$script:TestFailureLabelDescription = 'An automated test is failing.'
$script:ControllerLabelPrefix = 'controller:'
$script:ControllerLabelColor = '1D76DB'
$script:ControllerLabelDescriptionPrefix = 'Failures in the '
$script:ControllerLabelDescriptionSuffix = ' controller tests.'
$script:FailureNodeQuery = "*[local-name()='failure' or local-name()='error']"
$script:FailedTestCaseQuery = "//*[local-name()='testcase'][*[local-name()='failure'] or *[local-name()='error']]"
$script:FailedTrxResultQuery = "//*[local-name()='UnitTestResult' and @outcome='Failed']"
$script:TrxErrorInfoQuery = "./*[local-name()='Output']/*[local-name()='ErrorInfo']"

function ConvertTo-WindowsCommandLineArgument {
    param([Parameter(Mandatory)][string]$Argument)

    if ($Argument.Length -eq 0) {
        return '""'
    }

    if ($Argument -notmatch '[\s"]') {
        return $Argument
    }

    $escapedArgument = [regex]::Replace($Argument, '(\\*)"', '$1$1\"')
    $escapedArgument = [regex]::Replace($escapedArgument, '(\\+)$', '$1$1')
    return "`"$escapedArgument`""
}

function Invoke-GitHubCli {
    param([Parameter(Mandatory)][string[]]$Arguments)

    $processStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $processStartInfo.FileName = 'gh'
    $processStartInfo.Arguments = ($Arguments | ForEach-Object {
        ConvertTo-WindowsCommandLineArgument -Argument $_
    }) -join ' '
    $processStartInfo.UseShellExecute = $false
    $processStartInfo.RedirectStandardOutput = $true
    $processStartInfo.RedirectStandardError = $true

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $processStartInfo
    [void]$process.Start()
    $standardOutput = $process.StandardOutput.ReadToEnd()
    $standardError = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "GitHub CLI command failed: $standardError"
    }

    return $standardOutput.Trim()
}

function Get-TestMethodIdentifier {
    param([Parameter(Mandatory)][string]$TestIdentifier)

    return ($TestIdentifier -replace '\s*\(.*$', '').Trim()
}

function Get-ControllerLabel {
    param([Parameter(Mandatory)][string]$TestIdentifier)

    $controllerMatch = [regex]::Match(
        $TestIdentifier,
        '(?:^|\.)(?<controller>[A-Za-z0-9]+)ControllerTests(?:\.|$)')
    if (-not $controllerMatch.Success) {
        return $null
    }

    $controllerName = $controllerMatch.Groups['controller'].Value
    $labelSuffix = [regex]::Replace($controllerName, '(?<=[a-z0-9])([A-Z])', '-$1').
        ToLowerInvariant()
    return "$script:ControllerLabelPrefix$labelSuffix"
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'The GitHub CLI (gh) must be installed and authenticated before creating issues.'
}

if (-not (Test-Path -LiteralPath $ReportPath -PathType Leaf)) {
    throw "Test report not found at '$ReportPath'. Generate a TRX report with 'dotnet test .\ApiTests\ApiTests.csproj --logger `"trx;LogFileName=test-results.trx`" --results-directory .\TestResults' and pass '.\TestResults\test-results.trx' to this script."
}

$resolvedReportPath = (Resolve-Path -LiteralPath $ReportPath).Path
[xml]$report = Get-Content -LiteralPath $resolvedReportPath -Raw
$failedTests = @(
    if ($report.DocumentElement.LocalName -eq 'TestRun') {
        $report.SelectNodes($script:FailedTrxResultQuery) | ForEach-Object {
            $errorInfo = $_.SelectSingleNode($script:TrxErrorInfoQuery)
            [PSCustomObject]@{
                Identifier = $_.GetAttribute('testName')
                FailureDetails = if ($null -eq $errorInfo) {
                    $_.InnerText.Trim()
                }
                else {
                    $errorInfo.InnerText.Trim()
                }
            }
        }
    }
    else {
        $report.SelectNodes($script:FailedTestCaseQuery) | ForEach-Object {
            $className = $_.GetAttribute('classname')
            $testName = $_.GetAttribute('name')
            $testIdentifier = if ([string]::IsNullOrWhiteSpace($className)) {
                $testName
            }
            else {
                "$className.$testName"
            }

            $failureDetails = ($_.SelectNodes($script:FailureNodeQuery) | ForEach-Object {
                $message = $_.GetAttribute('message')
                $type = $_.GetAttribute('type')
                @(
                    if (-not [string]::IsNullOrWhiteSpace($type)) { "Type: $type" }
                    if (-not [string]::IsNullOrWhiteSpace($message)) { "Message: $message" }
                    $_.InnerText.Trim()
                ) -join [Environment]::NewLine
            }) -join ([Environment]::NewLine + [Environment]::NewLine)

            [PSCustomObject]@{
                Identifier = $testIdentifier
                FailureDetails = $failureDetails
            }
        }
    }
)

if ($null -eq $failedTests -or $failedTests.Count -eq 0) {
    Write-Host 'No failed test cases were found in the test report.'
    return
}

$labelListArguments = @('label', 'list', '--limit', '1000', '--json', 'name', '--jq', '.[].name')
if (-not [string]::IsNullOrWhiteSpace($Repository)) {
    $labelListArguments += @('--repo', $Repository)
}

$knownLabels = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)
$labelListOutput = Invoke-GitHubCli -Arguments $labelListArguments
($labelListOutput -split [Environment]::NewLine) | ForEach-Object {
    if (-not [string]::IsNullOrWhiteSpace($_)) {
        [void]$knownLabels.Add($_)
    }
}

if ($Label -contains $script:TestFailureLabel -and $knownLabels.Add($script:TestFailureLabel)) {
    $testFailureLabelArguments = @(
        'label',
        'create',
        $script:TestFailureLabel,
        '--color',
        $script:TestFailureLabelColor,
        '--description',
        $script:TestFailureLabelDescription
    )
    if (-not [string]::IsNullOrWhiteSpace($Repository)) {
        $testFailureLabelArguments += @('--repo', $Repository)
    }

    if ($PSCmdlet.ShouldProcess($script:TestFailureLabel, 'Create GitHub label')) {
        Invoke-GitHubCli -Arguments $testFailureLabelArguments
    }
}

$processedTestIdentifiers = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)

foreach ($failedTest in $failedTests) {
    $testIdentifier = Get-TestMethodIdentifier -TestIdentifier $failedTest.Identifier
    if ([string]::IsNullOrWhiteSpace($testIdentifier)) {
        Write-Warning 'Skipping a failed test case without a name attribute.'
        continue
    }

    if (-not $processedTestIdentifiers.Add($testIdentifier)) {
        continue
    }

    $controllerLabel = Get-ControllerLabel -TestIdentifier $testIdentifier
    if ($null -ne $controllerLabel -and $knownLabels.Add($controllerLabel)) {
        $controllerLabelArguments = @(
            'label',
            'create',
            $controllerLabel,
            '--color',
            $script:ControllerLabelColor,
            '--description',
            "$script:ControllerLabelDescriptionPrefix$controllerLabel$script:ControllerLabelDescriptionSuffix"
        )
        if (-not [string]::IsNullOrWhiteSpace($Repository)) {
            $controllerLabelArguments += @('--repo', $Repository)
        }

        if ($PSCmdlet.ShouldProcess($controllerLabel, 'Create GitHub label')) {
            Invoke-GitHubCli -Arguments $controllerLabelArguments
        }
    }

    $issueTitle = "$script:IssueTitlePrefix$testIdentifier"
    $searchArguments = @(
        'issue',
        'list',
        '--state',
        'open',
        '--search',
        "in:title $issueTitle",
        '--json',
        'title',
        '--jq',
        '.[].title'
    )
    if (-not [string]::IsNullOrWhiteSpace($Repository)) {
        $searchArguments += @('--repo', $Repository)
    }

    $existingIssueTitles = Invoke-GitHubCli -Arguments $searchArguments

    if (($existingIssueTitles -split [Environment]::NewLine) -contains $issueTitle) {
        Write-Host "Skipping '$testIdentifier' because an open issue already exists."
        continue
    }

    $failureDetails = $failedTest.FailureDetails

    if ($failureDetails.Length -gt $script:MaximumFailureDetailsLength) {
        $failureDetails = $failureDetails.Substring(0, $script:MaximumFailureDetailsLength)
    }

    $issueBody = @"
## Failed test

`$testIdentifier`

## Test report

`$resolvedReportPath`

## Failure details

```text
$failureDetails
```
"@

    $issueLabels = @($Label)
    if ($null -ne $controllerLabel) {
        $issueLabels += $controllerLabel
    }
    $issueLabels = @($issueLabels | Select-Object -Unique)

    $createArguments = @('issue', 'create', '--title', $issueTitle, '--body', $issueBody)
    if (-not [string]::IsNullOrWhiteSpace($Repository)) {
        $createArguments += @('--repo', $Repository)
    }

    foreach ($issueLabel in $issueLabels) {
        $createArguments += @('--label', $issueLabel)
    }

    if ($PSCmdlet.ShouldProcess($testIdentifier, 'Create GitHub issue')) {
        Invoke-GitHubCli -Arguments $createArguments
    }
}
