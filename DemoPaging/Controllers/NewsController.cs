using DemoPaging.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DemoPaging.Models
{
    public class NewsController : Controller
    {
        NewsHelper _newsHelper;

        public NewsController()
        {
            _newsHelper = new NewsHelper();
        }
        // GET: News
        public ActionResult Index(int page = 1)
        {
            ViewBag.Title = "News Page";
            var articles = _newsHelper.GetNewsByPageView(page);
            return View(articles);
        }
    }
}