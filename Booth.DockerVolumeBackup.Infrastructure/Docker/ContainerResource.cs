using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;


namespace Booth.DockerVolumeBackup.Infrastructure.Docker
{
    internal class ContainerResource : IContainerResource
    {
        private readonly HttpClient _HttpClient;

        internal ContainerResource(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<IList<Container>> ListAsync()
        {
            var response = await _HttpClient.GetFromJsonAsync<Container[]>("/containers/json");
            if (response != null)
                return response;
            else
                return [];      
        }
    }
}
