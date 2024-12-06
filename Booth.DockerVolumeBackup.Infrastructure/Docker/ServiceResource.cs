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
    internal class ServiceResource : IServiceResource
    {
        private readonly HttpClient _HttpClient;

        internal ServiceResource(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<IList<Service>> ListAsync()
        {
            var response = await _HttpClient.GetFromJsonAsync<Service[]>("/services");
            return response;
        }

        public async Task ScaleAsync(string id, int scale)
        {
            var response = await _HttpClient.GetAsync("/services/" + id);

            var responseContent = await response.Content.ReadAsStringAsync();
            var serviceConfig = JsonNode.Parse(responseContent);

            var version = serviceConfig["Version"]["Index"];
            var specJson = serviceConfig["Spec"];
            var replicasNode = specJson["Mode"]["Replicated"]["Replicas"];
            replicasNode.ReplaceWith(scale);

            var updatedContent = new StringContent(specJson.ToString(), Encoding.UTF8, "application/json");
            response = await _HttpClient.PostAsync("/services/" + id + "/update?version=" + version.ToString(), updatedContent);
            var x = await response.Content.ReadAsStringAsync();
        }
    }
}
