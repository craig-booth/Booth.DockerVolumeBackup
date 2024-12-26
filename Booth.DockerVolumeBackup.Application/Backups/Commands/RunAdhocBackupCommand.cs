using Dapper;
using MediatR;
using ErrorOr;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands
{
    public record RunAdhocBackupCommand(IEnumerable<string> Volumes) : IRequest<ErrorOr<int>>;

    public class RunAdhocBackupCommandValidator : AbstractValidator<RunAdhocBackupCommand>
    {
        public RunAdhocBackupCommandValidator()
        {
             RuleFor(x => x.Volumes).NotEmpty().WithMessage("Atleast one volume must be selected.");
        }
    }

    internal class RunAdhocBackupCommandHandler(IDataContext dataContext) : IRequestHandler<RunAdhocBackupCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(RunAdhocBackupCommand request, CancellationToken cancellationToken)
        {
            var backupId = 0;

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    INSERT INTO Backup (ScheduleId, Status)
                        VALUES (NULL, @Status) RETURNING RowId;
                """;
                backupId = await connection.ExecuteScalarAsync<int>(sql, new { Status = Status.Queued });

                sql = """
                        INSERT INTO BackupVolume(BackupId, Volume, Status)
                        VALUES(@BackupId, @Volume, @Status);

                    """;

                await connection.ExecuteAsync(sql, request.Volumes.Select(x => new { BackupId = backupId, Volume = x, Status = Status.Queued }));
            }

            return backupId; 
        }
    }
}
