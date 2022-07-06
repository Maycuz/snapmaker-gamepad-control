using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapmakerControl
{
    internal class SerialConnection : IPrinterConnection
    {
        private struct PrinterGCodeCommand
        {
            public string code;
        }

        private string comPort = "";
        private SerialPort? port;

        public SerialConnection(string port)
        {
            comPort = port;
        }

        /*
         *  Connect using Snapmaker's default serial port settings.
         */
        public bool Connect()
        {
            try
            {
                port = new SerialPort(comPort,
                    115200, Parity.None, 8, StopBits.One);
                port.Open();
                return true;
            }
            catch
            {
                Console.WriteLine("Failed to connect to " + comPort);
                return false;
            }
        }

        public void Disconnect()
        {
            if(port != null)
                port.Close();
        }

        public bool IsConnected()
        {
            if (port != null)
                return port.IsOpen;
            else
                return false;
        }

        /*
         *  Send the constructed GCode over the serial port.
         *  Exceptions are printed rather than thrown; as they may be "non-fatal".
         */
        private bool PostCommand(PrinterGCodeCommand command)
        {
            if (port == null || !IsConnected())
                return false; 

            try
            {
                // Console.WriteLine("Write: " + command.code);
                port.WriteLine(command.code);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
                return false;
            }
        }

        /*
         *  Method to construct the GCode for the required movement.
         *  Moves the target axis/axes by the specified amount.
         *  Note: Not awaiting/checking OK's from the printer, no need to resend a command given the update rate over serial.
         */
        public bool MovePrinter(IPrinterConnection.MovementAxis axes, double amount)
        {
            if (port == null || !IsConnected())
                return false;

            PrinterGCodeCommand relativeCommand;
            relativeCommand.code = "G91";

            var firstRelativeCommandResult = PostCommand(relativeCommand);
            var moveCommandResult = true;

            List<string> commands = new List<string>();

            if (axes.HasFlag(IPrinterConnection.MovementAxis.X))
            {
                commands.Add("G0 X" + amount.ToString(new CultureInfo("en-US")) + " F1500");
            }
            if (axes.HasFlag(IPrinterConnection.MovementAxis.Y))
            {
                commands.Add("G0 Y" + amount.ToString(new CultureInfo("en-US")) + " F1500");
            }
            if (axes.HasFlag(IPrinterConnection.MovementAxis.Z))
            {
                commands.Add("G0 Z" + amount.ToString(new CultureInfo("en-US")) + " F1500");
            }

            foreach (var command in commands)
            {
                PrinterGCodeCommand moveCommand;
                moveCommand.code = command;
                moveCommandResult = PostCommand(moveCommand);
            }

            relativeCommand.code = "G90";
            var secondRelativeCommandResult = PostCommand(relativeCommand);

            return moveCommandResult && firstRelativeCommandResult && secondRelativeCommandResult;
        }
    }
}
