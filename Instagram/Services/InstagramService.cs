using System.Text.Json;
using Instagram.Models;
using System.Linq;
using Microsoft.Win32;
using PuppeteerSharp;

// PRIJE POKRETANJA PROMIJENITI https://www.instagram.com/username/

namespace Instagram.Services
{
    public class InstagramService
    {
        public HttpClient client = new HttpClient();
        public RequestModel model = new RequestModel();
        public InstagramService()
        {
            model = getRequestAsync().GetAwaiter().GetResult();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Cookie", model.Cookie);
            client.DefaultRequestHeaders.Add("User-Agent", model.UserAgent);
            client.DefaultRequestHeaders.Add("x-ig-app-id", model.XIgAppId);
        }

        private static string getChromePath()
        {
            var chromePath = "";
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe"))
            {
                if (key != null)
                {
                    chromePath = key.GetValue("") as string;
                }
                return chromePath;
            }
        }
        // Funkcija koja nam dobavlja Cookie, User-Agent, x-ig-app-id
        public async Task<RequestModel> getRequestAsync()
        {
            RequestModel model = new RequestModel();

            string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data");
            string chromePath = getChromePath();

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true, // ako želimo da nam otvori Browser
                ExecutablePath = chromePath, // Putanja do lokalno instaliranog Chrome-a
                UserDataDir = userPath // Putanja do Chrome Profila
            });
            var page = await browser.NewPageAsync();

            string x_ig_app_id = string.Empty;

            // Pretplata na događaj za zahtjeve prije nego što se stranica učita za dohvacanje x-ig-app-id
            page.Request += (sender, e) =>
            {
                var headers = e.Request.Headers;

                if (headers.ContainsKey("X-Ig-App-Id"))
                {
                    x_ig_app_id = headers["x-ig-app-id"];
                }
            };

            var response = await page.GoToAsync("https://www.instagram.com/username/");
            await Task.Delay(2000);

            model.XIgAppId = x_ig_app_id;

            // Dohvatanje kolačića
            var cookiesList = await page.GetCookiesAsync();

            string finalCookies = "";
            foreach (var cookie in cookiesList)
            {
                finalCookies = finalCookies + cookie.Name +"="+cookie.Value + "; ";
                if(cookie.Name == "ds_user_id")
                {
                    model.UserId = cookie.Value;
                }
            }
            model.Cookie = finalCookies;

            // Dohvatanje userAgenta
            model.UserAgent = await page.EvaluateExpressionAsync<string>("navigator.userAgent");

            await browser.CloseAsync();
            return model;
        }

        public async Task<List<User>> GetFollowersAsync()
        {
            string url = "https://www.instagram.com/api/v1/friendships/" + model.UserId + "/followers/?count=25&search_surface=follow_list_page";

            HttpResponseMessage response = await client.GetAsync(url);

            List<User> followersList = new List<User>();   

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                Root? instagramData = JsonSerializer.Deserialize<Root>(json);

                followersList.AddRange(instagramData.users);
                
                // dobavljanje narednih 
                string? nextMaxId = instagramData.next_max_id ;

                while(nextMaxId != null)
                {
                    url = url + "&max_id=" + nextMaxId;
                    response = await client.GetAsync(url);
                    json = await response.Content.ReadAsStringAsync();
                    instagramData = JsonSerializer.Deserialize<Root>(json);
                    followersList.AddRange(instagramData.users);
                    nextMaxId = instagramData.next_max_id;
                    await Task.Delay(1000);
                }

                // Vraćanje rezultata kao JSON odgovor
                return followersList;
            }
            else
            {
                // Vraćanje greške ako odgovor nije uspešan
                throw new Exception("Greska");
            }
        }

        public async Task<List<User>> GetFollowingAsync()
        {
            string url = "https://www.instagram.com/api/v1/friendships/" + model.UserId + "/following/?count=50";

            HttpResponseMessage response = await client.GetAsync(url);

            List<User> followingList = new List<User>();

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                Root? instagramData = JsonSerializer.Deserialize<Root>(json);

                followingList.AddRange(instagramData.users);

                // dobavljanje narednih 
                string? nextMaxId = instagramData.next_max_id;

                while (nextMaxId != null)
                {
                    url = url + "&max_id=" + nextMaxId;
                    response = await client.GetAsync(url);
                    json = await response.Content.ReadAsStringAsync();
                    instagramData = JsonSerializer.Deserialize<Root>(json);
                    followingList.AddRange(instagramData.users);
                    nextMaxId = instagramData.next_max_id;
                    await Task.Delay(2000);
                }

                // Vraćanje rezultata kao JSON odgovor
                return followingList;
            }
            else
            {
                // Vraćanje greške ako odgovor nije uspešan
                throw new Exception("Greska");
            }
        }

        public async Task<List<User>> NotFollowingBackAsync()
        {
            var followersList = await GetFollowersAsync();
            var followingList = await GetFollowingAsync();

            List<User> notFollowingBack = new List<User>(); 

            bool followMe = false;

            foreach (var followingUser in followingList)
            {
                followMe = false;
                foreach (var followerUser in followersList)
                {
                    if (followingUser.username == followerUser.username)
                    {
                        followMe = true;
                    }
                }
                if (!followMe) { 
                    notFollowingBack.Add(followingUser);
                }
            }
            return notFollowingBack;
        }
    }
}

