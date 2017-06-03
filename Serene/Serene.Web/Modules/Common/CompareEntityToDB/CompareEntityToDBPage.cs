
namespace Serene.Common.Pages
{
    using Northwind;
    using Northwind.Entities;
    using Serenity;
    using Serenity.Data;
    using System;
    using System.Web.Mvc;
    using System.Linq;
    using System.Reflection;
    using Serenity.Data.Mapping;

    [RoutePrefix("CompareEntityToDB"), Route("{action=index}")]
    public class CompareEntityToDBController : Controller
    {
        [Authorize, HttpGet]
        public ActionResult Index()
        {
            var model = new CompareEntityToDBPageModel();
            

            return View(MVC.Views.Common.CompareEntityToDB.CompareEntityToDBIndex, model);
        }
    }
}
