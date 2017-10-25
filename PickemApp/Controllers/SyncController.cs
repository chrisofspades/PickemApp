using System;
using System.Collections.Generic;
using System.IO;
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

        [Authorize(Roles = "admin")]
        public ActionResult Picks(string h = "")
        {
            if (!string.IsNullOrEmpty(h))
            {
                if (h.Contains(".xls"))
                {
                    PickSync.UpdatePicksXls(h);
                }
                else
                {
                    PickSync.UpdatePicks(h);
                }

                ViewBag.Message = "Picks updated.";

                return View("Index");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Picks(HttpPostedFileBase file)
        {

            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/datafiles"), fileName);
                file.SaveAs(path);

                if (fileName.Contains(".xls"))
                {
                    PickSync.UpdatePicksXls(fileName);
                }
                else if (fileName.Contains(".csv"))
                {
                    PickSync.UpdatePicksCsv(fileName);
                }
                else
                {
                    PickSync.UpdatePicks(fileName);
                }

                ViewBag.Message = "Picks updated.";
            }

            return View("Index");
            //return RedirectToAction("Index");
        }
    }
}
