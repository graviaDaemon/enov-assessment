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

public class Delete
{
    private readonly IDatabaseService _service;

    public Delete(IDatabaseService service)
    {
        _service = service;
    }
    
    [FunctionName("Delete")]
    [OpenApiOperation("Run", tags: new[] { "Run" }, Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "ids", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "The query parameter for the ID.")]
    [OpenApiResponseWithBody(contentType: "application/json", bodyType: typeof(ResponseBody),
        statusCode: HttpStatusCode.OK, Description = "The result", Example = typeof(ResponseBodyExample))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req, ILogger log)
    {
        StringValues query = req.Query["ids"];
        log.LogInformation(string.Join("_", query));
        string[] ids = query[0].Split(",");

        // TODO: Return only the IDS that were deleted, this check should happen in the _service.DeleteAsync() method
        bool result = await _service.DeleteAsync(ids);
        
        if (result)
            return new OkObjectResult($"Success!\nThe following ids were deleted: {string.Join(", ", ids)}");

        return new BadRequestObjectResult("Some Ids were not deleted, check the database for further information.");
    }
}