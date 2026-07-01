using FWITD;
using System.Diagnostics;

namespace FWShellMAUI {
    public partial class MainPage : ContentPage {
        private const int id_webview = 0;
        private static readonly TimeSpan LoadingMessageInterval = TimeSpan.FromSeconds(5.5);
        private static readonly TimeSpan SpinnerTickInterval = TimeSpan.FromMilliseconds(30);
        private static readonly double SpinnerRotationDurationMs = 1751;

        private IDispatcherTimer? _loadingMessageTimer;
        private IDispatcherTimer? _spinnerTimer;
        private double _spinnerAngle;

        public MainPage() {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, EventArgs e) {
            RequestDispatcher.Register(WebView, id_webview);
            WebView.Navigated += OnFirstNavigated;
            StartLoadingMessages();
            StartSpinner();
            await EnsureHandlerAsync(WebView);

            try {
                await AppLoader.LoadAsync(id_webview, App.RequestedStartApp);
            } catch (InvalidOperationException ex) {
                WebView.Navigated -= OnFirstNavigated;
                StopLoadingMessages();
                StopSpinner();
                LoadingOverlay.IsVisible = false;
                await DisplayAlertAsync("Warning", ex.Message, "OK");
            }
        }

        private void OnFirstNavigated(object? sender, WebNavigatedEventArgs e) {
            // WebView2's EnsureCoreWebView2Async() triggers its own internal navigation to
            // "about:blank" before the real content loads; ignore it so the overlay doesn't
            // dismiss itself before AppLoader ever navigates to the actual target.
            if (string.IsNullOrEmpty(e.Url) || e.Url.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
                return;

            WebView.Navigated -= OnFirstNavigated;
            StopLoadingMessages();
            StopSpinner();

            if (e.Result == WebNavigationResult.Success) {
                LoadingOverlay.IsVisible = false;
                return;
            }
            LoadingIndicator.IsVisible = false;
            LoadingMessageLabel.IsVisible = false;
            LoadingErrorLabel.Text = e.Result == WebNavigationResult.Timeout
                ? "Connection timed out."
                : $"Unable to load app ({e.Result}).";
            LoadingErrorLabel.IsVisible = true;

            try {
                AppLoader.LoadAsync(id_webview, StartApp.ServerStatus);
            } catch { } finally {
                LoadingOverlay.HorizontalOptions = LayoutOptions.Center;
                LoadingOverlay.VerticalOptions = LayoutOptions.Start;
            }
        }

        private void StartLoadingMessages() {
            _ = SetRandomLoadingMessageAsync();

            _loadingMessageTimer = Dispatcher.CreateTimer();
            _loadingMessageTimer.Interval = LoadingMessageInterval;
            _loadingMessageTimer.Tick += async (_, _) => await SetRandomLoadingMessageAsync();
            _loadingMessageTimer.Start();
        }

        private void StopLoadingMessages() {
            _loadingMessageTimer?.Stop();
            _loadingMessageTimer = null;
        }

        private async Task SetRandomLoadingMessageAsync() {
            LoadingMessageLabel.Text = await LoadingMessagesProvider.GetRandomMessageAsync();
        }

        private void StartSpinner() {
            _spinnerAngle = 0;
            LoadingIndicator.Rotation = 0;

            double degreesPerTick = 360.0 * SpinnerTickInterval.TotalMilliseconds / SpinnerRotationDurationMs;
            _spinnerTimer = Dispatcher.CreateTimer();
            _spinnerTimer.Interval = SpinnerTickInterval;
            _spinnerTimer.Tick += (_, _) => {
                _spinnerAngle = (_spinnerAngle + degreesPerTick) % 360;
                LoadingIndicator.Rotation = _spinnerAngle;
            };
            _spinnerTimer.Start();
        }

        private void StopSpinner() {
            _spinnerTimer?.Stop();
            _spinnerTimer = null;
        }

        private static async Task EnsureHandlerAsync(WebView webView) {
            if (webView.Handler?.PlatformView is null) {
                var tcs = new TaskCompletionSource();
                void onHandlerChanged(object? _, EventArgs __) {
                    if (webView.Handler?.PlatformView is null)
                        return;
                    webView.HandlerChanged -= onHandlerChanged;
                    tcs.TrySetResult();
                }
                webView.HandlerChanged += onHandlerChanged;
                await tcs.Task;
            }

#if WINDOWS
            // PlatformView existing doesn't mean CoreWebView2 is ready yet on Windows;
            // setting Source before it initializes can silently drop the navigation.
            if (webView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 winWebView)
                await winWebView.EnsureCoreWebView2Async();
#endif
        }
    }
}
