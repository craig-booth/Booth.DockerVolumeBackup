using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSubstitute;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Test.Fixtures.Factories;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal static class DockerClientMock
    {

        public static IDockerClient CreateMock()
        {
            var volumeResource = Substitute.For<IVolumeResource>();
            volumeResource.ListAsync().Returns(VolumeFactory.Generate(10));

            var dockerClient = Substitute.For<IDockerClient>();
            dockerClient.Volumes.Returns(volumeResource);

            return dockerClient;
        }
    }
}
