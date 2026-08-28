namespace WebAppTest.Models
{
	public class UserList
	{
		public string UserId { get; set; }
		public int ListId { get; set; }

		public UserList(string userId, int listId)
		{
			UserId = userId;
			ListId = listId;
		}
	}
}
