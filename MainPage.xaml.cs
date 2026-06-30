using FWITD;
using System.Diagnostics;

namespace FWShellMAUI {
    public partial class MainPage : ContentPage {
        private const int id_webview = 0;

        public MainPage() {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, EventArgs e) {
            RequestDispatcher.Register(WebView, id_webview);

            if (!AppConfig._scripts.TryGetValue(App.RequestedStartApp, out var entry)) {
                await DisplayAlertAsync("Warning", $"No configuration found for app: {App.RequestedStartApp}", "OK");
                return;
            }

            if (entry.main.script is JSProvider.JS.pages page) {
                var basePath = await JSProvider.getPathJSHTMLApp(page, id_webview);
                WebView.Source = new UrlWebViewSource { Url = new Uri(basePath + ".html").AbsoluteUri };
                return;
            }

            if (entry.main.script is JSProvider.JS.injectable_apps injectableApp) {
                WebView.Navigated += async (_, args) => {
                    if (args.Url.StartsWith("wawapp://", StringComparison.OrdinalIgnoreCase))
                        return;
                    string script = await JSProvider.getScriptApp(injectableApp, id_webview);
                    await InjectScriptAsync(WebView, script);
                };
            }

            if (entry.main.url is not null) {
                string url = entry.main.url;
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    url = "http://" + url;
                WebView.Source = new UrlWebViewSource { Url = url };
            } else
                await DisplayAlertAsync("Warning", $"No URL configured for app: {App.RequestedStartApp}", "OK");
        }

        private static async Task InjectScriptAsync(WebView webView, string script) {
            string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(script));
            const int chunkSize = 4_500_000;//4.5 MB

            await webView.EvaluateJavaScriptAsync("window.__fw_chunks=[];");
            for (int i = 0; i < b64.Length; i += chunkSize) {
                string chunk = b64.Substring(i, Math.Min(chunkSize, b64.Length - i));
                await webView.EvaluateJavaScriptAsync($"window.__fw_chunks.push('{chunk}');");
            }
            await webView.EvaluateJavaScriptAsync(
                "try{eval(new TextDecoder().decode(Uint8Array.from(atob(window.__fw_chunks.join('')),c=>c.charCodeAt(0))))}finally{delete window.__fw_chunks}");
        }
    }
}
