﻿using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest.iOS;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 31330, "Disabled context actions appear enabled")]
	public class Bugzilla31330 : TestContentPage
	{
		protected override void Init ()
		{
			var vm = new ListViewModel ();
			BindingContext = vm;
			vm.Init ();
			var listview = new ListView ();
			listview.SetBinding (ListView.ItemsSourceProperty, new Binding ("Items"));
			listview.ItemTemplate = new DataTemplate (typeof(CustomTextCell));
			listview.ItemSelected += (object sender, SelectedItemChangedEventArgs e) => {
				(e.SelectedItem as ListItemViewModel).CanExecute = true;
				((e.SelectedItem as ListItemViewModel).DeleteItemCommand as Command).ChangeCanExecute ();
			};
			// Initialize ui here instead of ctor
			Content = listview;
		}

		[Preserve (AllMembers = true)]
		public class CustomTextCell : TextCell
		{
			public CustomTextCell ()
			{
				SetBinding (TextProperty, new Binding ("Title"));
				var deleteMenuItem = new MenuItem ();
				deleteMenuItem.Text = "Delete";
				deleteMenuItem.IsDestructive = true;
				deleteMenuItem.SetBinding (MenuItem.CommandProperty, new Binding ("DeleteItemCommand"));
				ContextActions.Add (deleteMenuItem);
			}
		}

		[Preserve (AllMembers = true)]
		public class ListViewModel : ViewModel
		{
			public void Init ()
			{
				Items.Add (new ListItemViewModel (this) { Title = string.Format ("Something {0}", Items.Count.ToString ()) });
				Items.Add (new ListItemViewModel (this) { Title = string.Format ("Something {0}", Items.Count.ToString ()) });
				Items.Add (new ListItemViewModel (this) { Title = string.Format ("Something {0}", Items.Count.ToString ()) });
			}

			public ObservableCollection<ListItemViewModel> Items { get; } = new ObservableCollection<ListItemViewModel>();

			ICommand _disabledCommand;

			public ICommand DisabledCommand {
				get {
					if (_disabledCommand == null) {
						_disabledCommand = new Command (() => {
						}, () => false);
					}

					return _disabledCommand;
				}
			}

			ICommand _addItemCommand;

			public ICommand AddItemCommand {
				get {
					if (_addItemCommand == null) {
						_addItemCommand = new Command (() => Items.Add (new ListItemViewModel (this) { Title = string.Format ("Something {0}", Items.Count.ToString ()) }));
					}

					return _addItemCommand;
				}
			}
		}

		[Preserve (AllMembers = true)]
		public class ListItemViewModel : ViewModel
		{
			public bool CanExecute = false;
			readonly ListViewModel _listViewModel;

			public ListItemViewModel (ListViewModel listViewModel)
			{
				if (listViewModel == null) {
					throw new ArgumentNullException ("listViewModel");
				}
				_listViewModel = listViewModel;
			}

			public string Title { get; set; }

			ICommand _deleteItemCommand;

			public ICommand DeleteItemCommand {
				get {
					if (_deleteItemCommand == null) {
						_deleteItemCommand = new Command (() => _listViewModel.Items.Remove (this), () => CanExecute);
					}

					return _deleteItemCommand;
				}
			}

			ICommand _otherCommand;

			public ICommand OtherCommand {
				get {
					if (_otherCommand == null) {
						_otherCommand = new Command (() => {
						}, () => false);
					}

					return _otherCommand;
				}
			}
		}

		#if UITEST
		[Test]
		public void Bugzilla31330Test ()
		{
			var screenBounds = RunningApp.Query (q => q.Raw ("* index:0"))[0].Rect;

			var cell = RunningApp.Query (c => c.Marked ("Something 1")) [0];
			var cell2 = RunningApp.Query (c => c.Marked ("Something 2")) [0];
#if __IOS__
			RunningApp.DragCoordinates (screenBounds.Width - 10, cell.Rect.CenterY, 0, cell.Rect.CenterY);
			RunningApp.WaitForElement (c => c.Marked ("Delete"));
			RunningApp.Tap (c => c.Marked ("Delete"));
			RunningApp.WaitForElement (c => c.Marked ("Something 1"));
			RunningApp.Tap (c => c.Marked ("Something 2"));
			RunningApp.DragCoordinates (screenBounds.Width - 10, cell2.Rect.CenterY, 0, cell2.Rect.CenterY);
			RunningApp.Tap (c => c.Marked ("Delete"));
			RunningApp.WaitForNoElement (c => c.Marked ("Something 2"));
#else
			RunningApp.TouchAndHoldCoordinates (cell.Rect.CenterX, cell.Rect.CenterY);
			RunningApp.WaitForElement (c => c.Marked ("Delete"));
			RunningApp.Tap (c => c.Marked ("Delete"));
			RunningApp.Back ();
			RunningApp.WaitForElement (c => c.Marked ("Something 1"));
			RunningApp.Tap (c => c.Marked ("Something 2"));
			RunningApp.TouchAndHoldCoordinates (cell2.Rect.CenterX, cell2.Rect.CenterY);
			RunningApp.Tap (c => c.Marked ("Delete"));
			RunningApp.WaitForNoElement (c => c.Marked ("Something 2"));
#endif
		}
#endif
	}
}
