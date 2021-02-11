using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TeamBattles.Chic
{
    public struct ScoreInfo
    {
        [JsonProperty("props")]
        public Props Props;
    }

    public struct Props
    {
        [JsonProperty("pageProps")]
        public PageProps PageProps;
    }

    public struct PageProps
    {
        [JsonProperty("eventState")]
        public EventState EventState;
        [JsonProperty("teams")]
        public List<Team> Teams;
    }

    public struct EventState
    {
        [JsonProperty("eventState")]
        public string State;
        [JsonProperty("dayIndex")]
        public int DayIndex;
        [JsonProperty("nextDate")]
        public DateTime NextDate;
    }

    public struct Team
    {
        [JsonProperty("_id")]
        public string Id;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("region")]
        public string Region;
        [JsonProperty("nationality")]
        public string Nationality;
        [JsonProperty("score")]
        public int Score;
        [JsonProperty("lastUpdatedDate")]
        public long LastUpdatedDate;
    }

    public class TeamComparer : IComparer<Team>
    {
        public static TeamComparer Comparer = new TeamComparer();

        public int Compare(Team x, Team y)
        {
            return x.Score > y.Score ? -1 : x.Score < y.Score ? 1 : 0;
        }
    }
}
