# WinDialog

**WinDialog** is a powerful, customizable command-line dialog utility for Windows, built with WinUI 3. It is designed to mirror the functionality and aesthetics of macOS's [swiftDialog](https://github.com/swiftDialog/swiftDialog), allowing system administrators and developers to display native-looking notifications and dialogs to users.

## Features

- **Native WinUI 3 Design**: Modern, clean aesthetics that match Windows 11/10. Supports light and dark mode.
- **DPI-Aware**: Automatic scaling for any display resolution and scaling factor (100%-300%).
- **Rich Text Support**: Full Markdown support for messages, including **bold**, *italics*, lists, headers, code blocks, tables, and hyperlinks.
- **Customizable Layout**: Control window size (presets or pixel dimensions), position, icon size, and title bar visibility.
- **Icon Support**: PNG, JPG, ICO, and SVG icons from local files, URLs, Base64 strings, or data URIs.
- **Buttonless Dialogs**: Hide all buttons with `--button1 "none"` for display-only or kiosk scenarios.
- **Quit Key**: Define a custom key combination to dismiss the dialog (required when no buttons are visible).
- **Interactive Elements**: Custom buttons, help messages with popovers, and Escape key support for Cancel.
- **Automation Friendly**: Returns distinct exit codes based on user interaction for scripting.
- **Timer**: Auto-dismiss functionality with customizable countdown text.

## Installation

### Download

