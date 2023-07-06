using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Common
{
    public static class Utils
    {
        public static bool IsInLayerMask(LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return string.Empty;
            }

            // first remove all: punctuation, separator chars, control chars, and numbers (unicode style regexes)
            string initials = Regex.Replace(fullName, @"[\p{P}\p{S}\p{C}\p{N}]+", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));

            // Replacing all possible whitespace/separator characters (unicode style), with a single, regular ascii space.
            initials = Regex.Replace(initials, @"\p{Z}+", " ", RegexOptions.None, TimeSpan.FromMilliseconds(100));

            // Remove all Sr, Jr, I, II, III, IV, V, VI, VII, VIII, IX at the end of names
            initials = Regex.Replace(initials.Trim(), @"\s+(?:[JS]R|I{1,3}|I[VX]|VI{0,3})$", "", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

            // Extract up to 2 initials from the remaining cleaned name.
            initials = Regex.Replace(initials, @"^(\p{L})[^\s]*(?:\s+(?:\p{L}+\s+(?=\p{L}))?(?:(\p{L})\p{L}*)?)?$", "$1$2", RegexOptions.None, TimeSpan.FromMilliseconds(100)).Trim();

            if (initials.Length > 2)
            {
                // Worst case scenario, everything failed, just grab the first two letters of what we have left.
                initials = initials.Substring(0, 2);
            }

            return initials.ToUpperInvariant();
        }

        public const string TimeIntervalJustNow = "@Common:JustNow";
        public const string TimeIntervalAMinute = "@Common:MinuteAgo";
        public const string TimeIntervalMinutes = "@Common:MinutesAgo";
        public const string TimeIntervalAnHour = "@Common:HourAgo";
        public const string TimeIntervalHours = "@Common:HoursAgo";
        public const string TimeIntervalYesterday = "@Common:Yesterday";
        public const string TimeIntervalDays = "@Common:DaysAgo";
        public const string TimeIntervalAWeek = "@Common:LastWeek";
        public const string TimeIntervalWeeks = "@Common:WeeksAgo";
        public const string TimeIntervalAMonth = "@Common:LastMonth";
        public const string TimeIntervalMonths = "@Common:MonthsAgo";
        public const string TimeIntervalAYear = "@Common:LastYear";
        public const string TimeIntervalYears = "@Common:YearsAgo";

        public static string GetTimeIntervalSinceNow(DateTime dateTime, out object[] variables)
        {
            var now = DateTime.Now;
            var timeElapsed = now - dateTime;
            variables = null;

            var result = CheckSecond(timeElapsed, ref variables);
            if (result != null)
            {
                return result;
            }

            result = CheckMinute(timeElapsed, ref variables);
            if (result != null)
            {
                return result;
            }

            result = CheckHour(timeElapsed, ref variables);
            if (result != null)
            {
                return result;
            }

            result = CheckDay(timeElapsed, ref variables);
            if (result != null)
            {
                return result;
            }

            result = CheckWeek(timeElapsed, ref variables);
            if (result != null)
            {
                return result;
            }

            result = CheckMonth(timeElapsed, ref variables);
            if (result != null)
            {
                return result;
            }

            result = CheckYear(timeElapsed, ref variables);
            return result;
        }

        static string CheckSecond(TimeSpan timeElapsed, ref object[] variables)
        {
            int seconds = (int)timeElapsed.TotalSeconds;
            if (seconds < 60)
            {
                variables = null;
                return TimeIntervalJustNow;
            }

            return null;
        }

        static string CheckMinute(TimeSpan timeElapsed, ref object[] variables)
        {
            int minutes = (int)timeElapsed.TotalMinutes;
            if (minutes < 60)
            {
                if (minutes == 1)
                {
                    return TimeIntervalAMinute;
                }

                variables = new object[] { minutes };
                return TimeIntervalMinutes;
            }

            return null;
        }

        static string CheckHour(TimeSpan timeElapsed, ref object[] variables)
        {
            int hours = (int)timeElapsed.TotalHours;
            if (hours < 24)
            {
                if (hours == 1)
                {
                    variables = null;
                    return TimeIntervalAnHour;
                }

                variables = new object[] { hours };
                return TimeIntervalHours;
            }

            return null;
        }

        static string CheckDay(TimeSpan timeElapsed, ref object[] variables)
        {
            int days = (int)timeElapsed.TotalDays;
            if (days < 7)
            {
                if (days == 1)
                {
                    variables = null;
                    return TimeIntervalYesterday;
                }

                variables = new object[] { days };
                return TimeIntervalDays;
            }

            return null;
        }

        static string CheckWeek(TimeSpan timeElapsed, ref object[] variables)
        {
            int weeks = (int)timeElapsed.TotalDays / 7;
            if (weeks < 4)
            {
                if (weeks == 1)
                {
                    variables = null;
                    return TimeIntervalAWeek;
                }

                variables = new object[] { weeks };
                return TimeIntervalWeeks;
            }

            return null;
        }

        static string CheckMonth(TimeSpan timeElapsed, ref object[] variables)
        {
            int months = (int)timeElapsed.TotalDays / 30;
            if (months < 12)
            {
                if (months < 2)
                {
                    variables = null;
                    return TimeIntervalAMonth;
                }

                variables = new object[] { months };
                return TimeIntervalMonths;
            }

            return null;
        }

        static string CheckYear(TimeSpan timeElapsed, ref object[] variables)
        {
            int years = (int)timeElapsed.TotalDays / 365;
            if (years < 2)
            {
                variables = null;
                return TimeIntervalAYear;
            }

            variables = new object[] { years };
            return TimeIntervalYears;
        }
        
        public static void SetVisible(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
