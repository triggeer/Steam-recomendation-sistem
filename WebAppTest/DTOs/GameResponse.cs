using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class GameResponse
	{
		public string Name { get; set; }
		public List<SpyTag> Tags { get; set; }
		public string DetailedDescription { get; set; }
		public double Rating { get; set; }
		public long Owners {  get; set; }
		public int InitialPrice { get; set; }


	}
}
