using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

using Booth.Docker.Models;


namespace Booth.Docker
{
    internal class VolumeResource : IVolumeResource
    {
        private readonly HttpClient _HttpClient;

        internal VolumeResource(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }


        private class VolumeResponse
        {
            public List<Volume> Volumes { get; set; }
        }
        public async Task<IList<Volume>> ListAsync()
        {
            var response = await _HttpClient.GetFromJsonAsync<VolumeResponse>("/system/df?type=volume");
            return response.Volumes;
        }
    }
}
