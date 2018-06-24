using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
	class SnBase
	{
		private string sn;
		private string model;
		private string wip;
		private string location;
		private string time;

		public string Sn { get => sn; set => sn = value; }
		public string Model { get => model; set => model = value; }
		public string Wip { get => wip; set => wip = value; }
		public string Location { get => location; set => location = value; }
		public string Time { get => time; set => time = value; }
	}
}
