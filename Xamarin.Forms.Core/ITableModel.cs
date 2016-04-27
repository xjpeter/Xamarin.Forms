
namespace Xamarin.Forms
{
	public interface ITableModel
	{
		void RowSelected(object item);
		void RowLongPressed(int section, int row);
		string[] GetSectionIndexTitles();
		Cell GetCell(int section, int row);
		void RowSelected(int section, int row);
		object GetItem(int section, int row);
		string GetSectionTitle(int section);
		Cell GetHeaderCell(int section);
		int GetRowCount(int section);
		int GetSectionCount();
	}
}
