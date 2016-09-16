using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace PickemApp.Models
{
    public class Season
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }

        public static DateTime GetDeadline(int week, int year)
        {
            using (PickemDBContext db = new PickemDBContext())
            {
                /* The deadline should be the day before the first game of the week, at 8pm.
                 * Because our Game model does not contain a game date, we must calculate the game date based 
                 * on the date the season started, the week of the game, and the day of the game.
                 */

                // First get the season start date
                var season = db.Seasons.SingleOrDefault(s => s.Year == year);
                if (season == null)
                    return DateTime.MinValue;

                // Calculate the date for this week of the season
                var weekOf = season.StartDate.AddDays((week - 1) * 7);

                // Get the first game of the week
                var firstGame = db.Games.Where(q => q.Week == week && q.Year == year && q.GameType == "REG").ToList()
                                    .OrderBy(o => o.Eid.Substring(0, 8))
                                    .ThenBy(o => o.Time.PadLeft(5, '0'))
                                    .ThenBy(o => o.Gsis)
                                    .FirstOrDefault();

                if (firstGame == null)
                    return weekOf.AddDays(-1).AddHours(20);

                // Calculate the date of the game based on the day of the game
                var shortDayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
                var dayOfIndex = Array.IndexOf(shortDayNames, firstGame.Day);
                var gameDate = weekOf.AddDays(-Convert.ToInt32(weekOf.DayOfWeek)).AddDays(dayOfIndex);

                return gameDate.AddDays(-1).AddHours(20);
            }
        }
    }
}