using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SlabFraming_1.MyWindow
{
	class ViewModel : INotifyPropertyChanged, IDataErrorInfo
	{
		SlabFraming slabFraming;
		readonly Document doc;
		Window window;

		public ViewModel(ExternalCommandData commandData, Window window)
		{
			slabFraming = new SlabFraming(commandData)
			{
				NameRebarShape = "Стж_Г",
				RebarSpace = 200,
				VolumeParametrA = 500,
				VolumeParametrB = 500
			};
			doc = commandData.Application.ActiveUIDocument.Document;
			this.window = window; 
		}

		public List<string> GetRebarBarType
		{
			get { return new GetRebarBarType(doc).GetRebarBarTypeInModel(); }
		}

		public List<string> GetNameRebarShape
		{
			get { return slabFraming.AvailableRebarShape(); }
		}

		public string NameRebarShape
		{
			get { return slabFraming.NameRebarShape; }
			set
			{
				slabFraming.NameRebarShape = value;
				GetImageName = value;
				OnPropertyChanged("NameRebarShape");
			}
		}

		private string getImageName = "Image/G.png";
		public string GetImageName
		{
			get { return getImageName; }
			set
			{
				if (value == "Стж_Г")
					getImageName = "Image/G.png";
				else if (value == "Стж_П")
					getImageName = "Image/P.png";
				OnPropertyChanged("GetImageName");
			}
		}

		public double RebarSpace
		{
			get { return slabFraming.RebarSpace; }
			set
			{
				slabFraming.RebarSpace = value;
				OnPropertyChanged("RebarSpace");
			}
		}

		public string NameRebarBarType
		{
			get { return slabFraming.NameRebarBarType; }
			set
			{
				slabFraming.NameRebarBarType = value;
				OnPropertyChanged("NameRebarBarType");
			}
		}

		public double VolumeParametrA
		{
			get { return slabFraming.VolumeParametrA; }
			set
			{
				slabFraming.VolumeParametrA = value;
				OnPropertyChanged("VolumeParametrA");
			}
		}

		public double VolumeParametrB
		{
			get { return slabFraming.VolumeParametrB; }
			set
			{
				slabFraming.VolumeParametrB = value;
				OnPropertyChanged("VolumeParametrB");
			}
		}

		private RelayCommand clickOk;
		public RelayCommand ClickOk
		{
			get
			{
				return clickOk ??
					  (clickOk = new RelayCommand(obj =>
					  {
						  try
						  {
							  slabFraming.GetFraming();
							  window.Close();
						  }
						  catch (ArgumentException ex)
						  {
							  MessageBox.Show(ex.Message);
						  }
					  },
					  obj =>
					  {
						  if (NameRebarBarType == null)
							  return false;
						  return true;
					  }));
			}
		}

		private RelayCommand closeWindowCommand;
		public RelayCommand CloseWindowCommand
		{
			get
			{
				return closeWindowCommand ??
					(closeWindowCommand = new RelayCommand(obj =>
					{
						window.Close();
					}));
			}
		}

		public string this[string columnName]
		{
			get
			{
				string error = string.Empty;
				switch (columnName)
				{
					case "VolumeParametrA":
						if (VolumeParametrA < 100)
						{
							error = "error";
						}
						break;
					case "VolumeParametrB":
						if (VolumeParametrB < 50)
						{
							error = "error";
						}
						break;
					case "RebarSpace":
						if (RebarSpace < 50)
						{
							error = "error";
						}
						break;
				}
				return error;
			}
		}
		public string Error
		{
			get { throw new NotImplementedException(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}