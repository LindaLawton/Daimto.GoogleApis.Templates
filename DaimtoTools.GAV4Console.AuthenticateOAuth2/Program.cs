using Google.Apis.AnalyticsReporting.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DaimtoTools.GAV4Console.AuthenticateOAuth2
{
    class Program
    {
        private static long _viewNumber = 78110423;  //Must be a view the user you are authncating has access to.
        private static int _pageSize = 10000;

        private static string _credFileName = @"C:\Users\lilaw\Documents\.credentials\NativeClient.json";
        private static string _file => File.ReadAllText(_credFileName);

        public delegate void HandelGetReportsResponse(GetReportsResponse response, string[] options);

        static async Task Main(string[] args)
        {
            // Instantiate the output delegates.
            HandelGetReportsResponse handlerConsole = WriteResponseToConsole;
            HandelGetReportsResponse handlerFile = WriteResponseToFile;

            // Setup authentication.
            var scope = new string[] { Google.Apis.AnalyticsReporting.v4.AnalyticsReportingService.Scope.AnalyticsReadonly };
            var credential = Oauth2Example.GetUserCredential(_credFileName, "user", scope);
            var serviceV4 = Oauth2Example.GetAnalyticsReportingService(credential);
            var serviceV3 = Oauth2Example.GetAnalyticsService(credential);

            // get views.
            var summeries = await serviceV3.Management.AccountSummaries.List().ExecuteAsync() ;

            foreach (var accountSummary in summeries.Items)
            {
                Console.WriteLine($"{accountSummary.Name}");
                foreach (var accountSummaryWebProperty in accountSummary.WebProperties)
                {
                    Console.WriteLine($"     {accountSummaryWebProperty.Name}");
                    foreach (var profileSummary in accountSummaryWebProperty.Profiles)
                    {
                        Console.WriteLine($"          {profileSummary.Name}: {profileSummary.Id }");
                    }
                }
            }

            string requestedView = string.Empty;
            do
            {
                Console.WriteLine($"Please select profile id");
                requestedView = Console.ReadLine();
            } while (!long.TryParse(requestedView, out _viewNumber));

            Console.WriteLine($"Requesting data for: {_viewNumber}");

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

            var requests = new List<ReportRequest> { reportRequest };

            // Create the GetReportsRequest object.
            var getReport = new GetReportsRequest() { ReportRequests = requests };

            do
            {
                Console.WriteLine($"Requesting data for Pagetoken: {getReport.ReportRequests.FirstOrDefault().PageToken}");

                // Call the batchGet method.
                var response = await serviceV4.Reports.BatchGet(getReport).ExecuteAsync();

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
