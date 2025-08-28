using System;
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.DockerVolumeBackup.Test.Fixtures;
using Booth.DockerVolumeBackup.Application.Backups.Common;
using Booth.DockerVolumeBackup.WebApi.Dtos;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetAllBackups;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackup;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackupStatus;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackupStatusEvents;


namespace Booth.DockerVolumeBackup.Test.EndPoints
{

    [Collection(nameof(WebApiFixtureCollection))]
    public class BackupTests(WebApiFixture fixture)
    {

        [Fact]
        public async Task GetAllBackups()
        {
            var httpClient = fixture.CreateClient();

            var backups = await httpClient.GetFromJsonAsync<IReadOnlyList<Application.Backups.Queries.GetAllBackups.BackupDto>>("api/backups", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().HaveCountGreaterThan(100);
                backups?[0].BackupId.Should().Be(1);
                backups?[0].ScheduleId.Should().Be(1);
                backups?[0].ScheduleName.Should().Be("ErgonomicFish");
                backups?[0].Status.Should().Be(StatusDto.Complete);
            }
        }

        [Fact]
        public async Task GetAllBackupsForSchedule()
        {
            var httpClient = fixture.CreateClient();

            var backups = await httpClient.GetFromJsonAsync<IReadOnlyList<Application.Backups.Queries.GetAllBackups.BackupDto>>("api/backups?scheduleid=2", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().HaveCount(46);
                backups.Should().AllSatisfy(x => x.ScheduleId.Should().Be(2));
            }
        }

        [Fact]
        public async Task GetBackupById()
        {
            var httpClient = fixture.CreateClient();

            var backup = await httpClient.GetFromJsonAsync<Application.Backups.Queries.GetBackup.BackupDto>("api/backups/1", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                backup.Should().NotBeNull();
                backup?.BackupId.Should().Be(1);
                backup?.ScheduleId.Should().Be(1);
                backup?.ScheduleName.Should().Be("ErgonomicFish");
                backup?.Status.Should().Be(StatusDto.Complete);
                backup?.StartTime.Should().BeBefore(DateTimeOffset.UtcNow);
                backup?.EndTime.Should().BeBefore(DateTimeOffset.UtcNow);
                backup?.Volumes.Should().HaveCount(3);
            }
        }


        [Fact]
        public async Task GetBackupByIdNotFound()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/backups/1000", TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetBackupStatus()
        {
            var httpClient = fixture.CreateClient();

            var status = await httpClient.GetFromJsonAsync<Application.Backups.Queries.GetBackupStatus.BackupStatusDto>("api/backups/1/status", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                status.Should().NotBeNull();
                status?.BackupId.Should().Be(1);
                status?.Status.Should().Be(StatusDto.Complete);
                status?.Volumes.Should().HaveCount(3);
            }
        }

        [Fact]
        public async Task GetBackupStatusNotFound()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/backups/9687/status", TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }

