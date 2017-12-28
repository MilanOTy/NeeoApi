using System;
using System.Text;

namespace Home.Neeo.Device.Validation
{
    public class UniqueName
    {
        static string Hash(string input)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public static string CreateUniqueName(string str, string uniqueString)
        {
            if (uniqueString == null)
                uniqueString = string.Empty;
            string result = uniqueString + NEEOEnvironment.MachineName  + str;
            result = Hash(result);
            return result;
        }
    }
}