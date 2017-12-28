using System.Collections.Generic;

namespace Home.Neeo.Device.ImplementationService
{
    public class DeviceState<T> where T : class
    {
        private static int                          _cachetimeMs;
        private Dictionary<int, DeviceEntry>        _deviceMap;
        public class DeviceEntry
        {
            public DeviceEntry (int id, T clientObject, bool reachable)
            {
                Id = id;
                ClientObject = clientObject;
                PromiseCache = PromiseCache<T>.BuildInstance(_cachetimeMs, $"NPC-{id}");
                Reachable = reachable;
            }
            int                         Id             { get; }
            public T                    ClientObject   { get; }
            public PromiseCache<T>      PromiseCache   { get; }
            public bool                 Reachable      { get; set; }
        }
        public DeviceState (int cacheTimeMs = 2000)
        {
            _cachetimeMs = cacheTimeMs;
            _deviceMap = new Dictionary<int, DeviceEntry>();
        }
        public void AddDevice (int id, T clientObject, bool reachable = true)
        {
            var entry = new DeviceEntry(id, clientObject, reachable);
            _deviceMap[id] = entry; 
        }

        public IEnumerable<DeviceEntry> GetAllDevices ()
        {
            return _deviceMap.Values;
        }
        public bool IsDeviceRegistered(int id)
        {
            return _deviceMap.ContainsKey(id);
        }
        public bool IsReachable(int id)
        {
            DeviceEntry entry;
            if (_deviceMap.TryGetValue (id, out entry))
                return entry.Reachable;
            return false;
        }
        public T GetClientObjectIfReachable(int id)
        {
            DeviceEntry entry;
            if (_deviceMap.TryGetValue(id, out entry) && entry.Reachable)
                return entry.ClientObject;
            return null;
        }
        public PromiseCache<T> GetCachePromise (int id)
        {
            DeviceEntry entry;
            if (_deviceMap.TryGetValue(id, out entry) && entry.Reachable)
                return entry.PromiseCache;
            return null;
        }
        public void UpdateReachable(int id, bool reachable)
        {
            DeviceEntry entry;
            if (_deviceMap.TryGetValue(id, out entry))
                entry.Reachable = reachable;
        }
        public static DeviceState<T> BuildInstance(int cacheTimeMs = 2000)
        {
            return new DeviceState<T>(cacheTimeMs);
        }
    }
}

