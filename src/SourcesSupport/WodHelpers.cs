using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SourcesSupport
{
	public class WodHelpers
	{
		/// <summary>
		/// Accesses crossfit-rosenheim.com and extracts the WOD from the HTML content.
		/// </summary>
		/// <param name="log"></param>
		/// <returns>the raw WOD content</returns>
		public static async Task<string> GetRawWodAsync(ILogger log)
		{
			string pageContent;
			using (var client = new HttpClient())
			{
				// Grab page content.
				pageContent = await client.GetStringAsync("http://www.crossfit-rosenheim.com/wod");
			}

			// Extract WOD from page content.
			var match = Regex.Match(
				pageContent,
				"<p class=\"btwb-workout-description\">(.+)</p>",
				RegexOptions.IgnoreCase | RegexOptions.Multiline);

			if (!match.Success)
			{
				if (log != null)
				{
					log.LogError($"Could not find WOD information in page content: {pageContent}");
				}
				return null;
			}

			var htmlWod = match.Groups[1].Value;
			// Get rid of HTML tags.
			var rawWod = Regex.Replace(htmlWod, @"<.*?>", " ").Replace("&amp;", " ").Replace("&nbsp;", " ");
			// Get rid of multiple blanks.
			rawWod = Regex.Replace(rawWod, "[ ]{2,}", " ", RegexOptions.None).Trim();

			if (string.IsNullOrWhiteSpace(rawWod))
			{
				if (log != null)
				{
					log.LogError($"Failed to extract raw WOD from '{htmlWod}'");

				}
				return null;
			}

			if (log != null)
			{
				log.LogInformation($"Found WOD: {rawWod}");
			}

			return rawWod;
		}
	}
}
