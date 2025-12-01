using System.Collections.Generic;

namespace WinDialog.Models
{
    public class DialogOptions
    {
        public string Title { get; set; } = "WinDialog";
        public string Message { get; set; } = "";
        public string Button1 { get; set; } = "OK";
        public string Button2 { get; set; } = null;
        public bool Help { get; set; } = false;
        public int Width { get; set; } = 1000;
        public int Height { get; set; } = 600;
        public bool HideTitleBar { get; set; } = false;
        public WindowPosition Position { get; set; } = WindowPosition.Center;
        public string Icon { get; set; } = null;
        public int? Timer { get; set; } = null;
        public string TimerText { get; set; } = "Closing in";
        public string HelpMessage { get; set; } = null;
        public bool Version { get; set; } = false;

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
                        if (i + 1 < args.Length) options.Button1 = args[++i];
                        break;
                    case "--button2text":
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
}
