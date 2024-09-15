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
		/// SignalR�ڑ����J�n���邽�߂̃G���h�|�C���g
		/// HubName : �ǂ�Hub�ɐڑ����邩�w��
		/// ConnectionStringSetting : �ڑ���̎w��
		/// UserId : �N���C�A���g���ʎq���w�b�_�[�Ŏ󂯎�鎞�̎��ʎq���w��i�����ݒ肵�Ȃ��ƌʃ��b�Z�[�W�̑��M���o���Ȃ��j
		/// ����̓��N�G�X�g�w�b�_�[�̂����A�ux-ms-signalr-userid�v�̒l���N���C�A���g���ʎq�Ƃ���
		/// </summary>
		[FunctionName("negotiate")]
		public static SignalRConnectionInfo Negotiate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")] HttpRequest req,
			[SignalRConnectionInfo(HubName = "MyHub", ConnectionStringSetting = "AzureSignalRConnectionString", UserId = "{headers.x-ms-signalr-userid}")] SignalRConnectionInfo connectionInfo)
		{
			return connectionInfo;
		}

		/// <summary>
		/// Http���N�G�X�g���瑗�M���邽�߂̃p�����[�^�[���󂯎��A
		/// SignalR�o�R�ŃN���C�A���g�ɒʒm���邽�߂̃G���h�|�C���g
		/// �X�R�A�̃f�[�^���t�����g�ɑ��M����
		/// </summary>
		/// <param name="req">HttpTrigger�̓��̓o�C���h</param>
		/// <param name="message">SignalR�̏o�̓o�C���h</param>
		/// <returns></returns>
		[FunctionName(nameof(HttpToSignalR))]
		public static async Task HttpToSignalR(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
			[SignalR(HubName = "MyHub", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> message)
		{
			// �󂯎�����f�[�^���f�V���A���C�Y���ASignalR�ŃN���C�A���g�֑��M����
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var data = JsonConvert.DeserializeObject<RequestBody>(requestBody);

			// �t�����g�ɃX�R�A�𑗐M���郁�b�Z�[�W���쐬
			await message.AddAsync(new SignalRMessage
			{
				Target = nameof(MessageType.ScoreUpdate), // ���M��̃��\�b�h��
				Arguments = new[] { data },
				UserId = data.UserId
			});
		}
	}

	/// <summary>
	/// HttpPost��SignalR�o�R�Œʒm����ۂ̃f�[�^�t�H�[�}�b�g
	/// </summary>
	public class RequestBody
	{
		[JsonProperty("score")]
		public int Score { get; set; } 

		[JsonProperty("userId")]
		public string UserId { get; set; }
	}

	/// <summary>
	/// ���b�Z�[�W�^�C�v
	/// �t�����g���󂯎��ہA���̃^�C�v�ɂ���Ď󂯎��֐��𐧌�\�i����̓X�R�A�̍X�V�p�j
	/// </summary>
	public enum MessageType
	{
		ScoreUpdate // �X�R�A�X�V�p�̃��b�Z�[�W�^�C�v��ǉ�
	}
}
