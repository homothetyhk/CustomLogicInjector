namespace CustomLogicInjector
{
    public class LogicPack
    {
        public string Name;
        public List<LogicSetting>? Settings;
        public List<LogicFile> Files;

        public void Toggle(bool value)
        {
            CustomLogicInjectorMod.GS.ActivePacks[Name] = value;

            if (value)
            {
                LogicHookManager.AddPackHook(this);
            }
            else
            {
                LogicHookManager.RemovePackHook(this);
            }
        }
    }
}
