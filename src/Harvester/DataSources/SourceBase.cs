using System.Threading.Tasks;
using Dashboard.Models;
using Harvester;
using Microsoft.Extensions.Logging;

namespace Dashboard.Harvester.DataSources
{
	public abstract class SourceBase
	{
		public SourceBase(HarvesterSettings harvesterSettings, ILogger logger)
		{
			_harvesterSettings = harvesterSettings;
			_logger = logger;
		}
		protected readonly HarvesterSettings _harvesterSettings;
		protected readonly ILogger _logger;
		
		public abstract Task<SourceData> GetData();
	}
}