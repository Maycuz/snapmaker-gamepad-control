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
        private string? connectionToken = null;

        public WirelessPrinterConnection(string targetAddress)
        {
            TargetAddress = targetAddress;
        }
        public bool Connect()
        {
            return InternalConnect().Result;
        }

        private async Task<bool> InternalConnect()
        {
            var targetUri = "http://" + TargetAddress + ":8080/api/v1/connect";

            try
            {
/*                var response = await httpClient.PostAsync(targetUri, null);
                string resp = await response.Content.ReadAsStringAsync();

                dynamic parsedResponse = JObject.Parse(resp);

                connectionToken = parsedResponse.token;

                Console.WriteLine("Got token: " + connectionToken);*/

                connectionToken = "5992a781-9082-4ff4-8821-91fa0a0bd545";

                var content = new Dictionary<string, string>();
                content.Add("token", connectionToken);

                // Do the actual request and await the response
                var httpResponse = await httpClient.PostAsync(targetUri, new FormUrlEncodedContent(content));

                return true;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        private async Task<bool> PostCommand(PrinterGCodeCommand command)
        {
            var targetUri = "http://" + TargetAddress + ":8080/api/v1/execute_code";

            var content = new Dictionary<string, string>();
            //content.Add("token", command.token);
            content.Add("token", "5992a781-9082-4ff4-8821-91fa0a0bd545");
            content.Add("code", command.code);

            //var httpContent = new FormUrlEncodedContent(content);

            // Do the actual request and await the response
            var httpResponse = await httpClient.PostAsync(targetUri, new FormUrlEncodedContent(content));

            // If the response contains content we want to read it!
            if (httpResponse.Content != null)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                return true;
            }

            return false;
        }

        public bool MovePrinter(IPrinterConnection.MovementAxis axes, double amount)
        {
            if (!IsConnected())
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
