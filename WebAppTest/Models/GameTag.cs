namespace WebAppTest.Models
{
	public class GameTag
	{
		public int GameId { get; private set; }
		public Game Game { get; private set; }
		public int TagId { get; private set; }
		public Tag Tag { get; private set; }
		public int Weight { get; private set; }
		public double Strength { get; private set; }

		private GameTag(){ }

		public GameTag(Game game, Tag tag, int weight, double strength)
		{
			Game = game;
			Tag = tag;
			Weight = weight;
			Strength = strength;
		}

		public void UpdateStrength(double strength)
		{
			Strength = strength;
		}
	}
}
