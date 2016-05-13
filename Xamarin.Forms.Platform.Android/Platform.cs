using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android.AppCompat;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace Xamarin.Forms.Platform.Android
{
	public class Platform : BindableObject, IPlatform, INavigation, IDisposable, IPlatformLayout
	{
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";

		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;
			});

		internal static readonly BindableProperty PageContextProperty = BindableProperty.CreateAttached("PageContext", typeof(Context), typeof(Platform), null);

		IMasterDetailPageController MasterDetailPageController => CurrentMasterDetailPage as IMasterDetailPageController;

		readonly Context _context;

		readonly PlatformRenderer _renderer;
		readonly ToolbarTracker _toolbarTracker = new ToolbarTracker();

		NavigationPage _currentNavigationPage;

		TabbedPage _currentTabbedPage;

		Color _defaultActionBarTitleTextColor;

		bool _disposed;

		bool _ignoreAndroidSelection;

		Page _navigationPageCurrentPage;
		NavigationModel _navModel = new NavigationModel();

		internal Platform(Context context)
		{
			_context = context;

			_defaultActionBarTitleTextColor = SetDefaultActionBarTitleTextColor();

			_renderer = new PlatformRenderer(context, this);

			FormsApplicationActivity.BackPressed += HandleBackPressed;

			_toolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
		}

		#region IPlatform implementation

		internal Page Page { get; private set; }

		#endregion

		ActionBar ActionBar
		{
			get { return ((Activity)_context).ActionBar; }
		}

		MasterDetailPage CurrentMasterDetailPage { get; set; }

		NavigationPage CurrentNavigationPage
		{
			get { return _currentNavigationPage; }
			set
			{
				if (_currentNavigationPage == value)
					return;

				if (_currentNavigationPage != null)
				{
					_currentNavigationPage.Pushed -= CurrentNavigationPageOnPushed;
					_currentNavigationPage.Popped -= CurrentNavigationPageOnPopped;
					_currentNavigationPage.PoppedToRoot -= CurrentNavigationPageOnPoppedToRoot;
					_currentNavigationPage.PropertyChanged -= CurrentNavigationPageOnPropertyChanged;
				}

				RegisterNavPageCurrent(null);

				_currentNavigationPage = value;

				if (_currentNavigationPage != null)
				{
					_currentNavigationPage.Pushed += CurrentNavigationPageOnPushed;
					_currentNavigationPage.Popped += CurrentNavigationPageOnPopped;
					_currentNavigationPage.PoppedToRoot += CurrentNavigationPageOnPoppedToRoot;
					_currentNavigationPage.PropertyChanged += CurrentNavigationPageOnPropertyChanged;
					RegisterNavPageCurrent(_currentNavigationPage.CurrentPage);
				}

				UpdateActionBarBackgroundColor();
				UpdateActionBarTextColor();
				UpdateActionBarUpImageColor();
				UpdateActionBarTitle();
			}
		}

		TabbedPage CurrentTabbedPage
		{
			get { return _currentTabbedPage; }
			set
			{
				if (_currentTabbedPage == value)
					return;

				if (_currentTabbedPage != null)
				{
					_currentTabbedPage.PagesChanged -= CurrentTabbedPageChildrenChanged;
					_currentTabbedPage.PropertyChanged -= CurrentTabbedPageOnPropertyChanged;

					if (value == null)
						ActionBar.RemoveAllTabs();
				}

				_currentTabbedPage = value;

				if (_currentTabbedPage != null)
				{
					_currentTabbedPage.PagesChanged += CurrentTabbedPageChildrenChanged;
					_currentTabbedPage.PropertyChanged += CurrentTabbedPageOnPropertyChanged;
				}

				UpdateActionBarTitle();

				ActionBar.NavigationMode = value == null ? ActionBarNavigationMode.Standard : ActionBarNavigationMode.Tabs;
				CurrentTabbedPageChildrenChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

#pragma warning disable 618 // Eventually we will need to determine how to handle the v7 ActionBarDrawerToggle for AppCompat
		ActionBarDrawerToggle MasterDetailPageToggle { get; set; }
#pragma warning restore 618

		void IDisposable.Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			SetPage(null);

			FormsApplicationActivity.BackPressed -= HandleBackPressed;
			_toolbarTracker.CollectionChanged -= ToolbarTrackerOnCollectionChanged;
			_toolbarTracker.Target = null;

			CurrentNavigationPage = null;
			CurrentMasterDetailPage = null;
			CurrentTabbedPage = null;
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on Android, please use a NavigationPage.");
		}

		IReadOnlyList<Page> INavigation.ModalStack => _navModel.Modals.ToList();

		IReadOnlyList<Page> INavigation.NavigationStack => new List<Page>();

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();

			modal.SendDisappearing();
			var source = new TaskCompletionSource<Page>();

			IVisualElementRenderer modalRenderer = GetRenderer(modal);
			if (modalRenderer != null)
			{
				if (animated)
				{
					modalRenderer.ViewGroup.Animate().Alpha(0).ScaleX(0.8f).ScaleY(0.8f).SetDuration(250).SetListener(new GenericAnimatorListener
					{
						OnEnd = a =>
						{
							modalRenderer.ViewGroup.RemoveFromParent();
							modalRenderer.Dispose();
							source.TrySetResult(modal);
							_navModel.CurrentPage?.SendAppearing();
						}
					});
				}
				else
				{
					modalRenderer.ViewGroup.RemoveFromParent();
					modalRenderer.Dispose();
					source.TrySetResult(modal);
					_navModel.CurrentPage?.SendAppearing();
				}
			}

			_toolbarTracker.Target = _navModel.Roots.Last();
			UpdateActionBar();

			return source.Task;
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		async Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			_navModel.CurrentPage?.SendDisappearing();

			_navModel.PushModal(modal);

			modal.Platform = this;

			await PresentModal(modal, animated);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				modal.SendAppearing();

			_toolbarTracker.Target = _navModel.Roots.Last();

			UpdateActionBar();
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Android, please use a NavigationPage.");
		}

		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			UpdateGlobalContext(element);

			IVisualElementRenderer renderer = Registrar.Registered.GetHandler<IVisualElementRenderer>(element.GetType()) ?? new DefaultRenderer();
			renderer.SetElement(element);

			return renderer;
		}

		public static IVisualElementRenderer GetRenderer(VisualElement bindable)
		{
			return (IVisualElementRenderer)bindable.GetValue(RendererProperty);
		}

		public static void SetRenderer(VisualElement bindable, IVisualElementRenderer value)
		{
			bindable.SetValue(RendererProperty, value);
		}

		public void UpdateActionBarTextColor()
		{
			SetActionBarTextColor();
		}

		protected override void OnBindingContextChanged()
		{
			SetInheritedBindingContext(Page, BindingContext);

			base.OnBindingContextChanged();
		}

		internal static IVisualElementRenderer CreateRenderer(VisualElement element, FragmentManager fragmentManager)
		{
			UpdateGlobalContext(element);

			IVisualElementRenderer renderer = Registrar.Registered.GetHandler<IVisualElementRenderer>(element.GetType()) ?? new DefaultRenderer();

			var managesFragments = renderer as IManageFragments;
			managesFragments?.SetFragmentManager(fragmentManager);

			renderer.SetElement(element);

			return renderer;
		}

		internal static Context GetPageContext(BindableObject bindable)
		{
			return (Context)bindable.GetValue(PageContextProperty);
		}

		internal ViewGroup GetViewGroup()
		{
			return _renderer;
		}

		internal void PrepareMenu(IMenu menu)
		{
			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
				item.PropertyChanged -= HandleToolbarItemPropertyChanged;
			menu.Clear();

			if (!ShouldShowActionBarTitleArea())
				return;

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
			{
				item.PropertyChanged += HandleToolbarItemPropertyChanged;
				if (item.Order == ToolbarItemOrder.Secondary)
				{
					IMenuItem menuItem = menu.Add(item.Text);
					menuItem.SetEnabled(item.IsEnabled);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(item.Activate));
				}
				else
				{
					IMenuItem menuItem = menu.Add(item.Text);
					if (!string.IsNullOrEmpty(item.Icon))
					{
						Drawable iconBitmap = _context.Resources.GetDrawable(item.Icon);
						if (iconBitmap != null)
							menuItem.SetIcon(iconBitmap);
					}
					menuItem.SetEnabled(item.IsEnabled);
					menuItem.SetShowAsAction(ShowAsAction.Always);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(item.Activate));
				}
			}
		}

		internal async void SendHomeClicked()
		{
			if (UpButtonShouldNavigate())
			{
				if (NavAnimationInProgress)
					return;
				NavAnimationInProgress = true;
				await CurrentNavigationPage.PopAsync();
				NavAnimationInProgress = false;
			}
			else if (CurrentMasterDetailPage != null)
			{
				if (MasterDetailPageController.ShouldShowSplitMode && CurrentMasterDetailPage.IsPresented)
					return;
				CurrentMasterDetailPage.IsPresented = !CurrentMasterDetailPage.IsPresented;
			}
		}

		internal void SetPage(Page newRoot)
		{
			var layout = false;
			if (Page != null)
			{
				_renderer.RemoveAllViews();

				foreach (IVisualElementRenderer rootRenderer in _navModel.Roots.Select(GetRenderer))
					rootRenderer.Dispose();
				_navModel = new NavigationModel();

				layout = true;
			}

			if (newRoot == null)
				return;

			_navModel.Push(newRoot, null);

			Page = newRoot;
			Page.Platform = this;
			AddChild(Page, layout);

			((Application)Page.RealParent).NavigationProxy.Inner = this;

			_toolbarTracker.Target = newRoot;

			UpdateActionBar();
		}

		internal static void SetPageContext(BindableObject bindable, Context context)
		{
			bindable.SetValue(PageContextProperty, context);
		}

		internal void UpdateActionBar()
		{
			List<Page> relevantAncestors = AncestorPagesOfPage(_navModel.CurrentPage);

			IEnumerable<NavigationPage> navPages = relevantAncestors.OfType<NavigationPage>();
			if (navPages.Count() > 1)
				throw new Exception("Android only allows one navigation page on screen at a time");
			NavigationPage navPage = navPages.FirstOrDefault();

			IEnumerable<TabbedPage> tabbedPages = relevantAncestors.OfType<TabbedPage>();
			if (tabbedPages.Count() > 1)
				throw new Exception("Android only allows one tabbed page on screen at a time");
			TabbedPage tabbedPage = tabbedPages.FirstOrDefault();

			CurrentMasterDetailPage = relevantAncestors.OfType<MasterDetailPage>().FirstOrDefault();
			CurrentNavigationPage = navPage;
			CurrentTabbedPage = tabbedPage;

			if (navPage != null && navPage.CurrentPage == null)
			{
				throw new InvalidOperationException("NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");
			}

			UpdateActionBarTitle();

			if (ShouldShowActionBarTitleArea() || tabbedPage != null)
				ShowActionBar();
			else
				HideActionBar();
			UpdateMasterDetailToggle();
		}

		internal void UpdateActionBarBackgroundColor()
		{
			if (!((Activity)_context).ActionBar.IsShowing)
				return;
			Color colorToUse = Color.Default;
			if (CurrentNavigationPage != null)
			{
#pragma warning disable 618 // Make sure Tint still works 
				if (CurrentNavigationPage.Tint != Color.Default)
					colorToUse = CurrentNavigationPage.Tint;
#pragma warning restore 618
				else if (CurrentNavigationPage.BarBackgroundColor != Color.Default)
					colorToUse = CurrentNavigationPage.BarBackgroundColor;
			}
			using (Drawable drawable = colorToUse == Color.Default ? GetActionBarBackgroundDrawable() : new ColorDrawable(colorToUse.ToAndroid()))
				((Activity)_context).ActionBar.SetBackgroundDrawable(drawable);
		}

		internal void UpdateMasterDetailToggle(bool update = false)
		{
			if (CurrentMasterDetailPage == null)
			{
				if (MasterDetailPageToggle == null)
					return;
				// clear out the icon
				ClearMasterDetailToggle();
				return;
			}
			if (!CurrentMasterDetailPage.ShouldShowToolbarButton() || string.IsNullOrEmpty(CurrentMasterDetailPage.Master.Icon) ||
				(MasterDetailPageController.ShouldShowSplitMode && CurrentMasterDetailPage.IsPresented))
			{
				//clear out existing icon;
				ClearMasterDetailToggle();
				return;
			}

			if (MasterDetailPageToggle == null || update)
			{
				ClearMasterDetailToggle();
				GetNewMasterDetailToggle();
			}

			bool state;
			if (CurrentNavigationPage == null)
				state = true;
			else
				state = !UpButtonShouldNavigate();
			if (state == MasterDetailPageToggle.DrawerIndicatorEnabled)
				return;
			MasterDetailPageToggle.DrawerIndicatorEnabled = state;
			MasterDetailPageToggle.SyncState();
		}

		internal void UpdateNavigationTitleBar()
		{
			UpdateActionBarTitle();
			UpdateActionBar();
			UpdateActionBarUpImageColor();
		}

		void AddChild(VisualElement view, bool layout = false)
		{
			if (GetRenderer(view) != null)
				return;

			SetPageContext(view, _context);
			IVisualElementRenderer renderView = CreateRenderer(view);
			SetRenderer(view, renderView);

			if (layout)
				view.Layout(new Rectangle(0, 0, _context.FromPixels(_renderer.Width), _context.FromPixels(_renderer.Height)));

			_renderer.AddView(renderView.ViewGroup);
		}

