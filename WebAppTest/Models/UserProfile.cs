using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppTest.DTOs;

namespace WebAppTest.Models
{
	public class UserProfile
	{
		public string Id { get; set; }
		[Column(TypeName = "jsonb")]
		public Dictionary<int, double> TagStrength { get; set; } = [];
		public double Length { get; set; }
		public int GameAmount { get; set; }
		public DateTime UpdatedAt { get; set; }
		private UserProfile() { }
		public UserProfile(string id, Dictionary<int, double> tagStrength, double length, int gameAmount, DateTime updatedAt)
		{
			Id = id;
			TagStrength = tagStrength;
			Length = length;
			GameAmount = gameAmount;
			UpdatedAt = updatedAt.ToUniversalTime();
		}

		public void Update(string id, Dictionary<int, double> tagStrength, double length, int gameAmount, DateTime updatedAt)
		{
			Id = id;
			TagStrength = tagStrength;
			Length = length;
			GameAmount = gameAmount;
			UpdatedAt = updatedAt.ToUniversalTime();
		}
	}
}
//[HttpGet]
//public async Task<IActionResult> ShowPreviousLists(string userLink)
//{
//	string userId = await _userService.TransformLinkToId(userLink);

//	Dictionary<int, List<int>> prevLists = await _recommendationService.CollectFormedList(userId);

//	HttpContext.Session.SetString("Recommendations", JsonSerializer.Serialize(prevLists));

//	return View(prevLists);

//}

//[HttpGet]
//public async Task<IActionResult> ShowChosenList(int listId, int index = 0)
//{
//	string json = HttpContext.Session.GetString("Recommendations");

//	Dictionary<int, List<int>> lists = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(json);

//	var chosenList = lists[listId];

//	var chosenId = chosenList[index];

//	var game = await _context.Games
//		.Include(gt => gt.GameTags)
//				.ThenInclude(t => t.Tag)
//		.FirstOrDefaultAsync(x => x.Id == chosenId);

//	var model = new GameViewModel
//	{
//		Game = game,
//		Index = index,
//		ListSize = chosenList.Count,
//		ListId = listId
//	};

//	return View("Game", model);
//}