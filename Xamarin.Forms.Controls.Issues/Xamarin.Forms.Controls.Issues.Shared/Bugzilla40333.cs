using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40333, "[Android] IllegalStateException: Recursive entry to executePendingTransactions", PlatformAffected.Android)]
	public class Bugzilla40333 : TestNavigationPage
	{
		protected override void Init()
		{
			var navButton = new Button { Text = "Test With NavigationPage" };
			navButton.Clicked += (sender, args) => { PushAsync(new _40333MDP(false)); };

			var tabButton = new Button { Text = "Test With TabbedPage" };
			tabButton.Clicked += (sender, args) => { PushAsync(new _40333MDP(true)); };

			var content = new ContentPage {
				Content = new StackLayout {
					Children = { navButton, tabButton }
				}
			};

			PushAsync(content);
		}

		[Preserve(AllMembers = true)]
		public class _40333MDP : TestMasterDetailPage
		{
			readonly bool _showTabVersion;

			public _40333MDP(bool showTabVersion)
			{
				_showTabVersion = showTabVersion;
			}

			protected override void Init()
			{
				Master = new NavigationPage(new _40333NavPusher("Root")) { Title = "MasterNav" };

				if (_showTabVersion)
				{
					Detail = new TabbedPage() { Title = "DetailNav", Children = { new _40333DetailPage("T1") } };
				}
				else
				{
					Detail = new NavigationPage(new _40333DetailPage("Detail") { Title = "DetailPage" }) { Title = "DetailNav" };
				}

				MasterBehavior = MasterBehavior.Split;
			}

			[Preserve(AllMembers = true)]
			public class _40333DetailPage : ContentPage
			{
				public _40333DetailPage(string title)
				{
					Title = title;

					var openMaster = new Button();
					openMaster.Text = "Open Master";
					openMaster.Clicked += (sender, args) => ((MasterDetailPage)this.Parent.Parent).IsPresented = true;

					Content = new StackLayout() {
						Children = { new Label { Text = "Detail Text" }, openMaster }
					};
				}
			}

			[Preserve(AllMembers = true)]
			public class _40333NavPusher : ContentPage
			{
				readonly ListView _listView = new ListView();

				public _40333NavPusher(string title)
				{
					Title = title;
					_listView.ItemsSource = new[] { "1", "2 Click This", "3 Still Here" };
					_listView.ItemTapped += OnItemTapped;

					Content = new StackLayout {
						Children = { _listView }
					};
				}

				async void OnItemTapped(object sender, EventArgs e)
				{
					var masterNav = ((MasterDetailPage)this.Parent.Parent).Master.Navigation;

					var newTitle = $"{Title}.{_listView.SelectedItem}";
					await masterNav.PushAsync(new _40333NavPusher(newTitle));
				}

				protected override async void OnAppearing()
				{
					base.OnAppearing();

					var newPage = new _40333DetailPage(Title);

					var detailNav = ((MasterDetailPage)this.Parent.Parent).Detail.Navigation;
					var currentRoot = detailNav.NavigationStack[0];
					detailNav.InsertPageBefore(newPage, currentRoot);
					await detailNav.PopToRootAsync();
				}
			}

			[Preserve(AllMembers = true)]
			public class _40333TabPusher : ContentPage
			{
				readonly ListView _listView = new ListView();

				public _40333TabPusher(string title)
				{
					Title = title;
					_listView.ItemsSource = new[] { "1", "2 Click This", "3 Still Here" };
					_listView.ItemTapped += OnItemTapped;

					Content = new StackLayout {
						Children = { _listView }
					};
				}

				async void OnItemTapped(object sender, EventArgs e)
				{
					var masterNav = ((MasterDetailPage)this.Parent.Parent).Master.Navigation;

					var newTitle = $"{Title}.{_listView.SelectedItem}";
					await masterNav.PushAsync(new _40333TabPusher(newTitle));
				}

				protected override void OnAppearing()
				{
					base.OnAppearing();

					var newPage = new _40333DetailPage(Title);

					var detailTab = (TabbedPage)((MasterDetailPage)this.Parent.Parent).Detail;

					detailTab.Children.Add(newPage);
					detailTab.CurrentPage = newPage;
				}
			}
		}

#if UITEST
		[Test]
		public void ClickingOnMenuItemInMasterDoesNotCrash_NavPageVersion()
		{
			RunningApp.Tap(q => q.Marked("Test With NavigationPage"));
			RunningApp.WaitForElement(q => q.Marked("Open Master"));

			RunningApp.Tap(q => q.Marked("Open Master"));
			RunningApp.WaitForElement(q => q.Marked("2 Click This"));

			RunningApp.Tap(q => q.Marked("2 Click This"));
			RunningApp.WaitForElement(q => q.Marked("3 Still Here")); // If the bug isn't fixed, the app will have crashed by now
		}

		[Test]
		public void ClickingOnMenuItemInMasterDoesNotCrash_TabPageVersion()
		{
			RunningApp.Tap(q => q.Marked("Test With TabbedPage"));
			RunningApp.WaitForElement(q => q.Marked("Open Master"));

			RunningApp.Tap(q => q.Marked("Open Master"));
			RunningApp.WaitForElement(q => q.Marked("2 Click This"));

			RunningApp.Tap(q => q.Marked("2 Click This"));
			RunningApp.WaitForElement(q => q.Marked("3 Still Here")); // If the bug isn't fixed, the app will have crashed by now
		}
#endif
	}
}