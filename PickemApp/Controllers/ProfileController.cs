using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

using Dapper;

using PickemApp.Filters;
using PickemApp.Models;
using PickemApp.ViewModels;

namespace PickemApp.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class ProfileController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        //
        // GET: /Profile/
        
        public ActionResult Index()
        {
            var user = Auth.User;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(new ProfileEdit { Name = user.Name, Email = user.Email, Username = user.Username });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(ProfileEdit form)
        {
            var user = Auth.User;
            if (user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                user.Username = form.Username;
                user.Email = form.Email;
                user.Name = form.Name;

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("Index", new ProfileEdit { Name = user.Name, Email = user.Email, Username = user.Username });
        }

        public ActionResult ChangePassword(ProfileChangePassword form)
        {
            var user = Auth.User;
            if (user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, form.OldPassword, form.NewPassword);
                }
                catch (Exception ex)
                {
                    changePasswordSucceeded = false;
                    ModelState.AddModelError("password1", ex.Message);
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("password2", "The current password is incorrect or the new password is invalid.");
                }
            }

            return View("Index", new ProfileEdit { Name = user.Name, Email = user.Email, Username = user.Username });
        }

        [AllowAnonymous]
        public ActionResult Stats(int id = 0)
        {
            Player user;

            if (id > 0)
            {
                user = db.Players.Find(id);
            }
            else
            {
                user = Auth.User;
            }

            if (user == null)
            {
                return HttpNotFound();
            }

            var vm = new ProfileStats();

            vm.Player = user;

            var sql = @"exec spGetPlayerStats @playerId";
            int year = ConfigurationManager.AppSettings["currentYear"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["currentYear"]) : DateTime.Today.Year;

            using (var multi = db.Database.Connection.QueryMultiple(sql, new { playerId = user.Id, year = year }))
            {
                var statItems = multi.Read<StatsItem>().ToList();
                var seasonSummaries = multi.Read<StatsSeasonSummary>().ToList();

                vm.StatItems = statItems;
                vm.Summaries = seasonSummaries;
            } 


            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
