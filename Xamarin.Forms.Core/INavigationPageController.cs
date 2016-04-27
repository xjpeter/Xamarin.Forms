using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public interface INavigationPageController
	{
		Stack<Page> StackCopy { get; }

		int StackDepth { get; }

		Task<Page> PopAsyncInner(bool animated, bool fast = false);
	}
}