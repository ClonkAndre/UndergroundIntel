using Newtonsoft.Json;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace UndergroundIntel.Classes.Json
{
    internal class Prompt
    {

        #region Variables
        [JsonIgnore] public string KeyEdit;
        public string Key;

        [JsonIgnore] public string ValueEdit;
        public string Value;
        #endregion

        #region Constructor
        public Prompt(string key, string value)
        {
            KeyEdit = string.Empty;
            Key = key;
            ValueEdit = string.Empty;
            Value = value;
        }
        public Prompt()
        {
            KeyEdit = string.Empty;
            Key = string.Empty;
            ValueEdit = string.Empty;
            Value = string.Empty;
        }
        #endregion

        public void Init()
        {
            KeyEdit = Key;
            ValueEdit = Value;
        }

    }
}

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value