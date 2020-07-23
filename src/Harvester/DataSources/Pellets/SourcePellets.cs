using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dashboard.Models;
using Flurl;
using Flurl.Http;
using Harvester;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Dashboard.Harvester.DataSources.Pellets
{
    public class SourcePellets : SourceBase
    {
		public SourcePellets(HarvesterSettings harvesterSettings, ILogger logger) : base(harvesterSettings, logger)
		{
		}
	
		public async override Task<SourceData> GetData()
		{
			const string suppliesUrlPath = "user/var/112/10201/0/0/12015";
			const string errorsUrlPath = "user/errors";

			var result = await _harvesterSettings.PelletsUnitUri
				.AppendPathSegment(suppliesUrlPath)
				.GetStringAsync();

			var xml = XElement.Parse(result).ToString();
			var value = Convert.ToDouble(this.GetContentElement(xml, "value").Value);
			var divider = Convert.ToDouble(this.GetContentElement(xml, "value").Attribute("scaleFactor").Value);

			var amountKg = (int)Math.Round(value / divider);

			var dataItems = new List<DataItem>
			{
				new DataItem
				{
					Id = "supplies",
					Type = DataItemType.Integer,
					Label = "Lager (kg)",
					Value = amountKg
				}
			};

			result = await _harvesterSettings.PelletsUnitUri
				.AppendPathSegment(errorsUrlPath)
				.GetStringAsync();

			xml = XElement.Parse(result).ToString();

			foreach (var errorEl in this.GetContentElements(xml, "error", true))
			{
				var msg = errorEl.Attribute("msg").Value;
				var desc = errorEl.Value.ToString();
				var errorTypeRaw = errorEl.Attribute("priority").Value;
				var errorType = errorTypeRaw.ToLower().Contains("warn") ? "warning" : "error";

				dataItems.Add(
					new DataItem
					{
						Id = "error",
						Type = DataItemType.Text,
						Label = errorType,
						Value = msg + ": " + desc
					}
				);
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