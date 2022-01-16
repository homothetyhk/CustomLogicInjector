using RandomizerMod.RC;

namespace CustomLogicInjector
{
    public class LogicPack
    {
        public string Name;
        public List<LogicSetting> Settings;
        public List<LogicFile> Files;

        public void Toggle(bool value)
        {
            CustomLogicInjectorMod.GS.ActivePacks[Name] = value;

            if (value)
            {
                foreach (LogicFile lf in Files) RCData.RuntimeLogicOverride.Subscribe(lf.Priority, lf.Apply);
            }
            else
            {
                foreach (LogicFile lf in Files) RCData.RuntimeLogicOverride.Unsubscribe(lf.Priority, lf.Apply);
            }
        }
    }
}
