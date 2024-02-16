using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using CloudSwyft.Web.Api.Models;
using Microsoft.WindowsAzure.Storage.File;
using Newtonsoft.Json;

namespace CloudSwyft.Web.Api.Controllers
{
    [RoutePrefix("api/Regions")]
    public class RegionsController : ApiController
    {
        private readonly VirtualEnvironmentDbContext _db = new VirtualEnvironmentDbContext();

        [HttpGet]
        [Route("GetRegions")]
        public List<string> GetRegions()
        {
            return _db.Regions.Select(q => q.RegionName).ToList();
        }


        [HttpGet]
        [Route("GetTimeZone")]
        public List<TimeZoneRegions> GetTimeZone() 
        {
            var timeZones = new List<TimeZoneRegions>();

            ReadOnlyCollection<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo zone in zones)
            {
                var timeZone = new TimeZoneRegions();

                timeZone.Id = zone.Id;
                timeZone.Name = zone.DisplayName;
                timeZone.Hours = zone.BaseUtcOffset.Hours;
                
                timeZones.Add(timeZone);
                

            }

            return timeZones;
        }

        [HttpGet]
        [Route("Test")]
        public List<string> Test()
        {
            var timeZones = new List<string>();
            try
            {

                var cs = _db.CloudLabUsers.Select(q => q.Email).ToList();

                foreach (var zone in cs)
                {
                    timeZones.Add(zone);
                }

                var dataMessage = JsonConvert.SerializeObject(timeZones);


                var data = new
                {
                    applicationSubscriptionId = "e31da928-53df-4a6d-bcbc-474003cd2859",
                    applicationTenantId = "40082ace-5811-423c-9ab7-ad70a2e499ad",
                    applicationSecret = "_fa8Q~MwQHgF7gbrcwsodeLV-jDcYO7z98en3dxo",
                    applicationId = "44ffe7d5-21cf-4605-9e86-569f1c443e84"
                };
                var dataMsg = JsonConvert.SerializeObject(data);

                var sss = "https://35398432-ea25-463a-afb0-a55d10490820.webhook.ea.azure-automation.net/webhooks?token=9qgr%2bHi6H8ZlVccCr6yUM5I88aYkLzYPsj6ALAiF0Vk%3d";

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(sss);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.PostAsync("", new StringContent(dataMsg, Encoding.UTF8, "application/json"));
                return timeZones;

            }
            catch {
                return timeZones;

            }

        }

        [HttpGet]
        [Route("GetListTimeRange")]
        public List<string> GetListTimeRange()
        {
            var timeRange = new List<string>();

            var start = DateTime.Today;

            var clockQuery = from offset in Enumerable.Range(0, 48)
                             select TimeSpan.FromMinutes(30 * offset);
            foreach (var time in clockQuery)
            {
                var s = (start + time);
                var sss = s.ToString("hh:mm tt").ToUpper();
                
                timeRange.Add(sss);
            }

            return timeRange;
        }
    }

    

    //[HttpGet]
    //[Route("GetRegions")]
    //public async Task<AzureRegions[]> GetRegions(int tenantId)
    //{
    //    //var url = "/labs/location";

    //    //var tenant = _dbTenant.AzTenants.Where(q => q.TenantId == tenantId).Select(w => new { w.TenantKey, w.SubscriptionKey }).FirstOrDefault();
    //    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

    //    //HttpClient client = new HttpClient();
    //    //client.BaseAddress = new Uri(AzureVM);
    //    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    //    //client.DefaultRequestHeaders.Add("TenantId", tenant.TenantKey);
    //    //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", tenant.SubscriptionKey);

    //    //HttpResponseMessage response = null;
    //    //response = await client.GetAsync(url);

    //    //var result = await response.Content.ReadAsStringAsync();
    //    //var data = JsonConvert.DeserializeObject<AzureRegions[]>(result);
    //    //return data;

    //}
}
