using RestApiRabbitMqDemoApp.Domain;
using System.Collections.Generic;

namespace RestApiRabbitMqDemoApp.MessageProcessing
{
	/// <summary>
	/// Интерфейс для синглтона, в который помещаются обработанные хэндлером
	/// сообщения и в котором их ищет контроллер после отправки сообщения в очередт на обработку
	/// </summary>
	public interface IMessageReceiver
	{
		IEnumerable<Message> HandledMessages { get; set; }
	}
}
