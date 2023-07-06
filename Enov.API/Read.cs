using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Enov.API.OpenApi;
using Enov.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace Enovation.Assessment.API;

public class Read
{
    private readonly IDatabaseService _service;

    public Read(IDatabaseService service)
    {
        _service = service;
    }
    
    [FunctionName("Read")]
    [OpenApiOperation("Run", tags: new[] { "Run" }, Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "ids", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "The query parameter for the ID.")]
    [OpenApiResponseWithBody(contentType: "application/json", bodyType: typeof(ResponseBody),
        statusCode: HttpStatusCode.OK, Description = "The result", Example = typeof(ResponseBodyExample))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
    {
        StringValues request = req.Query["ids"];
        string[] ids = request[0].Split(",");

        IEnumerable<Item> res = await _service.GetAsync(ids);

        if (res.Any())
            return new OkObjectResult(res.ToArray());
        
        return new BadRequestObjectResult("Could not find any items with matching ids");
    }
}