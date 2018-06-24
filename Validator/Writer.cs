using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Validator
{
	class Writer
	{
		public void WriteSettings(string fileName)
		{
			//fileName = "ValidatorNew_ini.xml";

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";

			using (XmlWriter writer = XmlWriter.Create(fileName, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("Settings");
				writer.WriteElementString("ConnectionString", "Data Source=validatordb.sqlite;Version=3;");
				writer.WriteElementString("SnLength", "7");

				writer.WriteStartElement("Cells");

				writer.WriteStartElement("Cell");
				writer.WriteElementString("Index", "0");
				writer.WriteElementString("Cell_name", "Gocator 1");
				writer.WriteElementString("Folder", "Y:\\Gocator\\1x00_Calibration");
				writer.WriteEndElement();

				writer.WriteStartElement("Cell");
				writer.WriteElementString("Index", "1");
				writer.WriteElementString("Cell_name", "Gocator 2");
				writer.WriteElementString("Folder", "Y:\\Gocator\\2xx_Calibration");
				writer.WriteEndElement();

				writer.WriteStartElement("Cell");
				writer.WriteElementString("Index", "2");
				writer.WriteElementString("Cell_name", "Gocator 3");
				writer.WriteElementString("Folder", "Y:\\Gocator\\3xx0_Calibration");
				writer.WriteEndElement();

				writer.WriteStartElement("Cell");
				writer.WriteElementString("Index", "3");
				writer.WriteElementString("Cell_name", "Gocator 20x0");
				writer.WriteElementString("Folder", "Y:\\Gocator\\20x0_Calibration");
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}
	}
}
