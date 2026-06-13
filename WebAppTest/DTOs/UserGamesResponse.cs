using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class UserGamesResponse
	{
		public int game_count {  get; set; }
		public List<OwnedGameDto> games { get; set; }
	}
}
