using System;

namespace Swm.Device.Rgv
{
    public class SRgvStateChangedEventArgs : EventArgs
    {
        public SRgvStateChangedEventArgs(SRgvState? previousState, SRgvState? newState)
        {
            this.PreviousState = previousState;
            this.NewState = newState;
        }

        public SRgvState? PreviousState { get; init; }

        public SRgvState? NewState { get; init; }
    }
}