using Booth.DockerVolumeBackup.Infrastructure.Docker;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal class DockerFactoryMock : IDockerClientFactory
    {
        public event EventHandler<DockerHttpHandlerEvent>? OnMessageHandlerEvent;
        public IDockerClient CreateClient()
        {
            var messageHandler = new DockerHttpMessageHandlerMock();
            messageHandler.OnMessageHandlerEvent += (s, e) => OnMessageHandlerEvent?.Invoke(s, e);


            var httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri("http://mock")
            };
            return new DockerClient(httpClient);
        }
    }
}
