using Microsoft.UI.Xaml.Navigation;

namespace WinDialog
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window window = Window.Current;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            var options = WinDialog.Models.DialogOptions.Parse(args);

            if (options.Version)
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"WinDialog v{version}");
                Environment.Exit(0);
            }

            if (options.Help)
            {
                Console.WriteLine("WinDialog - A customizable dialog utility for Windows");
                Console.WriteLine("Usage: WinDialog.exe [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  --title <text>    Set the window title");
                Console.WriteLine("  --message <text>  Set the message text");
                Console.WriteLine("  --button1 <text>  Set the primary button text (default: OK)");
                Console.WriteLine("  --button2 <text>  Set the secondary button text (optional)");
                Console.WriteLine("  --width <int>     Set the window width (default: 800)");
                Console.WriteLine("  --height <int>    Set the window height (default: 600)");
                Console.WriteLine("  --hide-titlebar   Hide the window title bar");
                Console.WriteLine("  --position <pos>  Set window position (Center, TopLeft, TopRight, BottomLeft, BottomRight)");
                Console.WriteLine("  --icon <path>     Set icon (URL, file path, or Base64 string)");
                Console.WriteLine("  --timer <seconds> Set a countdown timer to auto-click the default button");
                Console.WriteLine("  --timer-text <txt> Customize the text before the timer (default: 'Closing in')");
                Console.WriteLine("  --helpmessage <txt> Set a help message to be displayed when the help button is clicked");
                Console.WriteLine("  --version         Show version information");
                Console.WriteLine("  --help            Show this help message");
                Environment.Exit(0);
            }

            window ??= new Window();

            try
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new Windows.Graphics.SizeInt32(options.Width, options.Height));

                if (options.HideTitleBar)
                {
                    if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    {
                        presenter.SetBorderAndTitleBar(true, false);
                    }
                }

                // Handle positioning
                var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;
                int x = 0, y = 0;

                switch (options.Position)
                {
                    case WinDialog.Models.WindowPosition.Center:
                        x = workArea.X + (workArea.Width - options.Width) / 2;
                        y = workArea.Y + (workArea.Height - options.Height) / 2;
                        break;
                    case WinDialog.Models.WindowPosition.TopLeft:
                        x = workArea.X;
                        y = workArea.Y;
                        break;
                    case WinDialog.Models.WindowPosition.TopRight:
                        x = workArea.X + workArea.Width - options.Width;
                        y = workArea.Y;
                        break;
                    case WinDialog.Models.WindowPosition.BottomLeft:
                        x = workArea.X;
                        y = workArea.Y + workArea.Height - options.Height;
                        break;
                    case WinDialog.Models.WindowPosition.BottomRight:
                        x = workArea.X + workArea.Width - options.Width;
                        y = workArea.Y + workArea.Height - options.Height;
                        break;
                }
                appWindow.Move(new Windows.Graphics.PointInt32(x, y));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to configure window: {ex.Message}");
            }

            if (window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                window.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(Views.MainPage), options);
            window.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
