using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;

namespace Xamarin.Forms
{
	public interface ITemplatedItemsList<TItem> : IReadOnlyList<TItem>, INotifyCollectionChanged where TItem : BindableObject
	{
		IEnumerable ItemsSource { get; }
		int GetGlobalIndexForGroup(ITemplatedItemsList<TItem> group);
		int IndexOf(TItem item);
		string Name { get; set; }
		event PropertyChangedEventHandler PropertyChanged;
		IReadOnlyList<string> ShortNames { get; }
		int GetGlobalIndexOfItem(object item);
		ITemplatedItemsList<TItem> GetGroup(int index);
		Tuple<int, int> GetGroupAndIndexOfItem(object group, object item);
		Tuple<int, int> GetGroupAndIndexOfItem(object item);
		int GetGroupIndexFromGlobal(int globalIndex, out int leftOver);
		TItem HeaderContent { get; }
		TItem UpdateContent(TItem content, int index);
		TItem UpdateHeader(TItem content, int groupIndex);
		object GetValueBindingContext();
		event NotifyCollectionChangedEventHandler GroupedCollectionChanged;
	}
}
