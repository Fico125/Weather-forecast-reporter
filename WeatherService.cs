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
    public partial class WeatherService : ServiceBase
    {
        private const string API_KEY = "df146d8c3452593654e8ec0ffb54fd7e";
        private static readonly MemoryCache memoryCache = MemoryCache.Default;
        private static readonly HttpClient client = new HttpClient();
        private readonly AppDatabase appDatabase = new AppDatabase();
        private readonly string cacheKey = "weatherReport";
        private int eventId = 1;

        public WeatherService(string[] args)
        {
            InitializeComponent();

            var eventSourceName = "WeatherReportSourceEvent";
            var logName = "WeatherReportLogger";

            if (args.Length > 0) eventSourceName = args[0];

            if (args.Length > 1) logName = args[1];

            eventLogger = new EventLog();

            if (!EventLog.SourceExists(eventSourceName)) EventLog.CreateEventSource(eventSourceName, logName);

            eventLogger.Source = eventSourceName;
            eventLogger.Log = logName;
        }

        protected async void OnStartAsync()
        {
            // first timer, runs every x seconds and retrieves new weather data from OpenWeather API and stores it into the database.
            var retrievingAndStoringDataTimer = new Timer();
            retrievingAndStoringDataTimer.Interval = 10000; // 10 seconds
            retrievingAndStoringDataTimer.Elapsed += RetrieveAndStoreNewWeatherDataAsync;
            retrievingAndStoringDataTimer.Start();

            // second timer, runs every y seconds and reports weather data to Event Viewer.
            var reportingTimer = new Timer();
            reportingTimer.Interval = 20000; // 20 seconds
            reportingTimer.AutoReset = true;
            reportingTimer.Elapsed += ReportData;
            reportingTimer.Start();
        }

        protected override void OnContinue()
        {
        }

        protected override void OnStop()
        {
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
                so that new GET request can store new data into the cache.*/

                memoryCache.Add(cacheKey, fullWeatherReport, DateTimeOffset.UtcNow.AddSeconds(9));
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
                /* I've made ID a primary key of the sql table, so when I tried inserting a new value I was getting an error:
                 * Cannot insert explicit value for identity column in table 'WeatherReport' when IDENTITY_INSERT is set to OFF. 
                 * Because of this, I've set the ID to null which is not the ideal solution but for this scenario it will be sufficient. */

                weatherReport.id = null;
                var entry = appDatabase.WeatherReport.Add(weatherReport);
                await appDatabase.SaveChangesAsync();
                Console.WriteLine("New data saved into the database.");
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
                // There's no cached response which means we have to query the database.
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

        internal async void TestStartupAndStopAsync()
        {
            OnStartAsync();
            OnStop();
            Console.ReadLine();
        }
    }
}