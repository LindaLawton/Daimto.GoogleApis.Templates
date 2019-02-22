using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DaimtoTools.GAV4AspNetMvc.ServiceAccount.Models;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;

namespace DaimtoTools.GAV4AspNetMvc.ServiceAccount.Controllers
{
    public class HomeController : Controller
    {
        private readonly AnalyticsReportingService _service;

        public HomeController(GAV4Console.ServiceAccount.ServiceAccount account)
        {
            _service = account.service;
        }

        public IActionResult Index()
        {
            var dateRange = new DateRange() { StartDate = "2018-06-01", EndDate = "2019-01-01" };

            // Create the Metrics object.
            var dimensions = new List<Dimension>()
            {
                new Dimension {Name = "ga:browser"},
                new Dimension {Name = "ga:date"},
                new Dimension {Name = "ga:userType"}
            };

            //Create the Dimensions object.
            var metrics = new List<Metric>()
            {
                new Metric {Expression = "ga:sessions", Alias = "Sessions"},
                new Metric {Expression = "ga:users", Alias = "Users"},
                new Metric {Expression = "ga:newUsers", Alias = "New Users"}
            };

            // Create the ReportRequest object.
            var reportRequest = new ReportRequest
            {
                ViewId = "78110423",
                DateRanges = new List<DateRange>() { dateRange },
                Dimensions = dimensions,
                Metrics = metrics,
                PageSize = 10000
            };

            var requests = new List<ReportRequest> { reportRequest };

            // Create the GetReportsRequest object.
            var getReport = new GetReportsRequest() { ReportRequests = requests };

            var rows = new List<ReportRow>();

            do
            {
                Console.WriteLine($"Requesting data for Pagetoken: {getReport.ReportRequests.FirstOrDefault().PageToken}");

                // Call the batchGet method.
                var response = _service.Reports.BatchGet(getReport).Execute();

                rows.AddRange(response.Reports.FirstOrDefault().Data.Rows);

                getReport.ReportRequests.FirstOrDefault().PageToken = response.Reports.FirstOrDefault().NextPageToken;

            } while (getReport.ReportRequests.FirstOrDefault().PageToken != null);


            return View(new ReportResponseModel {Rows = rows});
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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

    public class ReportResponseModel
    {
        public List<ReportRow> Rows { get; set; }
    }
}
