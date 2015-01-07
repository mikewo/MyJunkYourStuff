using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJunkYourStuff.Models
{
    public static class ImageHtmlUrlHelper
    {
        private static readonly string _defaultBaseUri;

        static ImageHtmlUrlHelper()
        {
            _defaultBaseUri = ConfigurationManager.AppSettings["Image.baseUri"];
            if (string.IsNullOrWhiteSpace(_defaultBaseUri))
                {
                    _defaultBaseUri = String.Concat(HttpContext.Current.Request.Url.GetComponents(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.Unescaped), "/Content/locationimages/");
                }
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string imagePath, string baseUri = "", object htmlAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                baseUri = _defaultBaseUri;
            }

            string src = new Uri(new Uri(baseUri), imagePath).ToString();

            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", src);
            if (htmlAttributes != null)
            {
                IDictionary<string, object> htmlAttribs = ((IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
                foreach (KeyValuePair<string, object> htmlAttrib in htmlAttribs)
                {
                    builder.MergeAttribute(htmlAttrib.Key, htmlAttrib.Value.ToString());
                }
            }
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

    }
}