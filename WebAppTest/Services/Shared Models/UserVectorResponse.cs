namespace WebAppTest.DTOs
{
	public class UserVectorResponse
	{
		public Dictionary<int, double> TagStrength { get; set; } = [];
		public double Length { get; set; }
		public int GameAmount { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
