using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using WinDialog.Models;
using System;
using CommunityToolkit.WinUI.UI.Controls;

namespace WinDialog.Views
{
    public sealed partial class MainPage : Page
    {
        DialogOptions _options;
        int _remainingSeconds;

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
                {
                    MessageText.Text = options.Message;
                    MessageText.LinkClicked += OnLinkClicked;
                }

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

        private async void OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Link))
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }

        private void UpdateTimerText()
        {
            if (_remainingSeconds > 60)
            {
                TimeSpan t = TimeSpan.FromSeconds(_remainingSeconds);
                TimerText.Text = $"{_options.TimerText} {t.Minutes}:{t.Seconds:D2}";
            }
            else
            {
                TimerText.Text = $"{_options.TimerText} {_remainingSeconds}s";
            }
        }

        private void OnButton1Clicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(_options?.Button1 ?? "OK");
            Environment.Exit(0);
        }

        private void OnButton2Clicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(_options?.Button2 ?? "Cancel");
            Environment.Exit(2);
        }

        private async Task LoadIconAsync(string icon)
        {
            // Strip data URI prefix if present (e.g. "data:image/png;base64,...")
            string base64Data = null;
            if (icon.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                int commaIndex = icon.IndexOf(',');
                if (commaIndex >= 0)
                    base64Data = icon.Substring(commaIndex + 1);
            }

            if (base64Data != null)
            {
                // Decoded from data: URI
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
                // Treat as raw base64 string
                await SetIconFromBase64(icon);
            }
        }

        private async Task SetIconFromBase64(string base64)
        {
            // Clean up whitespace/newlines that may be present in long base64 strings
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
                IconImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
            }
            else if (sizeOption.WidthPixels.HasValue || sizeOption.HeightPixels.HasValue)
            {
                // When only one axis is specified, set the other to NaN (Auto)
                // so the image scales proportionally via Stretch.Uniform
                if (sizeOption.WidthPixels.HasValue && sizeOption.HeightPixels.HasValue)
                {
                    IconImage.Width = sizeOption.WidthPixels.Value;
                    IconImage.Height = sizeOption.HeightPixels.Value;
                    IconImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
                }
                else if (sizeOption.WidthPixels.HasValue)
                {
                    IconImage.Width = sizeOption.WidthPixels.Value;
                    IconImage.Height = double.NaN;
                    IconImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
                }
                else
                {
                    IconImage.Width = double.NaN;
                    IconImage.Height = sizeOption.HeightPixels.Value;
                    IconImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
                }
            }
            // else: no --iconsize specified, responsive auto-scaling applies
        }

        private bool HasExplicitIconSize =>
            _options?.IconSizeOption?.Preset != null ||
            _options?.IconSizeOption?.WidthPixels != null ||
            _options?.IconSizeOption?.HeightPixels != null;

        private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = e.NewSize.Width;
            double h = e.NewSize.Height;

            // Scale title font: 18-26 based on width
            double titleSize = Math.Clamp(w * 0.04, 18, 26);
            TitleText.FontSize = titleSize;

            // Scale message font: 13-16 based on width
            double messageSize = Math.Clamp(w * 0.026, 13, 16);
            MessageText.FontSize = messageSize;

            // Only auto-scale icon if no explicit --iconsize was set
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
                var markdownBlock = new MarkdownTextBlock
                {
                    Text = _options.HelpMessage,
                    TextWrapping = TextWrapping.Wrap,
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)
                };
                markdownBlock.LinkClicked += OnLinkClicked;

                var dialog = new ContentDialog
                {
                    Title = "Help",
                    Content = new ScrollViewer { Content = markdownBlock },
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}
