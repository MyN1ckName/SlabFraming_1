using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace SlabFraming_1
{
	class GetRebarBarType
	{
		Document doc;
		public GetRebarBarType(Document doc)
		{
			this.doc = doc;
		}

		private string pattern = @"^ADDITIONAL(\w*)";

		private List<string> rbt;
		public List<string> GetRebarBarTypeInModel()
		{
			FilteredElementCollector collector = new FilteredElementCollector(doc);
			collector.OfClass(typeof(RebarBarType))
				.OfCategory(BuiltInCategory.OST_Rebar);

			rbt = new List<string>();
			foreach (Element elm in collector)
			{
				if (Regex.IsMatch(elm.Name, pattern, RegexOptions.IgnoreCase))
					rbt.Add(elm.Name);
			}
			return rbt;
		}
	}
}