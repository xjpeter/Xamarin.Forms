using System;
using System.Drawing;
using System.ComponentModel;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, UISearchBar>
	{
		UIColor _cancelButtonTextColorDefaultDisabled;
		UIColor _cancelButtonTextColorDefaultHighlighted;
		UIColor _cancelButtonTextColorDefaultNormal;

		UIColor _defaultTextColor;
		UIColor _defaultTintColor;
		UITextField _textField;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.CancelButtonClicked -= OnCancelClicked;
					Control.SearchButtonClicked -= OnSearchButtonClicked;
					Control.TextChanged -= OnTextChanged;

					Control.OnEditingStarted -= OnEditingEnded;
					Control.OnEditingStopped -= OnEditingStarted;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var searchBar = new UISearchBar(RectangleF.Empty) { ShowsCancelButton = true, BarStyle = UIBarStyle.Default };

					var cancelButton = searchBar.FindDescendantView<UIButton>();
					_cancelButtonTextColorDefaultNormal = cancelButton.TitleColor(UIControlState.Normal);
					_cancelButtonTextColorDefaultHighlighted = cancelButton.TitleColor(UIControlState.Highlighted);
					_cancelButtonTextColorDefaultDisabled = cancelButton.TitleColor(UIControlState.Disabled);

					SetNativeControl(searchBar);

					Control.CancelButtonClicked += OnCancelClicked;
					Control.SearchButtonClicked += OnSearchButtonClicked;
					Control.TextChanged += OnTextChanged;

					Control.OnEditingStarted += OnEditingStarted;
					Control.OnEditingStopped += OnEditingEnded;
				}

				UpdatePlaceholder();
				UpdateText();
				UpdateFont();
				UpdateIsEnabled();
				UpdateCancelButton();
				UpdateAlignment();
				UpdateTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName || e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
				UpdateTextColor();
				UpdatePlaceholder();
			}
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == SearchBar.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
				UpdateCancelButton();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;

			if (_defaultTintColor == null)
			{
				if (Forms.IsiOS7OrNewer)
					_defaultTintColor = Control.BarTintColor;
				else
					_defaultTintColor = Control.TintColor;
			}

			if (Forms.IsiOS7OrNewer)
				Control.BarTintColor = color.ToUIColor(_defaultTintColor);
			else
				Control.TintColor = color.ToUIColor(_defaultTintColor);

			if (color.A < 1)
				Control.SetBackgroundImage(new UIImage(), UIBarPosition.Any, UIBarMetrics.Default);

			// updating BarTintColor resets the button color so we need to update the button color again
			UpdateCancelButton();
		}

		void OnCancelClicked(object sender, EventArgs args)
		{
			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, null);
			Control.ResignFirstResponder();
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			if (Element != null)
				((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnEditingStarted(object sender, EventArgs e)
		{
			if (Element != null)
				((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnSearchButtonClicked(object sender, EventArgs e)
		{
			((ISearchBarController)Element).OnSearchButtonPressed();
			Control.ResignFirstResponder();
		}

		void OnTextChanged(object sender, UISearchBarTextChangedEventArgs a)
		{
			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, Control.Text);
		}

		void UpdateAlignment()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			_textField.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateCancelButton()
		{
			Control.ShowsCancelButton = !string.IsNullOrEmpty(Control.Text);

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time
			var cancelButton = Control.FindDescendantView<UIButton>();

			if (cancelButton == null)
				return;

			if (Element.CancelButtonColor == Color.Default)
			{
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultNormal, UIControlState.Normal);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultHighlighted, UIControlState.Highlighted);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				cancelButton.SetTitleColor(Element.CancelButtonColor.ToUIColor(), UIControlState.Normal);
				cancelButton.SetTitleColor(Element.CancelButtonColor.ToUIColor(), UIControlState.Highlighted);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
			}
		}

		void UpdateFont()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			_textField.Font = Element.ToUIFont();
		}

		void UpdateIsEnabled()
		{
			Control.UserInteractionEnabled = Element.IsEnabled;
		}

		void UpdatePlaceholder()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			var formatted = (FormattedString)Element.Placeholder ?? string.Empty;
			var targetColor = Element.PlaceholderColor;

			// Placeholder default color is 70% gray
			// https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder

			var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : ColorExtensions.SeventyPercentGrey.ToColor();

			_textField.AttributedPlaceholder = formatted.ToAttributed(Element, color);
		}

		void UpdateText()
		{
			Control.Text = Element.Text;
			UpdateCancelButton();
		}

		void UpdateTextColor()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			_defaultTextColor = _defaultTextColor ?? _textField.TextColor;
			var targetColor = Element.TextColor;

			var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : _defaultTextColor.ToColor();

			_textField.TextColor = color.ToUIColor();
		}
	}
}