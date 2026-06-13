namespace WebAppTest.Interfaces
{
	public interface IImportService
	{
		Task<List<int>> Get100Games();
		Task Import100Games(List<int> ids);
	}
}
