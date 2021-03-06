﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Deserializers;

namespace LobotJR.Shared.Utility
{
    public class NewtonsoftDeserializer : IDeserializer
    {
        public static NewtonsoftDeserializer Default { get; private set; } = new NewtonsoftDeserializer();

        T IDeserializer.Deserialize<T>(IRestResponse response)
        {
            var resolver = new DefaultContractResolver();
            resolver.NamingStrategy = new SnakeCaseNamingStrategy(true, false, true);
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = resolver;
            return JsonConvert.DeserializeObject<T>(response.Content, settings);
        }
    }
}
