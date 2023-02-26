using Microsoft.EntityFrameworkCore;

namespace WeatherReport
{
    internal class AppDatabase : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherReport>(entity =>
            {
                entity.ToTable("WeatherReport");

                entity.Property(e => e.id).HasColumnName("id");

                entity.OwnsOne(e => e.coord, coord =>
                {
                    coord.Property(c => c.lon).HasColumnName("coord_lon");
                    coord.Property(c => c.lat).HasColumnName("coord_lat");
                });

                entity.OwnsMany(e => e.weather, weather =>
                {
                    weather.Property(w => w.id).HasColumnName("weather_id");
                    weather.Property(w => w.main).HasColumnName("weather_main");
                    weather.Property(w => w.description).HasColumnName("weather_description");
                    weather.Property(w => w.icon).HasColumnName("weather_icon");
                });

                entity.Property(e => e._base).HasColumnName("base");

                entity.OwnsOne(e => e.main, main =>
                {
                    main.Property(m => m.temp).HasColumnName("main_temp");
                    main.Property(m => m.feels_like).HasColumnName("main_feels_like");
                    main.Property(m => m.temp_min).HasColumnName("main_temp_min");
                    main.Property(m => m.temp_max).HasColumnName("main_temp_max");
                    main.Property(m => m.pressure).HasColumnName("main_pressure");
                    main.Property(m => m.humidity).HasColumnName("main_humidity");
                    main.Property(m => m.sea_level).HasColumnName("main_sea_level");
                    main.Property(m => m.grnd_level).HasColumnName("main_grnd_level");
                });

                entity.Property(e => e.visibility).HasColumnName("visibility");

                entity.OwnsOne(e => e.wind, wind =>
                {
                    wind.Property(w => w.speed).HasColumnName("wind_speed");
                    wind.Property(w => w.deg).HasColumnName("wind_deg");
                    wind.Property(w => w.gust).HasColumnName("wind_gust");
                });

                entity.OwnsOne(e => e.clouds, clouds =>
                {
                    clouds.Property(c => c.all).HasColumnName("clouds_all");
                });

                entity.OwnsOne(e => e.rain, rain =>
                {
                    rain.Property(r => r._1h).HasColumnName("rain_1h");
                    rain.Property(r => r._3h).HasColumnName("rain_3h");
                });

                entity.OwnsOne(e => e.snow, snow =>
                {
                    snow.Property(s => s._1h).HasColumnName("snow_1h");
                    snow.Property(s => s._3h).HasColumnName("snow_3h");
                });

                entity.Property(e => e.dt).HasColumnName("dt");

                entity.OwnsOne(e => e.sys, sys =>
                {
                    sys.Property(s => s.type).HasColumnName("sys_type");
                    sys.Property(s => s.id).HasColumnName("sys_id");
                    sys.Property(s => s.country).HasColumnName("sys_country");
                    sys.Property(s => s.sunrise).HasColumnName("sys_sunrise");
                    sys.Property(s => s.sunset).HasColumnName("sys_sunset");
                });

                entity.Property(e => e.timezone).HasColumnName("timezone");

                entity.Property(e => e.name).HasColumnName("name");

                entity.Property(e => e.cod).HasColumnName("cod");

                entity.Property(e => e.timestamp).HasColumnName("timestamp");
            });
        }

        public DbSet<WeatherReport> WeatherReport { get; set; }
    }
}