        [Fact]
        public async Task GetBackupStatusEvents()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/backups/1/statusevents", TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            List<Application.Backups.Queries.GetBackupStatusEvents.BackupStatusDto> statuses = [];
            var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
            using (var streamReader = new StreamReader(stream))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync(TestContext.Current.CancellationToken);
                    if (!String.IsNullOrEmpty(line))
                    {
                        if (line.StartsWith("data: "))
                        {
                            var status = JsonSerializer.Deserialize<Application.Backups.Queries.GetBackupStatusEvents.BackupStatusDto>(line.Substring(6), fixture.JsonSerializerOptions);
                            if (status != null)
                                statuses.Add(status);
                        }
                    }
                }
            }

            statuses.Should().HaveCount(1);
            statuses.Last().Status.Should().Be(StatusDto.Complete);
        }

        [Fact]
        public async Task GetBackupStatusEventsNotFound()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/backups/9687/statusevents", TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }
        /*
                [Fact]
                public async Task GetBackupLog()
                {
                    throw new NotImplementedException();
                }

                [Fact]
                public async Task GetBackupLogNotFound()
                {
                    throw new NotImplementedException();
                } */

        [Fact]
        public async Task DeleteBackupNotFound()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.DeleteAsync("api/backups/9687", TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }

        [Fact]
        public async Task DeleteBackup()
        {
            var httpClient = fixture.CreateClient();

            // Create a backup to delete
            var request = new VolumeBackupRequestDto()
            {
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<VolumeBackupRequestDto>("api/backups/run", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            response = await httpClient.DeleteAsync($"api/backups/{id}", TestContext.Current.CancellationToken);
            response.Should().Be204NoContent();

            response = await httpClient.GetAsync($"api/backups/{id}", TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMulipleBackupsNoBackupIds()
        {
            var httpClient = fixture.CreateClient();

            var request = new BackupDeleteRequestDto() { };

            var response = await httpClient.PostAsJsonAsync<BackupDeleteRequestDto>("api/backups/delete", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be400BadRequest()
                .And.OnlyHaveError("BackupIds", "Atleast one backup must be selected.");
        }

        [Fact]
        public async Task DeleteMulipleBackups()
        {
            var httpClient = fixture.CreateClient();

            // Create a backup to delete
            var request = new VolumeBackupRequestDto()
            {
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<VolumeBackupRequestDto>("api/backups/run", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id1 = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            // Create a second backup to delete
            request = new VolumeBackupRequestDto()
            {
                Volumes = ["Volume1"]
            };
            response = await httpClient.PostAsJsonAsync<VolumeBackupRequestDto>("api/backups/run", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id2 = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);


            var deleteRequest = new BackupDeleteRequestDto()
            {
                BackupIds = [id1, id2]
            };

            response = await httpClient.PostAsJsonAsync<BackupDeleteRequestDto>("api/backups/delete", deleteRequest, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var count = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            count.Should().Be(2);
        }

        [Fact]
        public async Task DeleteMulipleBackupsPartialSuccess()
        {
            var httpClient = fixture.CreateClient();

            // Create a backup to delete
            var request = new VolumeBackupRequestDto()
            {
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<VolumeBackupRequestDto>("api/backups/run", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id1 = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);


            var id2 = 9875;

            var deleteRequest = new BackupDeleteRequestDto()
            {
                BackupIds = [id1, id2]
            };

            response = await httpClient.PostAsJsonAsync<BackupDeleteRequestDto>("api/backups/delete", deleteRequest, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var count = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            count.Should().Be(1);
        }

        [Fact]
        public async Task RunScheduledBackup()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.PostAsync("api/backups/1/run", null, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            var status = await httpClient.GetFromJsonAsync<Application.Backups.Queries.GetBackupStatus.BackupStatusDto>($"api/backups/{id}/status", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            using (var scope = new AssertionScope())
            {
                status.Should().NotBeNull();
                status?.BackupId.Should().Be(id);
                status?.Status.Should().Be(StatusDto.Queued);
                status?.Volumes.Should().HaveCount(3);
            }
        }

        [Fact]
        public async Task RunScheduledBackupNotFound()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.PostAsync("api/backups/9787/run", null, TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }

        [Fact]
        public async Task RunAdhocBackup()
        {
            var httpClient = fixture.CreateClient();

            var request = new VolumeBackupRequestDto()
            {
                Volumes = ["Volume1", "Volume2"]
            };

            var response = await httpClient.PostAsJsonAsync<VolumeBackupRequestDto>("api/backups/run", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            var status = await httpClient.GetFromJsonAsync<Application.Backups.Queries.GetBackupStatus.BackupStatusDto>($"api/backups/{id}/status", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            using (var scope = new AssertionScope())
            {
                status.Should().NotBeNull();
                status?.BackupId.Should().Be(id);
                status?.Status.Should().Be(StatusDto.Queued);
                status?.Volumes.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task RunAdhocBackupNoVolumes()
        {
            var httpClient = fixture.CreateClient();

            var request = new VolumeBackupRequestDto()
            {
                Volumes = []
            };

            var response = await httpClient.PostAsJsonAsync<VolumeBackupRequestDto>("api/backups/run", request, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be400BadRequest()
                    .And.OnlyHaveError("Volumes", "Atleast one volume must be selected.");
        }
    }
}
