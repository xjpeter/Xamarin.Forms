using System;

namespace Xamarin.Forms
{
	public interface IListViewController : IViewController
	{
		Element FooterElement { get; }
		Element HeaderElement { get; }
		bool RefreshAllowed { get; }
		event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		Cell CreateDefaultCell(object item);
		string GetDisplayTextFromGroup(object cell);
		ListViewCachingStrategy GetValueCachingStrategy();
		bool GetValueTakePerformanceHit();
		void InvokeNotifyRowTapped(int index, int inGroupIndex, Cell cell);
		void InvokeNotifyRowTapped(int index, Cell cell);
		void SendCellAppearing(Cell cell);
		void SendCellDisappearing(Cell cell);
		void SendRefreshing();
	}
}