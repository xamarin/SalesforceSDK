using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if (NETFX_CORE && (WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP))

namespace CoreSystem
{
    public partial class Uri
    {
        public static bool IsHexDigit(char digit)
        {
            return (('0' <= digit && digit <= '9') ||
                    ('a' <= digit && digit <= 'f') ||
                    ('A' <= digit && digit <= 'F'));
        }

        public static int FromHex(char digit)
        {
            if ('0' <= digit && digit <= '9')
            {
                return (int)(digit - '0');
            }

            if ('a' <= digit && digit <= 'f')
                return (int)(digit - 'a' + 10);

            if ('A' <= digit && digit <= 'F')
                return (int)(digit - 'A' + 10);

            throw new ArgumentException("digit");
        }
    }
}
#endif