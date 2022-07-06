using CommandLine;

namespace SnapmakerControl
{
    internal class Program
    {
        public class Options
        {
            // [Option('w', "wlan", Required = false, HelpText = "Set IP address of the printer if connecting over wlan. Example: \"192.168.178.89\".")]
            // public string? TargetURI { get; set; }

            [Option('s', "serial", Required = false, HelpText = "Set COM port of the printer if connecting over serial. Example: \"COM4\".")]
            public string? TargetSerialPort { get; set; }
        }

        static IPrinterConnection? _printerConnection;
        static Options? _userOptions;

        static void Main(string[] args)
        {
            _userOptions = new Options();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ProcessParsedArgs)
                .WithNotParsed(ProcessNonParsedArgs);

            if (_userOptions.TargetSerialPort != null)
            {
                _printerConnection = new SerialConnection(_userOptions.TargetSerialPort);
            }
            // else if(_userOptions.TargetURI != null)
            // {
            //     _printerConnection = new WirelessPrinterConnection(_userOptions.TargetURI);
            // }

            if (_printerConnection != null)
            {
                Console.WriteLine("Starting Snapmaker gamepad control...");

                GamepadInputHandler gamepadInputHandler = new GamepadInputHandler();

                if (!gamepadInputHandler.Connect())
                {
                    Console.WriteLine("Could not find controller. Make sure it's connected.");
                    Environment.Exit(1);
                }

                gamepadInputHandler.LeftThumbstickMoved += HandleLeftStick;
                gamepadInputHandler.RightThumbstickMoved += HandleRightStick;

                if (!_printerConnection.Connect())
                {
                    Console.WriteLine("Connection to printer failed.");
                    Environment.Exit(1);
                }

                while (true)
                {
                    // TODO: Exit button
                    gamepadInputHandler.HandleInput();
                    Thread.Sleep(10);
                }
            }
            else
            {
                Console.WriteLine("No printer connection specified. Please check \"--help\" for supported arguments.");
                Console.WriteLine("Please be aware that connecting to your printer over WiFi is not (yet!) supported.");
            }
        }

        static void HandleLeftStick(object sender, GamepadInputHandler.ThumbstickMovedEventArgs e)
        {
            if (_printerConnection != null && _printerConnection.IsConnected())
            {
                if (e.ThumbstickState.HasFlag(GamepadInputHandler.ThumbstickState.LEFT))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.X, 0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(GamepadInputHandler.ThumbstickState.RIGHT))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.X, -0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(GamepadInputHandler.ThumbstickState.UP))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Y, 0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(GamepadInputHandler.ThumbstickState.DOWN))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Y, -0.05 * e.Velocity);
            }
        }

        static void HandleRightStick(object sender, GamepadInputHandler.ThumbstickMovedEventArgs e)
        {
            if (_printerConnection != null && _printerConnection.IsConnected())
            {
                if (e.ThumbstickState.HasFlag(GamepadInputHandler.ThumbstickState.UP))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Z, 0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(GamepadInputHandler.ThumbstickState.DOWN))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Z, -0.05 * e.Velocity);
            }
        }

        static void ProcessParsedArgs(Options options)
        {
            _userOptions = options;
        }
        static void ProcessNonParsedArgs(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);
                if (error.Tag == ErrorType.HelpRequestedError ||
                    error.Tag == ErrorType.VersionRequestedError)
                    Environment.Exit(0);
            }
        }
    }
}