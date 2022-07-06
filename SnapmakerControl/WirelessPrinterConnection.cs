using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SnapmakerControl
{
    internal class WirelessPrinterConnection : IPrinterConnection
    {
        private struct PrinterGCodeCommand
        {
            public string token;
            public string code;
        }

        public string TargetAddress {  get; }
        
        private readonly HttpClient httpClient = new HttpClient();

        // private const string connectionToken = "5992a781-9082-4ff4-8821-91fa0a0bd545";
        private string? connectionToken = null;

        public WirelessPrinterConnection(string targetAddress)
        {
            TargetAddress = targetAddress;
        }
        public bool Connect()
        {
            return InitialConnect().Result;
        }

        /*
         * Constructs POST request for initial connect/token and sends to Snapmaker API.
         */
        private async Task<bool> InitialConnect()
        {
            var targetUri = "http://" + TargetAddress + ":8080/api/v1/connect";

            try
            {
                /*
                 * Code commented out below was supposed to receive the token from the Snapmaker API,
                 * store it and then use it for the GCode execution requests. This did not work as it returned
                 * "401 access denied". Using the token that Luban uses seems to work, but may not work 
                 */
                
                var response = await httpClient.PostAsync(targetUri, null);
                string resp = await response.Content.ReadAsStringAsync();
                dynamic parsedResponse = JObject.Parse(resp);
                connectionToken = parsedResponse.token;
                Console.WriteLine("Got token: " + connectionToken);
                
                var content = new Dictionary<string, string>();
                content.Add("token", connectionToken);

                var httpResponse = await httpClient.PostAsync(targetUri, new FormUrlEncodedContent(content));

                return true;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            httpClient.Dispose();
        }

        /*
         * Constructs POST request for GCode and sends to Snapmaker API.
         */
        private async Task<bool> PostCommand(PrinterGCodeCommand command)
        {
            if (connectionToken == null)
                return false;

            var targetUri = "http://" + TargetAddress + ":8080/api/v1/execute_code";

            var content = new Dictionary<string, string>();
            content.Add("token", connectionToken);
            content.Add("code", command.code);

            var httpResponse = await httpClient.PostAsync(targetUri, new FormUrlEncodedContent(content));

            // Currently not interested in the response.
            if (httpResponse.Content != null)
            {
                _ = await httpResponse.Content.ReadAsStringAsync();
                return true;
            }

            return false;
        }

        /*
         *  Method to construct the GCode for the required movement.
         *  Moves the target axis/axes by the specified amount.
         */
        public bool MovePrinter(IPrinterConnection.MovementAxis axes, double amount)
        {
            if (!IsConnected() || connectionToken == null)
                return false;

            PrinterGCodeCommand relativeCommand;
            relativeCommand.token = connectionToken;
            relativeCommand.code = "G91";

            var relativeCommandResult = PostCommand(relativeCommand).Result;
            var moveCommandResult = true;

            List<string> commands = new List<string>();

            if (axes.HasFlag(IPrinterConnection.MovementAxis.X))
            {
                commands.Add("G0 X" + amount + " F1500");
            }
            if (axes.HasFlag(IPrinterConnection.MovementAxis.Y))
            {
                commands.Add("G0 Y" + amount + " F1500");
            }
            if (axes.HasFlag(IPrinterConnection.MovementAxis.Z))
            {
                commands.Add("G0 Z" + amount + " F1500");
            }

            foreach (var command in commands)
            { 
                PrinterGCodeCommand moveCommand;
                moveCommand.token = connectionToken;
                moveCommand.code = command;

                moveCommandResult = PostCommand(moveCommand).Result && moveCommandResult;
            }

            return moveCommandResult && relativeCommandResult;
        }

        public bool IsConnected()
        {
            return connectionToken != null;
        }
    }
}
