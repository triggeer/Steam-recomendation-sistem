namespace WebAppTest.Models
{
	public class ListGame
	{
		public int ListId { get; private set; }
		public int GameId { get; private set; }
		public int GamePosition { get; private set; }

		private ListGame(){ }
		public ListGame(int listId, int gameId, int gamePosition)
		{
			ListId = listId;
			GameId = gameId;
			GamePosition = gamePosition;
		}
	}
}
