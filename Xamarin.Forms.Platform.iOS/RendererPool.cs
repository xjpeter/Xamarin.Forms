using System;
using System.Collections.Generic;
using System.Linq;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public sealed class RendererPool
	{
		readonly Dictionary<Type, Stack<IVisualElementRenderer>> _freeRenderers = new Dictionary<Type, Stack<IVisualElementRenderer>>();

		readonly VisualElement _oldElement;

		readonly IVisualElementRenderer _parent;

		public RendererPool(IVisualElementRenderer renderer, VisualElement oldElement)
		{
			if (renderer == null)
				throw new ArgumentNullException("renderer");

			if (oldElement == null)
				throw new ArgumentNullException("oldElement");

			_oldElement = oldElement;
			_parent = renderer;
		}

		public IVisualElementRenderer GetFreeRenderer(VisualElement view)
		{
			if (view == null)
				throw new ArgumentNullException("view");

			var rendererType = Registrar.Registered.GetHandlerType(view.GetType()) ?? typeof(ViewRenderer);

			Stack<IVisualElementRenderer> renderers;
			if (!_freeRenderers.TryGetValue(rendererType, out renderers) || renderers.Count == 0)
				return null;

			var renderer = renderers.Pop();
			renderer.SetElement(view);
			return renderer;
		}

		public void UpdateNewElement(VisualElement newElement)
		{
			if (newElement == null)
				throw new ArgumentNullException("newElement");

			var sameChildrenTypes = true;

			var oldChildren = _oldElement.LogicalChildren;
			var newChildren = newElement.LogicalChildren;

			if (oldChildren.Count == newChildren.Count)
			{
				for (var i = 0; i < oldChildren.Count; i++)
				{
					if (oldChildren[i].GetType() != newChildren[i].GetType())
					{
						sameChildrenTypes = false;
						break;
					}
				}
			}
			else
				sameChildrenTypes = false;

			if (!sameChildrenTypes)
			{
				ClearRenderers(_parent);
				FillChildrenWithRenderers(newElement);
			}
			else
				UpdateRenderers(newElement);
		}

		void ClearRenderers(IVisualElementRenderer renderer)
		{
			if (renderer == null)
				return;

			var subviews = renderer.NativeView.Subviews;
			for (var i = 0; i < subviews.Length; i++)
			{
				var childRenderer = subviews[i] as IVisualElementRenderer;
				if (childRenderer != null)
				{
					PushRenderer(childRenderer);

					if (ReferenceEquals(childRenderer, Platform.GetRenderer(childRenderer.Element)))
						childRenderer.Element.ClearValue(Platform.RendererProperty);
				}

				subviews[i].RemoveFromSuperview();
			}
		}

		void FillChildrenWithRenderers(VisualElement element)
		{
			foreach (var logicalChild in element.LogicalChildren)
			{
				var child = logicalChild as VisualElement;
				if (child != null)
				{
					var renderer = GetFreeRenderer(child) ?? Platform.CreateRenderer(child);
					Platform.SetRenderer(child, renderer);

					_parent.NativeView.AddSubview(renderer.NativeView);
				}
			}
		}

		void PushRenderer(IVisualElementRenderer renderer)
		{
			var rendererType = renderer.GetType();

			Stack<IVisualElementRenderer> renderers;
			if (!_freeRenderers.TryGetValue(rendererType, out renderers))
				_freeRenderers[rendererType] = renderers = new Stack<IVisualElementRenderer>();

			renderers.Push(renderer);
		}

		void UpdateRenderers(Element newElement)
		{
			if (newElement.LogicalChildren.Count == 0)
				return;

			var subviews = _parent.NativeView.Subviews;
			for (var i = 0; i < subviews.Length; i++)
			{
				var childRenderer = subviews[i] as IVisualElementRenderer;
				if (childRenderer == null)
					continue;

				var x = (int)childRenderer.NativeView.Layer.ZPosition / 1000;
				var element = newElement.LogicalChildren[x] as VisualElement;
				if (element == null)
					continue;

				if (childRenderer.Element != null && ReferenceEquals(childRenderer, Platform.GetRenderer(childRenderer.Element)))
					childRenderer.Element.ClearValue(Platform.RendererProperty);

				childRenderer.SetElement(element);
				Platform.SetRenderer(element, childRenderer);
			}
		}
	}
}