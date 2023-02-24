using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace WeatherReport
{
    public partial class WeatherService : ServiceBase
    {
        private int eventId = 1;
        private const string API_KEY = "df146d8c3452593654e8ec0ffb54fd7e";
        static readonly HttpClient client = new HttpClient();

        public WeatherService(string[] args)
        {
            InitializeComponent();

            string eventSourceName = "WeatherReportSourceEvent";
            string logName = "WeatherReportLogger";

            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }

            if (args.Length > 1)
            {
                logName = args[1];
            }

            eventLog1 = new EventLog();

            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        protected async Task OnStartAsync()
        {
            eventLog1.WriteEntry("WeatherService - In OnStart.");
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Calling asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather?lat=45.81&lon=15.96&units=metric&appid=" + API_KEY);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                eventLog1.WriteEntry(responseBody);
                Console.WriteLine(responseBody);

                WeatherReport weatherResponseObject = JsonConvert.DeserializeObject<WeatherReport>(responseBody);
                await WriteToDatabaseAsync(weatherResponseObject);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
            }
        }

        private async Task WriteToDatabaseAsync(WeatherReport weatherResponseObject)
        {
            try
            {
                var appDatabase = new AppDatabase();
                /* I've made ID a primary key of the sql table, so when I tried inserting a new value I was getting an error:
                 * Cannot insert explicit value for identity column in table 'WeatherReport' when IDENTITY_INSERT is set to OFF. 
                 * Because of this, I've set the ID to null which is not the ideal solution but for this scenario it will be sufficient. */
                weatherResponseObject.id = null;
                var entry = appDatabase.WeatherReport.Add(weatherResponseObject);
                await appDatabase.SaveChangesAsync();
                Console.WriteLine("\nData saved.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("WeatherService - In OnContinue.");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("WeatherService - In OnStop.");
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring/reporting activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        internal void TestStartupAndStop()
        {
            this.OnStartAsync();
            Console.ReadLine();
            this.OnStop();
        }
    }
}
