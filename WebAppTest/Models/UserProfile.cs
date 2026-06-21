using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppTest.Models
{
	public class UserProfile
	{
		public string Id { get; set; }
		[Column(TypeName = "jsonb")]
		public Dictionary<string, double> TagStrength { get; set; } = [];
		public double Length { get; set; }
		public int GameAmount { get; set; }
		private UserProfile() { }
		public UserProfile(string id, Dictionary<string, double> tagStrength, double length, int gameAmount)
		{
			Id = id;
			TagStrength = tagStrength;
			Length = length;
			GameAmount = gameAmount;
		}

		public void Update(string id, Dictionary<string, double> tagStrength, double length, int gameAmount)
		{
			Id = id;
			TagStrength = tagStrength;
			Length = length;
			GameAmount = gameAmount;
		}
	}
}
