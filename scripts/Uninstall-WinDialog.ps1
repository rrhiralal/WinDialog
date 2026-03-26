#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Uninstalls WinDialog system-wide and removes the signing certificate.
#>

$ErrorActionPreference = "Stop"

# Remove provisioned package (prevents install for new users)
$provisioned = Get-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq "WinDialog" }
if ($provisioned) {
    Write-Host "Removing provisioned package..."
    Remove-AppxProvisionedPackage -Online -PackageName $provisioned.PackageName | Out-Null
}

# Remove from all existing users
$packages = Get-AppxPackage -AllUsers -Name "WinDialog" -ErrorAction SilentlyContinue
if ($packages) {
    foreach ($pkg in $packages) {
        Write-Host "Removing WinDialog from user $($pkg.PackageUserInformation.UserSecurityId)..."
        Remove-AppxPackage -Package $pkg.PackageFullName -AllUsers
    }
    Write-Host "WinDialog removed from all users."
} elseif (-not $provisioned) {
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
