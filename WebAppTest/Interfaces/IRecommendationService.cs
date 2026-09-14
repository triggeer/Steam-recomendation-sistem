using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface IRecommendationService
	{
		Task<List<RecommendationDto>> FormRecommendationListAsync(string userId);
		Task<RecommendationsOnTeg> FormRecomendationsOnTagAsync(string userId, int tagId);
		double CosSimilarity(
			Dictionary<int, double> firstGameTags,
			Dictionary<int, double> secondGameTags,
			double fristVectorLen, double secondVectorLen
		);
		Task<List<RecommendationDto>> ForUniqueRecomendationsAsync(string userId);
	}
}
