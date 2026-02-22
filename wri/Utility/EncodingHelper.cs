using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    static public class EncodingHelper
    {
        public static Encoding ToEncodingOrNull(this string encode)
        {
            try
            {
                if (string.IsNullOrEmpty(encode))
                {
                    return null;
                }

                switch (encode.ToLower())
                {
                    case "utf8":
                    case "utf-8":
                        // BOMなしUTF-8
                        return new UTF8Encoding(false, false);
                    case "932":
                    case "cp932":
                    case "shift_jis":
                    case "sjis":
                        return Encoding.GetEncoding(932);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Encoding ToEncodingOrDefault(this string encode) => ToEncodingOrNull(encode) ?? Encoding.UTF8;

        public static Encoding ToEncoding(this string encode) => ToEncodingOrDefault(encode);
    }
}
