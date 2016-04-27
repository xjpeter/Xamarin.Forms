
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms
{
	public interface ITemplatedItemsView<TItem> : IItemsView<TItem> where TItem : BindableObject
	{
		IListProxy ListProxy { get; }
		ITemplatedItemsList<TItem> TemplatedItems { get; }
		event PropertyChangedEventHandler PropertyChanged;
	}
}
