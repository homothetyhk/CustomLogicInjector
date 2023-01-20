using Modding;
using MonoMod.ModInterop;

namespace CustomLogicInjector
{
    internal static class SettingsInterop
    {
        [ModImportName("RandoSettingsManager")]
        internal static class RSMImport
        {
            public static Action<Mod, Type, Delegate, Delegate>? RegisterConnectionSimple = null;
            static RSMImport() => typeof(RSMImport).ModInterop();
        }

        public class RSMData
        {
            public RSMData() { }
            public RSMData(GlobalSettings gs)
            {
                ActiveSettings = new(gs.ActiveSettings);
                SharedPacks = CustomLogicInjectorMod.Packs.Where(p => gs.IsPackActive(p.Name))
                    .Select(p => p.ToShareablePack())
                    .ToList();
            }

            public List<LogicPack> SharedPacks;
            public HashSet<string> ActiveSettings;
        }

        internal static void Setup(Mod mod)
        {
            RSMImport.RegisterConnectionSimple?.Invoke(mod, typeof(RSMData), ReceiveSettings, SendSettings);
        }

        internal static void ReceiveSettings(RSMData? data)
        {
            MenuHolder.Instance.ToggleAllOff();
            LogicHookManager.Reset(); // unnecessary check for safety

            if (data is not null)
            {
                CustomLogicInjectorMod.GS.ActivePacks.Clear();
                CustomLogicInjectorMod.GS.ActiveSettings.Clear();
                CustomLogicInjectorMod.Packs.Clear();
                CustomLogicInjectorMod.Packs.AddRange(data.SharedPacks);
                CustomLogicInjectorMod.GS.ActivePacks.UnionWith(data.SharedPacks.Select(p => p.Name));
                CustomLogicInjectorMod.GS.ActiveSettings.UnionWith(data.ActiveSettings);
                MenuHolder.Instance.ReconstructMenu();
                MenuHolder.Instance.CreateRestoreLocalPacksButton();
                LogicHookManager.Setup();
            }
        }

        internal static RSMData? SendSettings()
        {
            return CustomLogicInjectorMod.GS.ActivePacks.Count > 0 ? (new(CustomLogicInjectorMod.GS)) : null;
        }
    }
}
