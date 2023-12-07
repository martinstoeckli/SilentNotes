// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Workers
{
    /// <summary>
    /// This class can translate a timespan into a human readable form like:
    /// <example>
    /// - X days ago
    /// - X months ago
    /// - today
    /// - yesterday
    /// </example>
    /// </summary>
    public class TimeAgo
    {
        private readonly Localization _localization;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeAgo"/> class.
        /// </summary>
        /// <param name="localization"></param>
        public TimeAgo(Localization localization)
        {
            _localization = localization;
        }

        /// <summary>
        /// Translates the timespan between two dates into a human readable form. The time parts
        /// are ignored, so even if the dates are 01.01.2000 23:55 and 02.01.2000 00:05 it would
        /// return "yesterday" regardless that not 24h has passed since.
        /// </summary>
        /// <param name="formerEvent">The former date.</param>
        /// <param name="laterEvent">The later date.</param>
        /// <returns>Human readable form of the timespan between the dates.</returns>
        public string PrettyPrint(DateTime formerEvent, DateTime laterEvent)
        {
            // Ingore the time part
            DateTime formerDay = formerEvent.Date;
            DateTime laterDay = laterEvent.Date;

            // Can't be younger than 0...
            if (formerDay > laterDay)
                return null;

            TimeSpan difference = laterDay.Subtract(formerDay);
            double daysBetween = difference.TotalDays;

            if (daysBetween < 1)
            {
                return _localization.Today;
            }
            else if (daysBetween < 2)
            {
                return _localization.Yesterday;
            }
            else if (daysBetween < 14)
            {
                return string.Format(_localization.NumberOfDaysAgo, daysBetween);
            }
            else
            {
                int monthsBetween = AgeInMonths(formerDay, laterDay);
                if (monthsBetween < 2)
                {
                    return string.Format(_localization.NumberOfWeeksAgo, AgeInWeeks(daysBetween));
                }
                else
                {
                    int yearsBetween = AgeInYears(formerDay, laterDay);
                    if (yearsBetween < 2)
                    {
                        return string.Format(_localization.NumberOfMonthsAgo, monthsBetween);
                    }
                    else
                    {
                        return string.Format(_localization.NumberOfYearsAgo, yearsBetween);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the age (number of whole years) between two dates.
        /// </summary>
        /// <param name="birthday">The former date.</param>
        /// <param name="ageAt">The later date.</param>
        /// <returns>The number of whole years between.</returns>
        public int AgeInYears(DateTime birthday, DateTime ageAt)
        {
            // Can't be younger than 0...
            if (ageAt < birthday)
                return 0;

            // Get the years between
            int result = ageAt.Year - birthday.Year;

            // If we counted an unfinished year, decrease the number of years.
            if ((ageAt.Month < birthday.Month) ||
                (ageAt.Month == birthday.Month && ageAt.Day < birthday.Day))
            {
                result--;
            }
            return result;
        }

        /// <summary>
        /// Calculates the age in months (number of whole months) between two dates.
        /// </summary>
        /// <param name="birthday">The former date.</param>
        /// <param name="ageAt">The later date.</param>
        /// <returns>The number of whole months between.</returns>
        public int AgeInMonths(DateTime birthday, DateTime ageAt)
        {
            // Can't be younger than 0...
            if (ageAt < birthday)
                return 0;

            // Get the months between
            int result = (12 * ageAt.Year + ageAt.Month) - (12 * birthday.Year + birthday.Month);

            // If we counted an unfinished month, decrease the number of months.
            if (ageAt.Day < birthday.Day)
            {
                result--;
            }
            return result;
        }

        /// <summary>
        /// Calculates the age in weeks (number of whole weeks) from a number of days.
        /// Remove the time part from two dates before taking the difference.
        /// </summary>
        /// <param name="numberOfDays">Number of days with a fractional time.</param>
        /// <returns>Number of whole weeks.</returns>
        internal int AgeInWeeks(double numberOfDays)
        {
            return (int)numberOfDays / 7;
        }

        /// <summary>
        /// Holds the localized strings which are used by <see cref="PrettyPrint(DateTime, DateTime)"/>.
        /// </summary>
        public class Localization
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Localization"/> class.
            /// </summary>
            public Localization()
            {
                Today = "today";
                Yesterday = "yesterday";
                NumberOfDaysAgo = "{0} days ago";
                NumberOfWeeksAgo = "{0} weeks ago";
                NumberOfMonthsAgo = "{0} month ago";
                NumberOfYearsAgo = "{0} years ago";
            }

            /// <summary>
            /// Gets or sets the localized string to express today.
            /// <example>"today"</example>
            /// </summary>
            public string Today { get; set; }

            /// <summary>
            /// Gets or sets the localized string to express yesterday.
            /// <example>"yesterday"</example>
            /// </summary>
            public string Yesterday { get; set; }

            /// <summary>
            /// Gets or sets the localized string to express how many days in the past.
            /// <example>"{0} days ago"</example>
            /// </summary>
            public string NumberOfDaysAgo { get; set; }

            /// <summary>
            /// Gets or sets the localized string to express how many weeks in the past.
            /// <example>"{0} weeks ago"</example>
            /// </summary>
            public string NumberOfWeeksAgo { get; set; }

            /// <summary>
            /// Gets or sets the localized string to express how many months in the past.
            /// <example>"{0} months ago"</example>
            /// </summary>
            public string NumberOfMonthsAgo { get; set; }

            /// <summary>
            /// Gets or sets the localized string to express how many years in the past.
            /// <example>"{0} years ago"</example>
            /// </summary>
            public string NumberOfYearsAgo { get; set; }
        }
    }
}
