using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
	class Cell
	{
		private string name;
		private string location;
		private int index;

		public string Name { get => name; set => name = value; }
		public string Location { get => location; set => location = value; }
		public int Index { get => index; set => index = value; }
	}
}
