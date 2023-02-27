using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace WeatherReport.Data
{
    internal class AppDatabase : DbContext
    {
        public DbSet<WeatherReport> WeatherReport { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connStrings = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            optionsBuilder.UseSqlServer(connStrings);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherReport>(entity =>
            {
                entity.ToTable("WeatherReport");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.OwnsOne(e => e.Coord, coord =>
                {
                    coord.Property(c => c.Lon).HasColumnName("coord_lon");
                    coord.Property(c => c.Lat).HasColumnName("coord_lat");
                });

                entity.OwnsMany(e => e.Weather, weather =>
                {
                    weather.Property(w => w.Id).HasColumnName("weather_id");
                    weather.Property(w => w.Main).HasColumnName("weather_main");
                    weather.Property(w => w.Description).HasColumnName("weather_description");
                    weather.Property(w => w.Icon).HasColumnName("weather_icon");
                });

                entity.Property(e => e.Base).HasColumnName("base");

                entity.OwnsOne(e => e.Main, main =>
                {
                    main.Property(m => m.Temp).HasColumnName("main_temp");
                    main.Property(m => m.Feels_like).HasColumnName("main_feels_like");
                    main.Property(m => m.Temp_min).HasColumnName("main_temp_min");
                    main.Property(m => m.Temp_max).HasColumnName("main_temp_max");
                    main.Property(m => m.Pressure).HasColumnName("main_pressure");
                    main.Property(m => m.Humidity).HasColumnName("main_humidity");
                    main.Property(m => m.Sea_level).HasColumnName("main_sea_level");
                    main.Property(m => m.Grnd_level).HasColumnName("main_grnd_level");
                });

                entity.Property(e => e.Visibility).HasColumnName("visibility");

                entity.OwnsOne(e => e.Wind, wind =>
                {
                    wind.Property(w => w.Speed).HasColumnName("wind_speed");
                    wind.Property(w => w.Deg).HasColumnName("wind_deg");
                    wind.Property(w => w.Gust).HasColumnName("wind_gust");
                });

                entity.OwnsOne(e => e.Clouds, clouds => { clouds.Property(c => c.All).HasColumnName("clouds_all"); });

                entity.OwnsOne(e => e.Rain, rain =>
                {
                    rain.Property(r => r._1h).HasColumnName("rain_1h");
                    rain.Property(r => r._3h).HasColumnName("rain_3h");
                });

                entity.OwnsOne(e => e.Snow, snow =>
                {
                    snow.Property(s => s._1h).HasColumnName("snow_1h");
                    snow.Property(s => s._3h).HasColumnName("snow_3h");
                });

                entity.Property(e => e.Dt).HasColumnName("dt");

                entity.OwnsOne(e => e.Sys, sys =>
                {
                    sys.Property(s => s.Type).HasColumnName("sys_type");
                    sys.Property(s => s.Id).HasColumnName("sys_id");
                    sys.Property(s => s.Country).HasColumnName("sys_country");
                    sys.Property(s => s.Sunrise).HasColumnName("sys_sunrise");
                    sys.Property(s => s.Sunset).HasColumnName("sys_sunset");
                });

                entity.Property(e => e.Timezone).HasColumnName("timezone");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Cod).HasColumnName("cod");

                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            });
        }
    }
}