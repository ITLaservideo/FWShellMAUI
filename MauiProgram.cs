using Microsoft.Extensions.Logging;

namespace FWShellMAUI {
    public static class MauiProgram {
        public static MauiApp CreateMauiApp() {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if ANDROID
            // Android 11+ (API 30) defaults WebSettings.AllowFileAccess to false, which blocks
            // file:// navigation to our cache-dir HTML/JS/CSS and causes net::ERR_ACCESS_DENIED.
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("AllowFileAccess", (handler, view) => {
                handler.PlatformView.Settings.AllowFileAccess = true;
            });
#endif

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
