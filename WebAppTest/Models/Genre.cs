namespace WebAppTest.Models
{
	public class Genre
	{
		public int Id { get; private set; }
		public string Name { get; private set; }
		public ICollection<GameGenre> GameGenres { get; private set; }
		private Genre() { }
		public Genre(string name)
		{
			Name = name;
			GameGenres = new List<GameGenre>();
		}

	}
}
