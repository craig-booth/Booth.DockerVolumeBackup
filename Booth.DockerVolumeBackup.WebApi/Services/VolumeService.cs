using Booth.Docker;
using Booth.DockerVolumeBackup.WebApi.DataProviders;
using Booth.DockerVolumeBackup.WebApi.Models;

namespace Booth.DockerVolumeBackup.WebApi.Services
{
    public class VolumeService
    {
        private readonly IBackupDataProvider _DataProvider;
        private readonly IDockerClient _DockerClient;

        public VolumeService(IDockerClient dockerClient, IBackupDataProvider dataProvider) 
        {
            _DataProvider = dataProvider;
            _DockerClient = dockerClient;
        }

        public async Task<List<Volume>> ListAsync()
        {
            var volumes = await _DockerClient.Volumes.ListAsync();

            var volumeLastBackup = new Dictionary<string, DateTimeOffset?>();
            foreach (var volume in volumes)
            {
                volumeLastBackup.Add(volume.Name, null);
            }
            await _DataProvider.GetLastBackupDates(volumeLastBackup);

            return volumes.Select(x => new Volume { Name = x.Name, Size = x.UsageData.Size, LastBackup = volumeLastBackup[x.Name] } ).ToList();    
        }


    }
}
