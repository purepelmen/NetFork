public enum StoppedReason : uint
{
    LocalStopped,
    RemotelyDisconnected = 0xF0,
    ServerClosing = 0xF1,
    RemoteError = 0xF2,
    Timeout
}
