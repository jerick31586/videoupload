﻿using System.Text;

namespace VideoUpload.Web.Common
{
    public class EmailTemplate
    {
        public static string GetTemplate(AppUser currentUser, string message, string url)
        {

            var template = new StringBuilder();
            template.Append("<p style='font:15px Porsche News Gothic'>" + message + "<br/>");
            template.Append("<p style='font:15px Porsche News Gothic'>" + url + "<br/>");
            template.Append("<div><p style='font:15px Porsche News Gothic'>Best Regards,</p>");
            template.Append("<span style='font:15px Porsche News Gothic'><b>" + currentUser.FullName + "</b></span><br/>");
            template.Append("<span style='font:15px Porsche News Gothic'>" + currentUser.JobTitle + "<span><br/><br/>");
            template.Append("<span style='font:15px Porsche News Gothic'>Porsche Center Doha</span><br/>");
            template.Append("<span style='font:15px Porsche News Gothic'>Alboraq Automobiles Co. w.l.l<span><br/>");
            template.Append("<span style='font:15px Porsche News Gothic'>" + currentUser.WorkAddress + "</span><br/><br/>");
            template.Append("<b>Phone:</b> " + currentUser.PhoneNumber + "<br/>");
            template.Append("<b>Direct Line:</b> " + currentUser.DirectLine + "<br/>");
            template.Append("<b>Fax:</b> " + currentUser.FaxNumber + "<br/>");
            template.Append("<b>Email:</b> " + currentUser.Email + "<br/></div>");
            return template.ToString();
        }
    }
}