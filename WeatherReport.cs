using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherReport
{
    public class WeatherReport
    {
        public Coord coord { get; set; }
        public ICollection<Weather> weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public Rain rain { get; set; }
        public Snow snow { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int? id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class Coord
    {
        [Column("coord_lon")]
        public float lon { get; set; }
        [Column("coord_lat")]
        public float lat { get; set; }
    }

    public class Weather
    {
        [Column("weather_id")]
        public int id { get; set; }
        [Column("weather_main")]
        public string main { get; set; }
        [Column("weather_description")]
        public string description { get; set; }
        [Column("weather_icon")]
        public string icon { get; set; }
    }

    public class Main
    {
        [Column("main_temp")]
        public float temp { get; set; }
        [Column("main_feels_like")]
        public float feels_like { get; set; }
        [Column("main_temp_min")]
        public float temp_min { get; set; }
        [Column("main_temp_max")]
        public float temp_max { get; set; }
        [Column("main_pressure")]
        public int pressure { get; set; }
        [Column("main_humidity")]
        public int humidity { get; set; }
        [Column("main_sea_level")]
        public int sea_level { get; set; }
        [Column("main_grnd_level")]
        public int grnd_level { get; set; }
    }

    public class Clouds
    {
        [Column("clouds_all")]
        public int all { get; set; }
    }

    public class Wind
    {
        [Column("wind_speed")]
        public float speed { get; set; }
        [Column("wind_deg")]
        public int deg { get; set; }
        [Column("wind_gust")]
        public float gust { get; set; }
    }

    public class Rain
    {
        [Column("rain_1h")]
        public int _1h { get; set; }
        [Column("rain_3h")]
        public int _3h { get; set; }
    }

    public class Snow
    {
        [Column("snow_1h")]
        public int _1h { get; set; }
        [Column("snow_3h")]
        public int _3h { get; set; }
    }

    public class Sys
    {
        [Column("sys_type")]
        public int type { get; set; }
        [Column("sys_id")]
        public int id { get; set; }
        [Column("sys_country")]
        public string country { get; set; }
        [Column("sys_sunrise")]
        public int sunrise { get; set; }
        [Column("sys_sunset")]
        public int sunset { get; set; }
    }
}