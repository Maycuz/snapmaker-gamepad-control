using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.XInput;

namespace SnapmakerControl
{
    internal class InputHandler
    {
        private const int threshold = 30000;
        private Controller inputController;

        public class ThumbstickMovedEventArgs : EventArgs
        {
            public ThumbstickMovedEventArgs(ThumbstickState state, double velocity) { ThumbstickState = state; Velocity = velocity; }
            public ThumbstickState ThumbstickState { get; }
            public double Velocity { get; }
        }

        public delegate void ThumbstickMovedEventHandler(object sender, ThumbstickMovedEventArgs e);
        
        public event ThumbstickMovedEventHandler LeftThumbstickMoved = delegate { };
        public event ThumbstickMovedEventHandler RightThumbstickMoved = delegate { };

        [Flags]
        public enum ThumbstickState : short
        {
            IDLE = 0,
            UP = 1,
            DOWN = 2,
            LEFT = 4,
            RIGHT = 8
        }

        public InputHandler()
        {
            inputController = new Controller(UserIndex.One);

            if (inputController.IsConnected)
                Console.WriteLine("Controller is connected");
            else
                Console.WriteLine("Controller not connected on init");
        }

        private Gamepad GetCurrentState(Controller controller)
        {
            return controller.GetState().Gamepad;
        }

        private ThumbstickMovedEventArgs CheckThumbstick(short thumbstickValueX, short thumbstickValueY, bool swap)
        {
            ThumbstickState currentState = ThumbstickState.IDLE;

            double velocity = 1;

            var xAxis = (int)thumbstickValueX;
            var yAxis = (int)thumbstickValueY;

            var xIsNegative = xAxis < 0;
            var yIsNegative = yAxis < 0;

            if (xIsNegative)
                xAxis = Math.Abs(xAxis);
            if (yIsNegative)
                yAxis = Math.Abs(yAxis);

            if (xAxis > yAxis && xAxis > 5000)
            {
                if (xIsNegative)
                    currentState = swap ? ThumbstickState.RIGHT : ThumbstickState.LEFT;
                else
                    currentState = swap ? ThumbstickState.LEFT : ThumbstickState.RIGHT;

                velocity = xAxis;
            }
            else if (yAxis > 5000)
            {
                if (yIsNegative)
                    currentState = swap ? ThumbstickState.UP : ThumbstickState.DOWN;
                else
                    currentState = swap ? ThumbstickState.DOWN : ThumbstickState.UP;

                velocity = yAxis;
            }

            // Magic number :)
            velocity /= 3200;

            Console.WriteLine(velocity);

            return new ThumbstickMovedEventArgs(currentState, velocity);
        }

        // Should be subscribed to an event?
        public void HandleInput()
        {
            if (!inputController.IsConnected)
                return;

            var padState = GetCurrentState(inputController);

            var leftThumbstickState = CheckThumbstick(padState.LeftThumbX, padState.LeftThumbY, true);
            var rightThumbstickState = CheckThumbstick(padState.RightThumbX, padState.RightThumbY, false);

            LeftThumbstickMoved?.Invoke(this, leftThumbstickState);
            RightThumbstickMoved?.Invoke(this, rightThumbstickState);
        }
    }
}
