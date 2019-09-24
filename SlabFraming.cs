using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;

namespace SlabFraming_1
{
	/// <summary>
	/// 
	/// </summary>
	[TransactionAttribute(TransactionMode.Manual)]
	[RegenerationAttribute(RegenerationOption.Manual)]
	public class SlabFraming
	{
		const double coof = 304.8;
		UIApplication uiApp;
		Document doc;
		Element host;
		public SlabFraming(ExternalCommandData commandDate)
		{
			uiApp = commandDate.Application;
			doc = uiApp.ActiveUIDocument.Document;
			host = GetHost();
		}

		public string NameRebarShape { get; set; }
		public string NameRebarBarType { get; set; }
		public double VolumeParametrA { get; set; }
		public double VolumeParametrB { get; set; }
		public double RebarSpace { get; set; }

		public List<string> AvailableRebarShape()
		{
			List<string> rs = new List<string>();
			rs.Add("Стж_П");
			rs.Add("Стж_Г");
			return rs;
		}

		public void GetFraming()
		{
			if (VolumeParametrA < 100)
				throw new ArgumentException("Значение A должно быть больше 100");
			if (VolumeParametrB < 50)
				throw new ArgumentException("Значение B должно быть больше 50");
			if (RebarSpace < 50)
				throw new ArgumentException("Значение RebarSpace должно быть больше 50");

			Options geoOptions = new Options()
			{
				View = doc.ActiveView,
				IncludeNonVisibleObjects = true
			};
			GeometryElement geoElement = host.get_Geometry(geoOptions);

			FaceArray faceArray = new FaceArray();
			foreach (GeometryObject geoObject in geoElement)
			{
				Solid geoSolid = geoObject as Solid;
				if (null != geoSolid)
				{
					faceArray = geoSolid.Faces;
				}
			}

			RebarShape rebarShape = GetRebarShape(NameRebarShape);
			RebarBarType rebarBarType = GetRebarBarType(NameRebarBarType);

			using (TransactionGroup tg = new TransactionGroup(doc
			, "TransactionGroupSlabReinforcement_3"))
			{
				tg.Start();
				foreach (PlanarFace planarFace in faceArray)
				{
					if (planarFace.FaceNormal.Z == 0)
					{
						XYZ origin = GetOrigin(planarFace
							, VolumeParametrA / coof
							, NameRebarShape);

						using (Transaction t = new Transaction(doc, "SlabReinforcement_3"))
						{
							t.Start();

							if (NameRebarShape == "Стж_Г")
							{
								XYZ xVec = planarFace.FaceNormal;
								XYZ yVec = -planarFace.XVector;

								Rebar rebar = Rebar.CreateFromRebarShape(doc, rebarShape
									, rebarBarType, host, origin, xVec, yVec);
								if (null != rebar)
								{
									bool barsOnNormalSide;
									if (planarFace.YVector == new XYZ(0, 1, 0))
										barsOnNormalSide = true;
									else barsOnNormalSide = false;

									int numberOfBarPositions = (int)Math.Round
										(
										((planarFace.GetBoundingBox().Max.V - (2 * 50 / coof))
										/
										(RebarSpace / coof)
										+ 1)
										);

									double spacing = RebarSpace / coof;
									bool includeFirstBar = true;
									bool includeLastBar = true;

									rebar.GetShapeDrivenAccessor()
										.SetLayoutAsNumberWithSpacing(numberOfBarPositions
										, spacing, barsOnNormalSide
										, includeFirstBar, includeLastBar);
									foreach (Parameter para in rebar.Parameters)
									{
										SetParametr(para);
									}
								}
							}
							if (NameRebarShape == "Стж_П")
							{
								XYZ xVec = planarFace.XVector;
								XYZ yVec = -planarFace.FaceNormal;

								Rebar rebar = Rebar.CreateFromRebarShape(doc, rebarShape
									, rebarBarType, host, origin, xVec, yVec);
								if (null != rebar)
								{
									bool barsOnNormalSide;
									if (planarFace.YVector == new XYZ(0, 1, 0))
										barsOnNormalSide = true;
									else barsOnNormalSide = false;

									int numberOfBarPositions = (int)Math.Round
										(
										((planarFace.GetBoundingBox().Max.V - (2 * 50 / coof))
										/
										(RebarSpace / coof)
										+ 1)
										);

									double spacing = RebarSpace / coof;
									bool includeFirstBar = true;
									bool includeLastBar = true;

									rebar.GetShapeDrivenAccessor()
										.SetLayoutAsNumberWithSpacing(numberOfBarPositions
										, spacing, barsOnNormalSide
										, includeFirstBar, includeLastBar);
									foreach (Parameter para in rebar.Parameters)
									{
										SetParametr(para);
									}
								}
							}
							t.Commit();
						}
					}
				}
				tg.Assimilate();
			}
		}

