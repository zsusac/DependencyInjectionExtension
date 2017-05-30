using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PartyInvites.Models;
using PartyInvites.Services;
using PartyInvites.HelloWorldDomain.Services;

namespace PartyInvites.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClock _clock;
        private readonly IHelloWorld _helloWorld;

        public HomeController(IClock clock, IHelloWorld helloWorld)
        {
            _clock = clock;
            _helloWorld = helloWorld;
        }

        public ViewResult Index()
        {
            int hour = DateTime.Now.Hour;
            ViewBag.Greeting = hour < 12 ? "Good Morning" : "Good Afternoon";
            ViewBag.TimeNow = _clock.GetTime().ToString();
            ViewBag.HelloWorld = _helloWorld.GetHelloWorld();
            return View("MyView");
        }

        [HttpGet]
        public ViewResult RsvpForm()
        {
            return View();
        }

        [HttpPost]
        public ViewResult RsvpForm(GuestResponse guestResponse)
        {
            if (ModelState.IsValid)
            {
                Repository.addResponse(guestResponse);

                return View("Thanks", guestResponse);
            }

            return View();
        }

        public ViewResult ListResponses()
        {
            return View(Repository.Responses.Where(r => r.WillAttend == true));
        }
    }
}
