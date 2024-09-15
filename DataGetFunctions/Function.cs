using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs;

namespace DataGetFunctions
{
	public class Function
	{
		/// <summary>
		/// SignalR接続を開始するためのエンドポイント
		/// HubName : どのHubに接続するか指定
		/// ConnectionStringSetting : 接続先の指定
		/// UserId : クライアント識別子をヘッダーで受け取る時の識別子を指定（これを設定しないと個別メッセージの送信が出来ない）
		/// 今回はリクエストヘッダーのうち、「x-ms-signalr-userid」の値をクライアント識別子とする
		/// </summary>
		[FunctionName("negotiate")]
		public static SignalRConnectionInfo Negotiate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")] HttpRequest req,
			[SignalRConnectionInfo(HubName = "MyHub", ConnectionStringSetting = "AzureSignalRConnectionString", UserId = "{headers.x-ms-signalr-userid}")] SignalRConnectionInfo connectionInfo)
		{
			return connectionInfo;
		}

		/// <summary>
		/// Httpリクエストから送信するためのパラメーターを受け取り、
		/// SignalR経由でクライアントに通知するためのエンドポイント
		/// スコアのデータをフロントに送信する
		/// </summary>
		/// <param name="req">HttpTriggerの入力バインド</param>
		/// <param name="message">SignalRの出力バインド</param>
		/// <returns></returns>
		[FunctionName(nameof(HttpToSignalR))]
		public static async Task HttpToSignalR(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
			[SignalR(HubName = "MyHub", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> message)
		{
			// 受け取ったデータをデシリアライズし、SignalRでクライアントへ送信する
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var data = JsonConvert.DeserializeObject<RequestBody>(requestBody);

			// フロントにスコアを送信するメッセージを作成
			await message.AddAsync(new SignalRMessage
			{
				Target = nameof(MessageType.ScoreUpdate), // 送信先のメソッド名
				Arguments = new[] { data },
				UserId = data.UserId
			});
		}
	}

	/// <summary>
	/// HttpPostとSignalR経由で通知する際のデータフォーマット
	/// </summary>
	public class RequestBody
	{
		[JsonProperty("score")]
		public int Score { get; set; } 

		[JsonProperty("userId")]
		public string UserId { get; set; }
	}

	/// <summary>
	/// メッセージタイプ
	/// フロントが受け取る際、このタイプによって受け取る関数を制御可能（今回はスコアの更新用）
	/// </summary>
	public enum MessageType
	{
		ScoreUpdate // スコア更新用のメッセージタイプを追加
	}
}
