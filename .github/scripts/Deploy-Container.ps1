[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ContainerName,

    [Parameter(Mandatory = $true)]
    [string]$ImageName,

    [Parameter(Mandatory = $true)]
    [string]$ImageTag,

    [Parameter(Mandatory = $true)]
    [string]$HostPort,

    [Parameter(Mandatory = $true)]
    [string]$ContainerPort,

    [Parameter(Mandatory = $true)]
    [string]$EnvironmentName,

    [Parameter(Mandatory = $false)]
    [string]$ContainerEnvFilePath = '',

    [Parameter(Mandatory = $false)]
    [string]$LegacyContainerName = '',

    [Parameter(Mandatory = $false)]
    [int]$HealthcheckRetries = 12,

    [Parameter(Mandatory = $false)]
    [int]$HealthcheckDelaySeconds = 10
)

$ErrorActionPreference = 'Stop'

function Invoke-DockerCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $false)]
        [switch]$IgnoreExitCode
    )

    & docker @Arguments

    if (-not $IgnoreExitCode -and $LASTEXITCODE -ne 0) {
        throw "Docker command failed: docker $($Arguments -join ' ')"
    }
}

function Remove-ContainerIfPresent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return
    }

    $existingContainer = @(docker container ls -a --filter "name=^${Name}$" --format '{{.Names}}')
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to query Docker container state for '$Name'."
    }

    if ($existingContainer | Where-Object { $_ -eq $Name }) {
        Invoke-DockerCommand -Arguments @('rm', '-f', $Name) -IgnoreExitCode
    }
}

$imageReference = "$ImageName`:$ImageTag"
$healthUrl = "http://127.0.0.1:$HostPort/"
$deployedAt = (Get-Date).ToUniversalTime().ToString('o')

Invoke-DockerCommand -Arguments @('image', 'inspect', $imageReference) -IgnoreExitCode
if ($LASTEXITCODE -ne 0) {
    throw "Expected deployment image '$imageReference' is not loaded on the runner."
}

Remove-ContainerIfPresent -Name $LegacyContainerName
Remove-ContainerIfPresent -Name $ContainerName

$dockerRunArguments = @(
    'run',
    '-d',
    '--name', $ContainerName,
    '--restart', 'unless-stopped',
    '-p', "$HostPort`:$ContainerPort",
    '-e', "ASPNETCORE_ENVIRONMENT=$EnvironmentName",
    '-e', "VERSION=$ImageTag",
    '-e', "COMMIT_NAME=$ImageTag",
    '-e', "DEPLOYED_AT=$deployedAt"
)

if (-not [string]::IsNullOrWhiteSpace($ContainerEnvFilePath)) {
    $dockerRunArguments += @('--env-file', $ContainerEnvFilePath)
}

$dockerRunArguments += $imageReference

Invoke-DockerCommand -Arguments $dockerRunArguments

for ($attempt = 1; $attempt -le $HealthcheckRetries; $attempt++) {
    try {
        $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing
        if ($response.Content -match 'Healthy') {
            Write-Host $response.Content
            exit 0
        }
    }
    catch {
        if ($attempt -eq $HealthcheckRetries) {
            throw
        }
    }

    Start-Sleep -Seconds $HealthcheckDelaySeconds
}

throw "Deployment health check failed for '$ContainerName'."
