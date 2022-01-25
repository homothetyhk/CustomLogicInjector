using MenuChanger;
using Modding;
using Newtonsoft.Json;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerMod;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace CustomLogicInjector
{
    public class CustomLogicInjectorMod : Mod, IGlobalSettings<GlobalSettings>
    {
        public static string ModDirectory { get; }
        public static GlobalSettings GS { get; private set; } = new();
        public static readonly List<LogicPack> Packs = new();

        public override void Initialize()
        {
            MenuChangerMod.OnExitMainMenu += MenuHolder.OnExitMenu;
            RandomizerMod.Menu.RandomizerMenuAPI.AddMenuPage(MenuHolder.ConstructMenu, MenuHolder.TryGetMenuButton);
            RCData.RuntimeLogicOverride.Subscribe(-1000f, CreateSettingsTerms);
            ProgressionInitializer.OnCreateProgressionInitializer += InitializeSettings;
            LoadFiles();
            RandomizerMod.Logging.SettingsLog.AfterLogSettings += LogSettings;
        }

        public override string GetVersion()
        {
            return "1.0.0";
        }

        public static void LoadFiles()
        {
            DirectoryInfo main = new(ModDirectory);

            foreach (DirectoryInfo di in main.EnumerateDirectories())
            {
                Dictionary<string, FileInfo> jsons = di.GetFiles("*.json").ToDictionary(fi => fi.Name.Substring(0, fi.Name.Length - 5).ToLower());
                if (jsons.TryGetValue("pack", out FileInfo fi))
                {
                    try
                    {
                        using StreamReader sr = new(fi.OpenRead());
                        using JsonTextReader jtr = new(sr);
                        LogicPack pack = JsonUtil.Deserialize<LogicPack>(jtr);
                        foreach (LogicFile lf in pack.Files) lf.directoryName = di.Name;
                        Packs.Add(pack);
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError($"Error deserializing pack.json in subdirectory {di.Name}:\n{e}");
                    }
                }
            }
        }

        public static void CreateSettingsTerms(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            foreach (LogicPack pack in Packs)
            {
                if (GS.ActivePacks.TryGetValue(pack.Name, out bool value) && value)
                {
                    if (pack.Settings == null) continue;
                    foreach (LogicSetting setting in pack.Settings) lmb.GetOrAddTerm(setting.LogicName);
                }
            }
        }

        public static void InitializeSettings(LogicManager lm, GenerationSettings gs, ProgressionInitializer pi)
        {
            foreach (LogicPack pack in Packs)
            {
                if (GS.ActivePacks.TryGetValue(pack.Name, out bool value) && value)
                {
                    if (pack.Settings == null) continue;
                    foreach (LogicSetting setting in pack.Settings)
                    {
                        if (lm.TermLookup.TryGetValue(setting.LogicName, out Term t) && GS.ActiveSettings.TryGetValue(setting.LogicName, out bool flag) && flag)
                        {
                            pi.Setters.Add(new(t, 1));
                        }
                    }
                }
            }
        }

        private static void LogSettings(RandomizerMod.Logging.LogArguments arg1, TextWriter tw)
        {
            tw.WriteLine("Logging CustomLogicInjector settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false, };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, GS);
            tw.WriteLine();
        }

        void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings s)
        {
            GS = s ?? new();
        }

        GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal()
        {
            return GS;
        }

        static CustomLogicInjectorMod()
        {
            ModDirectory = Path.GetDirectoryName(typeof(CustomLogicInjectorMod).Assembly.Location);
        }
    }
}
