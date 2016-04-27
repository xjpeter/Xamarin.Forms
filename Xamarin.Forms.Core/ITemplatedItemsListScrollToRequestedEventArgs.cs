using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Forms
{
	public interface ITemplatedItemsListScrollToRequestedEventArgs
	{
		object GetValueGroup();
		object GetValueItem();
	}
}
