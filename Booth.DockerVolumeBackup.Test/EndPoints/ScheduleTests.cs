using System;
using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.DockerVolumeBackup.Test.Fixtures;
using Booth.DockerVolumeBackup.Application.Schedules.Dtos;

namespace Booth.DockerVolumeBackup.Test.EndPoints
{
    [Collection(nameof(WebApiFixtureCollection))]
    public class ScheduleTests(WebApiFixture fixture)
    {

        [Fact]
        public async Task GetAllSchedules()
        {
            var httpClient = fixture.CreateClient();

            var schedules = await httpClient.GetFromJsonAsync<IReadOnlyList<ScheduleDto>>("api/schedules", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                schedules.Should().NotBeNull();
                schedules.Should().HaveCountGreaterThanOrEqualTo(5);
                schedules?[0].ScheduleId.Should().Be(1);
                schedules?[0].Name.Should().Be("HandmadeBacon");
                schedules?[0].Enabled.Should().BeTrue();
            }
        }

        [Fact]
        public async Task GetSchedule()
        {
            var httpClient = fixture.CreateClient();

            var schedule = await httpClient.GetFromJsonAsync<ScheduleDto>("api/schedules/1", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                schedule.Should().NotBeNull();
                schedule?.ScheduleId.Should().Be(1);
                schedule?.Name.Should().Be("HandmadeBacon");
                schedule?.Enabled.Should().BeTrue();
                schedule?.Days.Should().BeEquivalentTo(new { Sunday = true, Monday = true, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false });
                schedule?.Time.Should().BeCloseTo(new TimeOnly(2, 33, 00), TimeSpan.FromSeconds(1));
                schedule?.Volumes.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task GetScheduleNotExists()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/schedules/5343", TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateSchedule()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            var addedSchedule = await httpClient.GetFromJsonAsync<ScheduleDto>($"api/schedules/{id}", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            using (var scope = new AssertionScope())
            {
                addedSchedule.Should().NotBeNull();
                addedSchedule?.ScheduleId.Should().Be(id);
                addedSchedule?.Name.Should().Be("test");
                addedSchedule?.Enabled.Should().BeTrue();
                addedSchedule?.Days.Should().BeEquivalentTo(new { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false });
                addedSchedule?.Time.Should().Be(new TimeOnly(15, 30));
                addedSchedule?.Volumes.Should().HaveCount(2);
                addedSchedule?.Volumes.Should().Contain("Volume1");
                addedSchedule?.Volumes.Should().Contain("Volume2");
            }
        }

        [Fact]
        public async Task CreateScheduleWithoutName()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            response.Should().Be400BadRequest()
                    .And.OnlyHaveError("Name", "'Name' must not be empty.");
        }

        [Fact]
        public async Task CreateScheduleWithNoDaysSelected()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = false, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            response.Should().Be400BadRequest()
                    .And.OnlyHaveError("Days", "Atleast one day must be selected.");
        }

        [Fact]
        public async Task CreateScheduleWithNoVolumes()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = []
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            response.Should().Be400BadRequest()
                    .And.OnlyHaveError("Volumes", "Atleast one volume must be selected.");
        }

        [Fact]
        public async Task UpdateSchedule()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            schedule.Name = "New Test";
            schedule.Enabled = false;
            schedule.Days.Monday = true;
            schedule.Time = new TimeOnly(23, 45);
            schedule.Volumes = ["Volume3"];
            response = await httpClient.PutAsJsonAsync<ScheduleDto>($"api/schedules/{id}", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            var updatedSchedule = await httpClient.GetFromJsonAsync<ScheduleDto>($"api/schedules/{id}", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            
            using (var scope = new AssertionScope())
            {
                updatedSchedule.Should().NotBeNull();
                updatedSchedule?.ScheduleId.Should().Be(id);
                updatedSchedule?.Name.Should().Be("New Test");
                updatedSchedule?.Enabled.Should().BeFalse();
                updatedSchedule?.Days.Should().BeEquivalentTo(new { Sunday = true, Monday = true, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false });
                updatedSchedule?.Time.Should().Be(new TimeOnly(23, 45));
                updatedSchedule?.Volumes.Should().HaveCount(1);
                updatedSchedule?.Volumes.Should().Contain("Volume3");
            }
        }

        [Fact]
        public async Task UpdateScheduleNotExists()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PutAsJsonAsync<ScheduleDto>("api/schedules/7898", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }

        [Fact]
        public async Task UpdateScheduleWithoutName()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            schedule.Name = "";
            response = await httpClient.PutAsJsonAsync<ScheduleDto>($"api/schedules/{id}", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be400BadRequest()
                    .And.OnlyHaveError("Name", "'Name' must not be empty.");
        }

        [Fact]
        public async Task UpdateScheduleWithNoDaysSelected()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            schedule.Days = new ScheduleDaysDto() { Sunday = false, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false };
            response = await httpClient.PutAsJsonAsync<ScheduleDto>($"api/schedules/{id}", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            response.Should().Be400BadRequest()
                .And.OnlyHaveError("Days", "Atleast one day must be selected.");
        }

        [Fact]
        public async Task UpdateScheduleWithNoVolumes()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            schedule.Volumes.Clear();
            response = await httpClient.PutAsJsonAsync<ScheduleDto>($"api/schedules/{id}", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
           
            response.Should().Be400BadRequest()
                    .And.OnlyHaveError("Volumes", "Atleast one volume must be selected.");
        }

        [Fact]
        public async Task DeleteSchedule()
        {
            var httpClient = fixture.CreateClient();

            var schedule = new ScheduleDto()
            {
                Name = "test",
                Enabled = true,
                Days = new ScheduleDaysDto() { Sunday = true, Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = false },
                Time = new TimeOnly(15, 30),
                Volumes = ["Volume1", "Volume2"]
            };
            var response = await httpClient.PostAsJsonAsync<ScheduleDto>("api/schedules", schedule, fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);
            response.Should().Be200Ok();

            var id = await response.Content.ReadFromJsonAsync<int>(fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            response = await httpClient.DeleteAsync($"api/schedules/{id}", TestContext.Current.CancellationToken);
            response.Should().Be204NoContent();
        }

        [Fact]
        public async Task DeleteScheduleNotExists()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.DeleteAsync($"api/schedules/5656", TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }
    }
}
