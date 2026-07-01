using DotNet.Utility;
using FWITD;

namespace FWShellMAUI {

    public partial class App : Application {
        public static StartApp RequestedStartApp { get; private set; } =
            AppSettings.Get<StartApp>("App.RequestedStartApp", StartApp.Dashboard);

        public App() {
            InitializeComponent();
            ParseCommandLineArgs();
#if false
            SQL.Init();
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            return new Window(new MainPage());
        }

        private static void ParseCommandLineArgs() {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) {
                if (args[i] == "--start-app" && int.TryParse(args[i + 1], out int value) && Enum.IsDefined(typeof(StartApp), value)) {
                    RequestedStartApp = (StartApp)value;
                    break;
                }
            }
        }
    }
}
