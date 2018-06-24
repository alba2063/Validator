using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Validator
{
	class Reader
	{
		private string setFile = "Validator_ini.xml";
		private string snLength;
		public static string conString;

		//**********************************************************************************************************
		//**                                        Read ini file                                                 **
		//**********************************************************************************************************

		public CellCollection ReadIni()
		{
			CellCollection cells = new CellCollection();


			if (!File.Exists(setFile))
			{
				Writer writer = new Writer();
				writer.WriteSettings(setFile);
				Application.Restart();

				//MessageBox.Show(ex.Message);
			}
			else
			{				
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(setFile);

					XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Settings/Cells/Cell");
					conString = doc.DocumentElement.SelectSingleNode("/Settings/ConnectionString").InnerText;
					snLength = doc.DocumentElement.SelectSingleNode("/Settings/SnLength").InnerText;

					foreach (XmlNode node in nodes)
					{
						Cell cell = new Cell();

						var index = node.SelectSingleNode("Index").InnerText;
						int k;

						if (Int32.TryParse(index, out k))
						{
							cell.Index = k;
						}
						else
						{
							cell.Index = 100;
							MessageBox.Show("Validator_ini.xml wrong Index value");
						}
						cell.Name = node.SelectSingleNode("Cell_name").InnerText;
						cell.Location = node.SelectSingleNode("Folder").InnerText;

						cells.Add(cell);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error reading Validator_ini.xml");
					throw new ReadFileErrorException(setFile, ex);
				}
			}

			return cells;
		}

		//**********************************************************************************************************
		//**                                        Get Sn Length                                                 **
		//**********************************************************************************************************

		public string GetSnLength()
		{
			return snLength;
		}

		//**********************************************************************************************************
		//**                                      XML file Validator                                              **
		//**********************************************************************************************************

		public static bool IsValidXml(string xml)
		{
			XmlDocument doc = new XmlDocument();

			try
			{				
				doc.Load(xml);
				return true;
			}
			catch
			{
				return false;
			}
		}

		//**********************************************************************************************************
		//**                                             Get DB Date                                              **
		//**********************************************************************************************************

		public string GrtDBDate(string db_file)
		{
			string db_date;

			if (!File.Exists(db_file))
			{
				db_date = "No DB file";
			}
			else
			{
				db_date = File.GetLastWriteTime(db_file).ToLocalTime().ToString();
			}

			return db_date;
		}

		//**********************************************************************************************************
		//**                                        Read Last Task                                                **
		//**********************************************************************************************************

		public Sensor ReadLastTask(string path, string sn)
		{
			Sensor sensor = new Sensor();
			string verPath = path + sn + ".xml"; //path to main XML file

			if (!File.Exists(verPath))
			{
				sensor.Sn = sn;
				sensor.Model = "No file SNXXXXXXX.xml";
				sensor.Location = path;
			}
			else
			{
				if (IsValidXml(verPath))
				{

					XmlDocument doc = new XmlDocument();
					doc.Load(verPath);

					XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Entries/Entries/Item");

					int t = nodes.Count;

					foreach (XmlNode node in nodes)
					{
						if (!node.SelectSingleNode("SensorModel").InnerText.Equals(""))
						{
							sensor.Model = node.SelectSingleNode("SensorModel").InnerText;
							break;
						}
						else
						{
							sensor.Model = "No Sensor Model";
						}
					}

					sensor.Sn = sn;
					sensor.Location = path;

					if (nodes[t - 1].SelectSingleNode("Type").InnerText.Equals("StateMachine"))
					{
						t = t - 2;
					}
					else
					{
						t = t - 1;
					}

					string lastTask = nodes[t].SelectSingleNode("Type").InnerText;
					if (lastTask.StartsWith("Safety Range DigitalIo"))
					{
						sensor.LastTask = "IO Test";
					}
					else
					{
						sensor.LastTask = lastTask;
					}
					sensor.Tech = nodes[t].SelectSingleNode("User").InnerText;
					sensor.Workstation = nodes[t].SelectSingleNode("Workstation").InnerText;
					sensor.StartTime = nodes[t].SelectSingleNode("StartTime").InnerText;
					sensor.EndTime = nodes[t].SelectSingleNode("EndTime").InnerText;

					string status = nodes[t].SelectSingleNode("Status").InnerText;
					switch (status)
					{
						case "1":
							sensor.Status = "Operation successful";
							break;
						case "0":
							sensor.Status = "General error";
							break;
						case "-988":
							sensor.Status = "Operation aborted";
							break;
						case "-993":
							sensor.Status = "Action timed out";
							break;
						case "-1000":
							sensor.Status = "Invalid state";
							break;
						default:
							sensor.Status = status;
							break;
					}

					string result;

					if (nodes[t].SelectSingleNode("Result") == null)
					{
						result = "No info";
					}
					else
					{
						result = nodes[t].SelectSingleNode("Result").InnerText;

						switch (result)
						{
							case "1":
								sensor.Result = "Passed";
								break;
							case "0":
								sensor.Result = "N/A";
								break;
							case "-1":
								sensor.Result = "Failed";
								break;
							default:
								sensor.Result = result;
								break;
						}
					}

					if (lastTask.Equals("Prepare"))
					{
						if (nodes[t].SelectSingleNode("Values").SelectSingleNode("OptionCode") != null)
						{
							sensor.OptionCode = nodes[t].SelectSingleNode("Values").SelectSingleNode("OptionCode").InnerText;
						}

						if (nodes[t].SelectSingleNode("Values").SelectSingleNode("License") != null)
						{
							sensor.License = nodes[t].SelectSingleNode("Values").SelectSingleNode("License").InnerText;
						}

						if (nodes[t].SelectSingleNode("Values").SelectSingleNode("UpgradePackage") != null)
						{
							sensor.UpgradePackage = nodes[t].SelectSingleNode("Values").SelectSingleNode("UpgradePackage").InnerText;
						}
					}
				}
				else
				{
					sensor.Sn = sn;
					sensor.Model = "SNXXXXXXX.xml is not Valid";
					sensor.Location = path;
				}
			}
			return sensor;
		}

		//**********************************************************************************************************
		//**                                        Read All Tasks                                                **
		//**********************************************************************************************************

		public DataTable ReadAllTasks(string path, string sn)
		{
			string verPath = path + sn + ".xml"; //path to main XML file
			DataSet ds = new DataSet();

			if (!File.Exists(verPath))
			{

				return null;
			}
			else
			{
				if (IsValidXml(verPath))
				{
					try
					{
						ds.ReadXml(verPath);
					}
					catch (Exception ex)
					{
						throw new ReadFileErrorException(verPath, ex);
					}

					return ds.Tables[1];
				}
				else
				{
					return null;
				}
			}
		}

		//**********************************************************************************************************
		//**                                       Read Task Details                                              **
		//**********************************************************************************************************

		public DataTable ReadTaskDetails(string path, string sn, int rowIndex)
		{
			DataTable dt = new DataTable();

			string verPath = path + sn + ".xml"; //path to main XML file

			if (!File.Exists(verPath))
			{
				return null;
			}
			else
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(verPath);

				XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Entries/Entries/Item");

				dt.Columns.Add("Parameter", typeof(string));
				dt.Columns.Add("Value", typeof(string));

				//dt.Rows.Add(nodes[rowIndex].SelectSingleNode("Values"));
				if (nodes[rowIndex].SelectSingleNode("Values") != null)
				{
					foreach (XmlNode node in nodes[rowIndex].SelectSingleNode("Values"))
					{
						dt.Rows.Add(node.Name, node.InnerText);
					}
				}
			}

			return dt;
		}

		//**********************************************************************************************************
		//**                                       Read Custom Report                                             **
		//**********************************************************************************************************

		public DataTable ReadCustomReport(string path, string sn, string model, string selectedTask, List<string> selectedRows)
		{
			DataTable dt = new DataTable();

			string verPath = path + sn + ".xml"; //path to main XML file

			if (!File.Exists(verPath))
			{
				return null;
			}
			else
			{
				if (IsValidXml(verPath))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(verPath);

					XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Entries/Entries/Item");

					dt.Columns.Add("SensorId", typeof(string));
					dt.Columns.Add("SensorModel", typeof(string));
					dt.Columns.Add("User", typeof(string));
					dt.Columns.Add("Workstation", typeof(string));
					dt.Columns.Add("StartTime", typeof(string));
					dt.Columns.Add("EndTime", typeof(string));
					dt.Columns.Add("Status", typeof(string));
					dt.Columns.Add("Result", typeof(string));

					foreach (XmlNode taskNode in nodes)
					{
						if (taskNode.SelectSingleNode("Type").InnerText.Equals(selectedTask))
						{
							if (taskNode.SelectSingleNode("Values") != null)
							{
								DataRow dr = null;
								dr = dt.NewRow();// have new row on each iteration

								dr["SensorId"] = taskNode.SelectSingleNode("SensorId").InnerText;
								dr["SensorModel"] = model;// taskNode.SelectSingleNode("SensorModel").InnerText;
								dr["User"] = taskNode.SelectSingleNode("User").InnerText;
								dr["Workstation"] = taskNode.SelectSingleNode("Workstation").InnerText;
								dr["StartTime"] = taskNode.SelectSingleNode("StartTime").InnerText;
								dr["EndTime"] = taskNode.SelectSingleNode("EndTime").InnerText;
								dr["Status"] = taskNode.SelectSingleNode("Status").InnerText;
								if (taskNode.SelectSingleNode("Result") != null)
								{
									dr["Result"] = taskNode.SelectSingleNode("Result").InnerText;
								}

								foreach (XmlNode node in taskNode.SelectSingleNode("Values"))
								{
									foreach (string rowParameter in selectedRows)
									{
										if (node.Name.Equals(rowParameter))
										{
											if (!dr.Table.Columns.Contains(rowParameter))
											{
												dt.Columns.Add(rowParameter, typeof(string));
												dr[rowParameter] = node.InnerText;
											}
											else
											{
												dr[rowParameter] = node.InnerText;
											}
										}
									}
								}
								dt.Rows.Add(dr);
							}
						}
					}
				}
				else
				{
					return null;
				}
			}

			return dt;
		}

		//**********************************************************************************************************
		//**                                    Read Full Custom Report                                           **
		//**********************************************************************************************************

		public DataTable ReadFullCustomReport(SensorCollection sensors, string selectedTask, List<string> selectedRows)
		{
			DataTable dt = new DataTable();
			DataTable dtOne = new DataTable();

			if (sensors != null)
			{
				foreach (Sensor sensor in sensors)
				{
					if (!sensor.Model.Equals("No Folder"))
					{
						dtOne = ReadCustomReport(sensor.Location, sensor.Sn, sensor.Model, selectedTask, selectedRows);
						if (dtOne != null)
						{
							dt.Merge(dtOne);
						}
					}
				}
			}

			return dt;
		}

		//**********************************************************************************************************
		//**                                        Read SN from id.xml                                           **
		//**********************************************************************************************************

		public string ReadId(string path)
		{
			string verPath = path + "\\id.xml"; //path to main XML file
			string sn;

			if (!File.Exists(verPath))
			{
				sn = "No id.xml";
			}
			else
			{
				if (!File.Exists(verPath))
				{
					sn = "No Info";
				}
				else
				{
					XmlDocument doc = new XmlDocument();//Is it more efficient to read first few rows as a text file???
					doc.Load(verPath);

					sn = doc.DocumentElement.SelectSingleNode("/Identity/SerialNumber").InnerText;
				}
			}

			return sn;
		}

		//**********************************************************************************************************
		//**                                      Collect SnBaseCollection                                        **
		//**********************************************************************************************************

		public SnBaseCollection ReadCell(string location)
		{
			SnBaseCollection sensors = new SnBaseCollection();
			string sn;//

			if (Directory.Exists(location))
			{
				foreach (string path in Directory.GetDirectories(location))
				{
					sn = path.Remove(0, location.Length + 1);
					string verPath = path + "\\" + sn + ".xml"; //path to main XML file

					SnBase sensor = new SnBase();

					if (!File.Exists(verPath))
					{
						sensor.Sn = sn;
						sensor.Model = "No file SNXXXXXXX.xml";
						sensor.Location = path;
					}
					else
					{
						//sensor.Time = File.GetLastWriteTime(verPath).ToUniversalTime().ToString();
						sensor.Time = File.GetLastWriteTime(verPath).ToLocalTime().ToString();


						XmlDocument doc = new XmlDocument();

						if (IsValidXml(verPath))
						{
							doc.Load(verPath);

							XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Entries/Entries/Item");

							bool f = false;//

							foreach (XmlNode node in nodes)
							{
								if (!node.SelectSingleNode("SensorModel").InnerText.Equals(""))
								{
									sensor.Model = node.SelectSingleNode("SensorModel").InnerText;
								}

								if (!node.SelectSingleNode("SensorId").InnerText.Equals(""))
								{
									sensor.Sn = node.SelectSingleNode("SensorId").InnerText;
								}

								if (node.SelectSingleNode("Type").InnerText.Equals("Initialize"))
								{
									if (node.SelectSingleNode("Values") != null)
									{
										foreach (XmlNode innerNode in node.SelectSingleNode("Values"))
										{
											if (innerNode.Name.Equals("WorkOrder"))
											{
												sensor.Wip = innerNode.InnerText;
												f = true;
											}
										}
									}
								}
								sensor.Location = path;
								if (f) break;
							}
						}
						else
						{
							sensor.Sn = sn;
							sensor.Model = "XML is not valid";
							sensor.Location = path;
						}
					}

					sensors.Add(sensor);
				}
			}

			return sensors;
		}
	}
}
