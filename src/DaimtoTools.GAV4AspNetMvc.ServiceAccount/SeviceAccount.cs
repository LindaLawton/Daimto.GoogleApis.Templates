﻿// Copyright 2019 DAIMTO ([Linda Lawton](https://twitter.com/LindaLawtonDK)) :  [www.daimto.com](http://www.daimto.com/)

using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System;
using System.IO;
using Newtonsoft.Json;

namespace DaimtoTools.GAV4Console.ServiceAccount
{

    public class ServiceAccount
    {
        public readonly AnalyticsReportingService service;
        private static long _viewNumber = 78110423;
        private static int _pageSize = 10000;

        private static string _credFileName = @"C:\temp\.credentials\ServiceAccount.json";
        private static string _file => File.ReadAllText(_credFileName);

        public ServiceAccount()
        {

            var scope = new string[] { Google.Apis.AnalyticsReporting.v4.AnalyticsReportingService.Scope.AnalyticsReadonly };
            var creds = JsonConvert.DeserializeObject<ServiceAccountCreds>(_file);
            service = AuthenticateServiceAccount(creds.client_email, _credFileName, scope);

        }

        /// <summary>
        /// Authenticating to Google using a Service account
        /// Documentation: https://developers.google.com/accounts/docs/OAuth2#serviceaccount
        /// </summary>
        /// <param name="serviceAccountEmail">From Google Developer console https://console.developers.google.com</param>
        /// <param name="serviceAccountCredentialFilePath">Location of the Json Service account key file downloaded from Google Developer console https://console.developers.google.com</param>
        /// <returns>AnalyticsService used to make requests against the Analytics API</returns>
        public static AnalyticsReportingService AuthenticateServiceAccount(string serviceAccountEmail, string serviceAccountCredentialFilePath, string[] scopes)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceAccountCredentialFilePath))
                    throw new Exception("Path to the service account credentials file is required.");
                if (!File.Exists(serviceAccountCredentialFilePath))
                    throw new Exception("The service account credentials file does not exist at: " + serviceAccountCredentialFilePath);
                if (string.IsNullOrEmpty(serviceAccountEmail))
                    throw new Exception("ServiceAccountEmail is required.");

                GoogleCredential credential;
                using (var stream = new FileStream(serviceAccountCredentialFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                         .CreateScoped(scopes);
                }

                // Create the  Analytics service.
                return new AnalyticsReportingService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Analyticsreporting Service account Authentication Sample",
                });
            }
            catch (Exception ex)
            {
                throw new Exception("CreateServiceAccountAnalyticsreportingFailed", ex);
            }
        }
    }
}