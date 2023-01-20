namespace CustomLogicInjector
{
    public class LogicPack
    {
        public string Name;
        public List<LogicSetting>? Settings;
        public List<LogicFile> Files;

        public void Toggle(bool value)
        {
            CustomLogicInjectorMod.GS.SetPack(Name, value);

            if (value)
            {
                LogicHookManager.AddPackHook(this);
            }
            else
            {
                LogicHookManager.RemovePackHook(this);
            }
        }

        public LogicPack ToShareablePack()
        {
            LogicPack pack = new()
            {
                Name = Name,
                Settings = Settings,
                Files = new(),
            };
            foreach (LogicFile file in Files) 
            {
                if (file is RemoteLogicFile) pack.Files.Add(file);
                else pack.Files.Add(new RemoteLogicFile(file));
            }
            return pack;
        }

    }
}
