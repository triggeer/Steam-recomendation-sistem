namespace WebAppTest.Interfaces
{
	public interface IImportService
	{
		Task ImportNewGames(List<int> ids);
		Task ImportNewGames();
	}
}
