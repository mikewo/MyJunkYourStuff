using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyJunkYourStuff.Models
{
    public interface IImageRepository
    {
        string Add(string imageName, HttpPostedFileBase fileData);
        void Delete(string imagePath);
    }
}
