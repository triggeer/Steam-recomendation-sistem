namespace WebAppTest.Models
{
	public class UserList
	{
		public string UserId { get; private set; }
		public int ListId { get; private set; }
		private UserList(){ }
		public UserList(string userId, int listId)
		{
			UserId = userId;
			ListId = listId;
		}
	}
}
