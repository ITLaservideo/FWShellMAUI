using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.View;
using View = Android.Views.View;
using Firebase.Messaging;
using Java.Interop;
using Plugin.Firebase.CloudMessaging;

namespace FWShellMAUI {
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity {

        protected override void OnCreate(Android.OS.Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();
            AskNotificationPermission();
            ApplySystemBarInsets();
        }

        // MAUI's default Android theme draws edge-to-edge (content extends under the
        // status bar / notch and the bottom navigation/gesture bar). Pad the root content
        // view by the system bar insets so app content stays clear of those reserved areas.
        private void ApplySystemBarInsets() {
            var rootView = FindViewById(Android.Resource.Id.Content);
            ViewCompat.SetOnApplyWindowInsetsListener(rootView, new SystemBarsInsetsListener());
        }

        private sealed class SystemBarsInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener {
            public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets) {
                var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                v.SetPadding(systemBars.Left, systemBars.Top, systemBars.Right, systemBars.Bottom);
                return insets;
            }
        }
        protected override void OnNewIntent(Intent intent) {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private static void HandleIntent(Intent intent) {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }
        private void CreateNotificationChannelIfNeeded() {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                CreateNotificationChannel();
            }
        }
        private readonly AndroidX.Activity.Result.ActivityResultLauncher permissionLauncher;
                public MainActivity() {
            permissionLauncher = RegisterForActivityResult(
                new AndroidX.Activity.Result.Contract.ActivityResultContracts.RequestPermission(),
                new PermissionCallback(this)
            );
        }

        class PermissionCallback : Java.Lang.Object, AndroidX.Activity.Result.IActivityResultCallback {
            private readonly MainActivity activity;

            public PermissionCallback(MainActivity activity) {
                this.activity = activity;
            }

            public void OnActivityResult(Java.Lang.Object result) {
                bool isGranted = result.JavaCast<Java.Lang.Boolean>()!.BooleanValue();

                if (isGranted)
                    Android.Widget.Toast.MakeText(activity, "Notifications ✔", Android.Widget.ToastLength.Long).Show();
                else
                    Android.Widget.Toast.MakeText(activity, "Notifications disabled", Android.Widget.ToastLength.Long).Show();
            }
        }
        private void CreateNotificationChannel() {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channelId = $"{PackageName}.general";
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);

            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);

            FirebaseCloudMessagingImplementation.ChannelId = channelId;
        }
        
        private void AskNotificationPermission() {
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Tiramisu)
                return;

            var permission = Android.Manifest.Permission.PostNotifications;

            // Already granted
            if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(this, permission) == Android.Content.PM.Permission.Granted) {
                Toast.MakeText(this, "Notifications ✔", ToastLength.Short).Show();
                return;
            } else {
                Toast.MakeText(this, "Notifications are OFF.", ToastLength.Short).Show();
            }
            var prefs = GetSharedPreferences("app_prefs", FileCreationMode.Private);
            bool notdeclinedonce = !prefs.GetBoolean("declined_notifications_once", false);

            if (notdeclinedonce && ShouldShowRequestPermissionRationale(permission)) {
                new AlertDialog.Builder(this)
                    .SetTitle("Notification Permission Needed")
                    .SetMessage("This app needs notification permission to alert you about important events.")
                    .SetPositiveButton("Allow", (sender, args) => {
                        permissionLauncher.Launch(permission);
                    })
                    .SetNegativeButton("No thanks", (sender, args) => {
                        Toast.MakeText(this, "Notifications are OFF.", ToastLength.Short).Show();

                        // Save that the user declined once
                        var prefs = GetSharedPreferences("app_prefs", FileCreationMode.Private);
                        prefs.Edit().PutBoolean("declined_notifications_once", true).Apply();

                        notdeclinedonce = false;
                    })
                    .Show();

                return;
            }


            if (notdeclinedonce) {
                permissionLauncher.Launch(permission);
            }
        }
    }
}
