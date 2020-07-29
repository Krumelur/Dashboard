using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dashboard.Models;
using Harvester;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Dashboard.Support.Pellets;

namespace Dashboard.Harvester.DataSources.Pellets
{
    public class SourcePellets : SourceBase
    {
		public SourcePellets(HarvesterSettings harvesterSettings, ILogger logger) : base(harvesterSettings, logger)
		{
		}
	
		public async override Task<SourceData> GetData()
		{
			var pelletsHelper = new PelletsHelper(_harvesterSettings.PelletsUnitUri);
			var pelletsData = await pelletsHelper.GetPelletsData(30);

			var dataItems = new List<DataItem>
			{
				new DataItem
				{
					Id = "supplies",
					Type = DataItemType.Integer,
					Label = "Lager (kg)",
					Value = pelletsData.SuppliesKg
				}
			};

			if(pelletsData.Errors != null)
			{
				foreach (var error in pelletsData.Errors)
				{
					dataItems.Add(
						new DataItem
						{
							Id = "error",
							Type = DataItemType.Text,
							Label = error.ErrorTypeRaw,
							Value = error.Message + ": " + error.Description
						}
					);
				}
			}
			
			var sourceData = new SourceData
			{
				Id = Guid.NewGuid().ToString(),
				SourceId = "pellets",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = dataItems.ToArray()
			};

			return sourceData;
		}

		IList<XElement> GetContentElements(string xml, string elementName, bool includeDescendants = false)
		{
			List<XElement> elements;
			var xmlDoc = XDocument.Parse(xml);
			var ns = xmlDoc.Root.GetDefaultNamespace();
			elements = includeDescendants
				? xmlDoc.Root.Descendants(ns + elementName).ToList()
				: xmlDoc.Root.Elements(ns + elementName).ToList();
			
			return elements;
		}

		XElement GetContentElement(string xml, string elementName, bool includeDescendants = false)
		{
			var elements = this.GetContentElements(xml, elementName, includeDescendants);
			return elements?.SingleOrDefault();
		}
	}
}