		private void SetParametr(Parameter para)
		{
			if (para.Definition.Name == "A")
				para.Set(VolumeParametrA / coof);

			if (para.Definition.Name == "B")
				para.Set(VolumeParametrB / coof);
		}

		private RebarShape GetRebarShape(string nameRebarShape)
		{
			RebarShape rebarShape = null;
			FilteredElementCollector rsCollector = new FilteredElementCollector(doc);
			rsCollector.OfClass(typeof(RebarShape))
				.OfCategory(BuiltInCategory.OST_Rebar);
			foreach (RebarShape rs in rsCollector)
			{
				if (rs.Name == nameRebarShape)
					rebarShape = rs;
			}
			return rebarShape;
		}

		private RebarBarType GetRebarBarType(string nameRebarBarType)
		{
			RebarBarType rebarBarType = null;
			FilteredElementCollector rbtCollector = new FilteredElementCollector(doc);
			rbtCollector.OfClass(typeof(RebarBarType))
				.OfCategory(BuiltInCategory.OST_Rebar);
			foreach (RebarBarType rbt in rbtCollector)
			{
				if (rbt.Name == nameRebarBarType)
					rebarBarType = rbt;
			}
			return rebarBarType;
		}

		private Element GetHost()
		{
			Element host = null;
			Selection sel = uiApp.ActiveUIDocument.Selection;
			Reference pickRef = sel.PickObject(ObjectType.Element
				, new SlabPickFilter(), "Select Slab");
			host = doc.GetElement(pickRef.ElementId);
			return host;
		}

		private XYZ GetOrigin(PlanarFace planarFace
			, double volumeParametrA
			, string nameRebarShape)
		{
			double otherCoverDistance =
				(doc.GetElement(host.get_Parameter
				(BuiltInParameter.CLEAR_COVER_OTHER).AsElementId()) as RebarCoverType)
				.CoverDistance;
			double topCoverDistance =
				(doc.GetElement(host.get_Parameter
				(BuiltInParameter.CLEAR_COVER_TOP).AsElementId()) as RebarCoverType)
				.CoverDistance;

			if (nameRebarShape == "Стж_Г")
			{
				double x = planarFace.Origin.X;
				double y = planarFace.Origin.Y;
				double z = planarFace.Origin.Z + planarFace.GetBoundingBox().Max.U
					- topCoverDistance;

				if (planarFace.FaceNormal.X > 0)
					x =
						x - Math.Abs(planarFace.FaceNormal.X
						* (volumeParametrA + otherCoverDistance));
				if (planarFace.FaceNormal.X < 0)
					x =
						x + Math.Abs(planarFace.FaceNormal.X
						* (volumeParametrA + otherCoverDistance));
				if (planarFace.FaceNormal.Y > 0)
					y =
						y - Math.Abs(planarFace.FaceNormal.Y
						* (volumeParametrA + otherCoverDistance));
				if (planarFace.FaceNormal.Y < 0)
					y =
						y + Math.Abs(planarFace.FaceNormal.Y
						* (volumeParametrA + otherCoverDistance));
				return new XYZ(x, y, z);
			}

			if (nameRebarShape == "Стж_П")
			{
				double x = planarFace.Origin.X - Math.Abs(planarFace.FaceNormal.X
					* (otherCoverDistance - 500 / coof));
				double y = planarFace.Origin.Y - Math.Abs(planarFace.FaceNormal.Y
					* (otherCoverDistance - 500 / coof));
				double z = planarFace.Origin.Z + planarFace.GetBoundingBox().Max.U
					- topCoverDistance;

				return new XYZ(x, y, z);
			}
			else return null;
		}
	}
}