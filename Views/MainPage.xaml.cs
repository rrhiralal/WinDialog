using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using WinDialog.Models;
using System;
using Markdig;

namespace WinDialog.Views
{
    public sealed partial class MainPage : Page
    {
        DialogOptions? _options;
        int _remainingSeconds;
        static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public MainPage()
        {
            this.InitializeComponent();
            this.KeyDown += OnPageKeyDown;
        }

        private void OnPageKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape && Button2.Visibility == Visibility.Visible)
            {
                OnButton2Clicked(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is DialogOptions options)
            {
                _options = options;
                if (!string.IsNullOrEmpty(options.Title))
                    TitleText.Text = options.Title;

                if (!string.IsNullOrEmpty(options.Message))
                    await LoadMarkdownMessage(options.Message);

                if (!string.IsNullOrEmpty(options.Button1))
                    Button1.Content = options.Button1;

                if (!string.IsNullOrEmpty(options.Button2))
                {
                    Button2.Content = options.Button2;
                    Button2.Visibility = Visibility.Visible;
                }

                if (!string.IsNullOrEmpty(options.HelpMessage))
                {
                    HelpButton.Visibility = Visibility.Visible;
                }

                if (!string.IsNullOrEmpty(options.Icon))
                {
                    try
                    {
                        await LoadIconAsync(options.Icon);
                        ApplyIconSize(options.IconSizeOption);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to load icon: {ex.Message}");
                    }
                }

                if (options.Timer.HasValue && options.Timer.Value > 0)
                {
                    _remainingSeconds = options.Timer.Value;
                    UpdateTimerText();
                    TimerText.Visibility = Visibility.Visible;

                    var timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += (s, args) =>
                    {
                        _remainingSeconds--;
                        if (_remainingSeconds <= 0)
                        {
                            timer.Stop();
                            OnButton1Clicked(this, new RoutedEventArgs());
                        }
                        else
                        {
                            UpdateTimerText();
                        }
                    };
                    timer.Start();
                }
            }
        }

        /// <summary>
        /// Gets the actual background color from the WinUI theme resource
        private async Task LoadMarkdownMessage(string markdown)
        {
            string htmlBody = Markdig.Markdown.ToHtml(markdown, _pipeline);

            // Read the system theme directly from registry
            bool isDark = false;
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key?.GetValue("AppsUseLightTheme") is int val)
                    isDark = val == 0;
            }
            catch { }
            string textColor = isDark ? "#e4e4e4" : "#1a1a1a";
            string linkColor = isDark ? "#6cb6ff" : "#0969da";

            string html = $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  body {{
    font-family: 'Segoe UI Variable', 'Segoe UI', sans-serif;
    font-size: 14px;
    line-height: 1.5;
    color: {textColor};
    background: transparent;
    -webkit-user-select: text;
    user-select: text;
  }}
  h1 {{ font-size: 1.6em; font-weight: 600; margin: 0.4em 0; }}
  h2 {{ font-size: 1.3em; font-weight: 600; margin: 0.4em 0; }}
  h3 {{ font-size: 1.1em; font-weight: 600; margin: 0.3em 0; }}
  p {{ margin: 0.4em 0; }}
  ul, ol {{ margin: 0.4em 0; padding-left: 1.5em; }}
  li {{ margin: 0.2em 0; }}
  a {{ color: {linkColor}; text-decoration: none; }}
  a:hover {{ text-decoration: underline; }}
  code {{
    font-family: 'Cascadia Code', 'Consolas', monospace;
    font-size: 0.9em;
    background: {(isDark ? "#2d2d2d" : "#e8e8e8")};
    padding: 0.15em 0.35em;
    border-radius: 4px;
  }}
  pre {{
    background: {(isDark ? "#2d2d2d" : "#e8e8e8")};
    padding: 0.8em;
    border-radius: 6px;
    overflow-x: auto;
    margin: 0.5em 0;
  }}
  pre code {{ background: none; padding: 0; }}
  blockquote {{
    border-left: 3px solid {(isDark ? "#555" : "#d0d0d0")};
    padding-left: 0.8em;
    margin: 0.4em 0;
    opacity: 0.85;
  }}
  table {{ border-collapse: collapse; margin: 0.5em 0; }}
  th, td {{ border: 1px solid {(isDark ? "#555" : "#d0d0d0")}; padding: 0.4em 0.8em; }}
  th {{ font-weight: 600; }}
