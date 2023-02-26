using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using WeatherReport.Data;

namespace WeatherReport
{
    /// <summary>
    /// Weather service is the service used to retrieve weather data from public API, store it into the database and report using Event Viewer.
    /// </summary>
    public partial class WeatherService : ServiceBase
    {
        private const string API_KEY = "df146d8c3452593654e8ec0ffb54fd7e";
        private static readonly MemoryCache memoryCache = MemoryCache.Default;
        private static readonly HttpClient client = new HttpClient();
        private readonly AppDatabase appDatabase = new AppDatabase();
        private readonly string cacheKey = "weatherReport";
        private int retrieveTimerInterval = 300000;
        private int eventId = 1;

        public WeatherService()
        {
            InitializeComponent();

            var eventSourceName = "WeatherReportSourceEvent";
            var logName = "WeatherReportLogger";

            eventLogger = new EventLog();

            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLogger.Source = eventSourceName;
            eventLogger.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            // First timer, runs every 5 minutes and retrieves new weather data from OpenWeather API and stores it into the database.
            var retrievingAndStoringDataTimer = new Timer();

            retrievingAndStoringDataTimer.Interval = retrieveTimerInterval; // 5 minutes
            retrievingAndStoringDataTimer.Elapsed += RetrieveAndStoreNewWeatherDataAsync;
            retrievingAndStoringDataTimer.Start();

            // Second timer, runs every 10 minutes and reports weather data to Event Viewer.
            var reportingTimer = new Timer();
            reportingTimer.Interval = 600000; // 10 minutes
            reportingTimer.AutoReset = true;
            reportingTimer.Elapsed += ReportData;
            reportingTimer.Start();
        }

        protected override void OnStop()
        {
            eventLogger.WriteEntry("Weather service stopped.", EventLogEntryType.Information, eventId++);
        }

        private async void RetrieveAndStoreNewWeatherDataAsync(object sender, ElapsedEventArgs args)
        {
            eventLogger.WriteEntry("Retrieving and storing new weather data", EventLogEntryType.Information, eventId++);
            var weatherReport = await RetrieveWeatherDataAsync();
            if (weatherReport != null) WriteDataIntoDatabaseAsync(weatherReport);
        }

        private async Task<WeatherReport> RetrieveWeatherDataAsync()
        {
            // Calling asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                // TODO: Extract lattitude and longitude so that user could set them prior to service installation. 

                var responseMessage =
                    await client.GetAsync(
                        "https://api.openweathermap.org/data/2.5/weather?lat=45.81&lon=15.96&units=metric&appid=" +
                        API_KEY);
                responseMessage.EnsureSuccessStatusCode();

                var responseBody = await responseMessage.Content.ReadAsStringAsync();
                var weatherReport = JsonConvert.DeserializeObject<WeatherReport>(responseBody);
                weatherReport.timestamp = DateTime.Now;
                var fullWeatherReport = JsonConvert.SerializeObject(weatherReport);

                /* We do not want to keep cached data too long to prevent running out of memory.
                 Cached data should stay cached until we get new data from the API, 
                therefore absolute expiration should be set to value just short of the timer interval for queuing new data, 
                so that new GET request can store new data into the cache. */

                memoryCache.Add(cacheKey, fullWeatherReport, DateTimeOffset.UtcNow.AddSeconds(retrieveTimerInterval - 1));
                return weatherReport;
            }
            catch (HttpRequestException e)
            {
                var errorMsg = "There was an exception while getting the data from the server. Exception: " + e;
                eventLogger.WriteEntry(errorMsg, EventLogEntryType.Error, eventId++);
                return null;
            }
        }

        private async void WriteDataIntoDatabaseAsync(WeatherReport weatherReport)
        {
            try
            {
                weatherReport.id = null;
                /* I've made ID a primary key of the sql table, so when I tried inserting a new value I was getting an error stating that I
                 * cannot insert explicit value for identity column in table 'WeatherReport' when IDENTITY_INSERT is set to OFF. 
                 * Because of this, I've here setting the ID to null which is not the ideal solution but for this scenario it will be sufficient. */

                appDatabase.WeatherReport.Add(weatherReport);
                await appDatabase.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var errorMsg =
                    "An exception happened during the process of storing the data in the database. Exception: " + e;
                eventLogger.WriteEntry(errorMsg, EventLogEntryType.Error, eventId++);
            }
        }

        private void ReportData(object sender, ElapsedEventArgs args)
        {
            // Check if we have latest data in the cache, if we do use that and don't query the DB.
            if (memoryCache.Contains(cacheKey))
            {
                var cachedResponse = memoryCache.Get(cacheKey) as string;
                var weatherReport = JsonConvert.DeserializeObject<WeatherReport>(cachedResponse);
                FormatAndLogData(weatherReport);
            }
            else
            {
                // There's no cached response which means we have to query the database and get the last report.
                var latestDBEntry = appDatabase.WeatherReport.OrderByDescending(report => report.id).FirstOrDefault();
                FormatAndLogData(latestDBEntry);
            }
        }

        private void FormatAndLogData(WeatherReport weatherReport)
        {
            var tempOsc = weatherReport.main.temp_max - weatherReport.main.temp_min;

            var weatherReportEntry = "Location: " + weatherReport.name + "\nTimestamp: " + weatherReport.timestamp +
                                     "\nTemperature: " + weatherReport.main.temp +
                                     " °C\nBiggest temperature oscillation: " + tempOsc + " °C";

            eventLogger.WriteEntry(weatherReportEntry);
        }

        /// <summary>
        /// Method used to test the functionality of the service while debuggining.
        /// </summary>
        internal void TestStartupAndStop()
        {
            string[] args = null;
            OnStart(args);
            OnStop();
            Console.ReadLine();
        }
    }
}