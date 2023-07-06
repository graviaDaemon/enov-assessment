using System;
using Enov.API.Entities;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enov.API.OpenApi;

public class ResponseBody
{
    [JsonProperty("result")][OpenApiProperty] public Item Result { get; set; }
}

public class ResponseBodyExample : OpenApiExample<ResponseBody>
{
    public override IOpenApiExample<ResponseBody> Build(NamingStrategy namingStrategy = null)
    {
        Examples.Add(OpenApiExampleResolver.Resolve("ResponseBodyExample", new ResponseBody
        {
            Result = new Item
            {
                Id = 1,
                Name = "Soufian",
                IsNew = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        }));
        return this;
    }
}