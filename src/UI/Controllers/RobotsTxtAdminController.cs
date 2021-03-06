﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Framework.Configuration;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Web;

using POSSIBLE.RobotsTxtHandler.UI.Models;

namespace POSSIBLE.RobotsTxtHandler.UI.Controllers
{
     [GuiPlugIn(
        Area = PlugInArea.AdminMenu,
        DisplayName = "Manage robots.txt content",
        Description = "Tool to manage the robots.txt",
        Url = "~/modules/POSSIBLE.RobotsTxtHandler/RobotsTxtAdmin"
    )]
   [Authorize(Roles = "CmsAdmins")]
    public class RobotsTxtAdminController : Controller
    {
         protected IRobotsContentService RobotsService { get; set; }
         
         public RobotsTxtAdminController()
         {
             RobotsService = new RobotsContentService();
         }

        public ActionResult Index()
        {
            var model = new RobotsTxtViewModel();
    
            var allHostsList = GetHostsSelectListItems();
            model.SiteList = allHostsList;
            
            if (allHostsList.Count > 0)
                model.RobotsContent = RobotsService.GetRobotsContent(allHostsList.First().Value);
            
            return View(model);
        }

        [HttpPost]
        public ActionResult KeyChange(string key)
        {
            var robotsContent = RobotsService.GetRobotsContent(key);
            return Content(robotsContent);
        } 

         public ActionResult Edit(RobotsTxtViewModel model)
         {
             RobotsService.SaveRobotsContent(model.RobotsContent, model.SiteId);
             
             if (model.SiteId.EndsWith("*"))
             {
                 string siteId = model.SiteId.Split(">".ToCharArray())[0].Trim();
                 const string confirmationMessage = "Sucessfully saved robots.txt content. All requests for Site ID '{0}' that do not have an explicit host name mapping will serve this content";
                 model.SuccessMessage = string.Format(confirmationMessage, siteId);
             }
             else
             {
                 string hostLink = string.Format("<a href=\"http://{0}/robots.txt\" target=\"_blank\">http://{0}/robots.txt</a>", model.SiteId.Split(">".ToCharArray())[1].Trim());
                 string confirmationMessage = "Sucessfully saved robots.txt content. All requests to {0} will serve this content";
                 model.SuccessMessage = string.Format(confirmationMessage, hostLink);
             }

             model.KeyActionPath = "../";
             model.SiteList = GetHostsSelectListItems();
             return View("Index", model);
         }

         private List<SelectListItem> GetHostsSelectListItems()
         {
             var sdr = ServiceLocator.Current.GetInstance<SiteDefinitionRepository>();
             return sdr.List().SelectMany(d => d.Hosts, (s, h) => 
                 new SelectListItem
                     {
                         Value = RobotsService.GetSiteKey(s.Name, h.Name), 
                         Text = RobotsService.GetSiteKey(s.Name, h.Name)
                     }).ToList();

         }
    }
}