#pragma warning disable 618 // This may need to be updated to work with TabLayout/AppCompat
		ActionBar.Tab AddTab(Page page, int index)
#pragma warning restore 618
		{
			ActionBar actionBar = ((Activity)_context).ActionBar;
			TabbedPage currentTabs = CurrentTabbedPage;

			var atab = actionBar.NewTab();
			atab.SetText(page.Title);
			atab.TabSelected += (sender, e) =>
			{
				if (!_ignoreAndroidSelection)
					currentTabs.CurrentPage = page;
			};
			actionBar.AddTab(atab, index);

			page.PropertyChanged += PagePropertyChanged;
			return atab;
		}

		List<Page> AncestorPagesOfPage(Page root)
		{
			var result = new List<Page>();
			if (root == null)
				return result;

			if (root is IPageContainer<Page>)
			{
				var navPage = (IPageContainer<Page>)root;
				result.AddRange(AncestorPagesOfPage(navPage.CurrentPage));
			}
			else if (root is MasterDetailPage)
				result.AddRange(AncestorPagesOfPage(((MasterDetailPage)root).Detail));
			else
			{
				foreach (Page page in root.InternalChildren.OfType<Page>())
					result.AddRange(AncestorPagesOfPage(page));
			}

			result.Add(root);
			return result;
		}

		void ClearMasterDetailToggle()
		{
			if (MasterDetailPageToggle == null)
				return;

			MasterDetailPageToggle.DrawerIndicatorEnabled = false;
			MasterDetailPageToggle.SyncState();
			MasterDetailPageToggle.Dispose();
			MasterDetailPageToggle = null;
		}

		void CurrentNavigationPageOnPopped(object sender, NavigationEventArgs eventArg)
		{
			UpdateNavigationTitleBar();
		}

		void CurrentNavigationPageOnPoppedToRoot(object sender, EventArgs eventArgs)
		{
			UpdateNavigationTitleBar();
		}

		void CurrentNavigationPageOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#pragma warning disable 618 // Make sure Tint still works
			if (e.PropertyName == NavigationPage.TintProperty.PropertyName)
