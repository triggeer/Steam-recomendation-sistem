using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class PriceOverview
	{
		[JsonPropertyName("initial")]
		public int Initial { get; set; }

		[JsonPropertyName("final")]
		public int Final { get; set; }


	}
}
//"price_overview": {
//	"currency": "RUB",
//        "initial": 429900,
//        "final": 343900,
//        "discount_percent": 20,
//        "initial_formatted": "4299 руб.",
//        "final_formatted": "3439 руб.