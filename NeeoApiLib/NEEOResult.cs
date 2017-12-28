using Newtonsoft.Json;
using System;

namespace Home.Neeo
{
    public class SuccessResult
    {
        public SuccessResult(bool success)
        {
            Success = success;
        }
        [JsonProperty("success")]
        public bool Success { get; }
    }
    public class ValueResult
    {
        public ValueResult(object value)
        {
            if (value == null)
                Value = string.Empty;
            else if (value.GetType() == typeof(bool))
            {
                value = Convert.ToBoolean(value) ? "true" : "false";
            }
            else
            {
                Value = value.ToString();
            }
        }
        [JsonProperty("value")]
        public string Value { get; }
    }
}
