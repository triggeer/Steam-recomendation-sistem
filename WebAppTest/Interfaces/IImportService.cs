namespace WebAppTest.Interfaces
{
	public interface IImportService
	{
		Task<List<int>> GetNewGames();
		Task Import100Games(List<int> ids);
	}
}
