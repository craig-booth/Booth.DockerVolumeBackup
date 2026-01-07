using Booth.DockerVolumeBackup.Domain.Models;
using MediatR;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Linq;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal class DockerHttpMessageHandlerMock : HttpMessageHandler
    {
        private List<UriHandler> _UriHandlers = new List<UriHandler>();

        public DockerHttpMessageHandlerMock()
        {
            _UriHandlers.Add(new UriHandler("/system/df\\?type=volume", "./Fixtures/Mocks/Data/volumes.json"));
            _UriHandlers.Add(new UriHandler("/volumes/create", null, (_, p, request) =>
            {
                if (request != null)
                {
                    var name = request["name"]?.ToString();
                    if (name != null)
                    {
                        return new JsonObject
                        {
                            ["Name"] = name,
                            ["Driver"] = "local",
                            ["Mountpoint"] = $"/var/lib/docker/volumes/{name}/_data",
                            ["Labels"] = new JsonObject(),
                            ["Scope"] = "local"
                        };
                    }
                }
                return null;
            }));
            _UriHandlers.Add(new UriHandler("/services$", "./Fixtures/Mocks/Data/services.json"));
            _UriHandlers.Add(new UriHandler("/services/([^/]+)$", "./Fixtures/Mocks/Data/services.json", (json, p, _) =>
            {
                if (json is null)
                    return null;

                var service = (json as JsonArray)?.FirstOrDefault(x => (x != null) && (x["ID"]?.ToString() == p[1]));

                return service;
            }));
            _UriHandlers.Add(new UriHandler("/services/([^/]+)/update\\?version=([0-9]+)$", null, (_, p, request) =>
            {
                if (request != null)
                {
                    var replicas = request["Mode"]?["Replicated"]?["Replicas"];
                    if (replicas != null)
                        OnMessageHandlerEvent?.Invoke(null, new DockerHttpHandlerEvent($"Service {p[1]} scaled to {replicas} replicas."));
                }


                return null;
            }));
            _UriHandlers.Add(new UriHandler("/containers/json", "./Fixtures/Mocks/Data/containers.json"));
        }

        public event EventHandler<DockerHttpHandlerEvent>? OnMessageHandlerEvent;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null)
            {
                foreach (UriHandler handler in _UriHandlers)
                {
                    if (handler.IsMatch(request.RequestUri))
                    {
                        HttpResponseMessage response;

                        var content = handler.GetResponse(request);
                        if (content != null)
                        {
                            response = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = handler.GetResponse(request)
                            };
                        }
                        else
                        {
                            response = new HttpResponseMessage(HttpStatusCode.NotFound);
                        }

                        return Task.FromResult(response);
                    }
                }
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
        }
    }

    internal class UriHandler
    {
        public string RegEx { get; set; }
        public string? FileName { get; set; }
        public Func<JsonNode?, string[], JsonNode?, JsonNode?>? MutationFunction { get; set; }

        public bool IsMatch(Uri uri)
        {
            return Regex.IsMatch(uri.PathAndQuery, RegEx);
        }

        public HttpContent? GetResponse(HttpRequestMessage request)
        {

            if ((request == null) || (request.RequestUri == null))
                return null;

            var match = Regex.Match(request.RequestUri.PathAndQuery, RegEx);
            if (!match.Success)
                return null;

            JsonNode? jsonNode = null;

            if (FileName != null)
            {
                jsonNode = JsonNode.Parse(File.ReadAllText(FileName));
                if (jsonNode == null)
                    return null;
            }
           
            if (MutationFunction != null)
            {
                // GroupCollection does not support LINQ directly, use Cast<Group>() to enable Select
                var parms = match.Groups.Cast<Group>().Select(g => g.Value).ToArray();

                JsonNode? requestData = null;
                if (request.Content != null)
                    requestData = JsonNode.Parse(request.Content.ReadAsStringAsync().Result);

                jsonNode = MutationFunction?.Invoke(jsonNode, parms, requestData);
            }

            if (jsonNode != null)
            {
                var content = new StringContent(jsonNode.ToJsonString(), Encoding.UTF8, "application/json");
                return content;
            }
            else
            {
                return null;
            }

        }

        public UriHandler(string regEx, string? fileName)
            : this(regEx, fileName, null) { }

        public UriHandler(string regEx, string? fileName, Func<JsonNode?, string[], JsonNode?, JsonNode?>? mutationFuntion)
        {
            RegEx = regEx;
            FileName = fileName;
            MutationFunction = mutationFuntion;
        }
    }

    public class DockerHttpHandlerEvent : EventArgs
    {
        public string Message { get; }

        public DockerHttpHandlerEvent(string message)
        {
            Message = message;
        }
    }
}
