using Mallenom.ASP.NET.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Mallenom.ASP.NET.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        Resources r1 = new Resources();

        public IActionResult Index()
        {            
            r1.CPULoading=GetCpuMetrics().Result;
            foreach (var mem in GetMemoryMetrics("http://localhost:5275/api/metrics/memory").Result)
                {if (mem.Key == "total")
                    {
                    r1.TotalRAM = Convert.ToDouble(mem.Value);
                    }
                if (mem.Key == "free")
                    {
                        r1.FreeRAM = Convert.ToDouble(mem.Value);
                    }
                if (mem.Key == "used")
                    {
                        r1.UsedRAM = Convert.ToDouble(mem.Value);
                    }
                }

            foreach (var drive in GetDisksMetrics("http://localhost:5275/api/metrics/disks").Result)
            {
                r1.Drives = drive.Value;
            }                
        return View(r1);

        }
        public async Task<string> GetCpuMetrics()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:5275/api/metrics/cpu");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            string result = JsonConvert.DeserializeObject(responseBody).ToString();
            string cpuloading = "";
            for (int i = 0; i < result.Length; i++)
            {
                if (Char.IsDigit(result[i]))
                {
                    cpuloading += result[i];
                }
            }
            return cpuloading;
        }
        public async Task<Dictionary<string, string>> GetMemoryMetrics(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(responseBody);
            //string result = JsonConvert.DeserializeObject(responseBody).ToString();
            Dictionary<string, string> dict = jObject.ToObject<Dictionary<string, string>>();
            return dict;
        }
        public async Task<Dictionary<string, List<Resources.Drive>>> GetDisksMetrics(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(responseBody);
            Dictionary<string, List<Resources.Drive>> dict = jObject.ToObject<Dictionary<string, List<Resources.Drive>>>();
            return dict;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}