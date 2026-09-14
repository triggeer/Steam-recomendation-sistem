namespace WebAppTest.Models
{
	public class RecList
	{
		public int Id { get; private set; }
		public DateTime CreatedAt { get; private set; }

		private RecList() { }
		public RecList (DateTime createdAt)
		{
			CreatedAt = createdAt.ToUniversalTime();
		}
	}
}
