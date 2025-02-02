using Bogus;

using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Factories
{
    internal static class VolumeFactory
    {
        private static readonly Faker<Volume> VolumeFaker = new Faker<Volume>().UseSeed(1332).CustomInstantiator(f =>
        {
            var volume = new Volume()
            {
                Name = f.Name.FirstName() + "_" + f.Name.LastName(),
                Mountpoint = f.System.DirectoryPath(),
                UsageData = new VolumeUsageData()
                {
                    Size = f.Random.Number(0, 10000000)
                }
            };

            return volume;
        });

        public static Volume Generate() => VolumeFaker.Generate();

        public static List<Volume> Generate(int count) => VolumeFaker.Generate(count);
    }
}
