using System;
using System.Threading.Tasks;
using Booth.DockerClient;

namespace Booth.DockerTest
{
    class Program
    {
        static void Main(string[] args)
        {

            var t = TestClient();

            t.Wait();

            Console.WriteLine("Done!");
        }

        static async Task TestClient()
        {
            var dockerClient = new DockerClient.DockerClient();

            var volumes = await dockerClient.Volumes.List();

            foreach (var volume in volumes.Volumes)
                Console.WriteLine(volume.Name);
        }

    }

}
