using StreamlineFrame.Web.Models;
using StreamlineFrame.Web.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StreamlineFrame.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var test = new Test
            {
                Age = 1,
                Name = "testName"
            };
            //new TestRepository().Insert(test);
            new TestRepository().Get(x => x.Name.StartsWith("s"));

            new TestRepository().Get(x => x.Name == "testName" && (x.Pyh == null || (new string[] { "s", "ss" }).Contains(x.Name)));
            return View();
        }
    }
}