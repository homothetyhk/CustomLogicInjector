namespace CustomLogicInjector
{
    public class GlobalSettings
    {
        public Dictionary<string, bool> ActivePacks = new();
        public Dictionary<string, bool> ActiveSettings = new();

        public GlobalSettings GetDisplayableSettings()
        {
            Dictionary<string, bool> displayPacks = new();
            Dictionary<string, bool> displaySettings = new();

            foreach (LogicPack pack in CustomLogicInjectorMod.Packs)
            {
                if (!ActivePacks.TryGetValue(pack.Name, out bool active)) active = false;
                displayPacks[pack.Name] = active;

                if (pack.Settings != null && active)
                {
                    foreach (LogicSetting setting in pack.Settings)
                    {
                        if (!ActiveSettings.TryGetValue(setting.LogicName, out active)) active = false;
                        displaySettings[setting.LogicName] = active;
                    }
                }
            }

            return new()
            {
                ActivePacks = displayPacks,
                ActiveSettings = displaySettings,
            };
        }
    }
}
