namespace WebAppTest.Models
{
	public class Tag
	{
		public int Id { get; private set; }
		public string Name { get; private set; }

		public ICollection<GameTag> GameTags { get; private set; }
		private Tag() { }
		public Tag(string name)
		{
			Name = name;
			GameTags = new List<GameTag>();
		}

	}
}
