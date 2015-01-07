using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MyJunkYourStuff.Models;
using System.IO;

namespace MyJunkYourStuff.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IImageRepository _imageRepository;

        /// <summary>
        /// Initializes a new instance of the LocationsController class.
        /// </summary>
        /// <param name="locationRepository"></param>
        public LocationsController(ILocationRepository locationRepository, IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
            _locationRepository = locationRepository;
        }

        // GET: Locations
        public async Task<ActionResult> Index()
        {
            var locations = await _locationRepository.GetAllLocationsAsync();
            return View(locations);
        }

        // GET: Locations/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location location = await _locationRepository.FindAsync(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        // GET: Locations/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Title,StartTime,JunkerName,Description,Address")] Location location, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                location.Id = Guid.NewGuid();
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    if (imageFileIsValid(imageFile))
                    {
                        string imageStoreName = String.Concat(location.Id.ToString(), Path.GetExtension(imageFile.FileName));
                        location.MainImageName = _imageRepository.Add(imageStoreName, imageFile);    
                    }
                    else
                    {
                        return View(location);
                    }
                }
                await _locationRepository.AddAsync(location);
                return RedirectToAction("Index");
            }

            return View(location);
        }

        // GET: Locations/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location location = await _locationRepository.FindAsync(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,StartTime,MainImageName,JunkerName,Description,Address")] Location location)
        {
            if (ModelState.IsValid)
            {
                await _locationRepository.EditAsync(location);
                return RedirectToAction("Index");
            }
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<ActionResult> Delete(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location location = await _locationRepository.FindAsync(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Location location = await _locationRepository.FindAsync(id);
            if (!string.IsNullOrWhiteSpace(location.MainImageName))
            {
                _imageRepository.Delete(location.MainImageName);
            }

            await _locationRepository.DeleteAsync(id);
            
            return RedirectToAction("Index");
        }



        private bool imageFileIsValid(HttpPostedFileBase imageFile)
        {
            // check the file size (max 4 Mb)
            // NOTE: if you want larger files you'll also need to update the web.config to include httpRuntime maxRequestLength to increase
            // the default allowed size.
            if (imageFile.ContentLength > 1024 * 1024 * 4)
            {
                ModelState.AddModelError("MainImageName", "File size can't exceed 4 MB");
                return false;
            }

            // check file extension
            string extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (extension != ".png" && extension != ".jpg" )
            {
                ModelState.AddModelError("MainImageName", "Supported file extensions: png, jpg");
                return false;
            }

            return true;
        }
    }
}
