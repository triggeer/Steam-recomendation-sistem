namespace WebAppTest.Models
{
	public class GameGenre
	{
		public int GameId { get; private set; }

		public Game Game { get; private set; } 

		public int GenreId { get; private set; }

		public Genre Genre { get; private set; }

		private GameGenre() { }

		public GameGenre(Game game, Genre genre)
		{
			Game = game;
			Genre = genre;
		}
	}
}
