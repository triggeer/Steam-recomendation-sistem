namespace WebAppTest.Models
{
	public class ListGame
	{
		public int ListId { get; set; }
		public int GameId { get; set; }
		public int GamePosition { get; set; }

		//public ListGame(int Id, int GameId)
		//{
		//	_listId = Id;
		//	_gameId = GameId;

		//}
		public ListGame(int listId, int gameId, int gamePosition)
		{
			ListId = listId;
			GameId = gameId;
			GamePosition = gamePosition;
		}
	}
}
