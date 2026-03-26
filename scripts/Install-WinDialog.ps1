#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installs or updates WinDialog from GitHub with certificate trust.
.DESCRIPTION
    Downloads the latest WinDialog MSIX from GitHub, imports the signing
    certificate into the local machine's Trusted People store, and installs
    or updates the package. Designed to be run via MDM (Intune, etc.) as SYSTEM.
.PARAMETER Version
    The version to install (e.g., "1.1.0"). Defaults to latest.
.PARAMETER Architecture
    Target architecture: x64 or ARM64. Defaults to current system architecture.
.PARAMETER Force
    Force reinstall even if the same version is already installed.
#>
param(
    [string]$Version = "latest",
    [ValidateSet("x64", "ARM64")]
    [string]$Architecture,
    [switch]$Force
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
    $newCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(,$certBytes)
    $existing = Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -eq "CN=WinDialog, O=RichardHiralal" }

    if (-not $existing) {
        Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null
        Write-Host "Certificate imported successfully (expires $($newCert.NotAfter.ToString('yyyy-MM-dd')))."
    } elseif ($existing.Thumbprint -ne $newCert.Thumbprint) {
        # Different cert (renewed) — remove old, import new
        Write-Host "Updating certificate (old expires $($existing.NotAfter.ToString('yyyy-MM-dd')), new expires $($newCert.NotAfter.ToString('yyyy-MM-dd')))..."
        $existing | Remove-Item
        Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null
        Write-Host "Certificate updated successfully."
    } elseif ($existing.NotAfter -lt (Get-Date)) {
        Write-Warning "Certificate has expired ($($existing.NotAfter.ToString('yyyy-MM-dd'))). Update the install script with a renewed certificate."
    } else {
        Write-Host "Certificate already trusted (expires $($existing.NotAfter.ToString('yyyy-MM-dd')))."
    }
} finally {
    Remove-Item $certPath -Force -ErrorAction SilentlyContinue
}

# --- Check installed version (user or provisioned) ---
$installed = Get-AppxPackage -AllUsers -Name "WinDialog" -ErrorAction SilentlyContinue
if (-not $installed) {
    $provisioned = Get-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq "WinDialog" }
    if ($provisioned) {
        $installedVersion = [version]$provisioned.Version
        Write-Host "WinDialog v$installedVersion is provisioned (system-wide)."
    }
} else {
    $installedVersion = [version]$installed.Version
    Write-Host "WinDialog v$installedVersion is currently installed."
}

if (-not $installed -and -not $provisioned) {
    Write-Host "WinDialog is not currently installed."
}

# --- Resolve target version ---
if ($Version -eq "latest") {
    Write-Host "Fetching latest release..."
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repoOwner/$repoName/releases/latest"
    $Version = $release.tag_name -replace '^v', ''
    $assets = $release.assets
} else {
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repoOwner/$repoName/releases/tags/v$Version"
    $assets = $release.assets
}

# MSIX versions require 4 parts (x.x.x.0)
$targetVersion = [version]"$Version.0"

# --- Compare versions ---
$isInstalled = $installed -or $provisioned
if ($isInstalled -and -not $Force) {
    if ($installedVersion -eq $targetVersion) {
        Write-Host "WinDialog v$Version is already installed. Use -Force to reinstall."
        exit 0
    } elseif ($installedVersion -gt $targetVersion) {
        Write-Host "Installed version ($installedVersion) is newer than target ($targetVersion). Use -Force to downgrade."
        exit 0
    } else {
        Write-Host "Updating WinDialog from v$installedVersion to v$targetVersion..."
    }
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

# --- Install or update (system-wide) ---
try {
    if ($isInstalled) {
        Write-Host "Updating WinDialog to v$Version ($Architecture) system-wide..."
    } else {
        Write-Host "Installing WinDialog v$Version ($Architecture) system-wide..."
    }
    Add-AppxProvisionedPackage -Online -PackagePath $msixPath -SkipLicense | Out-Null
    Write-Host "WinDialog v$Version installed successfully for all users. Run 'WinDialog.exe' from any terminal."
} finally {
    Remove-Item $msixPath -Force -ErrorAction SilentlyContinue
}
