using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapmakerControl
{
    internal interface IPrinterConnection
    {
        [Flags]
        public enum MovementAxis : short
        { 
            UNKNOWN = 0,
            X = 1,
            Y = 2,
            Z = 4
        }
        abstract bool IsConnected();
        abstract bool Connect();
        abstract bool Disconnect();
        abstract bool MovePrinter(MovementAxis axes, double amount);
    }
}
