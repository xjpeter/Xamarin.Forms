using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Reflection;

using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using Xamarin.UITest.iOS;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("WebView")]
	internal class WebViewUITests : _ViewUITests
	{
		public WebViewUITests ()
		{
			PlatformViewType = Views.WebView;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.WebViewGallery);
		}
			
		[Category ("ManualReview")]
		public override void _IsEnabled ()
		{
			Assert.Inconclusive ("Does not make sense for WebView");
		}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _IsVisible () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction with Label")]
		public override void _Focus () {}

		// TODO
		public override void _GestureRecognizers () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction with Label")]
		public override void _IsFocused () {}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _Opacity () {}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _Rotation () {}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _RotationX () {}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _RotationY () {}


		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _TranslationX () {}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _TranslationY () {}

		[Test]
		[Category ("ManualReview")]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _Scale () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction with Label")]
		public override void _UnFocus () {}

		// TODO
		// Implement control specific ui tests

		protected override void FixtureTeardown ()
		{
			App.NavigateBack ();
			base.FixtureTeardown ();
		}
	}
}