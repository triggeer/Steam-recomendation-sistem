using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class Spy100Dto
	{
		[JsonPropertyName("appid")]
		public int AppId { get; set; }
	}
}
