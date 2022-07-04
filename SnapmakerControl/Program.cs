using System;
using static SnapmakerControl.WirelessPrinterConnection;

namespace SnapmakerControl // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static IPrinterConnection? _printerConnection;

        static void Main(string[] args)
        {
            InputHandler inputHandler = new InputHandler();

            Console.WriteLine("Starting SnapmakerControl");
            Console.WriteLine("-------------------------");

            inputHandler.LeftThumbstickMoved += HandleLeftStick;
            inputHandler.RightThumbstickMoved += HandleRightStick;

            IPrinterConnection? connection = null;

            // WIFI connection supported, Serial TODO.
            // connection = new WirelessPrinterConnection("192.168.178.89");
            connection = new SerialConnection("COM4");

            ConnectToPrinter(connection);

            while (true)
            {
                inputHandler.HandleInput();
                Thread.Sleep(10);
            }
        }

        static void ConnectToPrinter(IPrinterConnection connection)
        {
            if (!connection.Connect())
            {
                Console.WriteLine("Connection to printer failed");
            }
            else
            {
                _printerConnection = connection;
            }
        }

        static void HandleLeftStick(object sender, InputHandler.ThumbstickMovedEventArgs e)
        {
            if(_printerConnection != null && _printerConnection.IsConnected())
            {
                if (e.ThumbstickState.HasFlag(InputHandler.ThumbstickState.LEFT))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.X, 0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(InputHandler.ThumbstickState.RIGHT))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.X, -0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(InputHandler.ThumbstickState.UP))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Y, 0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(InputHandler.ThumbstickState.DOWN))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Y, -0.05 * e.Velocity);                 
            }
        }

        static void HandleRightStick(object sender, InputHandler.ThumbstickMovedEventArgs e)
        {
            if (_printerConnection != null && _printerConnection.IsConnected())
            {
                if (e.ThumbstickState.HasFlag(InputHandler.ThumbstickState.UP))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Z, 0.05 * e.Velocity);
                if (e.ThumbstickState.HasFlag(InputHandler.ThumbstickState.DOWN))
                    _printerConnection.MovePrinter(IPrinterConnection.MovementAxis.Z, -0.05 * e.Velocity);
            }
        }
    }
}