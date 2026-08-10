namespace WebAppTest.Models
{
	public class RecList
	{
		public int Id { get; set; }
		public DateTime CreatedAt { get; set; }

		public RecList (DateTime createdAt)
		{
			CreatedAt = createdAt.ToUniversalTime();
		}
	}
}
