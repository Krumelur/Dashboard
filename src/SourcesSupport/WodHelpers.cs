using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SourcesSupport
{
	public class WodHelpers
	{
		static string StripHtml(string source)
		{
			string result;

			// Remove HTML Development formatting
			// Replace line breaks with space
			// because browsers inserts space
			//result = source.Replace("\r", " ");
			// Replace line breaks with space
			// because browsers inserts space
			//result = result.Replace("\n", " ");
			// Remove step-formatting
			result = source.Replace("\t", string.Empty);

			// Remove repeating spaces because browsers ignore them
			result = Regex.Replace(result, @"( )+", " ");

			// Remove the header (prepare first by clearing attributes)
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*head([^>])*>", "<head>",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"(<( )*(/)( )*head( )*>)", "</head>",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(<head>).*(</head>)", string.Empty,
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// remove all scripts (prepare first by clearing attributes)
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*script([^>])*>", "<script>",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"(<( )*(/)( )*script( )*>)", "</script>",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			//result = System.Text.RegularExpressions.Regex.Replace(result,
			//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
			//         string.Empty,
			//         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"(<script>).*(</script>)", string.Empty,
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// remove all styles (prepare first by clearing attributes)
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*style([^>])*>", "<style>",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"(<( )*(/)( )*style( )*>)", "</style>",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(<style>).*(</style>)", string.Empty,
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// insert tabs in spaces of <td> tags
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*td([^>])*>", "\t",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// insert line breaks in places of <BR> and <LI> tags
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*br( )*>", "\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*li( )*>", "\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// insert line paragraphs (double line breaks) in place
			// if <P>, <DIV> and <TR> tags
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*div([^>])*>", "\r\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*tr([^>])*>", "\r\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<( )*p([^>])*>", "\r\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// Remove remaining tags like <a>, links, images,
			// comments etc - anything that's enclosed inside < >
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"<[^>]*>", string.Empty,
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// replace special characters:
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @" ", " ",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&bull;", " * ",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&lsaquo;", "<",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&rsaquo;", ">",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&trade;", "(tm)",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&frasl;", "/",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&lt;", "<",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&gt;", ">",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&copy;", "(c)",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&reg;", "(r)",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			// Remove all others. More can be added, see
			// http://hotwired.lycos.com/webmonkey/reference/special_characters/
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 @"&(.{2,6});", string.Empty,
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// for testing
			//System.Text.RegularExpressions.Regex.Replace(result,
			//       this.txtRegex.Text,string.Empty,
			//       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// make line breaking consistent
			result = result.Replace("\n", "\r");

			// Remove extra line breaks and tabs:
			// replace over 2 breaks with 2 and over 4 tabs with 4.
			// Prepare first to remove any whitespaces in between
			// the escaped characters and remove redundant tabs in between line breaks
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(\r)( )+(\r)", "\r\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(\t)( )+(\t)", "\t\t",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(\t)( )+(\r)", "\t\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(\r)( )+(\t)", "\r\t",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			// Remove redundant tabs
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(\r)(\t)+(\r)", "\r\r",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			// Remove multiple tabs following a line break with just one tab
			result = System.Text.RegularExpressions.Regex.Replace(result,
					 "(\r)(\t)+", "\r\t",
					 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			// Initial replacement target string for line breaks
			string breaks = "\r\r\r";
			// Initial replacement target string for tabs
			string tabs = "\t\t\t\t\t";
			for (int index = 0; index < result.Length; index++)
			{
				result = result.Replace(breaks, "\r\r");
				result = result.Replace(tabs, "\t\t\t\t");
				breaks = breaks + "\r";
				tabs = tabs + "\t";
			}

			// That's it.
			return result.Trim();
		}

		/// <summary>
		/// Accesses crossfit-rosenheim.com and extracts the WOD from the HTML content.
		/// </summary>
		/// <param name="log"></param>
		/// <returns>a list of workouts in plaint text format</returns>
		public static async Task<List<string>> GetRawWodAsync(ILogger log)
		{
			var textWods = new List<string>();

			string pageContent;
			using (var client = new HttpClient())
			{
				// Grab page content.
				try
				{
					pageContent = await client.GetStringAsync("https://beyondthewhiteboard.com/gyms/4348-crossfit-rosenheim");
				}
				catch (Exception ex)
				{
					log.LogError($"Failed to get page content: {ex}");
					return textWods;
				}
			}

			// Extract WOD from page content.
			var match = Regex.Match(
				pageContent,
				"<div class=\"event-workout pull-left\">(.+?)<\\/div>",
				RegexOptions.IgnoreCase | RegexOptions.Singleline);

			if (!match.Success)
			{
				if (log != null)
				{
					log.LogError($"Could not find WOD information in page content: {pageContent}");
				}
				return null;
			}

			if (match.Groups.Count <= 0)
			{
				return textWods;
			}

			for (int groupIndex = 0; groupIndex < match.Groups.Count; groupIndex++)
			{
				// Get rid of HTML tags.
				var textWod = StripHtml(match.Groups[groupIndex].Value);
				textWods.Add(textWod);
			}

			return textWods;
		}
	}
}
