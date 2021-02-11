using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamBattles.Chic
{
    public class Battles
    {
        static RestClient Client = new RestClient("https://teambattles.fortnite.com/");
        static IRestRequest Request = new RestRequest("leaderboards", Method.GET);

        public static ScoreInfo GetScoreInfo()
        {
            var html = new HtmlDocument();
            html.LoadHtml(Client.Execute(Request).Content);
            return JsonConvert.DeserializeObject<ScoreInfo>(html.GetElementbyId("__NEXT_DATA__").InnerHtml);
        }
    }
}
