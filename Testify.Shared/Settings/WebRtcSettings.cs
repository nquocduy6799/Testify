namespace Testify.Shared.Settings
{
    /// <summary>
    /// Configuration for WebRTC ICE servers (STUN/TURN).
    /// Bound from appsettings.json "WebRtc" section.
    /// </summary>
    public class WebRtcSettings
    {
        public const string SectionName = "WebRtc";

        public List<IceServerConfig> IceServers { get; set; } = new();
    }

    public class IceServerConfig
    {
        public string Urls { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Credential { get; set; }
    }
}
