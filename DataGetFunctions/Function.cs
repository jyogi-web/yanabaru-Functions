using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataGetFunctions
{
	public class Function
	{
		private readonly ILogger _logger;

		public Function(ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<Function>();
		}

		[Function("ScoreFunction")]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
		{
			_logger.LogInformation("C# HTTP trigger function received a request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			int? score = data?.score;

			HttpResponseData response;

			if (score == null)
			{
				response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
				await response.WriteStringAsync("Please pass a score in the request body");
			}
			else
			{
				_logger.LogInformation($"Received score: {score}");


				response = req.CreateResponse(System.Net.HttpStatusCode.OK);
				await response.WriteStringAsync(JsonConvert.SerializeObject(new { status = "Score received", score = score }));
			}

			return response;
		}
	}
}
