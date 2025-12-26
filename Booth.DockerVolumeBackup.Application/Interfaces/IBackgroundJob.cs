namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IBackgroundJob
    {
        int Id { get; }
        Task Execute(CancellationToken cancellationToken);
    }
}
