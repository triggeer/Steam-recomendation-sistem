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

		//public GameResponse(string name, List<string> genres, List<SpyTagDto> tags, string detailedDescription, double userScore, long owners, int initialPrice)
		//{
		//	Name = name;
		//	Genres = genres;
		//	Tags = tags;
		//	DetailedDescription = detailedDescription;
		//	UserScore = userScore;
		//	Owners = owners;
		//	if (initialPrice == null)
		//		InitialPrice = 0;
		//	else InitialPrice = initialPrice;

		//}
	}
}
