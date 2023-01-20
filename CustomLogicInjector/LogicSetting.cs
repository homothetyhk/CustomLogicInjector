namespace CustomLogicInjector
{
    public class LogicSetting
    {
        public string MenuName;
        public string LogicName;

        public void Toggle(bool value)
        {
            CustomLogicInjectorMod.GS.SetSetting(LogicName, value);
        }
    }
}
