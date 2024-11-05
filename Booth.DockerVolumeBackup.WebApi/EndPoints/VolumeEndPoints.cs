using Booth.DockerVolumeBackup.WebApi.Services;

namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class VolumeEndPoints
    {

        public static void AddVolumeEndPoints(this WebApplication app) 
        {
            app.MapGet("api/volumes", async (VolumeService volumeService) =>
            {
                var volumes = await volumeService.ListAsync();

                return volumes;
            });

        }


    }
}
