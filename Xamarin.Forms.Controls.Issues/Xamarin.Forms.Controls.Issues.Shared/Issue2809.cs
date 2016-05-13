﻿using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2809, "Secondary ToolbarItems cause app to hang during PushAsync", PlatformAffected.iOS)]
	public class Issue2809: TestContentPage
	{
		protected override void Init ()
		{
			ToolbarItems.Add(new ToolbarItem("Item 1", string.Empty,
				DummyAction, ToolbarItemOrder.Secondary));

			ToolbarItems.Add(new ToolbarItem("Item 2", string.Empty,
				DummyAction, ToolbarItemOrder.Secondary));
		}

		public void DummyAction()
		{
		}

#if UITEST
		[Test]
		public void TestPageDoesntCrash ()
		{
			ShouldShowMenu();
			RunningApp.Tap (c => c.Marked ("Item 1"));
			RunningApp.Screenshot ("Didn't crash");
		}

		void ShouldShowMenu ()
		{
#if __ANDROID__
			//show secondary menu
			RunningApp.Tap (c => c.Class ("android.support.v7.widget.ActionMenuPresenter$OverflowMenuButton"));
#endif
		}

#endif

		}
}

