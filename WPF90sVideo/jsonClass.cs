using Newtonsoft.Json;

namespace WPF90sVideo

{
    public class Rootobject
    {
        public Rootobject()
        {
            Hits = new Hits();
            Beat = new Beat();
            Dance = new Dance();
            Boygroups = new Boygroups();
        }

        [JsonProperty("1")]
        public Hits Hits;

        [JsonProperty("2")]
        public Beat Beat;

        [JsonProperty("3")]
        public Dance Dance;

        [JsonProperty("4")]
        public Boygroups Boygroups;
    }

    public class Hits
    {
        public string stream { get; set; }
        public int station_id { get; set; }
        public string song_title { get; set; }
        public string artist_name { get; set; }
        public CoversHits covers { get; set; }
    }

    public class CoversHits
    {
        public string cover_art_url_xs { get; set; }
        public string cover_art_url_s { get; set; }
        public string cover_art_url_m { get; set; }
        public string cover_art_url_l { get; set; }
        public string cover_art_url_xl { get; set; }
        public string cover_art_url_xxl { get; set; }
        public string cover_art_url_custom { get; set; }
    }

    public class Beat
    {
        public string stream { get; set; }
        public int station_id { get; set; }
        public string song_title { get; set; }
        public string artist_name { get; set; }
        public CoversBeat covers { get; set; }
    }

    public class CoversBeat
    {
        public string cover_art_url_xs { get; set; }
        public string cover_art_url_s { get; set; }
        public string cover_art_url_m { get; set; }
        public string cover_art_url_l { get; set; }
        public string cover_art_url_xl { get; set; }
        public string cover_art_url_xxl { get; set; }
        public string cover_art_url_custom { get; set; }
    }

    public class Dance
    {
        public string stream { get; set; }
        public int station_id { get; set; }
        public string song_title { get; set; }
        public string artist_name { get; set; }
        public CoversDance covers { get; set; }
    }

    public class CoversDance
    {
        public string cover_art_url_xs { get; set; }
        public string cover_art_url_s { get; set; }
        public string cover_art_url_m { get; set; }
        public string cover_art_url_l { get; set; }
        public string cover_art_url_xl { get; set; }
        public string cover_art_url_xxl { get; set; }
        public string cover_art_url_custom { get; set; }
    }

    public class Boygroups
    {
        public string stream { get; set; }
        public int station_id { get; set; }
        public string song_title { get; set; }
        public string artist_name { get; set; }
        public CoversDance covers { get; set; }
    }

    public class CoversBoygroups
    {
        public string cover_art_url_xs { get; set; }
        public string cover_art_url_s { get; set; }
        public string cover_art_url_m { get; set; }
        public string cover_art_url_l { get; set; }
        public string cover_art_url_xl { get; set; }
        public string cover_art_url_xxl { get; set; }
        public string cover_art_url_custom { get; set; }
    }
}
