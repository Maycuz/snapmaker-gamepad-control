using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.XInput;

namespace SnapmakerControl
{
    internal class GamepadInputHandler
    {
        // Max. positive value of a thumbstick axis (as defined by SharpDX.XInput).
        private const int thumbstickMaxValue = 32767;
        // A deadzone to make the thumbstick less "twitchy".
        private const int thumbstickCenterDeadzone = 5000;
        // Used to determine "velocity"; i.e. moving the thumbstick further means faster printer movement.
        private const int velocityDivisor = thumbstickMaxValue / 10;
        private Controller? inputController;

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

        public bool Connect()
        {
            inputController = new Controller(UserIndex.One);

            if (inputController.IsConnected)
            {
                Console.WriteLine("Controller is connected");
                return true;
            }
            else
            {
                Console.WriteLine("Controller not connected on init");
                return false;
            }
        }

        private Gamepad GetCurrentState(Controller controller)
        {
            return controller.GetState().Gamepad;
        }

        /*
         * Check how the thumbstick was moved. 
         * A thumbstick has an X and a Y axis. The axes range from -32768 to 32767. 
         * To give the user more precise control, combined X/Y movements are not supported, as this was found to introduce a lot of unintended movement.
         * This code checks which axis has most movement (X+, X-, Y+, Y-). The highest value "wins" and the corresponding axis 
         * is considered the direction the printer needs to move in.
         */
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

            if (xAxis > yAxis && xAxis > thumbstickCenterDeadzone)
            {
                if (xIsNegative)
                    currentState = swap ? ThumbstickState.RIGHT : ThumbstickState.LEFT;
                else
                    currentState = swap ? ThumbstickState.LEFT : ThumbstickState.RIGHT;

                velocity = xAxis;
            }
            else if (yAxis > thumbstickCenterDeadzone)
            {
                if (yIsNegative)
                    currentState = swap ? ThumbstickState.UP : ThumbstickState.DOWN;
                else
                    currentState = swap ? ThumbstickState.DOWN : ThumbstickState.UP;

                velocity = yAxis;
            }

            /*
             * Below some "napkin math" is used to translate velocity to movement. There's probably room for improvement.
             * 
             * High velocity example: 32000 / 3276.7 = ~9.7mm of movement on desired axis.
             * Low velocity example: 12500 / 3276.7 = ~3.8mm of movement on desired axis.
             */
            velocity /= velocityDivisor;

            return new ThumbstickMovedEventArgs(currentState, velocity);
        }

        public void HandleInput()
        {
            // TODO: At least add an error message print
            if (inputController == null || !inputController.IsConnected)
            {
                Console.WriteLine("Error: could not handle input (controller likely not connected)");
                return;
            }

            var padState = GetCurrentState(inputController);

            var leftThumbstickState = CheckThumbstick(padState.LeftThumbX, padState.LeftThumbY, true);
            var rightThumbstickState = CheckThumbstick(padState.RightThumbX, padState.RightThumbY, false);

            LeftThumbstickMoved?.Invoke(this, leftThumbstickState);
            RightThumbstickMoved?.Invoke(this, rightThumbstickState);
        }
    }
}
