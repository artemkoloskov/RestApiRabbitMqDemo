﻿using System;
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
		private readonly Sender _sender = new("localhost", "restQueue");
		private readonly IConfiguration _configuration;
		private readonly string _hostMame;
		private readonly string _queueName;

		public MessagesController(ILogger<MessagesController> logger, IMessageReceiver messageReceiver, IConfiguration configuration)
		{
			_configuration = configuration;

			_logger = logger;

			_hostMame = _configuration["HostName"];

			_queueName = _configuration["QueueName"];

			_sender = new(_hostMame, _queueName);

			_messageReceiver = messageReceiver;
		}

		[HttpPost]
		public IActionResult SendMessage([FromBody] Message message)
		{
			DateTime handlingStartTime = DateTime.Now;

			_sender.SendMessage(message);

			TimeSpan timeToHandle = DateTime.Now - handlingStartTime;

			while (
				(_messageReceiver.HandledMessages is null ||
				!_messageReceiver.HandledMessages.Where(m => m.Id == message.Id).Any()) &&
				timeToHandle.TotalMilliseconds < double.Parse(_configuration["MessageHandlingTimeout"]))
			{
				timeToHandle = DateTime.Now - handlingStartTime;
			}

			if (_messageReceiver.HandledMessages is null || !_messageReceiver.HandledMessages.Where(m => m.Id == message.Id).Any())
			{
				return StatusCode(408);
			}

			timeToHandle = DateTime.Now - handlingStartTime;

			Response response = new() { Message = _messageReceiver.HandledMessages.Where(m => m.Id == message.Id).FirstOrDefault(), TimeToHandle = timeToHandle.TotalSeconds };

			return Ok(response);
		}
	}
}
