using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Booth.DockerClient.Models;
using Newtonsoft.Json;

namespace Booth.DockerClient
{
    public class VolumeResource
    {
        private readonly HttpClient _HttpClient;

        public VolumeResource(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<VolumeResponse> List()
        {
            var response = await _HttpClient.GetAsync("/volumes");
            if (response.IsSuccessStatusCode)
            {
                var volumes = JsonConvert.DeserializeObject<VolumeResponse>(await response.Content.ReadAsStringAsync());

                return volumes;
            }
            else
                return null;
        }
    }
}
