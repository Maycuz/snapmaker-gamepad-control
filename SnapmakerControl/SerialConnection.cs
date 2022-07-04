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
                Console.WriteLine("Failed to connect to COM4");
                return false;
            }
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return port.IsOpen;
        }

        private bool PostCommand(PrinterGCodeCommand command)
        {
            try
            {
                Console.WriteLine("Write: " + command.code);
                port.WriteLine(command.code);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
                return false;
            }
        }

        public bool MovePrinter(IPrinterConnection.MovementAxis axes, double amount)
        {
            if (!IsConnected())
                return false;

            PrinterGCodeCommand relativeCommand;
            relativeCommand.code = "G91";

            var relativeCommandResult = PostCommand(relativeCommand);
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

            relativeCommandResult = PostCommand(relativeCommand);

            return moveCommandResult && relativeCommandResult;
        }
    }
}
