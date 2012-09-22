using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.Models;

namespace PickemApp.Controllers
{
    public class BulkPickEditorController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index()
        {
            ViewBag.PlayerId = new SelectList(db.Players, "Id", "Name");
            ViewBag.Year = new SelectList(db.Games.Select(t => new { Year = t.Year }).Distinct(), "Year", "Year");
            ViewBag.Week = new SelectList(db.Games.Select(t => new { Week = t.Week }).Distinct(), "Week", "Week");

            return View();
        }

        [HttpPost]
        public ActionResult PickGames(BulkPickEditor bpe)
        {
            bpe.Player = db.Players.Find(bpe.PlayerId);
            var pgs = from g in db.Games.Where(q => q.Week == bpe.Week && q.Year == bpe.Year)
                        join p in db.Picks.Where(q => q.PlayerId == bpe.PlayerId) on g.Id equals p.GameId into j1
                        from j2 in j1.DefaultIfEmpty()
                        select new { Game = g, Pick = j2 };
            
            List<Pick> picks = new List<Pick>();

            foreach (var pg in pgs)
            {
                Pick pick = pg.Pick ?? new Pick();
                pick.GameId = pg.Game.Id;
                pick.Game = pg.Game;
                pick.PlayerId = bpe.PlayerId;
                pick.Player = bpe.Player;

                picks.Add(pick);
                
            }

            bpe.Picks = picks;

            return View(bpe);
        }

        [HttpPost]
        public ActionResult SubmitPicks(BulkPickEditor bpe)
        {
            if (ModelState.IsValid)
            {
                foreach (var pick in bpe.Picks)
                {
                    if (pick.Id != 0)
                    {
                        db.Entry(pick).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        db.Picks.Add(pick);
                    }
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
