using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace CustomLogicInjector
{
    internal static class LogicHookManager
    {
        private static readonly HashSet<string> subscribedPacks = new();

        public static void Setup()
        {
            RCData.RuntimeLogicOverride.Subscribe(-1000f, CreateSettingsTerms);
            ProgressionInitializer.OnCreateProgressionInitializer += InitializeSettings;
            foreach (LogicPack pack in CustomLogicInjectorMod.Packs)
            {
                if (CustomLogicInjectorMod.GS.ActivePacks.TryGetValue(pack.Name, out bool value) && value)
                {
                    AddPackHook(pack);
                }
            }
        }

        public static void AddPackHook(LogicPack pack)
        {
            if (!subscribedPacks.Add(pack.Name)) return;
            foreach (LogicFile lf in pack.Files) RCData.RuntimeLogicOverride.Subscribe(lf.Priority, lf.Apply);
        }

        public static void RemovePackHook(LogicPack pack)
        {
            if (!subscribedPacks.Remove(pack.Name)) return;
            foreach (LogicFile lf in pack.Files) RCData.RuntimeLogicOverride.Unsubscribe(lf.Priority, lf.Apply);
        }

        public static void CreateSettingsTerms(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            foreach (LogicPack pack in CustomLogicInjectorMod.Packs)
            {
                if (CustomLogicInjectorMod.GS.ActivePacks.TryGetValue(pack.Name, out bool value) && value)
                {
                    if (pack.Settings == null) continue;
                    foreach (LogicSetting setting in pack.Settings) lmb.GetOrAddTerm(setting.LogicName);
                }
            }
        }

        public static void InitializeSettings(LogicManager lm, GenerationSettings gs, ProgressionInitializer pi)
        {
            foreach (LogicPack pack in CustomLogicInjectorMod.Packs)
            {
                if (CustomLogicInjectorMod.GS.ActivePacks.TryGetValue(pack.Name, out bool value) && value)
                {
                    if (pack.Settings == null) continue;
                    foreach (LogicSetting setting in pack.Settings)
                    {
                        if (lm.TermLookup.TryGetValue(setting.LogicName, out Term t) && CustomLogicInjectorMod.GS.ActiveSettings.TryGetValue(setting.LogicName, out bool flag) && flag)
                        {
                            pi.Setters.Add(new(t, 1));
                        }
                    }
                }
            }
        }
    }
}
