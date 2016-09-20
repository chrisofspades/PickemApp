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
    [Authorize(Roles="admin")]
    public class PlayerController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        //
        // GET: /Player/

        public ActionResult Index()
        {
            return View(db.Players.ToList());
        }

        //
        // GET: /Player/Details/5

        public ActionResult Details(int id = 0)
        {
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
            return View(player);
        }

        //
        // GET: /Player/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Player/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Player player)
        {
            if (ModelState.IsValid)
            {
                if (db.Players.Any(p => p.Name == player.Name))
                {
                    ModelState.AddModelError("name", "That name already exists.");
                }
                if (db.Players.Any(p => p.Username == player.Username))
                {
                    ModelState.AddModelError("username", "That username already exists.");
                }
                if (db.Players.Any(p => p.Email == player.Email))
                {
                    ModelState.AddModelError("email", "That email already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Players.Add(player);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(player);
        }

        //
        // GET: /Player/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
            return View(player);
        }

        //
        // POST: /Player/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Player player)
        {
            if (ModelState.IsValid) 
            {
                if (db.Players.Any(p => p.Name == player.Name && p.Id != player.Id))
                {
                    ModelState.AddModelError("name", "That name already exists.");
                }
                if (db.Players.Any(p => p.Username == player.Username && p.Id != player.Id))
                {
                    ModelState.AddModelError("username", "That username already exists.");
                }
                if (db.Players.Any(p => p.Email == player.Email && p.Id != player.Id))
                {
                    ModelState.AddModelError("email", "That email already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(player);
        }

        //
        // GET: /Player/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
            return View(player);
        }

        //
        // POST: /Player/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Player player = db.Players.Find(id);
            db.Players.Remove(player);
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