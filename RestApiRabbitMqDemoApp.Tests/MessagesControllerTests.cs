using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RestApiRabbitMqDemoApp.Controllers;
using RestApiRabbitMqDemoApp.Domain;
using RestApiRabbitMqDemoApp.MessageProcessing;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RestApiRabbitMqDemoApp.Tests
{
	public class MessagesControllerTests
	{
		private readonly TestServer _server;
		private readonly HttpClient _client;

		public MessagesControllerTests()
		{
			// Arrange
			string projectDir = "C:\\Users\\artem\\source\\repos\\RestApiRabbitMqDemo\\RestApiRabbitMqDemoApp";

			_server = new TestServer(new WebHostBuilder().UseContentRoot(projectDir)
				.UseEnvironment("Development")
				.UseConfiguration(new ConfigurationBuilder()
				.SetBasePath(projectDir)
					.AddJsonFile("appsettings.json")
					.Build())
					.UseStartup<Startup>()
				.UseStartup<Startup>());

			_client = _server.CreateClient();
		}

		[Fact]
		public async Task MessageControllerOkResult()
		{
			// Act
			Message message = new("test"); //Создаем тестовое сообщение, сериализуем и превращаем в массив байтов

			string myContent = JsonSerializer.Serialize(message);

			byte[] buffer = Encoding.UTF8.GetBytes(myContent);

			ByteArrayContent byteContent = new ByteArrayContent(buffer);

			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			HttpResponseMessage actionResult = await _client.PostAsync("/Messages", byteContent); // Отправляем контроллеру пост-запрос с полученным массивом

			// Assert
			HttpResponseMessage viewResult = Assert.IsType<HttpResponseMessage>(actionResult); // Проверка того, что ответ является HttpResponseMessage

			string content = Encoding.UTF8.GetString(await viewResult.Content.ReadAsByteArrayAsync());

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
			};

			Response response = JsonSerializer.Deserialize<Response>(content, options);

			Assert.NotNull(response.Message); // Проверка того, что ответ содержит обработанное сообщение
		}

		[Fact]
		public async Task MessageControllerTimeOutResult()
		{
			// Act
			Message message = new("test................."); //Создаем тестовое сообщение, которое вызовет ответ тайм-аут 

			string myContent = JsonSerializer.Serialize(message);

			byte[] buffer = Encoding.UTF8.GetBytes(myContent);

			ByteArrayContent byteContent = new ByteArrayContent(buffer);

			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			HttpResponseMessage actionResult = await _client.PostAsync("/Messages", byteContent); // Отправляем контроллеру пост-запрос с полученным массивом

			// Assert
			HttpResponseMessage viewResult = Assert.IsType<HttpResponseMessage>(actionResult); // Проверка того, что ответ является HttpResponseMessage

			string content = Encoding.UTF8.GetString(await viewResult.Content.ReadAsByteArrayAsync());

			Assert.Contains("408", content);
		}
	}
}
