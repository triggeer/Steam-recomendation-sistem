using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Models
{
	public class UserProfile
	{
		public string UserId { get; init; }
		[Column(TypeName = "jsonb")]
		public Dictionary<int, double> TagStrength { get; private set; } = [];
		public double Length { get; private set; }
		public int GameAmount { get; private set; }
		public DateTime UpdatedAt { get; private set; }
		private UserProfile() { }
		public UserProfile(string id, Dictionary<int, double> tagStrength, double length, int gameAmount, DateTime updatedAt)
		{
			UserId = id;
			TagStrength = tagStrength;
			Length = length;
			GameAmount = gameAmount;
			UpdatedAt = updatedAt.ToUniversalTime();
		}

		public void Update(Dictionary<int, double> tagStrength, double length, int gameAmount, DateTime updatedAt)
		{
			TagStrength = tagStrength;
			Length = length;
			GameAmount = gameAmount;
			UpdatedAt = updatedAt.ToUniversalTime();
		}
	}
}
//gameData = await GetUserGamesFromSteamAsync(userId);
//List<UserGame> userGames = new List<UserGame>();
//foreach (var game in gameData)
//{
//	UserGame userGame = new UserGame(userId, game.AppId, game.PlayTime);
//	userGames.Add(userGame);
//}
//await _context.UserGames.AddRangeAsync(userGames);
//await _context.SaveChangesAsync();

//await UpdateUserProfile(userId);