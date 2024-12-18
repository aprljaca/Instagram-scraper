﻿using System.Text.Json;
using Instagram.Models;
using System.Linq;
using Microsoft.Win32;
using PuppeteerSharp;
using System;

namespace Instagram.Services
{
    public class InstagramService
    {
        public HttpClient client = new HttpClient();

        public static RequestModel model = null;

        public static List<User> followersList = new List<User>();
        
        public static List<User> followingList = new List<User>();

        Random random = new Random();

        public InstagramService()
        {
            if(model == null)
            {
                model = new RequestModel();
                model = getRequestAsync().GetAwaiter().GetResult();
            }
            client.DefaultRequestHeaders.Add("Cookie", model.Cookie);
            client.DefaultRequestHeaders.Add("User-Agent", model.UserAgent);
            client.DefaultRequestHeaders.Add("x-ig-app-id", model.XIgAppId);
            if (!followersList.Any())
            {
                GetFollowersAsync().GetAwaiter().GetResult(); // ne možemo koristiti await u konstruktoru 
                GetFollowingAsync().GetAwaiter().GetResult();
            }
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
        
        public async Task<RequestModel> getRequestAsync() // Funkcija koja nam dobavlja Cookie, User-Agent, x-ig-app-id
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

            var response = await page.GoToAsync("https://www.instagram.com");
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

            try
            {
                string json = await response.Content.ReadAsStringAsync();
                Root? instagramData = JsonSerializer.Deserialize<Root>(json);

                followersList.AddRange(instagramData.users);

                // dobavljanje narednih 
                string? nextMaxId = instagramData.next_max_id;

                while (nextMaxId != null)
                {
                    url = url + "&max_id=" + nextMaxId;
                    response = await client.GetAsync(url);
                    json = await response.Content.ReadAsStringAsync();
                    instagramData = JsonSerializer.Deserialize<Root>(json);
                    followersList.AddRange(instagramData.users);
                    nextMaxId = instagramData.next_max_id;

                    int delay = random.Next(2000, 5001);
                    await Task.Delay(delay);
                }

                // Vraćanje rezultata kao JSON odgovor
                return followersList;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<User>> GetFollowingAsync()
        {
            string url = "https://www.instagram.com/api/v1/friendships/" + model.UserId + "/following/?count=50";

            HttpResponseMessage response = await client.GetAsync(url);

            try {
                
                    string json = await response.Content.ReadAsStringAsync();
                    Root? instagramData = JsonSerializer.Deserialize<Root>(json);

                    followingList.AddRange(instagramData.users);

                    // Dobavljanje narednih user-a
                    string? nextMaxId = instagramData.next_max_id;
       
                    while (nextMaxId != null)
                    {
                        url = url + "&max_id=" + nextMaxId;
                        response = await client.GetAsync(url);
                        json = await response.Content.ReadAsStringAsync();
                        instagramData = JsonSerializer.Deserialize<Root>(json);
                        followingList.AddRange(instagramData.users);
                        nextMaxId = instagramData.next_max_id;

                        int delay = random.Next(2000, 5001);
                        await Task.Delay(delay);
                    }

                    // Vraćanje rezultata kao JSON odgovor      
                    return followingList;
                
            } catch (Exception)
            {
                throw;
            }
        }

        public List<UserResponseModel> NotFollowingBackAsync()
        {
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
            return UserToResponseModelAsync(notFollowingBack);
        }

        public List<UserResponseModel> FollowBackAsync()
        {
            List<User> FollowBack = new List<User>();

            bool follow = false;

            foreach (var followerUser in followersList)
            {
                follow = false;
                foreach (var followingUser in followingList)
                {
                    if (followerUser.username == followingUser.username)
                    {
                        follow = true;
                    }
                }
                if (!follow)
                {
                    FollowBack.Add(followerUser);
                }
            }
            return UserToResponseModelAsync(FollowBack);
        }

        public List<UserResponseModel> UserToResponseModelAsync(List<User> users)
        {
            List<UserResponseModel> userResponse = new List<UserResponseModel>();
            
            foreach (var user in users)
            {
                UserResponseModel responseModel = new UserResponseModel();
                responseModel.UserName = user.username;
                responseModel.FullName = user.full_name;
                responseModel.ProfilePicUrl = user.profile_pic_url;
                userResponse.Add(responseModel);
            }
            return userResponse;
        }


    }
}

