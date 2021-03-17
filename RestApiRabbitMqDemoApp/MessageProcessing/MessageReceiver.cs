using RestApiRabbitMqDemoApp.Domain;

namespace RestApiRabbitMqDemoApp.MessageProcessing
{
	public class MessageReceiver : IMessageReceiver
	{
		public Message HandledMessage { get; set; }
	}
}
