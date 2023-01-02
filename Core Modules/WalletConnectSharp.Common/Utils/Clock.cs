using System;

namespace WalletConnectSharp.Common.Utils
{
    public static class Clock
    {
        public const long ONE_SECOND = 1;

        public const long FIVE_SECONDS = 5;

        public const long TEN_SECONDS = 10;

        public const long THIRTY_SECONDS = 30;

        public const long SIXTY_SECONDS = 60;

        public const long ONE_MINUTE = SIXTY_SECONDS;

        public const long FIVE_MINUTES = ONE_MINUTE * 5;

        public const long TEN_MINUTES = ONE_MINUTE * 10;

        public const long THIRTY_MINUTES = ONE_MINUTE * 30;

        public const long SIXTY_MINUTES = ONE_MINUTE * 60;

        public const long ONE_HOUR = SIXTY_MINUTES;

        public const long THREE_HOURS = ONE_HOUR * 3;

        public const long SIX_HOURS = ONE_HOUR * 6;

        public const long TWELVE_HOURS = ONE_HOUR * 12;

        public const long TWENTY_FOUR_HOURS = ONE_HOUR * 24;

        public const long ONE_DAY = TWENTY_FOUR_HOURS;

        public const long THREE_DAYS = ONE_DAY * 3;

        public const long FIVE_DAYS = ONE_DAY * 5;

        public const long SEVEN_DAYS = ONE_DAY * 7;

        public const long THIRTY_DAYS = ONE_DAY * 30;

        public const long ONE_WEEK = SEVEN_DAYS;

        public const long TWO_WEEKS = ONE_WEEK * 2;

        public const long THREE_WEEKS = ONE_WEEK * 3;

        public const long FOUR_WEEKS = ONE_WEEK * 4;

        public const long ONE_YEAR = ONE_DAY * 365;

        public static long CalculateExpiry(long ttl, DateTime now = default)
        {
            if (now == default)
                now = DateTime.Now;

            return ((DateTimeOffset) now).ToUnixTimeSeconds() + ttl;
        }

        public static bool IsExpired(long expiry)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(expiry).ToLocalTime();

            return DateTime.Now >= dateTime;
        }

        public static long Now()
        {
            return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }
    }
}