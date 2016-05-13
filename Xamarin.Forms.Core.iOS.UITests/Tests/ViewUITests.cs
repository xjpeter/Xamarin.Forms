﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;

using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract class _ViewUITests : BaseTestFixture
	{
		/* Under score prefixes ensure inherited properties run first in test suite */
		//[Test]
		//[Category ("View")]
		//[UiTestBroken (BrokenReason.UITestBug, "Issue #115 - when trying to get anchorPoint, iOS")]
		//[UiTest (Test.VisualElement.AnchorX)]
		public virtual void _AnchorX ()
		{
			
		}

		// [Test]
		// [UiTest (Test.VisualElement.AnchorY)]
		// TODO: working on some views, others not
		public virtual void _AnchorY ()
		{
			
		}

		// [Test]
		// [UiTest (Test.VisualElement.BackgroundColor)]
		// [UiTestBroken (BrokenReason.UITestBug, "UITest Issue #107")]
		public virtual void _BackgroundColor ()
		{
			//TODO: this was failing and is changing in next version of calabash (UI-Test-pre nuget) to a json rgb
//			var remote = RemoteFactory.CreateRemote<ViewContainerRemote> (App, "BackgroundColor", PlatformViewType);
//			remote.GoTo ();
//			if (App is iOSApp) {
//				var backgroundColor = remote.GetProperty<Color> (View.BackgroundColorProperty);
//				Assert.AreEqual (Color.Blue, backgroundColor);
//			}

		}

		[Test]
		[UiTest (typeof(VisualElement), "Focus")]
		public abstract void _Focus ();

		[Test]
		[UiTest (typeof (VisualElement), "GestureRecognizers")]
		public abstract void _GestureRecognizers ();

		//[Test]
		[UiTest (typeof (VisualElement), "InputTransparent")]
		public virtual void _InputTransparent ()
		{
			//var remote = new LayeredViewContainerRemote (App, Test.VisualElement.InputTransparent, PlatformViewType);
			//remote.GoTo ();

			//var hiddenButtonClickedLabelTextPre = remote.GetLayeredLabel ().Text;
			//Assert.AreEqual ("Hidden Button (Not Clicked)", hiddenButtonClickedLabelTextPre);

			//remote.TapHiddenButton ();

			//var hiddenButtonClickedLabelTextPost = remote.GetLayeredLabel ().Text;
			//var hiddenButtonClicked = hiddenButtonClickedLabelTextPost == "Hidden Button (Clicked)";

			//// Allow tests to continue by dismissing DatePicker that should not show
			//// Remove when InputTransparency works
			//if (!hiddenButtonClicked && PlatformViewType == PlatformViews.DatePicker)
			//	remote.DismissPopOver ();

			//Assert.True (hiddenButtonClicked);		
		}

		[Test]
		[UiTest (typeof (VisualElement), "IsEnabled")]
		public virtual void _IsEnabled ()
		{
			//var propName = Test.VisualElement.IsEnabled.ToString ();
			var remote = new StateViewContainerRemote (App, Test.VisualElement.IsEnabled, PlatformViewType);
			remote.GoTo ();

			var enabled = remote.GetProperty<bool> (View.IsEnabledProperty);
			Assert.IsTrue (enabled);

			remote.TapStateButton ();

			enabled = remote.GetProperty<bool> (View.IsEnabledProperty);
			Assert.IsFalse (enabled);

			remote.TapStateButton ();

			var isEnabled = remote.GetStateLabel ().Text;
			Assert.AreEqual ("True", isEnabled);

			remote.TapStateButton ();

			var isDisabled = remote.GetStateLabel ().Text;
			Assert.AreEqual ("False", isDisabled);
		}

		[Test]
		[UiTest (typeof (VisualElement), "IsFocused")]
		public abstract void _IsFocused ();

		[Test]
		[UiTest (typeof (VisualElement), "IsVisible")]
		public virtual void _IsVisible ()
		{
			var remote = new StateViewContainerRemote (App, Test.VisualElement.IsVisible, PlatformViewType);
			remote.GoTo ();

			var viewPre = remote.GetViews ();

			Assert.AreEqual (1, viewPre.Length);

			remote.TapStateButton ();

			var viewPost = remote.GetViews ();
	
			Assert.AreEqual (0, viewPost.Length);
		}
			
		[UiTestExemptAttribute (ExemptReason.None, "Not sure how to test at the moment")]
		public virtual void _Layout (){}

		[UiTestExemptAttribute (ExemptReason.None, "Not sure how to test at the moment")]
		public virtual void _Navigation () {}

		[Test]
		[UiTest (typeof (VisualElement), "Opacity")]
		public virtual void _Opacity ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.Opacity, PlatformViewType);
			remote.GoTo ();

			float opacity = -1f;
			opacity = remote.GetProperty<float> (View.OpacityProperty);
			Assert.AreEqual (0.5f, opacity);
		}

		[Test]
		[UiTest (typeof(VisualElement), "Rotation")]
		[UiTestBroken (BrokenReason.CalabashBug, "Calabash bug")]
		public virtual void _Rotation ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.Rotation, PlatformViewType);
			remote.GoTo ();

