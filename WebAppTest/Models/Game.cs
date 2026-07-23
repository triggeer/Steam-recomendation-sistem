using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppTest.Models
{
	public class Game
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; private set; }
		public int SteamAppId { get; private set; }
		public string Name { get; private set; }
		public ICollection<GameGenre> GameGenres{ get; private set; }
		public ICollection<GameTag> GameTags { get; private set; }
		public string DetailedDescription { get; private set; }
		public double UserScore { get; private set; }
		public int ReviewAmount { get; private set; }
		public long Owners {  get; private set; }
		public int? InitialPrice { get; private set; } = 0;
		public double VectorLength { get; private set; }
		private Game(){ }
		public Game(int steamAppId, string name, string detailed_description, double userScore, int reviewAmount, long owners, int? initPrice, double vectorLength)
		{
			SteamAppId = steamAppId;
			Name = name;
			GameGenres =  new List<GameGenre>();
			GameTags = new List<GameTag>();
			DetailedDescription = detailed_description;
			UserScore = userScore;
			ReviewAmount = reviewAmount;
			Owners = owners;
			InitialPrice = initPrice ?? 0;
			VectorLength = vectorLength;
		}
		public void UpdateInitialPrice(int? newInitPrice)
		{
			InitialPrice = newInitPrice ?? 0;
		}

		public void UpdateVectorLength(double? newVectorLength)
		{
			VectorLength = newVectorLength ?? 0;
		}
		
	}
}