#pragma warning restore 618
				UpdateActionBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateActionBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
			{
				UpdateActionBarTextColor();
				UpdateActionBarUpImageColor();
			}
			else if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				RegisterNavPageCurrent(CurrentNavigationPage.CurrentPage);
		}

		void CurrentNavigationPageOnPushed(object sender, NavigationEventArgs eventArg)
		{
			UpdateNavigationTitleBar();
		}

		void CurrentTabbedPageChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (CurrentTabbedPage == null)
				return;

			_ignoreAndroidSelection = true;

			e.Apply((o, index, create) => AddTab((Page)o, index), (o, index) => RemoveTab((Page)o, index), Reset);

			if (CurrentTabbedPage.CurrentPage != null)
			{
				Page page = CurrentTabbedPage.CurrentPage;
				int index = TabbedPage.GetIndex(page);
				if (index >= 0 && index < CurrentTabbedPage.Children.Count)
					ActionBar.GetTabAt(index).Select();
			}

			_ignoreAndroidSelection = false;
		}

		void CurrentTabbedPageOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "CurrentPage")
				return;

			UpdateActionBar();

			// If we switch tabs while pushing a new page, UpdateActionBar() can set currentTabbedPage to null
			if (_currentTabbedPage == null)
				return;

			NavAnimationInProgress = true;

			Page page = _currentTabbedPage.CurrentPage;
			if (page == null)
			{
				ActionBar.SelectTab(null);
				NavAnimationInProgress = false;
				return;
			}

			int index = TabbedPage.GetIndex(page);
			if (ActionBar.SelectedNavigationIndex == index || index >= ActionBar.NavigationItemCount)
			{
				NavAnimationInProgress = false;
				return;
			}

			ActionBar.SelectTab(ActionBar.GetTabAt(index));

			NavAnimationInProgress = false;
		}

		Drawable GetActionBarBackgroundDrawable()
		{
			int[] backgroundDataArray = { global::Android.Resource.Attribute.Background };

			using (var outVal = new TypedValue())
			{
				_context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarStyle, outVal, true);
				TypedArray actionBarStyle = _context.Theme.ObtainStyledAttributes(outVal.ResourceId, backgroundDataArray);

				Drawable result = actionBarStyle.GetDrawable(0);
				actionBarStyle.Recycle();
				return result;
			}
		}

		void GetNewMasterDetailToggle()
		{
			int icon = ResourceManager.GetDrawableByName(CurrentMasterDetailPage.Master.Icon);
			var drawer = GetRenderer(CurrentMasterDetailPage) as MasterDetailRenderer;
			if (drawer == null)
				return;

#pragma warning disable 618 // Eventually we will need to determine how to handle the v7 ActionBarDrawerToggle for AppCompat
			MasterDetailPageToggle = new ActionBarDrawerToggle(_context as Activity, drawer, icon, 0, 0);
#pragma warning restore 618

			MasterDetailPageToggle.SyncState();
		}

		bool HandleBackPressed(object sender, EventArgs e)
		{
			if (NavAnimationInProgress)
				return true;

			Page root = _navModel.Roots.Last();
			bool handled = root.SendBackButtonPressed();

			return handled;
		}

		void HandleToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
				(_context as Activity).InvalidateOptionsMenu();
			else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
				(_context as Activity).InvalidateOptionsMenu();
			else if (e.PropertyName == MenuItem.IconProperty.PropertyName)
				(_context as Activity).InvalidateOptionsMenu();
		}

		void HideActionBar()
		{
			ReloadToolbarItems();
			UpdateActionBarHomeAsUp(ActionBar);
			ActionBar.Hide();
		}

		void NavigationPageCurrentPageOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				UpdateActionBar();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateActionBarTitle();
		}

		void PagePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == Page.TitleProperty.PropertyName)
			{
				ActionBar actionBar = ((Activity)_context).ActionBar;
				TabbedPage currentTabs = CurrentTabbedPage;

				if (currentTabs == null || actionBar.TabCount == 0)
					return;

				var page = sender as Page;
				var atab = actionBar.GetTabAt(currentTabs.Children.IndexOf(page));
				atab.SetText(page.Title);
			}
		}

		Task PresentModal(Page modal, bool animated)
		{
			IVisualElementRenderer modalRenderer = GetRenderer(modal);
			if (modalRenderer == null)
			{
				SetPageContext(modal, _context);
				modalRenderer = CreateRenderer(modal);
				SetRenderer(modal, modalRenderer);

				if (modal.BackgroundColor == Color.Default && modal.BackgroundImage == null)
					modalRenderer.ViewGroup.SetWindowBackground();
			}
			modalRenderer.Element.Layout(new Rectangle(0, 0, _context.FromPixels(_renderer.Width), _context.FromPixels(_renderer.Height)));
			_renderer.AddView(modalRenderer.ViewGroup);

			var source = new TaskCompletionSource<bool>();
			NavAnimationInProgress = true;
			if (animated)
			{
				modalRenderer.ViewGroup.Alpha = 0;
				modalRenderer.ViewGroup.ScaleX = 0.8f;
				modalRenderer.ViewGroup.ScaleY = 0.8f;
				modalRenderer.ViewGroup.Animate().Alpha(1).ScaleX(1).ScaleY(1).SetDuration(250).SetListener(new GenericAnimatorListener
				{
					OnEnd = a =>
					{
						source.TrySetResult(false);
						NavAnimationInProgress = false;
					},
					OnCancel = a =>
					{
						source.TrySetResult(true);
						NavAnimationInProgress = false;
					}
				});
			}
			else
			{
				NavAnimationInProgress = false;
				source.TrySetResult(true);
			}

			return source.Task;
		}

		void RegisterNavPageCurrent(Page page)
		{
			if (_navigationPageCurrentPage != null)
				_navigationPageCurrentPage.PropertyChanged -= NavigationPageCurrentPageOnPropertyChanged;

			_navigationPageCurrentPage = page;

			if (_navigationPageCurrentPage != null)
				_navigationPageCurrentPage.PropertyChanged += NavigationPageCurrentPageOnPropertyChanged;
		}

		void ReloadToolbarItems()
		{
			var activity = (Activity)_context;
			activity.InvalidateOptionsMenu();
		}

		void RemoveTab(Page page, int index)
		{
			ActionBar actionBar = ((Activity)_context).ActionBar;
			page.PropertyChanged -= PagePropertyChanged;
			actionBar.RemoveTabAt(index);
		}

		void Reset()
		{
			ActionBar.RemoveAllTabs();

			if (CurrentTabbedPage == null)
				return;

			var i = 0;
			foreach (Page tab in CurrentTabbedPage.Children.OfType<Page>())
			{
				var realTab = AddTab(tab, i++);
				if (tab == CurrentTabbedPage.CurrentPage)
					realTab.Select();
			}
		}

		void SetActionBarTextColor()
		{
			Color navigationBarTextColor = CurrentNavigationPage == null ? Color.Default : CurrentNavigationPage.BarTextColor;
			TextView actionBarTitleTextView = null;

			int actionBarTitleId = _context.Resources.GetIdentifier("action_bar_title", "id", "android");
			if (actionBarTitleId > 0)
				actionBarTitleTextView = ((Activity)_context).FindViewById<TextView>(actionBarTitleId);

			if (actionBarTitleTextView != null && navigationBarTextColor != Color.Default)
				actionBarTitleTextView.SetTextColor(navigationBarTextColor.ToAndroid());
			else if (actionBarTitleTextView != null && navigationBarTextColor == Color.Default)
				actionBarTitleTextView.SetTextColor(_defaultActionBarTitleTextColor.ToAndroid());
		}

		Color SetDefaultActionBarTitleTextColor()
		{
			var defaultTitleTextColor = new Color();

			TextView actionBarTitleTextView = null;

			int actionBarTitleId = _context.Resources.GetIdentifier("action_bar_title", "id", "android");
			if (actionBarTitleId > 0)
				actionBarTitleTextView = ((Activity)_context).FindViewById<TextView>(actionBarTitleId);

			if (actionBarTitleTextView != null)
			{
				ColorStateList defaultTitleColorList = actionBarTitleTextView.TextColors;
				string defaultColorHex = defaultTitleColorList.DefaultColor.ToString("X");
				defaultTitleTextColor = Color.FromHex(defaultColorHex);
			}

			return defaultTitleTextColor;
		}

		bool ShouldShowActionBarTitleArea()
		{
			if (Forms.TitleBarVisibility == AndroidTitleBarVisibility.Never)
				return false;

			bool hasMasterDetailPage = CurrentMasterDetailPage != null;
			bool navigated = CurrentNavigationPage != null && ((INavigationPageController)CurrentNavigationPage).StackDepth > 1;
			bool navigationPageHasNavigationBar = CurrentNavigationPage != null && NavigationPage.GetHasNavigationBar(CurrentNavigationPage.CurrentPage);
			return navigationPageHasNavigationBar || (hasMasterDetailPage && !navigated);
		}

		bool ShouldUpdateActionBarUpColor()
		{
			bool hasMasterDetailPage = CurrentMasterDetailPage != null;
			bool navigated = CurrentNavigationPage != null && ((INavigationPageController)CurrentNavigationPage).StackDepth > 1;
			return (hasMasterDetailPage && navigated) || !hasMasterDetailPage;
		}

		void ShowActionBar()
		{
			ReloadToolbarItems();
			UpdateActionBarHomeAsUp(ActionBar);
			ActionBar.Show();
			UpdateActionBarBackgroundColor();
			UpdateActionBarTextColor();
		}

		void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			ReloadToolbarItems();
		}

		bool UpButtonShouldNavigate()
		{
			if (CurrentNavigationPage == null)
				return false;

			bool pagePushed = ((INavigationPageController)CurrentNavigationPage).StackDepth > 1;
			bool pushedPageHasBackButton = NavigationPage.GetHasBackButton(CurrentNavigationPage.CurrentPage);

			return pagePushed && pushedPageHasBackButton;
		}

		void UpdateActionBarHomeAsUp(ActionBar actionBar)
		{
			bool showHomeAsUp = ShouldShowActionBarTitleArea() && (CurrentMasterDetailPage != null || UpButtonShouldNavigate());
			actionBar.SetDisplayHomeAsUpEnabled(showHomeAsUp);
		}

		void UpdateActionBarTitle()
		{
			Page view = null;
			if (CurrentNavigationPage != null)
				view = CurrentNavigationPage.CurrentPage;
			else if (CurrentTabbedPage != null)
				view = CurrentTabbedPage.CurrentPage;

			if (view == null)
				return;

			ActionBar actionBar = ((Activity)_context).ActionBar;

			var useLogo = false;
			var showHome = false;
			var showTitle = false;

			if (ShouldShowActionBarTitleArea())
			{
				actionBar.Title = view.Title;
				FileImageSource titleIcon = NavigationPage.GetTitleIcon(view);
				if (!string.IsNullOrWhiteSpace(titleIcon))
				{
					actionBar.SetLogo(_context.Resources.GetDrawable(titleIcon));
					useLogo = true;
					showHome = true;
					showTitle = true;
				}
				else
				{
					showHome = true;
					showTitle = true;
				}
			}

			ActionBarDisplayOptions options = 0;
			if (useLogo)
				options = options | ActionBarDisplayOptions.UseLogo;
			if (showHome)
				options = options | ActionBarDisplayOptions.ShowHome;
			if (showTitle)
				options = options | ActionBarDisplayOptions.ShowTitle;
			actionBar.SetDisplayOptions(options, ActionBarDisplayOptions.UseLogo | ActionBarDisplayOptions.ShowTitle | ActionBarDisplayOptions.ShowHome);

			UpdateActionBarHomeAsUp(actionBar);
		}

		void UpdateActionBarUpImageColor()
		{
			Color navigationBarTextColor = CurrentNavigationPage == null ? Color.Default : CurrentNavigationPage.BarTextColor;
			ImageView actionBarUpImageView = null;

			int actionBarUpId = _context.Resources.GetIdentifier("up", "id", "android");
			if (actionBarUpId > 0)
				actionBarUpImageView = ((Activity)_context).FindViewById<ImageView>(actionBarUpId);

			if (actionBarUpImageView != null && navigationBarTextColor != Color.Default)
			{
				if (ShouldUpdateActionBarUpColor())
					actionBarUpImageView.SetColorFilter(navigationBarTextColor.ToAndroid(), PorterDuff.Mode.SrcIn);
				else
					actionBarUpImageView.SetColorFilter(null);
			}
			else if (actionBarUpImageView != null && navigationBarTextColor == Color.Default)
				actionBarUpImageView.SetColorFilter(null);
		}

		static void UpdateGlobalContext(VisualElement view)
		{
			Element parent = view;
			while (!Application.IsApplicationOrNull(parent.RealParent))
				parent = parent.RealParent;

			var rootPage = parent as Page;
			if (rootPage != null)
			{
				Context context = GetPageContext(rootPage);
				if (context != null)
					Forms.Context = context;
			}
		}

		internal class DefaultRenderer : VisualElementRenderer<View>
		{
		}

		#region IPlatformEngine implementation

		void IPlatformLayout.OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (changed)
			{
				// ActionBar title text color resets on rotation, make sure to update
				UpdateActionBarTextColor();
				foreach (Page modal in _navModel.Roots.ToList())
					modal.Layout(new Rectangle(0, 0, _context.FromPixels(r - l), _context.FromPixels(b - t)));
			}

			foreach (IVisualElementRenderer view in _navModel.Roots.Select(GetRenderer))
				view.UpdateLayout();
		}

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			Performance.Start();

			// FIXME: potential crash
			IVisualElementRenderer viewRenderer = GetRenderer(view);

			// negative numbers have special meanings to android they don't to us
			widthConstraint = widthConstraint <= -1 ? double.PositiveInfinity : _context.ToPixels(widthConstraint);
			heightConstraint = heightConstraint <= -1 ? double.PositiveInfinity : _context.ToPixels(heightConstraint);

			int width = !double.IsPositiveInfinity(widthConstraint)
							? MeasureSpecFactory.MakeMeasureSpec((int)widthConstraint, MeasureSpecMode.AtMost)
							: MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

			int height = !double.IsPositiveInfinity(heightConstraint)
							 ? MeasureSpecFactory.MakeMeasureSpec((int)heightConstraint, MeasureSpecMode.AtMost)
							 : MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

			SizeRequest rawResult = viewRenderer.GetDesiredSize(width, height);
			if (rawResult.Minimum == Size.Zero)
				rawResult.Minimum = rawResult.Request;
			var result = new SizeRequest(new Size(_context.FromPixels(rawResult.Request.Width), _context.FromPixels(rawResult.Request.Height)),
				new Size(_context.FromPixels(rawResult.Minimum.Width), _context.FromPixels(rawResult.Minimum.Height)));

			Performance.Stop();
			return result;
		}

		bool _navAnimationInProgress;

		internal bool NavAnimationInProgress
		{
			get { return _navAnimationInProgress; }
			set
			{
				if (_navAnimationInProgress == value)
					return;
				_navAnimationInProgress = value;
				if (value)
					MessagingCenter.Send(this, CloseContextActionsSignalName);
			}
		}

		#endregion
	}
}