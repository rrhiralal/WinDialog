#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Uninstalls WinDialog and removes the signing certificate.
#>

$ErrorActionPreference = "Stop"

# Remove the app package
$package = Get-AppxPackage -Name "WinDialog" -ErrorAction SilentlyContinue
if ($package) {
    Write-Host "Removing WinDialog..."
    Remove-AppxPackage -Package $package.PackageFullName
    Write-Host "WinDialog removed."
} else {
    Write-Host "WinDialog is not installed."
}

# Remove the signing certificate
$cert = Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -eq "CN=WinDialog, O=RichardHiralal" }
if ($cert) {
    Write-Host "Removing signing certificate..."
    $cert | Remove-Item
    Write-Host "Certificate removed."
} else {
    Write-Host "Certificate not found."
}
