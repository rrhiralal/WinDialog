# WinDialog

**WinDialog** is a powerful, customizable command-line dialog utility for Windows, built with WinUI 3. It is designed to mirror the functionality and aesthetics of macOS's [SwiftDialog](https://github.com/swiftDialog/swiftDialog), allowing system administrators and developers to display native-looking notifications and dialogs to users.

## Features

- **Native WinUI 3 Design**: Modern, clean aesthetics that match Windows 11/10.
- **Rich Text Support**: Full Markdown support for messages, including **bold**, *italics*, lists, headers, and hyperlinks.
- **Customizable Layout**: Control window size, position, and title bar visibility.
- **Interactive Elements**: Custom buttons, help messages with popovers, and icons (URL, file, or Base64).
- **Automation Friendly**: Returns exit codes based on user interaction (0 for OK, 2 for Cancel).
- **Timer**: Auto-dismiss functionality with customizable countdown text.

## Installation

### Download
Download the latest release from the [Releases](https://github.com/rrhiralal/WinDialog/releases/tag/Release) page (if applicable).

### Build from Source
To build WinDialog from source, you need the .NET 9 SDK.
See [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md) for detailed build instructions.

## Usage

```powershell
WinDialog.exe [options]
```

### Command Line Arguments

| Flag | Description | Default |
|------|-------------|---------|
| `--title <text>` | Set the window title. | "WinDialog" |
| `--message <text>` | Set the message text. Supports Markdown. | "" |
| `--button1 <text>` | Set the primary button text. | "OK" |
| `--button2 <text>` | Set the secondary button text. If omitted, the button is hidden. | Hidden |
| `--width <int>` | Set the window width. | 800 |
| `--height <int>` | Set the window height. | 600 |
| `--hide-titlebar` | Hide the standard window title bar. | False |
| `--position <pos>` | Set window position: `Center`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`. | `Center` |
| `--icon <path>` | Set the icon. Supports local file paths, HTTP(S) URLs, or Base64 strings. | None |
| `--timer <seconds>` | Set a countdown timer to auto-click the default button. | None |
| `--timer-text <txt>` | Customize the text displayed before the timer countdown. | "Closing in" |
| `--helpmessage <txt>` | Set a help message. Adds a help button that shows this text in a dialog. Supports Markdown. | None |
| `--version` | Show version information. | |
| `--help` | Show the help message. | |

### Return Codes

- **0**: User clicked Button 1 (OK) or the timer expired.
- **2**: User clicked Button 2 (Cancel).

## Examples

### Basic Message
```powershell
WinDialog.exe --title "Welcome" --message "Hello, world!"
```

### Markdown and Hyperlinks
```powershell
WinDialog.exe --title "Update Available" --message "# Version 2.0\n\n**New Features:**\n- Dark mode\n- [Release Notes](https://example.com)"
```

### Timer and Custom Button
```powershell
WinDialog.exe --title "Restart Required" --message "Your computer will restart in 60 seconds." --button1 "Restart Now" --timer 60 --timer-text "Restarting in"
```

### Help Message
```powershell
WinDialog.exe --title "Setup" --message "Please wait while we configure your system." --helpmessage "If this takes longer than 5 minutes, please contact IT Support."
```

### Custom Icon and Position
```powershell
WinDialog.exe --title "Notification" --message "Task completed successfully." --icon "C:\Images\success.png" --position BottomRight --width 400 --height 200
```
