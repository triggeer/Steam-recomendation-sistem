using Microsoft.EntityFrameworkCore.Update.Internal;

namespace WebAppTest.Models
{
	public class UserGame
	{
		public string UserId { get; init; }
		public int GameId { get; init; }
		public int PlayTime { get; private set; }

		private UserGame(){ }
		public UserGame(string userId, int gameId, int playTime)
		{
			UserId = userId;
			GameId = gameId;
			PlayTime = playTime;
		}

		public void UpdatePlayTime(int playTime)
		{
			if (playTime > PlayTime)
				PlayTime = playTime;
		}
	}
}
