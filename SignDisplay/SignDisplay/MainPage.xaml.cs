﻿using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SignDisplay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Screen[] _screens = null;
        ScreenManager _sm = new ScreenManager();
        string _url = "";
        string _feedId = "";
        int _currentIndex = 0;
        int _currentTimer = 0;
        DispatcherTimer _disTimer;
        Tweet _t = null;

        public MainPage()
        {
            this.InitializeComponent();

            var view = ApplicationView.GetForCurrentView();

            view.TitleBar.BackgroundColor = Color.FromArgb(255, 11, 140, 26);
            view.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 11, 140, 26);
            view.TitleBar.ButtonForegroundColor = Colors.White;
            view.TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 11, 140, 26);
            view.TitleBar.ButtonPressedBackgroundColor = Colors.White;
            view.TitleBar.ButtonHoverBackgroundColor = Colors.White;
            view.TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 11, 140, 26);

            _disTimer = new DispatcherTimer();
            _disTimer.Tick += _disTimer_Tick;

        }

        private void _disTimer_Tick(object sender, object e)
        {
            DisplayNext();
        }

        public async void GetScreens()
        {
            _screens = await _sm.GetScreensAsync(_url, _feedId);
            DisplayNext();
        }

        async void DisplayNext()
        {
            if (_currentIndex == _screens.Length) {
                _currentIndex = 0;
                _screens = await _sm.GetScreensAsync(_url, _feedId);
            }

            Screen s = _screens[_currentIndex];
            _t = null;

            if (s.sign_type == "web")
            {
                view_web.Visibility = Windows.UI.Xaml.Visibility.Visible;
                view_image.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_media.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                view_web.Visibility = Windows.UI.Xaml.Visibility.Visible;
                view_web.Navigate(new Uri(s.uri));

            }
            if (s.sign_type == "video")
            {
                view_web.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_image.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_media.Visibility = Windows.UI.Xaml.Visibility.Visible;

                view_media.Source = new Uri(s.uri);

            }
            else if (s.sign_type == "image")
            {
                view_image.Visibility = Windows.UI.Xaml.Visibility.Visible;
                view_web.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_media.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                BitmapImage imageSource = new BitmapImage(new Uri(s.uri));
                view_image.Width = imageSource.DecodePixelHeight = (int)this.ActualWidth;
                view_image.Source = imageSource;
            }
            else if (s.sign_type == "text")
            {
                view_text.Visibility = Windows.UI.Xaml.Visibility.Visible;
                view_web.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_image.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_media.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                view_text.Text = s.sign_text;
            }
            else if (s.sign_type == "tweet")
            {
                view_web.Visibility = Windows.UI.Xaml.Visibility.Visible;
                view_image.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                view_media.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                _t = await _sm.GetTweetAsync(s.uri);

                view_web.Source = new Uri("ms-appx-web:///Tweet.html");

            }

            _currentTimer = Convert.ToInt32(s.duration);
            _disTimer.Interval = new TimeSpan(0, 0, _currentTimer);
            _disTimer.Start();

            _currentIndex = _currentIndex + 1;
            
        }

        private void btn_run_Click(object sender, RoutedEventArgs e)
        {
            _url = txt_uri.Text;
            _feedId = txt_feed.Text;
            configstack.Visibility = Visibility.Collapsed;
            GetScreens();
        }

        private async void view_web_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (_t != null)
            {
                await view_web.InvokeScriptAsync("setAuthor", new string[] { _t.author_name.ToString() });
                await view_web.InvokeScriptAsync("setBody", new string[] { _t.html.ToString() });
            }
        }
    }
}
