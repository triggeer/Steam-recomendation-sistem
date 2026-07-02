using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class GameResponse
	{
		public string Name { get; set; }
		public List<string> Genres { get; set; }
		public List<SpyTagDto> Tags { get; set; }
		public string DetailedDescription { get; set; }
		public double UserScore { get; set; }
		public long Owners {  get; set; }
		public int InitialPrice { get; set; }
	}
}
