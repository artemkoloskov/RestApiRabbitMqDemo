namespace RestApiRabbitMqDemoApp.Domain
{
	/// <summary>
	/// Объект ответа на REST запрос. Содержит обработанное сообщение и время, которое
	/// ушло на его обработку
	/// </summary>
	public class Response
	{
		public Message Message { get; set; }
		public double TimeToHandle { get; set; }
	}
}