</style>
</head>
<body>{htmlBody}</body>
</html>";

            // Initialize WebView2 and set transparent background BEFORE navigation
            await MessageWebView.EnsureCoreWebView2Async();
            MessageWebView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

            MessageWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            MessageWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            MessageWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            // Open links in the system browser
            MessageWebView.CoreWebView2.NewWindowRequested += (sender, args) =>
            {
                args.Handled = true;
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri(args.Uri));
            };
            MessageWebView.CoreWebView2.NavigationStarting += (sender, args) =>
            {
                if (args.Uri != "about:blank" && !args.Uri.StartsWith("data:"))
                {
                    args.Cancel = true;
                    _ = Windows.System.Launcher.LaunchUriAsync(new Uri(args.Uri));
                }
            };

            MessageWebView.NavigateToString(html);
        }

        private void UpdateTimerText()
        {
            if (_remainingSeconds > 60)
            {
                TimeSpan t = TimeSpan.FromSeconds(_remainingSeconds);
                TimerText.Text = $"{_options!.TimerText} {t.Minutes}:{t.Seconds:D2}";
            }
            else
            {
                TimerText.Text = $"{_options!.TimerText} {_remainingSeconds}s";
            }
        }

        private void CleanupAndExit(int exitCode)
        {
            MessageWebView.Close();
            Environment.Exit(exitCode);
        }

        private void OnButton1Clicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(_options?.Button1 ?? "OK");
            CleanupAndExit(0);
        }

        private void OnButton2Clicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(_options?.Button2 ?? "Cancel");
            CleanupAndExit(2);
        }

        private async Task LoadIconAsync(string icon)
        {
            string? base64Data = null;
            if (icon.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                int commaIndex = icon.IndexOf(',');
                if (commaIndex >= 0)
                    base64Data = icon.Substring(commaIndex + 1);
            }

            if (base64Data != null)
            {
                await SetIconFromBase64(base64Data);
            }
            else if (icon.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                     icon.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                IconImage.Source = new BitmapImage(new Uri(icon));
                IconImage.Visibility = Visibility.Visible;
            }
            else if (System.IO.File.Exists(icon))
            {
                IconImage.Source = new BitmapImage(new Uri(icon));
                IconImage.Visibility = Visibility.Visible;
            }
            else
            {
                await SetIconFromBase64(icon);
            }
        }

        private async Task SetIconFromBase64(string base64)
        {
            base64 = base64.Trim();
            byte[] bytes = Convert.FromBase64String(base64);
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(bytes.AsBuffer());
            stream.Seek(0);
            var image = new BitmapImage();
            await image.SetSourceAsync(stream);
            IconImage.Source = image;
            IconImage.Visibility = Visibility.Visible;
        }

        private void ApplyIconSize(IconSize sizeOption)
        {
            if (sizeOption.Preset.HasValue)
            {
                double px = sizeOption.Preset.Value switch
                {
                    IconSizePreset.Small => 32,
                    IconSizePreset.Medium => 64,
                    IconSizePreset.Large => 128,
                    _ => 64
                };
                IconImage.Width = px;
                IconImage.Height = px;
                IconImage.Stretch = Stretch.Uniform;
            }
            else if (sizeOption.WidthPixels.HasValue || sizeOption.HeightPixels.HasValue)
            {
                if (sizeOption.WidthPixels.HasValue && sizeOption.HeightPixels.HasValue)
                {
                    IconImage.Width = sizeOption.WidthPixels.Value;
                    IconImage.Height = sizeOption.HeightPixels.Value;
                    IconImage.Stretch = Stretch.Fill;
                }
                else if (sizeOption.WidthPixels.HasValue)
                {
                    IconImage.Width = sizeOption.WidthPixels.Value;
                    IconImage.Height = double.NaN;
                    IconImage.Stretch = Stretch.Uniform;
                }
                else if (sizeOption.HeightPixels.HasValue)
                {
                    IconImage.Width = double.NaN;
                    IconImage.Height = sizeOption.HeightPixels.Value;
                    IconImage.Stretch = Stretch.Uniform;
                }
            }
        }

        private bool HasExplicitIconSize =>
            _options?.IconSizeOption?.Preset != null ||
            _options?.IconSizeOption?.WidthPixels != null ||
            _options?.IconSizeOption?.HeightPixels != null;

        private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = e.NewSize.Width;
            double h = e.NewSize.Height;

            double titleSize = Math.Clamp(w * 0.04, 18, 26);
            TitleText.FontSize = titleSize;

            if (!HasExplicitIconSize)
            {
                double dim = Math.Min(w, h);
                double iconSize = Math.Clamp(dim * 0.15, 48, 96);
                IconImage.Width = iconSize;
                IconImage.Height = iconSize;
            }
        }

        private async void OnHelpButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_options?.HelpMessage))
            {
                var textBlock = new TextBlock
                {
                    Text = _options.HelpMessage,
                    TextWrapping = TextWrapping.Wrap
                };

                var dialog = new ContentDialog
                {
                    Title = "Help",
                    Content = new ScrollViewer { Content = textBlock },
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}
