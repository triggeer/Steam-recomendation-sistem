using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class UserGamesResponse
	{
		public int gameСount {  get; set; }
		public List<OwnedGameDto> games { get; set; }
	}
}
