using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
	public class EntityType : ValidationBase
	{
		private string name;
		private string imgSrc = "pack://application:,,,/NetworkService;component/Images/no-image.jpg";

		public string Name
		{
			get { return name; }
			set
			{
				if (name != value)
				{
					name = value;
					ImgSrc = Data.ComboBoxItemsData.entityTypes[value];
					OnPropertyChanged("Name");
				}
			}
		}

		public string ImgSrc
		{
			get { return imgSrc; }
			set
			{
				if (imgSrc != value)
				{
					imgSrc = value;
					OnPropertyChanged("ImgSrc");
				}
			}
		}

		protected override void ValidateSelf()
		{
			if (this.Name == null)
			{
				this.ValidationErrors["Name"] = "Type must be selected.";
			}
		}
	}
}
