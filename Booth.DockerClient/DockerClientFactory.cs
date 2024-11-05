﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;


namespace Booth.Docker
{
    public class DockerClientFactory
    {

        public static IDockerClient CreateClient(string url)
        {
            if ((url != null) && url.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateHttpClient(url);
            }

            string unixUrl;
            if ((url != null) && url.StartsWith("unix://", StringComparison.CurrentCultureIgnoreCase))
            {
                unixUrl = url.Substring(7);
            }
            else
            {
                unixUrl = url;
            }

            return CreateUnixClient(unixUrl);
        }

        public static IDockerClient CreateUnixClient(string url)
        {
            var path = url;
            if (path == null)
                path = "/var/run/docker.sock";

            var messageHandler = new SocketsHttpHandler()
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(path));

                    return new NetworkStream(socket, false);
                }
            };
            var httpClient = new HttpClient(messageHandler);
            httpClient.BaseAddress = new Uri("http://127.0.0.1");

            return new DockerClient(httpClient);
        }

        public static IDockerClient CreateHttpClient(string url)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);

            return new DockerClient(httpClient);
        }
    }
}
