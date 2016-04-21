using Android.App;
using Android.Content;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class MasterDetailContainer : Xamarin.Forms.Platform.Android.MasterDetailContainer, IManageFragments
	{
		PageContainer _pageContainer;
		FragmentManager _fragmentManager;
		readonly bool _isMaster;
		readonly MasterDetailPage _parent;

		public MasterDetailContainer(MasterDetailPage parent, bool isMaster, Context context) : base(parent, isMaster, context)
		{
			Id = FormsAppCompatActivity.GetUniqueId();
			_parent = parent;
			_isMaster = isMaster;
		}

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = ((FormsAppCompatActivity)Context).SupportFragmentManager);

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);

			// If we're using a PageContainer (i.e., we've wrapped our contents in a Fragment),
			// Make sure that it gets laid out
			if (_pageContainer != null)
			{
				if (_isMaster)
				{
					var width = (int)Context.ToPixels(_parent.MasterBounds.Width);
					// Adding Top accounts for the top padding the base class is already giving us in GetBounds
					var height = (int)Context.ToPixels(_parent.MasterBounds.Height + _parent.MasterBounds.Top); 
					_pageContainer.Layout(0, 0, width, height);
				}
				else
				{
					_pageContainer.Layout(l, t, r, b);
				}
			}
		}

		protected override void AddChildView(VisualElement childView)
		{
			_pageContainer = null;

			Page page = childView as NavigationPage ?? (Page)(childView as TabbedPage);

			if (page == null)
			{
				// Not a NavigationPage or TabbedPage? Just do the normal thing
				base.AddChildView(childView);
			}
			else
			{
				// The renderers for NavigationPage and TabbedPage both host fragments, so they need to be wrapped in a 
				// FragmentContainer in order to get isolated fragment management

				Fragment fragment = FragmentContainer.CreateInstance(page);
				
				var fc = fragment as FragmentContainer;
				fc?.SetOnCreateCallback(pc =>
				{
					_pageContainer = pc;
					SetDefaultBackgroundColor(pc.Child);
					pc.Child.UpdateLayout();
				});

				FragmentTransaction transaction = FragmentManager.BeginTransaction();
				transaction.DisallowAddToBackStack();
				transaction.Add(Id, fragment);
				transaction.SetTransition((int)FragmentTransit.FragmentOpen);
				transaction.Commit();
			}
		}

		public void SetFragmentManager(FragmentManager fragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = fragmentManager;
		}
	}
}