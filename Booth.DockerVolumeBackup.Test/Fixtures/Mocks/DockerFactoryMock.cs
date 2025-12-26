using NSubstitute;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Test.Fixtures.Factories;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal class DockerFactoryMock : IDockerClientFactory
    {
        private IDockerClient _DockerClient;
        public DockerFactoryMock()
        {
            var volumeResource = Substitute.For<IVolumeResource>();
            volumeResource.ListAsync().Returns(VolumeFactory.Generate(10));

            _DockerClient = Substitute.For<IDockerClient>();
            _DockerClient.Volumes.Returns(volumeResource);
        }

        public IDockerClient CreateClient()
        {
            return _DockerClient;
        }
    }
}
