using System.Collections.Generic;
using System.IO;
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
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Enovation.Assessment.API;

public class Create
{
    private readonly IDatabaseService _service;

    public Create(IDatabaseService service)
    {
        _service = service;
    }
    
    [FunctionName("Create")]
    [OpenApiOperation("Run", tags: new[] { "Run" }, Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "application/json; charset=utf-8", bodyType: typeof(RequestBody),
        Description = "The example request JSON body", Example = typeof(IEnumerable<RequestBodyExample>))]
    [OpenApiResponseWithBody(contentType: "application/json", bodyType: typeof(ResponseBody),
        statusCode: HttpStatusCode.OK, Description = "The result", Example = typeof(ResponseBodyExample))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    {
        string request = await new StreamReader(req.Body).ReadToEndAsync();
        RequestBody data = JsonConvert.DeserializeObject<RequestBody>(request);
        
        log.LogInformation(JsonConvert.SerializeObject(data.Items[0]));

        IEnumerable<Item> results = await _service.CreateAsync(data.Items);

        if (!results.Any())
            return new BadRequestResult();
        
        return new OkObjectResult(results.ToArray());
    }
}