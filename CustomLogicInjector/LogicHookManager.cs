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
                if (CustomLogicInjectorMod.GS.IsPackActive(pack.Name))
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

        public static void Reset()
        {
            foreach (LogicPack p in CustomLogicInjectorMod.Packs) RemovePackHook(p);
            if (subscribedPacks.Count > 0) throw new InvalidOperationException("Unable to locate subscribed packs: " + string.Join(", ", subscribedPacks));
        }

        public static void CreateSettingsTerms(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            foreach (LogicPack pack in CustomLogicInjectorMod.Packs)
            {
                if (CustomLogicInjectorMod.GS.IsPackActive(pack.Name))
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
                if (CustomLogicInjectorMod.GS.IsPackActive(pack.Name))
                {
                    if (pack.Settings == null) continue;
                    foreach (LogicSetting setting in pack.Settings)
                    {
                        if (CustomLogicInjectorMod.GS.IsSettingActive(setting.LogicName))
                        {
                            Term t = lm.GetTermStrict(setting.LogicName);
                            pi.Setters.Add(new(t, 1));
                        }
                    }
                }
            }
        }
    }
}
