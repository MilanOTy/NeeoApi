using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Xml;

namespace Rts.Base
{
    public class Util
    {
        protected static XmlWriterSettings xmlWriterSettings;
        protected static XmlWriterSettings xmlWriterSettingsFragment;
        protected static XmlReaderSettings xmlReaderSettings;
        protected static XmlParserContext xmlParserContext;
        protected static string soapSchema;
        protected static Encoding utf8Encoding;
        protected static CacheControlHeaderValue noCacheCtrl;
        protected static string[] iso8601DateFormats;
        static long lastTimeStamp;

        static Util()
        {
            utf8Encoding = new UTF8Encoding(false);
            lastTimeStamp = DateTime.UtcNow.Ticks;

            xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = utf8Encoding;
            xmlWriterSettings.NamespaceHandling = NamespaceHandling.OmitDuplicates;

            xmlWriterSettingsFragment = new XmlWriterSettings();
            xmlWriterSettingsFragment.Encoding = utf8Encoding;
            xmlWriterSettingsFragment.NamespaceHandling = NamespaceHandling.OmitDuplicates;
            xmlWriterSettingsFragment.ConformanceLevel = ConformanceLevel.Fragment;

            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreWhitespace = true;
            xmlReaderSettings.CheckCharacters = false;

            xmlParserContext = new XmlParserContext(null, null, "en", XmlSpace.Default, utf8Encoding);

            noCacheCtrl = new CacheControlHeaderValue { NoCache = true, NoStore = true };

            iso8601DateFormats = new string[] {
                "yyyy-MM-ddTHH:mm:ss.FFFzzz",
                "yyyy-MM-ddTHH:mm:ss.FFFzz",
                "yyyy-MM-ddTHH:mm:ss.FFFZ",
                "yyyy-MM-ddTHH:mm:ss.FFFFZ",
                "yyyy-MM-ddTHH:mm:ss.FFFFFZ",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFZ",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
                // Extended formats
                "yyyy-MM-ddTHH:mm:sszzz",
                "yyyy-MM-ddTHH:mm:sszz",
                "yyyy-MM-ddTHH:mm:ssZ",
                // All of the above with reduced accuracy
                "yyyy-MM-ddTHH:mmzzz",
                "yyyy-MM-ddTHH:mmzz",
                "yyyy-MM-ddTHH:mmZ",
                };
        }

        public static Encoding Utf8Encoding
        {
            get { return utf8Encoding; }
        }

        public static Stream StringToStream(string str)
        {
            byte[] data = utf8Encoding.GetBytes(str);
            MemoryStream strm = new MemoryStream(data);
            return strm;
        }
        public static String StreamToString(Stream strm, int maxLength = 16384)
        {
            if (strm.CanSeek)
            {
                strm.Position = 0;
                byte[] data = new byte[strm.Length];
                strm.Read(data, 0, (int)strm.Length);
                return utf8Encoding.GetString(data, 0, data.Length);
            }
            else
            {
                byte[] data = new byte[maxLength];
                int c, count = 0;

                while ((c = strm.ReadByte()) >= 0)
                {
                    if (count >= maxLength)
                        throw new Exception("StreamToString: too long");
                    data[count++] = (byte)c;
                }
                return utf8Encoding.GetString(data, 0, count);
            }
        }

        public static XmlReader GetXmlReader(Stream strm)
        {
            return XmlReader.Create(strm, xmlReaderSettings, xmlParserContext);
        }

