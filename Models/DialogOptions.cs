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
