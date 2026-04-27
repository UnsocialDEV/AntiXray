namespace AntiXray.Config;

public sealed class AntiXrayConfig
{
    public int HideAirExposedOreBelowY = 100;
    public bool RevealOreOnProspectingPick = false;
    public int ProspectingPickRevealRadius = 8;
    public string[] ProspectingPickCodePatterns = ["prospectingpick-*"];
    public int BlockBreakRevealDistance = 3;
    public bool BlockBreakRevealConnectedVein = true;
    public int BlockBreakRevealMaxOreBlocks = 128;
    public bool RevealAirExposedOreNearPlayers = true;
    public int PlayerAirExposedOreRevealDistance = 24;
    public int PlayerAirExposedOreRevealMaxPerEvent = 64;
    public int AdminCommandDefaultRadius = 16;
    public int AdminCommandMaxRadius = 64;
    public bool ConvertExistingChunks = true;
    public string[] OreCodePatterns = ["ore-*"];
    public string PlaceholderFallbackBlockCode = "";
    public bool DebugLoggingEnabled = false;
}
