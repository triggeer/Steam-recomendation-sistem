using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class Syp100Dto
	{
		[JsonPropertyName("appid")]
		public int AppId { get; set; }

		//[JsonPropertyName("name")]
		//public string Name { get; set; }
	}
}
