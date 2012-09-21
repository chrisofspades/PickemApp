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
    public class PickController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        //
        // GET: /Pick/

        public ActionResult Index()
        {
            var picks = db.Picks.Include(p => p.Player).Include(p => p.Game);
            return View(picks.ToList());
        }

        //
        // GET: /Pick/Details/5

        public ActionResult Details(int id = 0)
        {
            Pick pick = db.Picks.Find(id);
            if (pick == null)
            {
                return HttpNotFound();
            }
            return View(pick);
        }

        //
        // GET: /Pick/Create

        public ActionResult Create()
        {
            ViewBag.PlayerId = new SelectList(db.Players, "Id", "Name");
            ViewBag.GameId = new SelectList(db.Games.Select(t => new { Id = t.Id, Desc = "Week " + t.Week + ", " + t.VisitorTeam + " @ " + t.HomeTeam }), "Id", "Desc");
            return View();
        }

        //
        // POST: /Pick/Create

        [HttpPost]
        public ActionResult Create(Pick pick)
        {
            if (ModelState.IsValid)
            {
                db.Picks.Add(pick);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PlayerId = new SelectList(db.Players, "Id", "Name", pick.PlayerId);
            ViewBag.GameId = new SelectList(db.Games, "Id", "Eid", pick.GameId);
            return View(pick);
        }

        //
        // GET: /Pick/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Pick pick = db.Picks.Find(id);
            if (pick == null)
            {
                return HttpNotFound();
            }
            ViewBag.PlayerId = new SelectList(db.Players, "Id", "Name", pick.PlayerId);
            ViewBag.GameId = new SelectList(db.Games, "Id", "Eid", pick.GameId);
            return View(pick);
        }

        //
        // POST: /Pick/Edit/5

        [HttpPost]
        public ActionResult Edit(Pick pick)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pick).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PlayerId = new SelectList(db.Players, "Id", "Name", pick.PlayerId);
            ViewBag.GameId = new SelectList(db.Games, "Id", "Eid", pick.GameId);
            return View(pick);
        }

        //
        // GET: /Pick/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Pick pick = db.Picks.Find(id);
            if (pick == null)
            {
                return HttpNotFound();
            }
            return View(pick);
        }

        //
        // POST: /Pick/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Pick pick = db.Picks.Find(id);
            db.Picks.Remove(pick);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}