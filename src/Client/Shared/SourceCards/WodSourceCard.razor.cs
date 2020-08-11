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
			TimeStampSource = "(aktualisieren)";
			_wodDate = DateTime.UtcNow;
			await UpdateInitialHistory();
			RefreshUI(InitialHistory);
		}

		DateTime _wodDate = DateTime.UtcNow;

		public async Task HandleTodayClick(MouseEventArgs args)
		{
			await UpdateInitialHistory();
			RefreshUI(InitialHistory);
		}

		public async Task HandlePreviousClick(MouseEventArgs args)
		{
			_wodDate = _wodDate.Date.Add(new TimeSpan(1, 0, 0));
			_wodDate = _wodDate.AddDays(-1);

			var history = await GetSourceHistoryFiltered(new Filter {
				StartDateUtc = _wodDate,
				EndDateUtc = new DateTime(_wodDate.Year, _wodDate.Month, _wodDate.Day, 23, 59, 0),
				SourceId = "wod",
				TakeNumberResults = 1
			});

			RefreshUI(history);
		}


		public async Task HandleNextClick(MouseEventArgs args)
		{
			_wodDate = _wodDate.Date.Add(new TimeSpan(1, 0, 0));
			_wodDate = _wodDate.AddDays(1);

			var history = await GetSourceHistoryFiltered(new Filter {
				StartDateUtc = _wodDate,
				EndDateUtc = new DateTime(_wodDate.Year, _wodDate.Month, _wodDate.Day, 23, 59, 0),
				SourceId = "wod",
				TakeNumberResults = 1
			});

			RefreshUI(history);
		}

		void RefreshUI(SourceHistory history)
		{
			if(history?.HistoryData != null
			&& history.HistoryData.Length > 0
			&& history.HistoryData[0].DataItems != null
			&& history.HistoryData[0].DataItems.Length > 0)
			{
				_wodDate = history.HistoryData[0].TimeStampUtc.UtcDateTime;
				TimeStampSource = _wodDate.ToLocalTime().ToString("g");
				WodHtml = history.HistoryData[0].DataItems[0].Value.ToString();
				if(string.IsNullOrWhiteSpace(WodHtml))
				{
					WodHtml = "Leider kein WOD gefunden :-(";	
				}
			}
			else
			{
				TimeStampSource = _wodDate.ToLocalTime().ToString("g");
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