using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Xunit;

using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

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
                var options = _Factory.Services.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
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
