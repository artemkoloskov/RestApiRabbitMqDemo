using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestApiRabbitMqDemoApp.Domain;

namespace RestApiRabbitMqDemoApp.MessageProcessing
{
	/// <summary>
	/// Бэкгрунд сервис слушающий очередь RabbitMQ, и обрабатывающий сообщения
	/// из нее, при поступлении
	/// </summary>
	public class MessageHandler : BackgroundService
	{
		private IModel _channel;
		private IConnection _connection;
		private readonly IConfiguration _configuration;
		private readonly IMessageReceiver _messageReceiver;
		private readonly string _hostMame;
		private readonly string _queueName;

		public MessageHandler(IMessageReceiver messageReceiver, IConfiguration configuration)
		{
			_messageReceiver = messageReceiver;

			_configuration = configuration;

			_hostMame = _configuration["HostName"];

			_queueName = _configuration["QueueName"];

			InitializeListener();
		}

		private void InitializeListener()
		{
			ConnectionFactory factory = new()
			{
				HostName = _hostMame,
			};

			_connection = factory.CreateConnection();

			_channel = _connection.CreateModel();

			_channel.QueueDeclare(
				queue: _queueName,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.ThrowIfCancellationRequested();

			EventingBasicConsumer consumer = new(_channel);

			consumer.Received += (ch, ea) =>
			{
				string content = Encoding.UTF8.GetString(ea.Body.ToArray());

				Message messageFromQueue = JsonSerializer.Deserialize<Message>(content);

				_messageReceiver.HandledMessages ??= new List<Message>();

				if (!_messageReceiver.HandledMessages.Where(m => m.Id == messageFromQueue.Id).Any())
				{
					HandleMessage(messageFromQueue);

					((List<Message>)_messageReceiver.HandledMessages).Add(messageFromQueue);
				}

				_channel.BasicAck(
					deliveryTag: ea.DeliveryTag,
					multiple: false);
			};

			_channel.BasicConsume(
				queue: _queueName,
				autoAck: false,
				consumer: consumer);

			return Task.CompletedTask;
		}

		/// <summary>
		/// "Обработка сообщения". Подсчитывает количество точек в сообщении, и 
		/// задеоживается на такое количество секунд, для симуляции обработки. 
		/// Приписывает к сообщению, что оно обработано и количество точек.
		/// </summary>
		/// <param name="message"></param>
		private static void HandleMessage(Message message)
		{
			if (message.Body is null)
			{
				return;
			}

			int dots = message.Body.Split('.').Length - 1;

			Thread.Sleep(dots * 1000);

			message.Body = $"Сообщение обработано: {message.Body.Replace(".", "")}, {dots} точек";
		}

		public override void Dispose()
		{
			_channel.Close();

			_connection.Close();

			base.Dispose();
		}
	}
}
