using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
	class Sensor
	{
		private string sn;
		private string model;
		//private string wip;
		private string location;
		private string lastTask;
		private string tech;
		private string workstation;
		private string startTime;
		private string endTime;
		private string status;
		private string result;
		private string optionCode;
		private string license;
		private string upgradePackage;

		public string Sn { get => sn; set => sn = value; }
		public string Model { get => model; set => model = value; }
		//public string Wip { get => wip; set => wip = value; }
		public string Location { get => location; set => location = value; }
		public string LastTask { get => lastTask; set => lastTask = value; }
		public string Tech { get => tech; set => tech = value; }
		public string Workstation { get => workstation; set => workstation = value; }
		public string StartTime { get => startTime; set => startTime = value; }
		public string EndTime { get => endTime; set => endTime = value; }
		public string Status { get => status; set => status = value; }
		public string Result { get => result; set => result = value; }
		public string OptionCode { get => optionCode; set => optionCode = value; }
		public string License { get => license; set => license = value; }
		public string UpgradePackage { get => upgradePackage; set => upgradePackage = value; }
	}
}
