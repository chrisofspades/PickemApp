using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.SyncUtils;
using PickemApp.Models;

namespace PickemApp.Controllers
{
    public class SyncController : Controller
    {
        //
        // GET: /Sync/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Nfl(string x = "http://www.nfl.com/liveupdate/scorestrip/ss.xml")
        {
            if (string.IsNullOrEmpty(x))
            {
                return HttpNotFound();
            }

            ViewBag.Message = "Games and pick results updated.";

            NflSync.UpdateGames(x);
            return View("Index");
        }

        public ActionResult Picks(string h)
        {
            if (string.IsNullOrEmpty(h))
            {
                return HttpNotFound();
            }

            PickSync.UpdatePicks(h);

            ViewBag.Message = "Picks updated.";

            return View("Index");
        }
    }
}
