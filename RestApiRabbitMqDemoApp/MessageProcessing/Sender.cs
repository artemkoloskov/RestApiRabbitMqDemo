using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RestApiRabbitMqDemoApp.Domain;

namespace RestApiRabbitMqDemoApp.MessageProcessing
{
	/// <summary>
	/// Объект отправляющий сообщения в очередь RabbitMQ.
	/// </summary>
	public class Sender
	{
		private readonly string _hostName;
		private readonly string _queueName;
		private readonly string _userName;
		private readonly string _password;

		private IConnection _connection;

		public Sender(string hostName, string queueName, string userName, string password)
		{
			_queueName = queueName;
			_hostName = hostName;
			_userName = userName;
			_password = password;

			CreateConnection();
		}

		public void SendMessage(Message message)
		{
			if (ConnectionExists())
			{
				using IModel channel = _connection.CreateModel();

				channel.QueueDeclare(
					queue: _queueName,
					durable: false,
					exclusive: false,
					autoDelete: false,
					arguments: null);

				string json = JsonSerializer.Serialize(message);
				byte[] body = Encoding.UTF8.GetBytes(json);

				channel.BasicPublish(
					exchange: "",
					routingKey: _queueName,
					basicProperties: null,
					body: body);
			}
		}

		private void CreateConnection()
		{
			try
			{
				ConnectionFactory factory = new()
				{
					HostName = _hostName,
					UserName = _userName,
					Password = _password,
				};

				_connection = factory.CreateConnection();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Не удалось создать подключение: {ex.Message}");
			}
		}

		private bool ConnectionExists()
		{
			if (_connection != null)
			{
				return true;
			}

			CreateConnection();

			return _connection != null;
		}
	}
}
