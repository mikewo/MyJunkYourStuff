using MyJunkYourStuff.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyJunkYourStuff.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILocationRepository _locationRepository;

        /// <summary>
        /// Initializes a new instance of the HomeController class.
        /// </summary>
        /// <param name="locationRepository"></param>
        public HomeController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<ActionResult> Index()
        {
            // NOTE: Will need to change how dates work to work with DocumentDb. See http://blogs.msdn.com/b/documentdb/archive/2014/11/18/working-with-dates-in-azure-documentdb.aspx.
            //       Also, DocumentDb does not support OrderBy in LINQ. Need to use an OrderBy stored procedure.

            // Get 5 garage sales coming in the next 7 days.
            var upcomingLocations = await _locationRepository.GetUpcomingLocationsAsync(new TimeSpan(7,0,0,0,0), 5);

            return View(upcomingLocations);
        }

        public ActionResult About()
        {
            return View();
        }


    }
}