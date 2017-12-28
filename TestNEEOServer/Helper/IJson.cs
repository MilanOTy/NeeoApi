using System.IO;

namespace Rts.Base.Json
{
    public interface IJson
    {
        string  SerializeValue      (object value);
        T       Deserialize<T>      (Stream strm);
        T       Deserialize<T>      (string str);
        string  Serialize<T>        (T request);
        void    Serialize<T>        (T value, Stream strm);
    }
}
