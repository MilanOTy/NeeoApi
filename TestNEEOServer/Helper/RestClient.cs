using Microsoft.Extensions.Logging;
using Rts.Base.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rts.Base.Rest
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ApiAttribute : Attribute
    {
        public ApiAttribute()
        {
        }
        public ApiAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoutingAttribute : Attribute
    {
        public RoutingAttribute(string path)
        {
            Path = path;
        }
        public RoutingAttribute(string path, string verbs)
        {
            Path = path; Verbs = verbs;
        }

        public string Notes     { get; set; }
        public string Path      { get; set; }
        public string Summary   { get; set; }
        public string Verbs     { get; set; }
    }

    public class RestClient : IRestClient, IDisposable
    {
        public class ResultType<T>
        {
            public ResultType(T res)
            {
                Result = res;
            }
            public T Result { get; set; }
        }


        protected string                    _uri = "http://ricservice.cloudapp.net/api/";
        protected int                       _timeout;
        protected IJson                     _json;
        protected Type                      _routeAttributeType;
        protected PropertyInfo              _piVerb;
        protected PropertyInfo              _piPath;
        protected OnError                   _onError;
        protected HttpStatusCode            _lastStatus;
        protected Stopwatch                 _stopWatch;
        protected HttpClient                _httpClient;
        protected CancellationTokenSource   _cts;
        private   ILogger                   _logger;

        public delegate void OnError (HttpStatusCode status);

        public RestClient (IJson json, string uri = null, int timeout = 10, OnError onError = null, ILogger logger = null)
        {
            _json = json; _uri = uri; _routeAttributeType = typeof(RoutingAttribute);
            _logger = logger;
            _onError = onError;
            _lastStatus = HttpStatusCode.OK;
            _stopWatch = null;
            _httpClient = null;
            _timeout = timeout;
        }
        public RestClient(IJson json, Type routeAttrType, string uri = null, int timeout = 10, OnError onError = null, ILogger logger = null)
        {
            _json = json; _uri = uri; _routeAttributeType = routeAttrType;
            _logger = logger;
            _piVerb = _routeAttributeType.GetRuntimeProperty("Verbs");
            _piPath = _routeAttributeType.GetRuntimeProperty("Path");
            if (_piPath == null)
                _logger.LogError("RestClient | Illegal RouteAttribute Type");
            _onError = onError;
            _lastStatus = HttpStatusCode.OK;
            _stopWatch = null;
            _httpClient = null;
            _timeout = timeout;
        }
        public IJson Json => _json; 

        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }
        public bool MeasureDuration
        {
            get { return _stopWatch != null; }
            set
            {
                if (value)
                    _stopWatch = new Stopwatch();
                else
                    _stopWatch = null;
            }
        }
        public int LastDuration 
        {
            get { return _stopWatch == null ? -1 :  (int)_stopWatch.Elapsed.TotalMilliseconds; }
        }
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        public HttpStatusCode LastStatus
        {
            get { return _lastStatus; }
            protected set
            {
                if (_lastStatus != value)
                {
                    _lastStatus = value;
                    if (_onError != null) {
                        switch (value)
                        {
                            case HttpStatusCode.Continue:
                            case HttpStatusCode.OK:
                            case HttpStatusCode.Created:
                            case HttpStatusCode.Accepted:
                                break;
                            default:
                                _onError(value);
                                break;
                        }
                    }
                }
            }
        }

        private string ToString(object request, string propName)
        {
            Type ty = request.GetType();
            PropertyInfo pi = ty.GetRuntimeProperty(propName);
            return _json.SerializeValue(pi.GetValue(request));
        }

        public string CreateUriByRootAttribute(string baseUri, object request, string verb, string format)
        {
            Type ty = request.GetType();
            string path = null;
            foreach (object obj in ty.GetTypeInfo().GetCustomAttributes(_routeAttributeType, false)) 
            {
                string verbs = null;
                string lpath = null;
                if (_routeAttributeType == typeof(RoutingAttribute))
                {
                    RoutingAttribute att = obj as RoutingAttribute;
                    verbs = att.Verbs;
                    lpath = att.Path;
                }
                else
                {
                    object o = _piVerb.GetValue(obj);
                    verbs = o as String;
                    o = _piPath.GetValue(obj);
                    lpath = o as String;
                }
                if (verbs != null || verbs.Contains(verb))
                {
                    path = lpath; break;
                }
            }
            if (path == null)
                throw new Exception("Can't find route");

            return CreateUri(baseUri, request, verb, path, format);
        }

        public string CreateUri(string baseUri, object request, string verb, string apipath, string format)
        {
            Type ty = request.GetType();

            string[] eles = apipath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            List<string> done = new List<string>();
            bool first = true;

            sb.Append(baseUri);

            // Process path
            foreach (string ele in eles)
            {
                if (!first)
                    sb.Append('/');
                else
                    first = false;

                if (ele[0] == '{')
                {
                    string propName = ele.Substring(1, ele.Length - 2);
                    string value = ToString(request, propName);
                    if (propName == "ContentType" && value.Contains("/"))      // Oh god
                        value = value.Replace('/', '_');
                    sb.Append(value);
                    done.Add(propName);
                }
                else
                    sb.Append(ele);
            }

            bool firstQuery = true;
            bool isRequestStream = ty.GetRuntimeProperty("RequestStream") != null;
            if (verb != "POST" || isRequestStream)
            {
                // Process object properties
                foreach (PropertyInfo pi in ty.GetRuntimeProperties())
                {
                    object propValue = pi.GetValue(request);
                    if (propValue == null)
                        continue;

                    if (pi.PropertyType == typeof(Stream))
                        continue;

                    if (done.Contains(pi.Name))
                        continue;

                    string valueString = _json.SerializeValue(propValue);
                    if (string.IsNullOrEmpty(valueString))
                        continue;

                    if (firstQuery)
                    {
                        sb.Append('?');
                        firstQuery = false;
                    }
                    else
                        sb.Append('&');
                    sb.Append(pi.Name); sb.Append('='); sb.Append(valueString);
                }
            }
            if (firstQuery)
                sb.Append('?');
            else
                sb.Append('&');
            sb.Append("format="); sb.Append(format);
            return sb.ToString();
        }

        public async Task<TResp> HttpGet<TResp, TRequest>(TRequest request)
        {
            string uri = CreateUriByRootAttribute(_uri, request, "GET", "json");
            return await HttpGet<TResp>(uri);
        }

        public async Task<TResp> HttpGet<TResp>(string uri) 
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Restart();
                HttpClient client = GetHttpClient();
                using (HttpResponseMessage httpresp = await client.GetAsync(uri, _cts.Token))
                {
                    LastStatus = httpresp.StatusCode;
                    if (httpresp.IsSuccessStatusCode)
                    {
                        using (Stream strm = await httpresp.Content.ReadAsStreamAsync())
                        {
                            TResp resp = _json.Deserialize<TResp>(strm);
                            if (_stopWatch != null) _stopWatch.Stop();
                            return resp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpGet : {uri} : { ex.Message}");
            }
            return default(TResp);
        }
        public async Task<int> HttpGet(string uri, Func<Stream, int> processStream)
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Restart();
                HttpClient client = GetHttpClient();
                using (HttpResponseMessage httpresp = await client.GetAsync(uri, _cts.Token))
                {
                    LastStatus = httpresp.StatusCode;
                    if (httpresp.IsSuccessStatusCode)
                    {
                        using (Stream strm = await httpresp.Content.ReadAsStreamAsync())
                        {
                            int ret = processStream(strm);
                            if (_stopWatch != null) _stopWatch.Stop();
                            return ret;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpGet : {uri} : { ex.Message}");
            }
            return -1;
        }
        public async Task<string> HttpGetString(string uri) 
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                using (HttpResponseMessage httpresp = await client.GetAsync(uri, _cts.Token))
                {
                    LastStatus = httpresp.StatusCode;
                    if (httpresp.IsSuccessStatusCode)
                    {
                        string str = await httpresp.Content.ReadAsStringAsync();
                        if (httpresp.Content.Headers.ContentType.MediaType == "application/json") {
                            ResultType<string> result = _json.Deserialize<ResultType<string>>(str);
                            if (result != null)
                                str = result.Result;
                        }
                        if (_stopWatch != null) _stopWatch.Stop();
                        return str;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpGetString : {uri} : { ex.Message}");
            }
            return null;
        }

        public async Task<TResp> HttpPost<TResp, TRequest>(TRequest request)
        {
            string uri = CreateUriByRootAttribute(_uri, request, "POST", "json");
            return await HttpPost<TResp, TRequest>(request, uri);
        }
        public async Task<TResp> HttpPost<TResp, TRequest>(TRequest request, string uri) 
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                string jsonReq = _json.Serialize<TRequest>(request);
                using (HttpResponseMessage httpresp = await client.PostAsync(uri, new StringContent(jsonReq, Encoding.UTF8, "application/json"), _cts.Token))
                {
                    LastStatus = httpresp.StatusCode;
                    return await GetResult<TResp>(httpresp);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpPost : {uri} : { ex.Message}");
            }
            return default(TResp);
        }
        async Task<TResp> GetResult<TResp> (HttpResponseMessage httpresp)
        {
            TResp resp;
            if (httpresp.IsSuccessStatusCode)
            {
                if (typeof(TResp) == typeof(bool))
                {
                    bool result = false;
                    if (httpresp.Content.Headers.ContentLength == 0)
                        result = true;
                    else
                    {
                        using (Stream strm = await httpresp.Content.ReadAsStreamAsync())
                        {
                            ResultType<bool> boolresp = _json.Deserialize<ResultType<bool>>(strm);
                            result = boolresp.Result;
                        }
                    }
                    resp = (TResp)Convert.ChangeType(result, typeof(bool));
                }
                else if (typeof(TResp) == typeof(string) && httpresp.Content.Headers.ContentType.MediaType == "text/plain")
                {
                    string result = await httpresp.Content.ReadAsStringAsync();
                    resp = (TResp)Convert.ChangeType(result, typeof(string));
                }
                else
                {
                    using (Stream strm = await httpresp.Content.ReadAsStreamAsync())
                    {
                        resp = _json.Deserialize<TResp>(strm);
                    }
                }
            }
            else
                resp = default(TResp);
            if (_stopWatch != null) _stopWatch.Stop();
            return resp;
        }
        public async Task<TResp> HttpPost<TResp>(string uri)
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                using (HttpResponseMessage httpresp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri), _cts.Token))
                {
                    LastStatus = httpresp.StatusCode;
                    return await GetResult<TResp>(httpresp);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpPost : {uri} : { ex.Message}");
            }
            return default(TResp);
        }
        public async Task<int> HttpPost<TRequest>(TRequest request, string uri, Func<Stream, int> processStream)
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                string jsonReq = _json.Serialize<TRequest>(request);
                using (var stringContent = new StringContent(jsonReq, Encoding.UTF8, "application/json"))
                {
                    using (HttpResponseMessage httpresp = await client.PostAsync(uri, stringContent, _cts.Token))
                    {
                        LastStatus = httpresp.StatusCode;
                        if (httpresp.IsSuccessStatusCode)
                        {
                            using (Stream strm = await httpresp.Content.ReadAsStreamAsync())
                            {
                                int ret = processStream(strm);
                                if (_stopWatch != null) _stopWatch.Stop();
                                return ret;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpPost : {uri} : { ex.Message}");
            }
            return -1;
        }
        public async Task<TResp> HttpPostString<TResp>(string request, string uri) 
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                using (var stringContent = new StringContent(request, Encoding.UTF8, "text/plain"))
                {
                    using (HttpResponseMessage httpresp = await client.PostAsync(uri, stringContent, _cts.Token))
                    {
                        LastStatus = httpresp.StatusCode;
                        return await GetResult<TResp>(httpresp);
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpPostString : {uri} : { ex.Message}");
            }
            return default(TResp);
        }

        public async Task<TResp> HttpPut<TResp, TRequest>(TRequest request)
        {
            string uri = CreateUriByRootAttribute(_uri, request, "PUT", "json");
            return await HttpPut<TResp, TRequest>(request, uri);
        }
        public async Task<TResp> HttpPut<TResp, TRequest>(TRequest request, string uri) 
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                string jsonReq = _json.Serialize<TRequest>(request);
                using (var stringContent = new StringContent(jsonReq, Encoding.UTF8, "application/json"))
                {
                    using (HttpResponseMessage httpresp = await client.PutAsync(uri, stringContent, _cts.Token))
                    {
                        LastStatus = httpresp.StatusCode;
                        return await GetResult<TResp>(httpresp);
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpPut : {uri} : { ex.Message}");
            }
            return default(TResp);
        }

        public async Task<TResp> HttpDelete<TResp, TRequest>(TRequest request)
        {
            string uri = CreateUriByRootAttribute(_uri, request, "DELETE", "json");
            return await HttpDelete<TResp>(uri);
        }
        public async Task<TResp> HttpDelete<TResp>(string uri)
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                HttpClient client = GetHttpClient();
                using (HttpResponseMessage httpresp = await client.DeleteAsync(uri, _cts.Token))
                {
                    LastStatus = httpresp.StatusCode;
                    return await GetResult<TResp>(httpresp);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;          // UnMarshall error
                _logger.LogWarning($"RestClient | HttpDelete : {uri} : { ex.Message}");
            }
            return default(TResp);
        }

        public async Task<TResp> UploadFile<TResp, TRequest>(TRequest request, string contentType, Stream stream, int timeout = 60)
        {
            string uri = CreateUriByRootAttribute(Uri, request, "POST", "json");
            return await UploadFile<TResp>(uri, contentType, stream, timeout);
        }
        public async Task<TResp> UploadFile<TResp> (string uri, string contentType, Stream stream, int timeout = 60)
        {
            try
            {
                if (_stopWatch != null) _stopWatch.Start();
                using (HttpClient client = CreateHttpClient(true, timeout))
                {
                    using (StreamContent content = new StreamContent(stream))
                    {
                        if (!string.IsNullOrEmpty(contentType))
                            content.Headers.Add("Content-Type", contentType);

                        using (HttpResponseMessage httpresp = await client.PostAsync(uri, content, _cts.Token))
                        {
                            LastStatus = httpresp.StatusCode;
                            if (typeof(TResp) == typeof(bool) && httpresp.Content.Headers.ContentLength == 0)
                            {
                                if (_stopWatch != null) _stopWatch.Stop();
                                return (TResp)Convert.ChangeType(httpresp.IsSuccessStatusCode, typeof(bool));
                            }
                            if (httpresp.IsSuccessStatusCode)
                            {
                                using (Stream strm = await httpresp.Content.ReadAsStreamAsync())
                                {
                                    TResp resp = _json.Deserialize<TResp>(strm);
                                    if (_stopWatch != null) _stopWatch.Stop();
                                    return resp;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LastStatus = HttpStatusCode.NotAcceptable;
                _logger.LogWarning($"RestClient | UploadFile : {uri} : { ex.Message}");
            }
            return default(TResp);
        }

        public async Task<bool> DownloadFile<TRequest>(TRequest request, Stream stream, int timeout = 60)
        {
            string uri = CreateUriByRootAttribute(Uri, request, "GET", "json");
            return await DownloadFile (uri, stream, timeout);
        }
        public async Task<bool> DownloadFile (string uri, Stream destStream, int timeout = 60)
        {
            if (_stopWatch != null) _stopWatch.Start();
            bool rc;
            using (HttpClient client = CreateHttpClient(true, timeout))
            {
                rc = await DownloadStream(client, uri, destStream, _cts.Token);
            }
            if (_stopWatch != null) _stopWatch.Stop();
            return rc;
        }
        public static async Task<bool> DownloadStream(HttpClient client, string uri, Stream destStream, CancellationToken token, ILogger logger = null)
        {
            try
            {
                using (HttpResponseMessage httpresp = await client.GetAsync(uri, token))
                {
                    if (httpresp.IsSuccessStatusCode)
                    {
                        using (Stream str = await httpresp.Content.ReadAsStreamAsync())
                        {
                            if (str == null)
                                return false;
                            await str.CopyToAsync(destStream);
                            return true;
                        }
                    }
                    else
                        return false;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
        public static async Task<bool> DownloadStream(string uri, Stream destStream, CancellationToken token, int timeout = 60, ILogger logger = null)
        {
            using (HttpClient client = Util.CreateHttpClient(true, timeout))
            {
                return await DownloadStream(client, uri, destStream, token, logger);
            }
        }
        public static async Task<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> GetHeader(string uri, int timeout = 10, ILogger logger = null)
        {
            try
            {
                using (HttpClient client = Util.CreateHttpClient(true, timeout))
                {
                    using (HttpResponseMessage httpresp = await client.SendAsync(new HttpRequestMessage { Method = HttpMethod.Head, RequestUri = new Uri(uri) }))
                    {
                        if (httpresp.IsSuccessStatusCode)
                        {
                            if  (httpresp.Content == null)
                                return httpresp.Headers;
                            else
                                return httpresp.Content.Headers;
                        }
                    }
                    using (HttpResponseMessage httpresp = await client.GetAsync(uri))
                    {
                        if (httpresp.IsSuccessStatusCode)
                        {
                            return httpresp.Content.Headers;
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
        public bool Cancel ()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _logger.LogTrace("RestClient | Cancel");
                _cts = null;
                return true;
            }
            return false;
        }
        private void ProcessStatus(HttpStatusCode status)
        {
            _lastStatus = status;
            if (status != HttpStatusCode.OK && _onError != null)
                _onError(status);
        }

        virtual protected HttpClient GetHttpClient()
        {
            if (_httpClient == null)
                _httpClient = Util.CreateHttpClient(true, _timeout);
            if (_cts == null)
                _cts = new CancellationTokenSource();
            return _httpClient;
        }
        virtual protected HttpClient CreateHttpClient(bool noCache, int timeout)
        {
            return Util.CreateHttpClient(true, _timeout);
        }

        public virtual void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
