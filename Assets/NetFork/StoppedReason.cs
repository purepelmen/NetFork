public enum StoppedReason : uint
{
    LocalStopped,
    Timeout,

    RemotelyDisconnected = 0xF0,
    ServerClosing = 0xF1,
    RemoteError = 0xF2
}
