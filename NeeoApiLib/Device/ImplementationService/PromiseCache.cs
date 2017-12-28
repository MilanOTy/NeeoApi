using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Home.Neeo.Device.ImplementationService
{
    public class PromiseCache<T> where T : class
    {
        Func<Task<T>>   _getPromiseFunction;
        Task<T>         _data;
        int             _cacheDurationMs;
        DateTime        _cacheExpire;
        string          _uniqueIdentifier;

        public PromiseCache (int cacheDurationMs = 10000, string uniquIdentifier = null)
        {
            _uniqueIdentifier = (uniquIdentifier == null) ? Guid.NewGuid().ToString() : uniquIdentifier;
            _cacheDurationMs = cacheDurationMs;
            _cacheExpire = DateTime.MinValue;
            _data = null;
            _getPromiseFunction = null;
        }

        public Task<T> GetValue (Func<Task<T>> getPromiseFunction = null)
        {
            DateTime now = DateTime.UtcNow;
            if (_data != null && now < _cacheExpire)
            {
                NEEOEnvironment.Logger.LogTrace ($"PromiseCache | use cache{_uniqueIdentifier}");
                return _data;
            }
            _data = null;

            if (getPromiseFunction != null)
            {
                NEEOEnvironment.Logger.LogTrace($"PromiseCache | requested new data {_uniqueIdentifier}");
                _getPromiseFunction = getPromiseFunction;
                _cacheExpire = now.AddMilliseconds(_cacheDurationMs);
                return _data = _getPromiseFunction();
            }

            throw new NEEOException("NO_CALLBACK_FUNCTION_DEFINED");
        }
        public void Invalidate ()
        {
            _cacheExpire = DateTime.MinValue;
        }
        public static PromiseCache<T> BuildInstance (int cacheDurationMs = 10000, string uniquIdentifier = null) 
        {
            return new PromiseCache<T>(cacheDurationMs, uniquIdentifier);
        }
    }
}
