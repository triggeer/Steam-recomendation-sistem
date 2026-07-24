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
		Task<List<RecommendationDto>> ForUniqueRecomendationsAsync(string userId);
		Task AddGameVector();
		Task<double> CosSimilarity1(
			Dictionary<int, double> firstGameTags,
			Dictionary<int, double> secondGameTags,
			double fristVectorLen, double secondVectorLen
		);
		Task<List<RecommendationDto>> ForUniqueRecomendationsAsync1(string userId);
	}
}
