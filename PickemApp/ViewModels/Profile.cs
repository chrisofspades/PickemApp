using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

using PickemApp.Models;

namespace PickemApp.ViewModels
{
    public class ProfileEdit
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

    }

    public class ProfileChangePassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ProfileStats
    {
        public Player Player { get; set; }
        public List<StatsItem> StatItems { get; set; }
        public List<StatsSeasonSummary> Summaries { get; set; }
    }

    public class StatsItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Value { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }

        public string YearAndWeek
        {
            get
            {
                if (Year == 0 && Week == 0)
                    return "";

                var sb = new StringBuilder();

                sb.Append("(");

                var space = "";

                if (Year > 0)
                {
                    sb.AppendFormat("{0}", Year);
                    space = " ";
                }

                if (Week > 0)
                {
                    sb.Append(space);
                    sb.AppendFormat("Week {0}", Week);
                }

                sb.Append(")");

                return sb.ToString();
            }

        }
    }

    public class StatsSeasonSummary
    {
        public int PlayerId { get; set; }
        public int Year { get; set; }
        public double CorrectPicks { get; set; }
        public double WrongPicks { get; set; }

        private double _averagePicksPerWeek;
        public double AveragePicksPerWeek
        {
            get { return Math.Round(_averagePicksPerWeek, 2); }
            set { _averagePicksPerWeek = value; }
        }

        public int MostPicks { get; set; }
        public int TotalWins { get; set; }

        public double Percentage
        {
            get
            {
                if ((CorrectPicks + WrongPicks) == 0)
                {
                    return 0;
                }
                return Math.Round((double)(CorrectPicks / (CorrectPicks + WrongPicks)), 3);
            }
        }
    }
}