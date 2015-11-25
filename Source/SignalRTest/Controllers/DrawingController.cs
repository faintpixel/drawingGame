using Microsoft.AspNet.SignalR;
using SignalRTest.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignalRTest.Controllers
{
    public class DrawingController : Controller
    {
        public static ConcurrentDictionary<string, ExquisiteCorpse> RoomId_Game = new ConcurrentDictionary<string, ExquisiteCorpse>(); // TO DO - why am i using ConcurrentDictionary instead of just dictionary?

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OtherPage()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<DrawingHub>();
            hub.Clients.All.broadcastMessage("IIS", "Someone is viewing the other page.");
            return View();
        }
    }
}