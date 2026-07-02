using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface IRecommendationService
	{
		Task<UserVectorResponse> FormUserTagVector(string userId);
		Task<double> CosSimilarity(
			Dictionary<string, double> userTags,
			Dictionary<string, double> gameTags);
		Task<List<RecommendationDto>> FormRecommendationListAsync(string userId);
		Task<RecommendationsOnTeg> FormRecomendationsOnTagAsync(string userId, int tagId);
	}
}
