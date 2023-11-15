using System.Web;

namespace WalletConnectSharp.Common.Utils
{
    /// <summary>
    /// General purpose extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns true if the given object is a numeric type
        /// </summary>
        /// <param name="o">The object to check</param>
        /// <returns>Returns true if the object has a numeric type</returns>
        public static bool IsNumericType(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Add a query parameter to the given source string
        /// </summary>
        /// <param name="source">The source string to add the generated query parameter to</param>
        /// <param name="key">The key of the query parameter</param>
        /// <param name="value">The value of the query parameter</param>
        /// <returns>The original source string with the generated query parameter appended</returns>
        public static string AddQueryParam(
            this string source, string key, string value)
        {
            string delim;
            if ((source == null) || !source.Contains("?"))
            {
                delim = "?";
            }
            else if (source.EndsWith("?") || source.EndsWith("&"))
            {
                delim = string.Empty;
            }
            else
            {
                delim = "&";
            }

            return source + delim + HttpUtility.UrlEncode(key)
                   + "=" + HttpUtility.UrlEncode(value);
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeout = 1000,
            string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }

            return ((Task<T>)resultT).Result;
        }

        public static async Task WithTimeout(this Task task, int timeout = 1000,
            string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout,
            string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }

            return ((Task<T>)resultT).Result;
        }

        public static async Task WithTimeout(this Task task, TimeSpan timeout,
            string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }
        }

        public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second,
            IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(second, comparer ?? EqualityComparer<T>.Default)
                .SetEquals(first);
        }

        public static Action ListenOnce(this EventHandler eventHandler, EventHandler handler)
        {
            EventHandler internalHandler = null;
            internalHandler = (sender, args) =>
            {
                eventHandler -= internalHandler;
                handler(sender, args);
            };

            eventHandler += internalHandler;

            return () =>
            {
                try
                {
                    eventHandler -= internalHandler;
                }
                catch (Exception e)
                {
                    // ignored
                }
            };
        }

        public static Action ListenOnce<TEventArgs>(
            this EventHandler<TEventArgs> eventHandler,
            EventHandler<TEventArgs> handler)
        {
            EventHandler<TEventArgs> internalHandler = null;
            internalHandler = (sender, args) =>
            {
                eventHandler -= internalHandler;
                handler(sender, args);
            };

            eventHandler += internalHandler;

            return () =>
            {
                try
                {
                    eventHandler -= internalHandler;
                }
                catch (Exception e)
                {
                    // ignored
                }
            };
        }
    }
}
