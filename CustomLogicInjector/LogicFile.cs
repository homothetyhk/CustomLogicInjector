using RandomizerMod.Settings;
using RandomizerCore.Logic;
using static RandomizerCore.Logic.LogicManagerBuilder;
using static RandomizerMod.LogHelper;

namespace CustomLogicInjector
{
    public class LogicFile
    {
        public string FileName;
        public float Priority;
        public JsonType JsonType;

        internal string directoryName;

        public void Apply(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            try
            {
                string filePath = Path.Combine(CustomLogicInjectorMod.ModDirectory, directoryName, FileName);
                using FileStream fs = File.OpenRead(filePath);
                lmb.DeserializeJson(JsonType, fs);
            }
            catch (Exception e)
            {
                LogError($"Error applying custom logic file {FileName} in subdirectory {directoryName}:\n{e}");
            }
        }

    }
}
