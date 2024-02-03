using RandomizerMod.Settings;
using RandomizerCore.Logic;
using RandomizerCore.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CustomLogicInjector
{
    [JsonConverter(typeof(LogicFileConverter))]
    public abstract class LogicFile
    {
        public string FileName;
        public float Priority;
        public LogicFileType JsonType;
        public abstract Stream GetData();

        public void Apply(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            try
            {
                lmb.DeserializeFile(JsonType, new JsonLogicFormat(), GetData());
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
            using StreamReader sr = new(file.GetData());
            RawJson = sr.ReadToEnd();
        }

        public string RawJson;
        public override Stream GetData()
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(RawJson));
        }
    }

    public class LocalLogicFile : LogicFile
    {
        internal string directoryName;

        public override Stream GetData()
        {
            string filePath = Path.Combine(CustomLogicInjectorMod.ModDirectory, directoryName, FileName);
            return File.OpenRead(filePath);
        }
    }
}
