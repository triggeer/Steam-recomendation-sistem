using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface IRecommendationService
	{
		Task<UserVectorResponse> FormUserTagVector(string userId);
		Task<List<RecommendationDto>> FormRecommendationListAsync(string userId);
		Task<RecommendationsOnTeg> FormRecomendationsOnTagAsync(string userId, int tagId);
		Task AddGameVector();
		double CosSimilarity(
			Dictionary<int, double> firstGameTags,
			Dictionary<int, double> secondGameTags,
			double fristVectorLen, double secondVectorLen
		);
		Task<List<RecommendationDto>> ForUniqueRecomendationsAsync(string userId);
	}
}
