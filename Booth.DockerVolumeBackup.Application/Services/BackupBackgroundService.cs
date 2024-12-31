using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetNextBakupToRun;

namespace Booth.DockerVolumeBackup.Application.Services
{
    public class BackupBackgroundService : BackgroundService
    {
        private const int SLEEP_TIME = 15;
        private readonly IBackupService _BackupService;
        private IMediator _Mediator;
        private ILogger<BackupBackgroundService> _Logger;
        public BackupBackgroundService(IBackupService backupService, IMediator mediator, ILogger<BackupBackgroundService> logger) 
        {
            _BackupService = backupService;
            _Mediator = mediator;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10000);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _Mediator.Send(new GetNextBackupToRunQuery())
                    .ThenDoAsync(nextBackup => ExecuteBackupAsync(nextBackup, stoppingToken));

                await Task.Delay(SLEEP_TIME * 1000);
            }
        }

        private async Task ExecuteBackupAsync(int backupId, CancellationToken stoppingToken)
        { 
            await _BackupService.RunBackupAsync(backupId, stoppingToken);
        }

 

    }
}
