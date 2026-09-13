using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IUserListService
	{
		Task AddUserList(List<RecommendationDto> recommendedList, string userId);
		Task<List<PrevListDto>> CollectFormedList(string userId);
		HashSet<int> FormIdBanList(string userId);
	}
}
