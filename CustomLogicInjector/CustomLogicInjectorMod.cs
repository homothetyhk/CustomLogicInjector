using MenuChanger;
using Modding;
using Newtonsoft.Json;
using RandomizerCore.Json;
using RandomizerMod;

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
            LoadFiles();
            LogicHookManager.Setup();
            RandomizerMod.Logging.SettingsLog.AfterLogSettings += LogSettings;
        }

        public override string GetVersion()
        {
            return "1.0.2";
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

        private static void LogSettings(RandomizerMod.Logging.LogArguments arg1, TextWriter tw)
        {
            tw.WriteLine("Logging CustomLogicInjector settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false, };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, GS.GetDisplayableSettings());
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
