using RandomizerMod.Settings;
using RandomizerCore.Logic;
using static RandomizerCore.Logic.LogicManagerBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Modding;

namespace CustomLogicInjector
{
    [JsonConverter(typeof(LogicFileConverter))]
    public abstract class LogicFile
    {
        public string FileName;
        public float Priority;
        public JsonType JsonType;
        public abstract TextReader GetReader();

        public void Apply(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            try
            {
                using TextReader tr = GetReader();
                using JsonTextReader jtr = new(tr);
                lmb.DeserializeJson(JsonType, jtr);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error applying {GetType().Name} {FileName} of type {JsonType}", e);
            }
        }

        private class LogicFileConverter : JsonConverter
        {
            public override bool CanRead => true;
            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(LogicFile);
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                JObject o = JObject.Load(reader);
                if (o.TryGetValue(nameof(RemoteLogicFile.RawJson), StringComparison.InvariantCultureIgnoreCase, out _))
                {
                    RemoteLogicFile rlf = new();
                    serializer.Populate(new JTokenReader(o), rlf);
                    return rlf;
                }
                else
                {
                    LocalLogicFile llf = new();
                    serializer.Populate(new JTokenReader(o), llf);
                    return llf;
                }
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class RemoteLogicFile : LogicFile
    {
        public RemoteLogicFile() { }
        public RemoteLogicFile(LogicFile file)
        {
            FileName = file.FileName;
            Priority = file.Priority;
            JsonType = file.JsonType;
            using TextReader tr = file.GetReader();
            RawJson = tr.ReadToEnd();
        }

        public string RawJson;
        public override TextReader GetReader()
        {
            return new StringReader(RawJson);
        }
    }

    public class LocalLogicFile : LogicFile
    {
        internal string directoryName;

        public override TextReader GetReader()
        {
            string filePath = Path.Combine(CustomLogicInjectorMod.ModDirectory, directoryName, FileName);
            return new StreamReader(File.OpenRead(filePath));
        }
    }
}
