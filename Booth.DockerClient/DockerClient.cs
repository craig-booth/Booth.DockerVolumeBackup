using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Booth.DockerClient.Models;

namespace Booth.DockerClient
{
    public class DockerClient
    {
        public VolumeResource Volumes { get; }

        public DockerClient()
        {
            var messageHandler = new SocketsHttpHandler();
            messageHandler.ConnectCallback = ConnectCallback;
            var httpClient = new HttpClient(messageHandler);

            Volumes = new VolumeResource(httpClient);
        }
        
        private async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext connectionContext, CancellationToken cancellationToken)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await socket.ConnectAsync(new UnixDomainSocketEndPoint("/var/run/docker.sock"));

            var networkStream = new NetworkStream(socket);

            return networkStream;
        } 
    }
}
