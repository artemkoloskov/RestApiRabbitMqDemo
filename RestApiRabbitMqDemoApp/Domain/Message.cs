using System;

namespace RestApiRabbitMqDemoApp.Domain
{
	/// <summary>
	/// Объект сообщения, для обмена с очередью.
	/// </summary>
	public class Message
	{
		public Guid Id { get; set; }
		public string Body { get; set; }

		public Message()
		{
			Id = Guid.NewGuid();
		}

		public Message(string body)
		{
			Id = Guid.NewGuid();

			Body = body;
		}

		public Message(Guid id)
		{
			Id = id;
		}
	}
}
