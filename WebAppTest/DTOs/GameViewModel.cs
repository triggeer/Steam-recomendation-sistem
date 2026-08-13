using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class GameViewModel
	{
		public Game Game { get; set; }
		public int Index { get; set; }
		public int ListSize { get; set; }
		public int? ListId { get; set; } 
		//public string PreviousUrl { get; set; }
		//public string NextUrl { get; set; }

	}
}
