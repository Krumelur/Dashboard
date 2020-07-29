using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Dashboard.Support.Pellets
{
	public class PelletsError
	{
		public string Message { get; set; }
		public string Description { get; set; }
		public string ErrorTypeRaw { get; set; }
		public string ErrorTypeParsed { get; set; }
	}

	public class PelletsData
	{
		public int SuppliesKg { get; set; }
		public IList<PelletsError> Errors { get; set; }
	}

    public class PelletsHelper
    {
		public PelletsHelper(string pelletsUnitUri)
		{
			_pelletsUnitUri = pelletsUnitUri;
		}

		string _pelletsUnitUri;
	
		public async Task<PelletsData> GetPelletsData(int timeOut)
		{
			const string suppliesUrlPath = "user/var/112/10201/0/0/12015";
			const string errorsUrlPath = "user/errors";

			var result = await _pelletsUnitUri
				.AppendPathSegment(suppliesUrlPath)
				.WithTimeout(TimeSpan.FromSeconds(timeOut))
				.GetStringAsync();

			var xml = XElement.Parse(result).ToString();
			var value = Convert.ToDouble(this.GetContentElement(xml, "value").Value);
			var divider = Convert.ToDouble(this.GetContentElement(xml, "value").Attribute("scaleFactor").Value);
			
			var pelletsData = new PelletsData
			{
				SuppliesKg = (int)Math.Round(value / divider),
				Errors = new List<PelletsError>()
			};
			
			result = await _pelletsUnitUri
				.AppendPathSegment(errorsUrlPath)
				.GetStringAsync();

			xml = XElement.Parse(result).ToString();

			foreach (var errorEl in this.GetContentElements(xml, "error", true))
			{
				var msg = errorEl.Attribute("msg").Value;
				var desc = errorEl.Value.ToString();
				var errorTypeRaw = errorEl.Attribute("priority").Value;
				var errorType = errorTypeRaw.ToLower().Contains("warn") ? "warning" : "error";

				pelletsData.Errors.Add(new PelletsError {
					Description = desc,
					ErrorTypeRaw = errorTypeRaw,
					ErrorTypeParsed = errorType,
					Message = msg
				});
			}
			
			return pelletsData;
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