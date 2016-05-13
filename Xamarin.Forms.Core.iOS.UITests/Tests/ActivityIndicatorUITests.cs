using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("ActivityIndicator")]
	internal class ActivityIndicatorUITests : _ViewUITests
	{
		public ActivityIndicatorUITests ()
		{
			PlatformViewType = Views.ActivityIndicator;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ActivityIndicatorGallery);
		}

		// View tests
		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _Focus () {}

		public override void _GestureRecognizers ()
		{
			// TODO Can implement this
			var remote = new ViewContainerRemote (App, Test.View.GestureRecognizers, PlatformViewType);
			remote.GoTo ();
		}
		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsEnabled () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsFocused () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _UnFocus () {}

		// ActivityIndicator tests
		[Test]
		[UiTest (typeof(ActivityIndicator), "IsRunning")]
		public void IsRunning ()
		{
			var remote = new ViewContainerRemote (App, Test.ActivityIndicator.IsRunning, PlatformViewType);
			remote.GoTo ();

			var isRunning = remote.GetProperty<bool> (ActivityIndicator.IsRunningProperty);
			Assert.IsTrue (isRunning);
		}
		
		protected override void FixtureTeardown ()
		{
			App.NavigateBack ();
			base.FixtureTeardown ();
		}

	}
}