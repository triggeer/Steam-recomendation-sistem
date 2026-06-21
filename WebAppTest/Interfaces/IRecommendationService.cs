using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface IRecommendationService
	{
		//Task<float> CosSimilarity(
		//Dictionary<string, Dictionary<string, double>> userVector,
		//string userId,
		//Dictionary<string, Dictionary<string, double>> gameVector,
		//int gameId);
		Task<UserVectorResponse> FormUserTagVector(string userId);
		Task<double> CosSimilarity(
			Dictionary<string, double> userTags,
			Dictionary<string, double> gameTags);
		Task<List<RecommendationDto>> FormRecommendationListAsync(string userId);
	}
}
