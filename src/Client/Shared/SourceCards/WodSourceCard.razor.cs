using System;
using System.Threading.Tasks;
using Dashboard.Models;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Shared.SourceCards
{
	public partial class WodSourceCard : BaseSourceCard
	{
		public override string SourceId => "wod";

		public override int NumInitialDataPoints => 1;

		public string WodHtml { get; set; }

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			await UpdateInitialHistory();
			RefreshUI(InitialHistory);
		}

		void RefreshUI(SourceHistory history)
		{
			if(history?.HistoryData != null && history.HistoryData[0]?.DataItems != null)
			{
				WodHtml = history.HistoryData[0].DataItems[0].Value.ToString();
			}
			else
			{
				WodHtml = "Leider kein WOD gefunden :-(";
			}
		}
		
		protected override Task InitialHistoryLoaded()
		{
			RefreshUI(InitialHistory);
			return Task.CompletedTask;			
		}
	}
}