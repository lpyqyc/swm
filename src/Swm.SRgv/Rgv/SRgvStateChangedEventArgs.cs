namespace Swm.Device.Rgv
{
    public record SRgvStateChangedEventArgs(SRgvState? PreviousState, SRgvState NewState)
    {
        public SRgvState? PreviousState { get; init; } = PreviousState;

        public SRgvState NewState { get; init; } = NewState;
    }
}