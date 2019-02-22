using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.AnalyticsReporting.v4.Data;
using Newtonsoft.Json;

namespace DaimtoTools.GAV4Console.ServiceAccount
{
    class Program
    {
        private static long  _viewNumber = 78110423;
        private static int _pageSize = 10000;

        private static string _credFileName = @"C:\temp\.credentials\ServiceAccount.json";
        private static string _file => File.ReadAllText(_credFileName);

        public delegate void HandelGetReportsResponse(GetReportsResponse response, string[] options);

        static void Main(string[] args)
        {
            // Instantiate the output delegates.
            HandelGetReportsResponse handlerConsole = WriteResponseToConsole;
            HandelGetReportsResponse handlerFile = WriteResponseToFile;

            // Setup authentication.
            var scope = new string[] { Google.Apis.AnalyticsReporting.v4.AnalyticsReportingService.Scope.AnalyticsReadonly };
            var creds = JsonConvert.DeserializeObject<ServiceAccountCreds>(_file);
            var service = ServiceAccount.AuthenticateServiceAccount(creds.client_email, _credFileName, scope);

            // Create the DateRange object.
            var dateRange = new DateRange() { StartDate = "2010-01-01", EndDate = "2019-01-01" };

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
                ViewId = _viewNumber.ToString(),
                DateRanges = new List<DateRange>() { dateRange },
                Dimensions = dimensions,
                Metrics = metrics,
                PageSize = _pageSize
            };

            var requests = new List<ReportRequest> {reportRequest};

            // Create the GetReportsRequest object.
            var getReport = new GetReportsRequest() { ReportRequests = requests };

            do
            {
                Console.WriteLine($"Requesting data for Pagetoken: {getReport.ReportRequests.FirstOrDefault().PageToken}");

                // Call the batchGet method.
                var response = service.Reports.BatchGet(getReport).Execute();

                getReport.ReportRequests.FirstOrDefault().PageToken = response.Reports.FirstOrDefault().NextPageToken;

                // Call the delegate.
                handlerConsole(response, null);
                handlerFile(response, new string[] { @".\GAOutPut" });

            } while (getReport.ReportRequests.FirstOrDefault().PageToken != null);


            Console.ReadLine();
        }

        public static void WriteResponseToConsole(GetReportsResponse response, string[] optoins)
        {
            foreach (var responseReport in response.Reports)
            {
                foreach (var reportRow in responseReport.Data.Rows)
                {
                    var dimensions = reportRow.Dimensions.Aggregate((a, y) => a + ", " + y);
                    foreach (var reportRowMetric in reportRow.Metrics)
                    {
                        Console.WriteLine($"{dimensions}, {reportRowMetric.Values.Aggregate((a, y) => a + ", " + y)}");
                    }
                }
            }
        }

        public static void WriteResponseToFile(GetReportsResponse response, string[] optoins)
        {
            if (!Directory.Exists(optoins[0]))
                Directory.CreateDirectory(optoins[0]);

            var file = $"{optoins[0]}\\GoogleAnalyticsExport{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv";
            Console.WriteLine($"File writting to {file}");
            using (var writer = System.IO.File.AppendText(file))
            {
                foreach (var responseReport in response.Reports)
                {
                    foreach (var reportRow in responseReport.Data.Rows)
                    {
                        var dimensions = reportRow.Dimensions.Aggregate((a, y) => a + ", " + y);
                        foreach (var reportRowMetric in reportRow.Metrics)
                        {
                            writer.WriteLine(
                                $"{dimensions}, {reportRowMetric.Values.Aggregate((a, y) => a + ", " + y)}");
                        }
                    }
                }
            }
        }
    }
}
