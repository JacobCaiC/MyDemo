using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;

namespace MyDemo.Controllers
{
    public class HelloWorldController : Controller
    {
        // 
        // GET: /HelloWorld/

        public IActionResult Index()
        {
            return View();
        }



        // GET: /HelloWorld/Welcome/ 
        // Requires using System.Text.Encodings.Web;
        // https://localhost:{PORT}/HelloWorld/Welcome?name=Rick&numtimes=4
        public string Welcome(string name, int numTimes = 1)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numTimes}");
        }

        // https://localhost:{PORT}/HelloWorld/Welcome/3?name=Rick
        public string Welcome2(string name, int ID = 1)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, ID: {ID}");
        }

        /// <summary>
        /// @ViewData["Message"]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="numTimes"></param>
        /// <returns></returns>
        public IActionResult Welcome3(string name, int numTimes = 1)
        {
            ViewData["Message"] = "Hello " + name;
            ViewData["NumTimes"] = numTimes;

            return View();
        }
    }
}