using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Validator
{
	public partial class MainForm : Form
	{
		public static int snLength; //number of digits in serial number
		CellCollection cells;
		SensorCollection sensors;
		BindingSource source;
		DataTable dt;
		string sn;      //Current sn for Task History
		string path;    //Current path for Task History
		string model;
		DataTable dtTasks;
		string currentTask;
		List<string> selectedParameters;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			GetIni();
			toolStripStatusLabelObject.Text = "0 Objects";
			toolStripStatusLabelTasks.Text = "0 Task History";
		}

		//**********************************************************************************************************
		//**                                        Initialization                                                **
		//**********************************************************************************************************

		private void GetIni()
		{
			Reader reader = new Reader();
			cells = reader.ReadIni();

			string snl = reader.GetSnLength();
			int number;
			bool result = Int32.TryParse(snl, out number);
			if (result)
			{
				snLength = number;
			}
			else
			{
				snLength = 7;
			}

			comboBoxCell.DataSource = cells;
			comboBoxCell.DisplayMember = "Name";
			comboBoxCell.ValueMember = "Location";
			comboBoxCell.DropDownStyle = ComboBoxStyle.DropDownList;

			dbDate();
		}

		//**********************************************************************************************************
		//**                                         Get DB file Date                                             **
		//**********************************************************************************************************

		private void dbDate()
		{
			string conString = Reader.conString;
			string conStringPath = conString.Substring(12, conString.Length - 12 - 11);

			Reader reader = new Reader();
			string db_date = reader.GrtDBDate(conStringPath);

			lblLastDBUpdate.Text = string.Format("Last DB update: {0}", db_date);
		}

		//**********************************************************************************************************
		//**                                Create SN string collection                                           **
		//**********************************************************************************************************

		private List<string> CreateSNCollection()
		{
			List<string> sn = new List<string>();

			//fsn is collection for all rows in 'sn' textbox
			string[] fsn = textBoxSN.Text.Split('\n');
			//clear texbox
			textBoxSN.Text = string.Empty;

			string snz = string.Empty;
			Regex rgx = new Regex(@"[^0-9]");

			//remove all not digital symb or white space from evry element of fsni
			foreach (string s in fsn)
			{
				string snt = s;
				snt = rgx.Replace(s, "");

				//remove leading zerros
				int number;
				bool result = Int32.TryParse(s, out number);
				if (result)
				{
					if (number != 0)
					{
						snt = number.ToString();
					}
					else
					{
						snt = string.Empty;
					}
				}

				//Add leading zerros
				snz = snt.PadLeft(snLength, '0');

				//if element is not empty
				if (snt != string.Empty)
				{
					//sn is list of serial numbers with zeros
					sn.Add(snz);

					//rewrite text in the textbox
					textBoxSN.AppendText(snt);
					textBoxSN.AppendText("\n");
				}
			}

			return sn;
		}

		//**********************************************************************************************************
		//**                                          Clear btn                                                   **
		//**********************************************************************************************************

		private void btnClear_Click(object sender, EventArgs e)
		{
			textBoxSN.Text = string.Empty;
			dataGridViewReport.DataSource = null;
			dataGridViewReport.Columns.Clear();
			dataGridViewReport.Refresh();

			dataGridViewTasks.DataSource = null;
			dataGridViewTasks.Columns.Clear();
			dataGridViewTasks.Refresh();

			dataGridViewShortReport.DataSource = null;
			dataGridViewShortReport.Columns.Clear();
			dataGridViewShortReport.Refresh();

			ClearDatagrids();

			groupBoxTasks.Text = "<0>";
			groupBoxTaskDetails.Text = "<0>";
			groupBoxCustomReport.Text = "<0>";
			toolStripStatusLabelObject.Text = "0 Objects";
			toolStripStatusLabelTasks.Text = "0 Task History";
		}

		//**********************************************************************************************************
		//**                                          Report btn                                                  **
		//**********************************************************************************************************

		private void btnGenerate_Click(object sender, EventArgs e)
		{

			var sn = CreateSNCollection();

			Sniffer sniffer = new Sniffer();

			try
			{
				sensors = sniffer.Sniff(sn, cells);
			}
			catch (ReadFileErrorException ex)
			{
				MessageBox.Show(ex.Message, "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			DateTime currentDate = DateTime.Now;
			toolStripStatusLabelObject.Text = string.Format("{0} {1}. Report generated on: {2}", sn.Count, sn.Count == 1 ? "Object" : "Objects", currentDate);

			dataGridViewReport.DataSource = sensors;

			dataGridViewReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dataGridViewReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
			dataGridViewReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 120, 120);
			dataGridViewReport.DefaultCellStyle.SelectionForeColor = Color.LightYellow;
			//dataGridViewReport.DefaultCellStyle.Font

			dataGridViewTasks.DataSource = null;
			dataGridViewTasks.Columns.Clear();
			dataGridViewTasks.Refresh();

			ClearDatagrids();
		}

		//**********************************************************************************************************
		//**                                 Double Click on GridView Report                                      **
		//**********************************************************************************************************

		private void dataGridViewReport_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGridViewReport.SelectedRows.Count > 0)
			{

				sn = dataGridViewReport.CurrentRow.Cells["Sn"].Value.ToString();
				model = dataGridViewReport.CurrentRow.Cells["Model"].Value.ToString();
				path = dataGridViewReport.CurrentRow.Cells["Location"].Value.ToString();

				tabControlHistory.SelectedTab = tabPage1;

				GetTaskHistory(sn, model, path);
			}
		}

		//**********************************************************************************************************
		//**                                  Get Task History for a sensor                                       **
		//**********************************************************************************************************

		private void GetTaskHistory(string sn, string model, string path)
		{
			int rows;
			Reader reader = new Reader();

			dtTasks = new DataTable();
			dtTasks = reader.ReadAllTasks(path, sn);

			if (dtTasks == null)
			{
				rows = 0;
				dataGridViewTasks.DataSource = null;
				dataGridViewTasks.Columns.Clear();
				dataGridViewTasks.Refresh();
			}
			else
			{
				ClearDatagrids();

				rows = dtTasks.Rows.Count;

				dataGridViewTasks.DataSource = null;
				dataGridViewTasks.Columns.Clear();
				dataGridViewTasks.Refresh();
				dataGridViewTasks.DataSource = dtTasks;
				dataGridViewTasks.Columns["SensorId"].Visible = false;
				dataGridViewTasks.Columns["SensorModel"].Visible = false;
				dataGridViewTasks.Columns.Add("Duration", "Duration");

				foreach (DataGridViewRow row in dataGridViewTasks.Rows)
				{

					if (row.Index < dataGridViewTasks.RowCount)
					{
						var startTime = DateTime.Parse(row.Cells["StartTime"].Value.ToString());
						var endTime = DateTime.Parse(row.Cells["EndTime"].Value.ToString());

						var span = (endTime - startTime).Duration();

						row.Cells["Duration"].Value = string.Format("{0:00}.{1:00}:{2:00}:{3:00}", span.Days, span.Hours, span.Minutes, span.Seconds);

						switch (row.Cells["Status"].Value.ToString())
						{
							case "1":
								row.Cells["Status"].Value = "Operation successful";
								break;
							case "0":
								row.Cells["Status"].Value = "General error";
								break;
							case "-988":
								row.Cells["Status"].Value = "Operation aborted";
								break;
							case "-993":
								row.Cells["Status"].Value = "Action timed out";
								break;
							case "-1000":
								row.Cells["Status"].Value = "Invalid state";
								break;
							default:
								row.Cells["Status"].Value = row.Cells["Status"].Value.ToString();
								break;
						}

						if (dataGridViewTasks.Columns.Contains("Result"))
						{

							switch (row.Cells["Result"].Value.ToString())
							{
								case "1":
									row.Cells["Result"].Value = "Passed";
									break;
								case "0":
									row.Cells["Result"].Value = "N/A";
									break;
								case "-1":
									row.Cells["Result"].Value = "Failed";
									break;
								default:
									row.Cells["Result"].Value = row.Cells["Result"].Value.ToString();
									break;
							}
						}
					}
				}

				dataGridViewTasks.Columns["Duration"].DisplayIndex = 8;
				dataGridViewTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				dataGridViewTasks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
				dataGridViewTasks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 120, 120);
				dataGridViewTasks.DefaultCellStyle.SelectionForeColor = Color.LightYellow;
			}

			string header = string.Format(sn + ", " + model + " Task History:");
			groupBoxTasks.Text = header;

			string stripStatus = string.Format(rows + " tasks in Task History");
			toolStripStatusLabelTasks.Text = stripStatus;
		}

		//**********************************************************************************************************
		//**                                         Clear datagrids                                              **
		//**********************************************************************************************************

		private void ClearDatagrids()
		{
			dataGridViewTaskDetails.DataSource = null;
			dataGridViewTaskDetails.Columns.Clear();
			dataGridViewTaskDetails.Refresh();

			dataGridViewCustomReport.DataSource = null;
			dataGridViewCustomReport.Columns.Clear();
			dataGridViewCustomReport.Refresh();

			dataGridViewFullCustomReport.DataSource = null;
			dataGridViewFullCustomReport.Columns.Clear();
			dataGridViewFullCustomReport.Refresh();
		}

		//**********************************************************************************************************
		//**                                  Click on toolStrip Info lable                                       **
		//**********************************************************************************************************

		private void toolStripStatusLabelInfo_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Validator 1.3. February 2018. By Alex Baliasnikau", "Validator Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		//**********************************************************************************************************
		//**                              Double Click on Task History gridView                                   **
		//**********************************************************************************************************

		private void dataGridViewTasks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGridViewTasks.SelectedRows.Count > 0)
			{

				int rowIndex = dataGridViewTasks.CurrentRow.Index;
				currentTask = dataGridViewTasks.CurrentRow.Cells["Type"].Value.ToString();

				DataTable dt = new DataTable();

				Reader reader = new Reader();

				try
				{
					dt = reader.ReadTaskDetails(path, sn, rowIndex);
				}
				catch (ReadFileErrorException ex)
				{
					MessageBox.Show(ex.Message, "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				dataGridViewTaskDetails.DataSource = dt;
				dataGridViewTaskDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				//dataGridViewTaskDetails.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
				dataGridViewTaskDetails.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 120, 120);
				dataGridViewTaskDetails.DefaultCellStyle.SelectionForeColor = Color.LightYellow;

				tabControlDetails.SelectedTab = tabPageTaskDetails;
				groupBoxTaskDetails.Text = string.Format("Task: {0}, Index: {1}", currentTask, rowIndex);
			}
		}

		//**********************************************************************************************************
		//**                                      Custom Report btn                                               **
		//**********************************************************************************************************

		private void btnCustomeReport_Click(object sender, EventArgs e)
		{
			if (dataGridViewTaskDetails.SelectedRows.Count > 0)
			{
				selectedParameters = new List<string>();

				foreach (DataGridViewRow row in dataGridViewTaskDetails.SelectedRows)
				{
					selectedParameters.Add(row.Cells["Parameter"].Value.ToString());
				}



				DataTable dt = new DataTable();
				Reader reader = new Reader();

				try
				{
					dt = reader.ReadCustomReport(path, sn, model, currentTask, selectedParameters);
				}
				catch (ReadFileErrorException ex)
				{
					MessageBox.Show(ex.Message, "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				//DataTable dtFinal = new DataTable();

				//IEnumerable<DataRow> results = (from MyRows in dtTasks.AsEnumerable()
				//								where
				//								MyRows.Field<string>("Type") == currentTask
				//								select MyRows);
				//dtFinal = results.CopyToDataTable();

				dataGridViewCustomReport.DataSource = dt;
				dataGridViewCustomReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				dataGridViewCustomReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
				dataGridViewCustomReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 120, 120);
				dataGridViewCustomReport.DefaultCellStyle.SelectionForeColor = Color.LightYellow;
				dataGridViewCustomReport.Columns["SensorId"].Visible = false;
				dataGridViewCustomReport.Columns["SensorModel"].Visible = false;

				tabControlDetails.SelectedTab = tabPageCustomReport;

				string message = string.Empty;
				StringBuilder par = new StringBuilder();

				if (selectedParameters.Count > 0)
				{
					foreach (string parameter in selectedParameters)
					{
						par.Append(string.Format("{0}; ", parameter));
					}
					message = string.Format("SN: {0}, Task: {1}, par: {2}", sn, currentTask, par);
				}

				groupBoxCustomReport.Text = message;
			}
		}

		//**********************************************************************************************************
		//**                                    Full Custom Report btn                                            **
		//**********************************************************************************************************

		private void btnFullCustomReport_Click(object sender, EventArgs e)
		{
			if (dataGridViewTaskDetails.SelectedRows.Count > 0)
			{
				selectedParameters = new List<string>();

				foreach (DataGridViewRow row in dataGridViewTaskDetails.SelectedRows)
				{
					selectedParameters.Add(row.Cells["Parameter"].Value.ToString());
				}
			}

				DataTable dt = new DataTable();
			Reader reader = new Reader();

			try
			{
				dt = reader.ReadFullCustomReport(sensors, currentTask, selectedParameters);
			}
			catch (ReadFileErrorException ex)
			{
				MessageBox.Show(ex.Message, "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			dataGridViewFullCustomReport.DataSource = dt;
			dataGridViewFullCustomReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dataGridViewFullCustomReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
			dataGridViewFullCustomReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 120, 120);
			dataGridViewFullCustomReport.DefaultCellStyle.SelectionForeColor = Color.LightYellow;

			foreach (DataGridViewRow row in dataGridViewFullCustomReport.Rows)
			{

				if (row.Index < dataGridViewFullCustomReport.RowCount)
				{
					//var startTime = DateTime.Parse(row.Cells["StartTime"].Value.ToString());
					//var endTime = DateTime.Parse(row.Cells["EndTime"].Value.ToString());

					//var span = (endTime - startTime).Duration();

					//row.Cells["Duration"].Value = string.Format("{0:00}.{1:00}:{2:00}:{3:00}", span.Days, span.Hours, span.Minutes, span.Seconds);

					switch (row.Cells["Status"].Value.ToString())
					{
						case "1":
							row.Cells["Status"].Value = "Operation successful";
							break;
						case "0":
							row.Cells["Status"].Value = "General error";
							break;
						case "-988":
							row.Cells["Status"].Value = "Operation aborted";
							break;
						case "-993":
							row.Cells["Status"].Value = "Action timed out";
							break;
						case "-1000":
							row.Cells["Status"].Value = "Invalid state";
							break;
						default:
							row.Cells["Status"].Value = row.Cells["Status"].Value.ToString();
							break;
					}

					if (dataGridViewTasks.Columns.Contains("Result"))
					{

						switch (row.Cells["Result"].Value.ToString())
						{
							case "1":
								row.Cells["Result"].Value = "Passed";
								break;
							case "0":
								row.Cells["Result"].Value = "N/A";
								break;
							case "-1":
								row.Cells["Result"].Value = "Failed";
								break;
							default:
								row.Cells["Result"].Value = row.Cells["Result"].Value.ToString();
								break;
						}
					}
				}
			}

			tabControlHistory.SelectedTab = tabPage2;
		}

		//**********************************************************************************************************
		//**                                      Load button click                                               **
		//**********************************************************************************************************

		private void btnLoad_Click(object sender, EventArgs e)
		{
			if (!lblLastDBUpdate.Text.Equals("No DB file"))
			{
				Cell cell = new Cell();
				cell = comboBoxCell.SelectedItem as Cell;

				dt = new DataTable();
				source = new BindingSource();

				DB_Adapter adapter = new DB_Adapter();

				dt = adapter.ReadLocation(cell.Index);

				source.DataSource = dt;
				string rows = source.List.Count.ToString();

				advancedDataGridView.DataSource = null;
				advancedDataGridView.Columns.Clear();
				advancedDataGridView.Refresh();
				advancedDataGridView.DataSource = source;
				advancedDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				advancedDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
				advancedDataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 120, 120);
				advancedDataGridView.DefaultCellStyle.SelectionForeColor = Color.LightYellow;
				advancedDataGridView.Columns["id"].Visible = false;
				advancedDataGridView.Columns["folder"].Visible = false;
				advancedDataGridView.Columns["opt1"].Visible = false;

				lblTotalRows.Text = string.Format("Total Rows: {0}", rows);
			}
		}

		//**********************************************************************************************************
		//**                                          Sort event                                                  **
		//**********************************************************************************************************

		private void advancedDataGridView_SortStringChanged(object sender, EventArgs e)
		{
			source.Sort = this.advancedDataGridView.SortString;	
		}

		//**********************************************************************************************************
		//**                                         Filter event                                                 **
		//**********************************************************************************************************

		private void advancedDataGridView_FilterStringChanged(object sender, EventArgs e)
		{
			source.Filter = this.advancedDataGridView.FilterString;
			lblTotalRows.Text = string.Format("Total Rows: {0}", source.List.Count.ToString());
		}

		//**********************************************************************************************************
		//**                                      Accept button click                                             **
		//**********************************************************************************************************

		private void btnAccept_Click_1(object sender, EventArgs e)
		{
			List<string> snCollection = new List<string>();

			foreach (DataGridViewRow row in advancedDataGridView.SelectedRows)
			{
				snCollection.Add(row.Cells["sn"].Value.ToString());
			}

			foreach (string sn in snCollection)
			{
				textBoxSN.AppendText(string.Format("{0}\r\n", sn));
			}
		}
	}
}
