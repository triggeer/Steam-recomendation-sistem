using WebAppTest.DTOs;

namespace WebAppTest.Services.User
{
	public class UserGameData
	{
		public int AppId { get; set; }
		public int PlayTime { get; set; }
		public string Name { get; set; }
		public List<string> Genres { get; set; } = [];
		public List<SpyTag> Tags { get; set; } = [];
	}
}
