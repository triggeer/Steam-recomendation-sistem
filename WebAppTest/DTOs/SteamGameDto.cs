using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SteamGameDto
	{
		[JsonPropertyName("steam_appid")]
		public int SteamAppId { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }
		
		[JsonPropertyName("genres")]
		public List<SteamGenreDto> Genres { get; set; }
		
		[JsonPropertyName("detailed_description")]
		public string DetailedDescription { get; set; }
		
		[JsonPropertyName("header_image")]
		public string ImgUrl { get; set; }

		[JsonPropertyName("price_overview")]
		public PriceOverview Price {  get; set; }
	}
}
//"price_overview": {
//	"currency": "RUB",
//        "initial": 429900,
//        "final": 343900,
//        "discount_percent": 20,
//        "initial_formatted": "4299 руб.",
//        "final_formatted": "3439 руб.