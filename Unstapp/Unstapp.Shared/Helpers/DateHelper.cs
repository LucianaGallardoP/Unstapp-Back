namespace Unstapp.Shared.Helpers
{
    public static class DateHelper
    {
        public static TimeZoneInfo GetArgentinaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
        }
        public static DateOnly GetArgentinaToday()
        {
            var timeZone = GetArgentinaTimeZone();

            var nowArgentina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            return DateOnly.FromDateTime(nowArgentina);
        }
        public static DateTime GetArgentinaNow()
        {
            var timeZone = GetArgentinaTimeZone();

            var nowArgentina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            return nowArgentina;
        }
        public static DateTime ConvertUtcToArgentina(DateTime utcDateTime)
        {
            var argentinaTimeZone = GetArgentinaTimeZone();

            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, argentinaTimeZone);
        }

        public static DateTime ConvertArgentinaLocalToUtc(DateTime argentinaLocalDateTime)
        {
            var argentinaTimeZone = GetArgentinaTimeZone();
            var unespecifiedTime = DateTime.SpecifyKind(argentinaLocalDateTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(unespecifiedTime, argentinaTimeZone);
        }
    }
}
