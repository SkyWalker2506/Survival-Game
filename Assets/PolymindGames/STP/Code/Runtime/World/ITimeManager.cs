using UnityEngine.Events;

namespace PolymindGames.WorldManagement
{
    /// <summary>
    /// Interface for managing game time.
    /// </summary>
    public interface ITimeManager
    {
        /// <summary>
        /// Gets the current day of the month in the game.
        /// </summary>
        int Day { get; }

        /// <summary>
        /// Gets the current hour of the day (24-hour format) in the game.
        /// </summary>
        int Hour { get; }

        /// <summary>
        /// Gets the current minute of the hour in the game.
        /// </summary>
        int Minute { get; }

        /// <summary>
        /// Gets the current second of the minute in the game.
        /// </summary>
        int Second { get; }

        /// <summary>
        /// Gets the total number of minutes elapsed since the start of the game.
        /// </summary>
        int TotalMinutes { get; }

        /// <summary>
        /// Gets the total number of hours elapsed since the start of the game.
        /// </summary>
        int TotalHours { get; }
        
        /// <summary>
        /// Gets the current day time, ranging from 0 (start of the day) to 1 (end of the day).
        /// </summary>
        float DayTime { get; }

        /// <summary>
        /// Gets the current time of day in the game.
        /// </summary>
        TimeOfDay TimeOfDay { get; }

        /// <summary>
        /// Gets or sets the increment of normalized day time per second.
        /// </summary>
        float DayTimeIncrementPerSecond { get; set; }

        /// <summary>
        /// Event triggered when the minute changes in the game.
        /// </summary>
        event TimeChangedEventHandler MinuteChanged;

        /// <summary>
        /// Event triggered when the hour changes in the game.
        /// </summary>
        event TimeChangedEventHandler HourChanged;

        /// <summary>
        /// Event triggered when the day changes in the game.
        /// </summary>
        event TimeChangedEventHandler DayChanged;
        
        /// <summary>
        /// Event triggered when the day time changes in the game.
        /// </summary>
        event UnityAction<float> DayTimeChanged;

        /// <summary>
        /// Event triggered when the time of day changes in the game.
        /// </summary>
        event UnityAction<TimeOfDay> TimeOfDayChanged;
    }

    public delegate void TimeChangedEventHandler(int total, int change);
}