using Home.Neeo.Device.Validation;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home.Neeo.Device
{
    public class DataBase
    {
        const int       MAX_SEARCH_RESULTS = 10;
        const double    SEARCH_MATCHFACTOR = 0.5;
        public class SearchItem
        {
            public SearchItem (DeviceEntry entry, double score, double maxScore)
            {
                Item = entry;
                Score = score;
                MaxScore = maxScore;
            }
            [JsonProperty("item")]
            DeviceEntry Item        { get; }
            [JsonProperty("score")]
            double Score           { get; }
            [JsonProperty("maxScore")]
            double MaxScore        { get; }
        }
        public class DeviceEntry : ITokenSearch
        {
            [JsonConstructor]
            public DeviceEntry()
            {
            }
            public DeviceEntry(int index, NEEODevice adapter, NEEODevice.DevicePar device)
            {
                Id = index;
                AdapterName = adapter.AdapterName;
                Type = device.Type == DeviceType.TYPE.UNKNOWN ? adapter.Type : device.Type;
                Manufacturer = device.Manufacturer == null ? adapter.Manufacturer : device.Manufacturer;
                Name = device.Name;
                Tokens = device.Tokens == null ? string.Empty : string.Join(" ", device.Tokens);
                Device = device;
                Setup = adapter.Setup;
                Timing = adapter.Timing;
                Capabilities = adapter.Capabilities;
                DeviceCapabilities = adapter.DeviceCapabilities;
            }

            [JsonProperty("id")]
            public int                      Id                  { get; set; }
            [JsonProperty("adapterName")]
            public string                   AdapterName         { get; set; }
            [JsonProperty("name")]
            public string                   Name                { get; set; }
            [JsonProperty("type")]
            [JsonConverter(typeof(StringEnumConverter))]
            public DeviceType.TYPE          Type                { get; set; }
            [JsonProperty("manufacturer")]
            public string                   Manufacturer        { get; set; }
            [JsonProperty("tokens")]
            public string                   Tokens              { get; set; }
            [JsonProperty("device")]
            public NEEODevice.DevicePar     Device              { get; set; }
            [JsonProperty("setup")]
            public NEEODevice.SetupPar      Setup               { get; set; }
            [JsonProperty("timing")]
            public Timeing                  Timing              { get; set; }
            [JsonProperty("capabilities")]
            public Component[]              Capabilities        { get; set; }
            [JsonProperty("deviceCapabilities")]
            public string[]                 DeviceCapabilities  { get; set; }

            double              _score, _maxScore;
            HashSet<string>     _dataEntryTokens;
            double ITokenSearch.Score                           { get => _score; set => _score = value; }
            double ITokenSearch.MaxScore                        { get => _maxScore; set => _maxScore = value; }
            HashSet<string> ITokenSearch.DataEntryTokens        { get => _dataEntryTokens; set => _dataEntryTokens = value; }
            string ITokenSearch.GetKeyItem (string name)
            {
                switch (name)
                {
                    case "adaptername":     return Name;
                    case "manufacturer":    return Manufacturer;
                    case "name":            return Name;
                    case "type":            return Type.ToString();
                    case "tokens":          return Tokens;
                }
                return null;
            }

        }
        List<DeviceEntry>                   _devices;
        Dictionary<string, NEEODevice>      _deviceMap;
        HashSet<string>                     _initializedDevices;
        TokenSearch<DeviceEntry>            _tokenSearch;
        ILogger                             _logger;              
        internal DataBase (NEEODevice[] adapters)
        {
            int index = 0;
            _logger = NEEOEnvironment.Logger;

            _devices = new List<DeviceEntry>();
            _deviceMap = new Dictionary<string, NEEODevice>();

            foreach (var adapter in adapters)
            {
                _logger.LogTrace($"Database | build adapter {adapter} ");
                foreach (var device in adapter.Devices)
                {
                    DeviceEntry entry = new DeviceEntry(index++, adapter, device);
                    _devices.Add(entry);
                }
                _deviceMap.Add(adapter.AdapterName, adapter);
            }
            _initializedDevices = new HashSet<string>();
            _tokenSearch = new TokenSearch<DeviceEntry> (_devices, new TokenSearch<DeviceEntry>.Options
            {
                Unique = true,
                Delimiter = new char[] { ' ' },
                CollectionKeys = new string[] { "manufacturer", "name", "type", "tokens" },
                Threshold = SEARCH_MATCHFACTOR
            });
        }
        internal List<SearchItem> Search (string query)
        {
            var list = new List<SearchItem>();
            if (query != null)
            {
                var result = _tokenSearch.Search(query.ToLower());
                if (result.Count > MAX_SEARCH_RESULTS)
                {
                    result.RemoveRange(MAX_SEARCH_RESULTS, result.Count - MAX_SEARCH_RESULTS);
                }
                foreach (var entry in result)
                {
                    ITokenSearch token = entry as ITokenSearch;
                    list.Add(new SearchItem(entry, token.Score, token.MaxScore));
                }
            }
            return list;
        }
        internal DeviceEntry GetDevice(int databaseId)
        {
            if (databaseId >= _devices.Count)
            {
                throw new NEEOException($"INVALID_DEVICE_REQUESTED_{databaseId}");
            }
            _logger.LogTrace ($"Database | get device with id {databaseId}");
            return _devices[databaseId];
        }
        private async Task<bool> LazyInitController(NEEODevice device)
        {
            if (device == null || device.AdapterName == null)
            {
                return false;
            }
            string id = device.AdapterName;
            if (_initializedDevices.Contains (id))
            {
                return true;
            }
            if (device.InitializeFunction == null)
            {
                _logger.LogTrace($"Database | INIT_CONTROLLER_NOT_FOUND {id}");
                _initializedDevices.Add(id);
                return true;
            }
            _logger.LogTrace($"Database | INIT_CONTROLLER {id}");
            _initializedDevices.Add(id);

            bool rc = await device.InitializeFunction();
            if (!rc) { 
                _logger.LogWarning($"Database | INIT_CONTROLLER_FAILED {id}");
                _initializedDevices.Remove(id);
            }
            return rc;
        }
        internal async Task<NEEODevice> GetDeviceByAdapterId (string adapterId)
        {
            NEEODevice device;

            if (!_deviceMap.TryGetValue(adapterId, out device))
                return null;

            if (_initializedDevices.Contains(adapterId))
                return device;

            bool rc = await LazyInitController(device);
            if (rc)
                return device;

            return null;
        }

        internal static DataBase Build (NEEODevice[] devices)
        {
            return new DataBase(devices);
        }
    }
}
