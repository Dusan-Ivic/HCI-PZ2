using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Data
{
	public class ComboBoxItemsData
	{
		public static Dictionary<string, string> entityTypes = new Dictionary<string, string>()
		{
			{ "IA", "pack://application:,,,/NetworkService;component/Images/ia.png" },
			{ "IB", "pack://application:,,,/NetworkService;component/Images/ib.png" }
		};
	}
}
