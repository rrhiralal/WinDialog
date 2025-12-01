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
                        if (options.Icon.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            IconImage.Source = new BitmapImage(new Uri(options.Icon));
                            IconImage.Visibility = Visibility.Visible;
                        }
                        else if (System.IO.File.Exists(options.Icon))
                        {
                            IconImage.Source = new BitmapImage(new Uri(options.Icon));
                            IconImage.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // Try Base64
                            byte[] bytes = Convert.FromBase64String(options.Icon);
                            var stream = new InMemoryRandomAccessStream();
                            await stream.WriteAsync(bytes.AsBuffer());
                            stream.Seek(0);
                            var image = new BitmapImage();
                            await image.SetSourceAsync(stream);
                            IconImage.Source = image;
                            IconImage.Visibility = Visibility.Visible;
                        }
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
