using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestApiRabbitMqDemoApp.Domain;
using RestApiRabbitMqDemoApp.MessageProcessing;

namespace RestApiRabbitMqDemoApp.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class MessagesController : ControllerBase
	{
		private readonly ILogger<MessagesController> _logger;
		private readonly IMessageReceiver _messageReceiver;
		private readonly Sender _sender;
		private readonly IConfiguration _configuration;
		private readonly string _hostName;
		private readonly string _queueName;
		private readonly string _userName;
		private readonly string _password;

		public MessagesController(ILogger<MessagesController> logger, IMessageReceiver messageReceiver, IConfiguration configuration)
		{
			_configuration = configuration;

			_logger = logger;

			if (Environment.GetEnvironmentVariable("RabbitMq/Host") is null ||
				Environment.GetEnvironmentVariable("RabbitMq/Host") == "")
			{
				_hostName = _configuration["HostName"];
			}
			else
			{
				_hostName = Environment.GetEnvironmentVariable("RabbitMq/Host");
			}

			if (Environment.GetEnvironmentVariable("RabbitMq/UserName") is null ||
				Environment.GetEnvironmentVariable("RabbitMq/UserName") == "")
			{
				_userName = _configuration["RabbitUserName"];
			}
			else
			{
				_userName = Environment.GetEnvironmentVariable("RabbitMq/UserName");
			}

			if (Environment.GetEnvironmentVariable("RabbitMq/Password") is null ||
				Environment.GetEnvironmentVariable("RabbitMq/Password") == "")
			{
				_password = _configuration["Password"];
			}
			else
			{
				_password = Environment.GetEnvironmentVariable("RabbitMq/Password");
			}

			_queueName = _configuration["QueueName"];

			_sender = new(_hostName, _queueName, _userName, _password);

			_messageReceiver = messageReceiver;
		}

		[HttpPost]
		public IActionResult SendMessage([FromBody] Message message)
		{
			DateTime handlingStartTime = DateTime.Now;

			_sender.SendMessage(message); //Сообщение отправляется в очередь.

			TimeSpan timeToHandle = DateTime.Now - handlingStartTime;

			// Ожидаем возврата сообщения из очереди и его обарботки,
			// либо отсечки по таймауту
			while (
				(_messageReceiver.HandledMessages is null ||
				!_messageReceiver.HandledMessages.Where(m => m is not null && m.Id == message.Id).Any()) &&
				timeToHandle.TotalMilliseconds < double.Parse(_configuration["MessageHandlingTimeout"]))
			{
				timeToHandle = DateTime.Now - handlingStartTime;
			}

			// Если за время до тайм-аута ответ из очереди не был получен
			// возвращаем статус RequestTimeOut
			if (_messageReceiver.HandledMessages is null || !_messageReceiver.HandledMessages.Where(m => m.Id == message.Id).Any())
			{
				return StatusCode(408);
			}

			timeToHandle = DateTime.Now - handlingStartTime;

			// Если ответ получен, записываем его в объект ответа, с временем затраченным на обработку
			Response response = new()
			{
				Message = _messageReceiver.HandledMessages
					.Where(m => m.Id == message.Id).FirstOrDefault(),
				TimeToHandle = timeToHandle.TotalSeconds
			};

			((List<Message>)_messageReceiver.HandledMessages).Remove(response.Message);

			//Возвращаем статус ок, с ответом на запрос, содержащим обработанное сообщение и время, затраченное
			//на обработку
			return Ok(response);
		}
	}
}
