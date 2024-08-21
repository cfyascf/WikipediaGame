using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

using var client = new HttpClient(
    new HttpClientHandler {
        Proxy = new WebProxy {
            Address = new Uri("http://rb-proxy-ca1.bosch.com:8080"),
            Credentials = new NetworkCredential("disrct", "etsps2024401")
        }
    }
);

var regex = new Regex(@"href=""/wiki/(?![^""]*:[^""]*)[^"":]+""");

Queue<String> queue = new Queue<string>();
HashSet<String> visitted = new HashSet<string>();

string currPageLink = null;
string goalPageName = null;

while(currPageLink == null || goalPageName == null)
{
    Console.WriteLine("initial page link: ");
    currPageLink = Console.ReadLine();

    Console.WriteLine("goal page name: ");
    goalPageName = Console.ReadLine();
}

var goalPage = goalPageName.Replace(' ', '_');
var fGoalPage = char.ToUpper(goalPage[0]) + goalPage.Substring(1);

var found = false;
while(true) 
{
    var htmlContent = await client.GetStringAsync(currPageLink);
    visitted.Add(currPageLink);

    var dirtyLinks = regex.Matches(htmlContent);
    var stringLinks = dirtyLinks.Select(x => x.ToString());

    foreach(var link in stringLinks) 
    {
        var decoded = HttpUtility.UrlDecode(link);

        if(decoded.Contains(fGoalPage)) 
        {
            found = true;
            break;
        }

        var url = decoded.Split('=');
        var fUrl = "https://pt.wikipedia.org" + url[1].Trim('"');

        if(!visitted.Contains(fUrl))
        {
            queue.Enqueue(fUrl);
        }
    }

    if(found) break;

    currPageLink = queue.Dequeue();
    
}

Console.WriteLine(fGoalPage + " found on page:" + currPageLink);