        public static XmlWriter GetXmlWriter(Stream strm, bool fragment = false)
        {
            return XmlWriter.Create(strm, fragment ? xmlWriterSettingsFragment : xmlWriterSettings);
        }
        public static HttpClient CreateHttpClient(CacheControlHeaderValue cacheControl, int timeout = 6, int responseBufferSize = 0)
        {
            HttpClient hc = new HttpClient();
            if (cacheControl != null)
                hc.DefaultRequestHeaders.CacheControl = cacheControl;
            if (timeout > 0)
                hc.Timeout = TimeSpan.FromSeconds(timeout);
            if (responseBufferSize > 0)
                hc.MaxResponseContentBufferSize = responseBufferSize;
            return hc;
        }
        public static HttpClient CreateHttpClient(bool noCache = true, int timeout = 6, int responseBufferSize = 0)
        {
            return CreateHttpClient(noCache ? noCacheCtrl : null, timeout, responseBufferSize);
        }
        public static HttpClient CreateHttpClient(string userName, string password, bool noCache = true, int timeout = 6, int responseBufferSize = 0)
        {
            HttpClient httpClient = CreateHttpClient(noCache ? noCacheCtrl : null, timeout, responseBufferSize);
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                SetBasicAuthentication(httpClient, userName, password);
            return httpClient;
        }
        public static string EncodeCredential(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
            if (password == null) password = "";

            Encoding encoding = Encoding.UTF8;
            string credential = String.Format("{0}:{1}", userName, password);

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }
        public static void SetBasicAuthentication(HttpClient client, string userName, string password)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredential(userName, password));
        }
        public static void SetToken(HttpClient client, string scheme, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
        }

        public static void SetBearerToken(HttpClient client, string token)
        {
            SetToken(client, "Bearer", token);
        }

        public static string ToString(TimeSpan ts)
        {
            return string.Format("{0}:{1}:{2}", ts.Hours, ts.Minutes, ts.Seconds);
        }
        public static double ToDouble<T>(T value)
        {
            if (value.GetType() == typeof(string) && value.ToString().IndexOf (',') >= 0)
                return Convert.ToDouble(value);
            else
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
        public static T ToEnum<T>(object value) where T : struct
        {
            string str = value as string;
            if (str != null)
            {
                T val;
                if (Enum.TryParse<T>(str, out val))
                    return val;
                return default(T);
            }
            return (T)Enum.ToObject(typeof(T), value);
        }
        public static string MakeFileName(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            string replace = "\\/:*?\"<>|";
            for (int i = 0; i < str.Length; i++)
            {
                int idx = replace.IndexOf(str[i]);
                if (idx >= 0)
                    str = str.Replace(replace[idx], '_');
            }
            return str;
        }
        public static DateTime FromUnixTime(uint ut)
        {
            DateTime dt = FromUnixTimeUtc(ut);
            return dt.ToLocalTime();
        }
        public static DateTime FromUnixTimeUtc(uint ut)
        {
            if (ut == 0) return DateTime.MinValue;
            long l = ut;
            l += (long)(369 * 365 + 89) * 86400;
            l *= 10000000;
            return DateTime.FromFileTimeUtc(l);
        }
        public static uint ToUnixTime(DateTime val)
        {
            uint ut;
            try
            {
                if (val == DateTime.MinValue)
                    ut = 0;
                else
                {
                    long l = val.ToFileTimeUtc();
                    l /= 10000000;
                    l -= (long)(369 * 365 + 89) * 86400;
                    ut = (uint)l;
                }
            }
            catch
            {
                ut = 0;
            }
            return ut;
        }
        public static DateTime FromISO8601(string str, bool toUTC = false)
        {
            if (str.EndsWith("Z"))
            {
                return DateTime.ParseExact(str, iso8601DateFormats, CultureInfo.InvariantCulture, toUTC ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None);
            }
            DateTime dt = default(DateTime);
            DateTime.TryParseExact(str, "O", CultureInfo.InvariantCulture, toUTC ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.RoundtripKind, out dt);
            return dt;
        }
        public static string ToISO8601(DateTime dt)
        {
            return dt.ToString("O");
        }

        public static long UtcNowUniqueTicks
        {
            get
            {
                long original, newValue;
                do
                {
                    original = lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks;
                    newValue = Math.Max(now, original + 1);
                } while (Interlocked.CompareExchange(ref lastTimeStamp, newValue, original) != original);
                return newValue;
            }
        }
        public static DateTime UtcNowUnique
        {
            get { return new DateTime(UtcNowUniqueTicks, DateTimeKind.Utc); }
        }
        public static DateTime NowUnique
        {
            get { return UtcNowUnique.ToLocalTime(); }
        }
    }
}
