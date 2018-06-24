using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
	class Sniffer
	{
		//private string longSerial;
		//private string path;
		//private bool folderExists;

		public Sniffer(){}

		public SensorCollection Sniff(List<string> serials, CellCollection cells)
		{
			SensorCollection sensors = new SensorCollection();
			string longSerial;
			string path;
			bool folderExists;

			//Search SN in every Folder
			foreach (string sn in serials)
			{
				folderExists = false;
				Sensor sensor = new Sensor();
				longSerial = string.Format("SN{0}", sn);

				foreach (Cell cell in cells)
				{
					path = string.Format("{0}\\{1}\\", cell.Location, longSerial);

					if (Directory.Exists(path))
					{
						//read sensor model
						Reader reader = new Reader();
						sensor = reader.ReadLastTask(path, longSerial);//read SN00XXXXX.xml

						folderExists = true;

						break;
					}
				}

				if (!folderExists)
				{
					sensor.Sn = longSerial;
					sensor.Location = "In all locations";
					sensor.Model = "No Folder";
				}

				sensors.Add(sensor);
			}

			return sensors;
		}

	}
}
