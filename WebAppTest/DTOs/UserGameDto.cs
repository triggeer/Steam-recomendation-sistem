namespace WebAppTest.DTOs
{
	public class UserGameDto
	{
		public int AppId { get; set; }
		public int PlayTime { get; set; }

		public string Name { get; set; }

		public List<string> Genres { get; set; } = [];

		public List<SpyTagDto> Tags { get; set; } = [];
	}
}
