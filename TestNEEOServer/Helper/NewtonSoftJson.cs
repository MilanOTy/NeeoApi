using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Rts.Base.Json
{
    public class NewtonSoftJson : IJson
    {
        JsonSerializer          _json;
        JsonSerializerSettings  _jsonSettings;

        public NewtonSoftJson(bool isoDate, bool localDate, bool useCamelCase, bool writePretty)
        {
            _jsonSettings = new JsonSerializerSettings();
            _jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            if (isoDate)
                _jsonSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            if (useCamelCase)
                _jsonSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            if (localDate)
                _jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            else
                _jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            if (writePretty)
                _jsonSettings.Formatting = Formatting.Indented;

            //_jsonSettings.DateParseHandling = DateParseHandling.DateTime;
            _json = JsonSerializer.Create(_jsonSettings);
        }

        public string SerializeValue(object value)
        {
            string str = JsonConvert.ToString(value);
            if (str[0] == '"')
                str = str.Substring(1, str.Length - 2);
            return str;
        }

        public T Deserialize<T>(Stream strm)
        {
            using (StreamReader reader = new StreamReader(strm))
            {
                return (T)_json.Deserialize (reader, typeof(T));
            }
        }

        public T Deserialize<T>(string str)
        {
            return JsonConvert.DeserializeObject<T> (str, _jsonSettings);
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _jsonSettings);
        }
        public void Serialize<T>(T value, Stream strm)
        {
            using (StreamWriter writer = new StreamWriter(strm, Util.Utf8Encoding, 16384, true))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.Formatting = _jsonSettings.Formatting;
                    _json.Serialize(jsonWriter, value);
                }
            }
        }
    }
}
