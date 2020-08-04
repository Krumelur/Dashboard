using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Shared.SourceCards
{
	public partial class WodSourceCard : BaseSourceCard
	{
		public string WodHtml { get; set; }

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			var history = await GetSourceHistory("wod");
			if(history?.HistoryData != null && history.HistoryData[0]?.DataItems != null)
			{
				WodHtml = history.HistoryData[0].DataItems[0].Value.ToString();
			}
			else
			{
				WodHtml = "Leider kein WOD gefunden :-(";
			}
		}
		protected override async Task OnInitializedAsync()
		{
			await HandleRefreshClick(null);
		}
	}
}