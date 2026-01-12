using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Text.Json;
using Dapper;
using hello_world.Models;
using MySql.Data.MySqlClient;

namespace hello_world.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "test Page";
            ViewBag.Message = "Your application TIMO description page.";
            ViewBag.Messagejson = "Cannot fetch random fact";
            ViewBag.Grades = "Hello world, i grade this well";

            using (var client = new HttpClient())
            {
                var endpoint = new Uri("https://dogapi.dog/api/v2/facts?limit=1");
                var result = client.GetAsync(endpoint).Result;
                var json = result.Content.ReadAsStringAsync().Result;

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    string body = doc.RootElement
                        .GetProperty("data")[0]
                        .GetProperty("attributes")
                        .GetProperty("body")
                        .GetString();

                    ViewBag.Messagejson = body;
                    // use body here
                }

            }

            return View();
        }

        public ActionResult Contact()
        {
            var reservations = Reservation.GetAll();
            return View(reservations);
        }
        
        public ActionResult Reading()
        {
            ViewBag.Title = "Hello World!";
            ViewBag.Message = "Your reading page.";
            return View();
        }
    }
}