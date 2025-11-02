using Bogus;
using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;


namespace Booth.DockerVolumeBackup.Infrastructure.Docker
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
            public List<Volume> Volumes { get; set; } = new List<Volume>();
        }
        public async Task<IList<Volume>> ListAsync()
        {
            var response = await _HttpClient.GetFromJsonAsync<VolumeResponse>("/system/df?type=volume");
            return response != null ? response.Volumes : [];
        }

        public async Task<Volume?> CreateAsync(string name)
        {
            var volumeConfig = new VolumeConfiguration
            { 
                Name = name,
                Driver = "local"
            };
            var response = await _HttpClient.PostAsJsonAsync("/volumes/create", volumeConfig);

            if (!response.IsSuccessStatusCode)
                return null;

            var volume = await response.Content.ReadFromJsonAsync<Volume>();
            return volume;
        }
    }
}
