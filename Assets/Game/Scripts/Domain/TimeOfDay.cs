using System;

namespace Game.Domain
{
    /// <summary>
    /// A reading on the house clock. The day clock counts seconds; the player reads an hour, and
    /// "it is getting late" is a feeling the number carries better than a bar does.
    /// </summary>
    public readonly struct TimeOfDay
    {
        public TimeOfDay(int hour24, int minute)
        {
            if (hour24 < 0 || hour24 > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hour24), "An hour runs from 0 to 23.");
            }
            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute), "A minute runs from 0 to 59.");
            }
            Hour24 = hour24;
            Minute = minute;
        }

        public int Hour24 { get; }

        public int Minute { get; }

        /// <summary>Minutes since midnight. The cheap way to compare two readings.</summary>
        public int TotalMinutes => Hour24 * 60 + Minute;

        public bool IsBeforeNoon => Hour24 < 12;

        /// <summary>12-hour clock face: midnight and noon both read as 12.</summary>
        public int Hour12
        {
            get
            {
                int hour = Hour24 % 12;
                return hour == 0 ? 12 : hour;
            }
        }

        /// <summary>e.g. "8:05 AM", "2:00 PM".</summary>
        public override string ToString()
        {
            return $"{Hour12}:{Minute:00} {(IsBeforeNoon ? "AM" : "PM")}";
        }

        /// <summary>
        /// Maps how much of the day has been used onto the hours the house keeps.
        /// </summary>
        /// <param name="progress">0 at dawn, 1 at nightfall. Clamped.</param>
        /// <param name="startHour">The hour the day begins, e.g. 8 for 8 AM.</param>
        /// <param name="endHour">The hour night falls, e.g. 20 for 8 PM.</param>
        /// <param name="minuteStep">
        /// Rounds the minutes down to a multiple of this. A 180-second day covers 12 hours, so one
        /// displayed minute lasts a quarter of a second — stepping is what keeps the digits
        /// readable instead of a blur.
        /// </param>
        public static TimeOfDay FromDayProgress(float progress, int startHour, int endHour, int minuteStep)
        {
            if (startHour < 0 || startHour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(startHour), "An hour runs from 0 to 23.");
            }
            if (endHour <= startHour || endHour > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(endHour), "The day must end after it starts, and by midnight.");
            }
            if (minuteStep < 1 || minuteStep > 60)
            {
                throw new ArgumentOutOfRangeException(nameof(minuteStep), "A step runs from 1 to 60 minutes.");
            }

            if (progress < 0f)
            {
                progress = 0f;
            }
            else if (progress > 1f)
            {
                progress = 1f;
            }

            int dayLengthInMinutes = (endHour - startHour) * 60;

            // In double, not float. C# lets a float expression be evaluated with more precision
            // than float, so `720f * progress` can land on 599.99998 or on 600 depending on the
            // runtime — Mono, .NET and IL2CPP are free to disagree, and a domain rule that answers
            // differently per platform is not a rule. Double makes the truncation deterministic.
            int elapsed = (int)(dayLengthInMinutes * (double)progress);
            int stepped = elapsed / minuteStep * minuteStep;
            int absolute = startHour * 60 + stepped;

            int hour = absolute / 60;
            if (hour == 24)
            {
                hour = 0; // a day ending exactly at midnight reads as 12 AM, not as hour 24
            }
            return new TimeOfDay(hour, absolute % 60);
        }
    }
}
