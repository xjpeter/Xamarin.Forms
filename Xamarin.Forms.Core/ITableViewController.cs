
using System;

namespace Xamarin.Forms
{
	public interface ITableViewController
	{
		ITableModel GetValueModel();
		event EventHandler ModelChanged;
	}
}