Download the latest `.msix` package for your architecture from the [Releases](https://github.com/rrhiralal/WinDialog/releases) page:

- **WinDialog-x.x.x-x64.msix** — for Intel/AMD 64-bit systems
- **WinDialog-x.x.x-ARM64.msix** — for ARM64 systems (Surface Pro X, etc.)

Double-click the `.msix` to install, or deploy via Intune/SCCM/GPO for enterprise rollout.

After installation, `WinDialog.exe` is available from any terminal (PowerShell, cmd) via app execution alias.

### MDM Deployment

Use the included install/uninstall scripts for enterprise deployment:

```powershell
# Install (auto-detects architecture, fetches latest release)
.\scripts\Install-WinDialog.ps1

# Install specific version
.\scripts\Install-WinDialog.ps1 -Version 1.1.0

# Uninstall
.\scripts\Uninstall-WinDialog.ps1
```

### Build from Source

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and Windows 10 (build 19041+) or Windows 11.

```powershell
# Debug build
dotnet build -c Debug -p:Platform=x64

# Release MSIX package
dotnet build -c Release -p:Platform=x64 -p:AppxPackageSigningEnabled=false -p:GenerateAppxPackageOnBuild=true
```

## Usage

```powershell
WinDialog.exe [options]
```

### Command Line Arguments

| Flag | Description | Default |
|------|-------------|---------|
| `--title <text>` | Set the window title. | "WinDialog" |
| `--message <text>` | Set the message text. Supports Markdown. Use `\n` for newlines. | "" |
| `--button1 <text>` | Set the primary button text. Use `"none"` to hide all buttons. (Also accepts `--button1text`.) | "OK" |
| `--button2 <text>` | Set the secondary button text. If omitted, the button is hidden. (Also accepts `--button2text`.) | Hidden |
| `--quitkey <combo>` | Set a key combination to dismiss the dialog (exit code 5). Format: `"modifier,modifier,key"`. Required when `--button1` is `"none"`. | None |
| `--width <int>` | Set the window width in logical pixels (DPI-scaled automatically). | 600 |
| `--height <int>` | Set the window height in logical pixels (DPI-scaled automatically). | 400 |
| `--size <preset>` | Set window size relative to display: `small` (25%), `medium` (45%), `large` (70%), `fullscreen`. Overrides `--width`/`--height`. | None |
| `--hide-titlebar` | Hide the standard window title bar. | False |
| `--position <pos>` | Set window position: `Center`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`. | `Center` |
| `--icon <path>` | Set the icon. Supports local file paths, HTTP(S) URLs, Base64 strings, or data URIs. PNG, JPG, ICO, and SVG formats supported. | None |
| `--iconsize <size>` | Set icon size. Presets: `small` (32px), `medium` (64px), `large` (128px). Pixels: `WxH`, `W` (width, proportional height), or `xH` (height, proportional width). | Auto-scaled |
| `--timer <seconds>` | Set a countdown timer to auto-click the default button. | None |
| `--timer-text <txt>` | Customize the text displayed before the timer countdown. | "Closing in" |
| `--helpmessage <txt>` | Set a help message. Adds a help button that shows this text in a dialog. Supports Markdown. | None |
| `--version` | Show version information. | |
| `--help` | Show this help message. | |

### Quit Key Format

The `--quitkey` flag accepts a comma-separated combination of modifiers and a key:

```
--quitkey "control,shift,Q"
--quitkey "alt,F9"
--quitkey "control,alt,X"
```

Supported modifiers: `control` (or `ctrl`), `alt`, `shift`.

Some key combinations are restricted and cannot be used (e.g., `control,C`, `control,V`, `control,P`, `alt,F4`, `alt,Tab`, etc.).

### Return Codes

| Code | Meaning |
|------|---------|
| **0** | User clicked Button 1 (OK) or the timer expired. |
| **2** | User clicked Button 2 (Cancel) or pressed Escape. |
| **5** | User pressed the quit key combination. |
| **20** | Command-line error (invalid arguments). |

## Examples

### Basic Message
```powershell
WinDialog.exe --title "Welcome" --message "Hello, world!"
```

### Responsive Window Sizes
```powershell
# Small notification
WinDialog.exe --title "Alert" --message "Quick notice" --size small

# Large dialog for detailed content
WinDialog.exe --title "Setup" --message "# Device Enrollment\nConfiguring your workstation..." --size large

# Fullscreen for kiosk/provisioning
WinDialog.exe --title "Welcome" --message "Setting up your device..." --size fullscreen
```

### Markdown and Hyperlinks
```powershell
WinDialog.exe --title "Update Available" --message "# Version 2.0\n\n**New Features:**\n- Dark mode\n- [Release Notes](https://example.com)"
```

### Timer and Custom Buttons
```powershell
WinDialog.exe --title "Restart Required" --message "Your computer will restart in 60 seconds." --button1 "Restart Now" --button2 "Postpone" --timer 60 --timer-text "Restarting in"
```

### Buttonless Dialog with Quit Key
```powershell
# Display-only dialog, dismissed with Ctrl+Shift+Q
WinDialog.exe --title "Installing" --message "Please wait while we configure your system..." --button1 "none" --quitkey "control,shift,Q"

# Fullscreen kiosk-style dialog
WinDialog.exe --title "Welcome" --message "# Setting up your device\nPlease do not turn off your computer." --size fullscreen --button1 "none" --quitkey "control,alt,X" --hide-titlebar
```

### Quit Key with Buttons
```powershell
# Additional dismiss option alongside buttons
WinDialog.exe --title "Setup" --message "Installing software..." --quitkey "control,shift,Q"
```

### Icon with Custom Sizing
```powershell
# Large icon from file
WinDialog.exe --title "Success" --message "Task completed." --icon "C:\Images\check.png" --iconsize large

# SVG icon
WinDialog.exe --title "Info" --message "System update available." --icon "C:\Images\info.svg" --iconsize 120

# Icon scaled to specific width (height proportional)
WinDialog.exe --title "Warning" --message "Disk space low." --icon "C:\Images\warn.png" --iconsize 120
```

### Help Message
```powershell
WinDialog.exe --title "Setup" --message "Please wait while we configure your system." --helpmessage "If this takes longer than 5 minutes, please contact IT Support."
```

### Custom Position
```powershell
WinDialog.exe --title "Notification" --message "Task completed." --icon "C:\Images\success.png" --position BottomRight --size small
```

### Scripting with Exit Codes
```powershell
WinDialog.exe --title "Confirm" --message "Proceed with installation?" --button1 "Yes" --button2 "No" --quitkey "control,shift,Q"

switch ($LASTEXITCODE) {
    0 { Write-Host "User clicked Yes" }
    2 { Write-Host "User clicked No or pressed Escape" }
    5 { Write-Host "User pressed quit key" }
}
```
