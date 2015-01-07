using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MyJunkYourStuff.Models
{
    // NOTE: This is useful for local dev or a simple, single instance site; however, if this is deployed to a multi-instance farm that
    // does not share a file source the images could be lost and be different between servers.  Move to saving these images in a 
    // persisted and highly available store, like Azure Blob storage or S3.
    public class LocalImageRepository : IImageRepository
    {
        public string Add(string imageName, HttpPostedFileBase fileData)
        {
            string localImagePath = HttpContext.Current.Server.MapPath("~/Content/locationimages/");
            var path = Path.Combine(localImagePath, imageName);
            fileData.SaveAs(path);

            return imageName;
        }
        public void Delete(string imagePath)
        {
            string localImagePath = HttpContext.Current.Server.MapPath("~/Content/locationimages/");
            var path = Path.Combine(localImagePath, imagePath);
            if (File.Exists(path))
            {
                File.Delete(path);
            }


        }
    }
}
