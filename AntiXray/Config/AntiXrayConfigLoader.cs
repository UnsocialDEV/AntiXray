using Vintagestory.API.Server;

namespace AntiXray.Config;

public static class AntiXrayConfigLoader
{
    private const string ConfigFileName = "AntiXray.json";

    public static AntiXrayConfig Load(ICoreServerAPI api)
    {
        AntiXrayConfig config;

        try
        {
            config = api.LoadModConfig<AntiXrayConfig>(ConfigFileName) ?? new AntiXrayConfig();
        }
        catch
        {
            config = new AntiXrayConfig();
        }

        api.StoreModConfig(config, ConfigFileName);
        return config;
    }
}
