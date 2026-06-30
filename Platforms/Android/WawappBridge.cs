using Android.Webkit;
using FWITD;

namespace FWShellMAUI.Platforms.Android;

public class WawappBridge : Java.Lang.Object {
    [JavascriptInterface]
    public void postMessage(string message) =>
        MainThread.BeginInvokeOnMainThread(async () =>
            await RequestDispatcher.HandleMessageAsync(message));
}
