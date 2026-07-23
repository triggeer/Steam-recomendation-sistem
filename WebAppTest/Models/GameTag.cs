namespace WebAppTest.Models
{
	public class GameTag
	{
		public int GameId { get; set; }
		public Game Game { get; set; }
		public int TagId { get; set; }
		public Tag Tag { get; set; }
		public int Weight { get; set; }
		public double Strength { get; private set; }

		public void UpdateStrength(double strength)
		{
			Strength = strength;
		}
	}
}
