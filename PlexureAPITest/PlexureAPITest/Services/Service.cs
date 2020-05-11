using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PlexureAPITest.Entities;

namespace PlexureAPITest.Services
{
    public class Service : IDisposable
    {
        private HttpClient _client;

        public Service()
        {
            _client = new HttpClient {BaseAddress = new Uri("https://qatestapi.azurewebsites.net")};

            _client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Extracting this from the Login APi Call , hence commenting out

           // client.DefaultRequestHeaders.Add("token", "37cb9e58-99db-423c-9da5-42d5627614c5");
        }

        public Response<UserEntity> Login(string username, string password,bool clearHeaders = false)
        {
            if (clearHeaders)
            {
                _client.DefaultRequestHeaders.Clear();
            }

            var dict = new Dictionary<string, string> {{"UserName", username}, {"Password", password}};

            string json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using (var response = _client.PostAsync("api/login", httpContent).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    var user = JsonConvert.DeserializeObject<UserEntity>(response.Content.ReadAsStringAsync().Result);

                    _client.DefaultRequestHeaders.Add("token", user.AccessToken);

                    return new Response<UserEntity>(response.StatusCode, user);
                }

                return new Response<UserEntity>(response.StatusCode, response.Content.ReadAsStringAsync().Result);
            }
        }

        public Response<PurchaseEntity> Purchase(int productId)
        {
            var dict = new Dictionary<string, int> {{"ProductId", productId}};

            string json = JsonConvert.SerializeObject(dict, Formatting.Indented);

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using (var response = _client.PostAsync("api/purchase", httpContent).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    var purchase =
                        JsonConvert.DeserializeObject<PurchaseEntity>(response.Content.ReadAsStringAsync().Result);

                    return new Response<PurchaseEntity>(response.StatusCode, purchase);
                }

                return new Response<PurchaseEntity>(response.StatusCode, response.Content.ReadAsStringAsync().Result);
            }
        }

        public Response<PointsEntity> GetPoints()
        {
            using (var response = _client.GetAsync("api/points").Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    var points =
                        JsonConvert.DeserializeObject<PointsEntity>(response.Content.ReadAsStringAsync().Result);
                    return new Response<PointsEntity>(response.StatusCode, points);
                }

                return new Response<PointsEntity>(response.StatusCode, response.Content.ReadAsStringAsync().Result);
            }
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
    }
}