﻿using System.Collections.Generic;
using Enov.API.Entities;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enov.API.OpenApi;

/// <summary>
/// The Request Body for calling the API. Swagger Documentation classes
/// </summary>
public class RequestBody
{
    /// <summary>
    /// Expects a list of Item data models in the request body
    /// </summary>
    [JsonProperty("items")] [OpenApiProperty] public Item[] Items { get; set; }
}

/// <summary>
/// The request body example, to show in the swagger documentation
/// </summary>
[OpenApiExample(typeof(RequestBodyExample))]
public class RequestBodyExample : OpenApiExample<IEnumerable<RequestBody>>
{
    public override IOpenApiExample<IEnumerable<RequestBody>> Build(NamingStrategy namingStrategy = null)
    {
        Examples.Add(OpenApiExampleResolver.Resolve("RequestBodyExample", new RequestBody
        {
            Items = new []
            {
                new Item
                {
                    Id = 0,
                    Name = "string",
                    IsNew = true
                }
            }
        }));
        return this;
    }
}