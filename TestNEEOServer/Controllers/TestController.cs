using Home.Neeo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TestNEEOServer.Services;
using TestNEEOServer.Services.Neeo;

namespace TestNEEOServer.Controllers
{
    public class TestController : Controller
    {
        IRequestHandler _requestHandler;
        NEEOTestDevice  _testDevice;
        public TestController(NeeoService neeoService)
        {
            _requestHandler = neeoService.RequestHandler;
            _testDevice = neeoService.Devices.FirstOrDefault() as NEEOTestDevice; ;
        }

        [Route("/test/all")]
        [HttpGet]
        public IActionResult GetAll()
        {
            _testDevice.GetAllDevices();
            return Json(false);
        }
        [Route("/test/devices")]
        [HttpGet]
        public IActionResult GetDevice()
        {
            _testDevice.GetDevices();
            return Json(false);
        }
        [Route("/test/reachable")]
        [HttpGet]
        public IActionResult SetReachable()
        {
            _testDevice.SetReachable(true);
            return Json(false);
        }
        [Route("/test/notreachable")]
        [HttpGet]
        public IActionResult SetNotReachable()
        {
            _testDevice.SetReachable(false);
            return Json(false);
        }

    }
}
