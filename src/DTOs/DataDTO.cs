namespace DTOs
{
	public abstract class BaseDataDTO
	{
		public string Id { get; set; }
		public string DtoType => GetType().Name;
	}
}