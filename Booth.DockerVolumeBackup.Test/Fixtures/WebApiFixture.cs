using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Booth.DockerVolumeBackup.Test.Fixtures
{

    public class WebApiFixture : IDisposable
    {
        private readonly WebApiWebFactory _Factory;

        public WebApiFixture()
        {
            _Factory = new WebApiWebFactory();
        }

        public HttpClient CreateClient() => _Factory.CreateClient();
        public JsonSerializerOptions JsonSerializerOptions
        {
            get
            {
                var options = _Factory.Services.GetRequiredService<IOptions<JsonOptions>>();
                return options.Value.SerializerOptions;
            }
        }

        public void Dispose()
        {
        }
    }

    [CollectionDefinition(Name)]
    public class WebApiFixtureCollection : ICollectionFixture<WebApiFixture>
    {
        public const string Name = nameof(WebApiFixtureCollection);
    }
}
