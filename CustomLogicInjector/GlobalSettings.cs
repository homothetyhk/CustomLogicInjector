using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomLogicInjector
{
    public class GlobalSettings
    {
        [JsonConverter(typeof(DictToSetConverter))] public HashSet<string> ActivePacks = new();
        [JsonConverter(typeof(DictToSetConverter))] public HashSet<string> ActiveSettings = new();

        public bool IsPackActive(string packName) => ActivePacks.Contains(packName);
        public void SetPack(string packName, bool value) => _ = value ? ActivePacks.Add(packName) : ActivePacks.Remove(packName);
        public bool IsSettingActive(string settingName) => ActiveSettings.Contains(settingName);
        public void SetSetting(string settingName, bool value) => _ = value ? ActiveSettings.Add(settingName) : ActiveSettings.Remove(settingName);

        public void CleanData()
        {
            ActivePacks.IntersectWith(CustomLogicInjectorMod.Packs.Select(p => p.Name));
            ActiveSettings.IntersectWith(CustomLogicInjectorMod.Packs.SelectMany(p => p.Settings?.Select(s => s.LogicName) ?? Enumerable.Empty<string>()));
        }

        public GlobalSettings GetDisplayableSettings()
        {
            GlobalSettings gs = new();

            foreach (LogicPack pack in CustomLogicInjectorMod.Packs)
            {
                bool active = IsPackActive(pack.Name);
                gs.SetPack(pack.Name, active);

                if (pack.Settings != null && active)
                {
                    foreach (LogicSetting setting in pack.Settings)
                    {
                        gs.SetSetting(setting.LogicName, IsSettingActive(setting.LogicName));
                    }
                }
            }

            return gs;
        }

        private class DictToSetConverter : JsonConverter<HashSet<string>>
        {
            public override bool CanRead => true;
            public override bool CanWrite => false;

            public override HashSet<string>? ReadJson(JsonReader reader, Type objectType, HashSet<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JToken jt = JToken.Load(reader);
                if (jt is JArray ja) return new(ja.Select(t => t.Value<string>()));
                else if (jt is JObject jo) return new(jo.ToObject<Dictionary<string, bool>>().Where(kvp => kvp.Value).Select(kvp => kvp.Key));
                else return null;
            }

            public override void WriteJson(JsonWriter writer, HashSet<string>? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
