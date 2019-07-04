using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flurl;
using Flurl.Http;

namespace ETASupport
{
	public class Communication
	{
		public Communication(string pelletsUnitsHostUrl)
		{
			_pelletsUnitsHostUrl = pelletsUnitsHostUrl + ":8080";
		}
		readonly string _pelletsUnitsHostUrl;

		public async Task<PelletsData> GetPelletsDataAsync()
		{
			const string urlPath = "user/var/112/10201/0/0/12015";

			var result = await _pelletsUnitsHostUrl
				.AppendPathSegment(urlPath)
				.GetStringAsync()
				.ConfigureAwait(false);

			var suppliesXml = XElement.Parse(result).ToString();
			var value = Convert.ToDouble(this.GetContentElement(suppliesXml, "value").Value);
			var divider = Convert.ToDouble(this.GetContentElement(suppliesXml, "value").Attribute("scaleFactor").Value);

			var amountKg = (int)Math.Round(value / divider);
			var data = new PelletsData
			{
				SuppliesKg = amountKg
			};
			return data;
		}

		IList<XElement> GetContentElements(string xml, string elementName, bool includeDescendants = false)
		{
			List<XElement> elements = null;
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
