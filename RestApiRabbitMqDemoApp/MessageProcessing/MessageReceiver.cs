using RestApiRabbitMqDemoApp.Domain;
using System.Collections.Generic;

namespace RestApiRabbitMqDemoApp.MessageProcessing
{
	public class MessageReceiver : IMessageReceiver
	{
		public IEnumerable<Message> HandledMessages { get; set; }
	}
}
