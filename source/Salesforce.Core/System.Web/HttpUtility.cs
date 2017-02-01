// 
// System.Web.HttpUtility
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Web
{

#if !MOBILE && !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
    // CAS - no InheritanceDemand here as the class is sealed
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public sealed class HttpUtility
    {
        sealed class HttpQSCollection
            :
                #if !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
                NameValueCollection
                #else
                Dictionary<string, string> // Linq.Lookup<string, string>
                #endif
        {

            public override string ToString()
            {
                int count = Count;
                if (count == 0)
                    return "";
                StringBuilder sb = new StringBuilder();

                #if !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
                string[] keys = this.AllKeys;
                keys = this.AllKeys;
                #else
                string[] keys = new string[] { };
                this.Keys.CopyTo(keys,0);
                #endif
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("{0}={1}&", keys[i], this[keys[i]]);
                }
                if (sb.Length > 0)
                    sb.Length--;
                return sb.ToString();
            }
        }

        #region Constructors

        public HttpUtility()
        {
        }

        #endregion // Constructors

        #region Methods

        public static string UrlEncode(string str)
        {
            return UrlEncode(str, Encoding.UTF8);
        }

        public static string UrlEncode(string s, Encoding Enc)
        {
            if (s == null)
                return null;

            if (s == String.Empty)
                return String.Empty;

            bool needEncode = false;
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                char c = s[i];
                if ((c < '0') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || (c > 'z'))
                {
                    if (Util.HttpEncoder.NotEncoded(c))
                        continue;

                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
                return s;

            // avoided GetByteCount call
            byte[] bytes = new byte[Enc.GetMaxByteCount(s.Length)];
            int realLen = Enc.GetBytes(s, 0, s.Length, bytes, 0);

            string retval = null;

            #if !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
            retval = Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, 0, realLen));
            #else
            retval = Encoding.UTF8.GetString(bytes, 0, realLen);
            //mc++ throw new NotImplementedException("Salesforce PCL Bite-n-Switch Not ImplementedException");
            #endif

            return retval;
		}

		public static string UrlEncode (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return String.Empty;

            string retval = null;

            #if !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
            retval = Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, 0, bytes.Length));
            #else
            retval = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            //mc++ throw new NotImplementedException("Salesforce PCL Bite-n-Switch Not ImplementedException");
            #endif

            return retval;
		}

		public static string UrlEncode (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return String.Empty;

            string retval = null;

            #if !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
            retval = Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, offset, count));
            #else
            retval = Encoding.UTF8.GetString(bytes, offset, count);
            //mc++ throw new NotImplementedException("Salesforce PCL Bite-n-Switch Not ImplementedException");
            #endif

            return retval;
		}

		public static byte [] UrlEncodeToBytes (string str)
		{
			return UrlEncodeToBytes (str, Encoding.UTF8);
		}

		public static byte [] UrlEncodeToBytes (string str, Encoding e)
		{
			if (str == null)
				return null;

			if (str.Length == 0)
				return new byte [0];

			byte [] bytes = e.GetBytes (str);
			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return new byte [0];

			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;
			#if NET_4_0
			return Util.HttpEncoder.Current.UrlEncode (bytes, offset, count);
			#else
			return Util.HttpEncoder.UrlEncodeToBytes (bytes, offset, count);
			#endif
		}

		public static string UrlEncodeUnicode (string str)
		{
			if (str == null)
				return null;

            string retval = null;

            #if !PORTABLE && !(SILVERLIGHT && WINDOWS_PHONE) && !(NETFX_CORE && (WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_UWP))
            retval = Encoding.ASCII.GetString(UrlEncodeUnicodeToBytes(str));
            #else
            byte[] bytes = UrlEncodeUnicodeToBytes(str);
            retval = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            //mc++ throw new NotImplementedException("Salesforce PCL Bite-n-Switch Not ImplementedException");
            #endif

            return retval;
		}

		public static byte [] UrlEncodeUnicodeToBytes (string str)
		{
			if (str == null)
				return null;

			if (str.Length == 0)
				return new byte [0];

			MemoryStream result = new MemoryStream (str.Length);
			foreach (char c in str){
				Util.HttpEncoder.UrlEncodeChar (c, result, true);
			}
			return result.ToArray ();
		}

		#endregion // Methods
	}
}
