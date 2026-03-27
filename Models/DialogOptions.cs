using System.Collections.Generic;

namespace WinDialog.Models
{
    public class DialogOptions
    {
        public string Title { get; set; } = "WinDialog";
        public string Message { get; set; } = "";
        public string Button1 { get; set; } = "OK";
        public string? Button2 { get; set; } = null;
        public bool Help { get; set; } = false;
        public int Width { get; set; } = 600;
        public int Height { get; set; } = 400;
        public bool HideTitleBar { get; set; } = false;
        public WindowPosition Position { get; set; } = WindowPosition.Center;
        public string? Icon { get; set; } = null;
        public int? Timer { get; set; } = null;
        public string TimerText { get; set; } = "Closing in";
        public string? HelpMessage { get; set; } = null;
        public bool Version { get; set; } = false;
        public WindowSize? Size { get; set; } = null;
        public IconSize IconSizeOption { get; set; } = new IconSize();
        public QuitKeyCombo? QuitKey { get; set; } = null;

        public bool HasNoButtons => Button1.Equals("none", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrEmpty(Button2);

        public static DialogOptions Parse(string[] args)
        {
            var options = new DialogOptions();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--title":
                        if (i + 1 < args.Length) options.Title = args[++i];
                        break;
                    case "--message":
                        if (i + 1 < args.Length) options.Message = args[++i].Replace("\\n", "  \n");
                        break;
                    case "--button1text":
                    case "--button1":
                        if (i + 1 < args.Length) options.Button1 = args[++i];
                        break;
                    case "--button2text":
                    case "--button2":
                        if (i + 1 < args.Length) options.Button2 = args[++i];
                        break;
                    case "--helpmessage":
                        if (i + 1 < args.Length) options.HelpMessage = args[++i].Replace("\\n", "  \n");
                        break;
                    case "--width":
                    case "-w":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int w)) options.Width = w;
                        break;
                    case "--height":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int h)) options.Height = h;
                        break;
                    case "--size":
                    case "-s":
                        if (i + 1 < args.Length)
                        {
                            options.Size = args[++i].ToLower() switch
                            {
                                "small" => WindowSize.Small,
                                "medium" => WindowSize.Medium,
                                "large" => WindowSize.Large,
                                "fullscreen" => WindowSize.Fullscreen,
                                _ => null
                            };
                        }
                        break;
                    case "--hide-titlebar":
                        options.HideTitleBar = true;
                        break;
                    case "--position":
                    case "-p":
                        if (i + 1 < args.Length)
                        {
                            string pos = args[++i].ToLower();
                            options.Position = pos switch
                            {
                                "topleft" => WindowPosition.TopLeft,
                                "topright" => WindowPosition.TopRight,
                                "bottomleft" => WindowPosition.BottomLeft,
                                "bottomright" => WindowPosition.BottomRight,
                                _ => WindowPosition.Center
                            };
                        }
                        break;
                    case "--icon":
                    case "-i":
                        if (i + 1 < args.Length) options.Icon = args[++i];
                        break;
                    case "--iconsize":
                        if (i + 1 < args.Length) options.IconSizeOption = IconSize.Parse(args[++i]);
                        break;
                    case "--timer":
                    case "-t":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int t)) options.Timer = t;
                        break;
                    case "--timer-text":
                        if (i + 1 < args.Length) options.TimerText = args[++i];
                        break;
                    case "--quitkey":
                        if (i + 1 < args.Length) options.QuitKey = QuitKeyCombo.Parse(args[++i]);
                        break;
                    case "--version":
                    case "-v":
                        options.Version = true;
                        break;
                    case "--help":
                    case "-h":
                    case "/?":
                        options.Help = true;
                        break;
                }
            }

            // --size overrides --width/--height (actual pixel values resolved at display time)
            if (options.Size == null)
            {
                options.Width = Math.Clamp(options.Width, 200, 3840);
                options.Height = Math.Clamp(options.Height, 150, 2160);
            }

            // Validate: --quitkey is mandatory when no buttons are visible
            if (options.HasNoButtons && options.QuitKey == null)
            {
                Console.WriteLine("Error: --quitkey is required when --button1 is set to \"none\" (no buttons visible).");
                Environment.Exit(20);
            }

            return options;
        }
    }

    public enum WindowPosition
    {
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum WindowSize
    {
        Small,
        Medium,
        Large,
        Fullscreen
    }

    public class QuitKeyCombo
    {
        public bool Control { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public string Key { get; set; } = "";

        // Restricted combinations: system shortcuts + WebView2/browser defaults
        private static readonly HashSet<string> RestrictedCombos = new(StringComparer.OrdinalIgnoreCase)
        {
            // System
            "control+c", "control+v", "control+x", "control+z", "control+a", "control+s",
            "control+alt+delete", "alt+f4", "alt+tab", "control+shift+escape", "control+alt+end",
            // WebView2/browser defaults
            "control+p", "control+f", "control+g", "control+h", "control+j", "control+k",
            "control+l", "control+n", "control+o", "control+r", "control+t", "control+u", "control+w",
            "control+shift+i", "control+shift+j", "control+shift+c", "control+shift+n",
            "f5", "f7", "f11", "f12",
        };

        public static QuitKeyCombo? Parse(string value)
        {
            var combo = new QuitKeyCombo();
            var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                switch (part.ToLower())
                {
                    case "control":
                    case "ctrl":
                        combo.Control = true;
                        break;
                    case "alt":
                        combo.Alt = true;
                        break;
                    case "shift":
                        combo.Shift = true;
                        break;
                    default:
                        combo.Key = part;
                        break;
                }
            }

            if (string.IsNullOrEmpty(combo.Key))
            {
                Console.WriteLine("Error: --quitkey must include a key (e.g., --quitkey \"control,P\").");
                Environment.Exit(20);
                return null;
            }

            // Check restricted combinations
            string normalized = combo.ToNormalizedString();
            if (RestrictedCombos.Contains(normalized))
            {
                Console.WriteLine($"Error: The key combination \"{value}\" is restricted and cannot be used as a quit key.");
                Environment.Exit(20);
                return null;
            }

            return combo;
        }

        public string ToNormalizedString()
        {
            var parts = new List<string>();
            if (Control) parts.Add("control");
            if (Alt) parts.Add("alt");
            if (Shift) parts.Add("shift");
            parts.Add(Key.ToLower());
            return string.Join("+", parts);
        }
    }

    public class IconSize
    {
        public IconSizePreset? Preset { get; set; }
        public int? WidthPixels { get; set; }
        public int? HeightPixels { get; set; }

        public static IconSize Parse(string value)
        {
            var result = new IconSize();
            string lower = value.Trim().ToLower();

            switch (lower)
            {
                case "small":
                    result.Preset = IconSizePreset.Small;
                    return result;
                case "medium":
                    result.Preset = IconSizePreset.Medium;
                    return result;
                case "large":
                    result.Preset = IconSizePreset.Large;
                    return result;
            }

            // Parse pixel dimensions: "WxH", "W" (width only), "xH" (height only)
            if (lower.Contains('x'))
            {
                string[] parts = lower.Split('x', 2);
                if (!string.IsNullOrEmpty(parts[0]) && int.TryParse(parts[0], out int w))
                    result.WidthPixels = Math.Clamp(w, 16, 512);
                if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) && int.TryParse(parts[1], out int h))
                    result.HeightPixels = Math.Clamp(h, 16, 512);
            }
            else if (int.TryParse(lower, out int size))
            {
                // Single number = width, height scales proportionally
                result.WidthPixels = Math.Clamp(size, 16, 512);
            }

            return result;
        }
    }

    public enum IconSizePreset
    {
        Small,
        Medium,
        Large
    }
}
