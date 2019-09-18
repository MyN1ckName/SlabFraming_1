using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace SlabFraming_1
{
	/// <summary>
	///   Represents a filter by BuiltInCategory.OST_Floors in Autodesk Revit.
	/// </summary>
	public class SlabPickFilter : ISelectionFilter
	{
		public bool AllowElement(Element e)
		{
			return (e.Category.Id.IntegerValue.
				Equals((int)BuiltInCategory.OST_Floors));
		}
		public bool AllowReference(Reference r, XYZ p)
		{
			return false;
		}
	}
}