#if __ANDROID__
			var rotation = remote.GetProperty<float> (View.RotationProperty);
			Assert.AreEqual (10.0f, rotation);
#endif
#if __IOS__
			var rotationMatrix = remote.GetProperty<Matrix> (View.RotationProperty);
			Matrix generatedMatrix = NumericExtensions.CalculateRotationMatrixForDegrees (10, Axis.Z);
			Assert.AreEqual (generatedMatrix, rotationMatrix);
#endif
}

		[Test]
		[UiTest (typeof (VisualElement), "RotationX")]
		public virtual void _RotationX ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.RotationX, PlatformViewType);
			remote.GoTo ();

#if __ANDROID__
			var rotationX = remote.GetProperty<float> (View.RotationXProperty);
			Assert.AreEqual (33.0f, rotationX);
#endif
#if __IOS__
			var rotationXMatrix = remote.GetProperty<Matrix> (View.RotationXProperty);
			Matrix matrix = NumericExtensions.CalculateRotationMatrixForDegrees (33.0f, Axis.X);
			Assert.AreEqual (matrix, rotationXMatrix);
#endif
		}

		[Test]
		[UiTest (typeof (VisualElement), "RotationY")]
		public virtual void _RotationY ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.RotationY, PlatformViewType);
			remote.GoTo ();

#if __ANDROID__
			var rotationY = remote.GetProperty<float> (View.RotationYProperty);
			Assert.AreEqual (10.0f, rotationY);
#endif
#if __IOS__
			var rotationYMatrix = remote.GetProperty<Matrix> (View.RotationYProperty);
			Matrix matrix = NumericExtensions.CalculateRotationMatrixForDegrees (10.0f, Axis.Y);
			Assert.AreEqual (matrix, rotationYMatrix);
#endif
		}

		[Test]
		[UiTest (typeof (VisualElement), "Scale")]
		public virtual void _Scale ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.Scale, PlatformViewType);
			remote.GoTo ();

			var scaleMatrix = remote.GetProperty<Matrix> (View.ScaleProperty);
			Matrix generatedMatrix = NumericExtensions.BuildScaleMatrix (0.5f);
			Assert.AreEqual (generatedMatrix, scaleMatrix); 
		}

		[Test]
		[UiTest (typeof (VisualElement), "TranslationX")]
		[Category ("ManualReview")]
		public virtual void _TranslationX ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.TranslationX, PlatformViewType);
			remote.GoTo ();
		}

		[Test]
		[UiTest (typeof (VisualElement), "TranslationY")]
		[Category ("ManualReview")]
		public virtual void _TranslationY ()
		{
			var remote = new ViewContainerRemote (App, Test.VisualElement.TranslationY, PlatformViewType);
			remote.GoTo ();
		}

		[Test]
		[UiTest (typeof (VisualElement), "Unfocus")]
		public abstract void _UnFocus ();
	}
}