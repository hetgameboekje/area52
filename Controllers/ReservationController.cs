using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using area52.Models;

namespace area52.Controllers
{
    public class ReservationController : Controller
    {

        public ActionResult Index()
        {
            var reservations = ReservationModel.GetReservations();
            return View(reservations);
        }

        public ActionResult Form()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(int aantal_personen, DateTime reserveringsdatum, string opmerkingen)
        {
            try
            {
                
                if (aantal_personen <= 0)
                    return RedirectToAction("Form", new { status = "zeromembercount" });

                if (reserveringsdatum < DateTime.Today)
                    return RedirectToAction("Form", new { status = "reservationinpast" });

                // Roep model aan om reservering aan te maken
                int reserveringID = ReservationModel.CreateReservation(aantal_personen, reserveringsdatum, opmerkingen);


                return RedirectToAction("Index", new { status = "succes" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Form", new { status = $"Error: {ex.Message}" });
            }
        }
    }
}