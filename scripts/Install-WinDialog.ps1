#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installs WinDialog from GitHub with certificate trust.
.DESCRIPTION
    Downloads the latest WinDialog MSIX from GitHub, imports the signing
    certificate into the local machine's Trusted People store, and installs
    the package. Designed to be run via MDM (Intune, etc.) as SYSTEM.
.PARAMETER Version
    The version to install (e.g., "1.1.0"). Defaults to latest.
.PARAMETER Architecture
    Target architecture: x64 or ARM64. Defaults to current system architecture.
#>
param(
    [string]$Version = "latest",
    [ValidateSet("x64", "ARM64")]
    [string]$Architecture
)

$ErrorActionPreference = "Stop"
$repoOwner = "rrhiralal"
$repoName = "WinDialog"

# Detect architecture if not specified
if (-not $Architecture) {
    $Architecture = if ($env:PROCESSOR_ARCHITECTURE -eq "ARM64") { "ARM64" } else { "x64" }
}

# Embedded signing certificate (public key only)
$certBase64 = "MIIDODCCAiCgAwIBAgIQYMB8D+fnd5NJKcVepQ+DrTANBgkqhkiG9w0BAQsFADAtMRcwFQYDVQQKDA5SaWNoYXJkSGlyYWxhbDESMBAGA1UEAwwJV2luRGlhbG9nMB4XDTI2MDMyNTIwNDE0NloXDTI3MDMyNTIxMDE0NlowLTEXMBUGA1UECgwOUmljaGFyZEhpcmFsYWwxEjAQBgNVBAMMCVdpbkRpYWxvZzCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAOLzIYDwUxC0X897ZcdCNHe7MOBJxxagxCCw8bMNZc2OQxGzJHQ+DcH4t/HSP84GJfl4y0OcD4EOZYr279Kj832VuL5SSGIlxlkT+YkDRuRSHDVVsGJskLS4t57ymbHMgOVKHu62n+xofa7wZcOKxIR0Z0U1KxSFp3jWakt/EIm6K+NZ6tMk0egDq05UG4TJLoE+sBA+CWw55YIQ/3qu3kr3Z87ErnGFXC2uMzVGVndIGJ0NQBoBr1f/EWFECvmtS08lSptd3IyA6LdU7cg1npUgOO6bBwb8rqkkV+msh44qnOMNyYUm4o//L4d8qsaOieqznKKKregvskWXwc+hwskCAwEAAaNUMFIwDgYDVR0PAQH/BAQDAgeAMBMGA1UdJQQMMAoGCCsGAQUFBwMDMAwGA1UdEwEB/wQCMAAwHQYDVR0OBBYEFDzy5QO/Mfla3RKcvy9ROL4FUHvzMA0GCSqGSIb3DQEBCwUAA4IBAQBUmrolnmOfo74YKdSocpT/ZbwDeuTLnXXGTHBKiQEIuAT/h9GOJQlIfDCKb+hyPe9OvZu2NbIIA1hvrOcBWBHXNIi3VJ00R0FUU4HzgUtkz9/Y3yhOAntUIIE+0COBQqwwfkLOPAjamJViftox337UQ14N6pmXX9o5JFUOHaQ7FpcD8h+DrXHGKWxJa+6Mv/IsySzlYFLCMAIoWAqFLZxPJ3zCENjcLbbU3DKlXjVB8JWyZEvMlo5EQJ0EWdHkJnLUk6EbiJxD5ASPqVydzWrqY8H4qKuvekMkTHhYuTxUc20NTOT3Q6wUpiPIHDJ46HRLW3D+UsqqjeaMgrd7O323"

# --- Import certificate ---
Write-Host "Importing WinDialog signing certificate..."
$certBytes = [Convert]::FromBase64String($certBase64)
$certPath = Join-Path $env:TEMP "WinDialog.cer"
[IO.File]::WriteAllBytes($certPath, $certBytes)

try {
    $existing = Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -eq "CN=WinDialog, O=RichardHiralal" }
    if (-not $existing) {
        Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null
        Write-Host "Certificate imported successfully."
    } else {
        Write-Host "Certificate already trusted."
    }
} finally {
    Remove-Item $certPath -Force -ErrorAction SilentlyContinue
}

# --- Resolve version ---
if ($Version -eq "latest") {
    Write-Host "Fetching latest release..."
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repoOwner/$repoName/releases/latest"
    $Version = $release.tag_name -replace '^v', ''
    $assets = $release.assets
} else {
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repoOwner/$repoName/releases/tags/v$Version"
    $assets = $release.assets
}

# --- Download MSIX ---
$assetName = "WinDialog-$Version-$Architecture.msix"
$asset = $assets | Where-Object { $_.name -eq $assetName }
if (-not $asset) {
    Write-Error "Asset '$assetName' not found in release v$Version. Available: $($assets.name -join ', ')"
    exit 1
}

$msixPath = Join-Path $env:TEMP $assetName
Write-Host "Downloading $assetName..."
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $msixPath

# --- Install ---
try {
    Write-Host "Installing WinDialog v$Version ($Architecture)..."
    Add-AppxPackage -Path $msixPath
    Write-Host "WinDialog installed successfully. Run 'WinDialog.exe' from any terminal."
} finally {
    Remove-Item $msixPath -Force -ErrorAction SilentlyContinue
}
