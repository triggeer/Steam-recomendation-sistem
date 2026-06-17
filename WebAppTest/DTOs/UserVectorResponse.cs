namespace WebAppTest.DTOs
{
	public class UserVectorResponse
	{
		public Dictionary<string, double> TagStrength { get; set; }
		public double Length { get; set; }
		public int GameAmount { get; set; }
	}
}
