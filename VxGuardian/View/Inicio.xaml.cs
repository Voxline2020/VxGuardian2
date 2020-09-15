using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VxGuardian.Common;
using VxGuardian.Models;

namespace VxGuardian.View
{
	/// <summary>
	/// Lógica de interacción para Window1.xaml
	/// </summary>
	public partial class Inicio : Window
	{
		ConfiguracionFTP configFTP;
		PivoteFTP pivotFTP;
		TaskbarIcon tbi;
		public BlackScreen bs;
		public Log gLog;

		public VoxContext db;
		public Root root;
		public Config config;

		public Inicio()
		{
			InitializeComponent();
			ConfigTbi();
			gLog = new Log();
			ConnectionDB();
			CheckInitStatus();
		}

		private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
		{
			CheckSiguiente();
		}

		private void CheckInitStatus()
		{
			switch (config.SelectedMode)
			{
				case "FTP":
					InitFTPHide();
					break;
				case "Pivote":
					InitPivoteFTPHide();
					break;
				default:
					break;
			}
		}

		private void CheckSiguiente()
		{
			if (RBtnFTP.IsChecked == true)
			{
				InitFTP();
			}
			else if (RBtnPivote.IsChecked == true)
			{
				InitPivoteFTP();
			}
		}

		private void ShowSelected()
		{
			switch (config.SelectedMode.ToString())
			{
				case "FTP":
					configFTP.StopTime();
					InitFTPRestore();
					break;
				case "Pivote":
					pivotFTP.StopTime();
					InitPivoteFTPRestore();
					break;
				default:
					this.Show();
					break;
			}
		}


		//inicializa
		private void InitFTP()
		{
			configFTP = new ConfiguracionFTP(this);
			this.Hide();
			configFTP.Show();
		}

		//restaura
		private void InitFTPRestore()
		{
			this.Hide();
			configFTP.Show();
		}

		//inicia a escondidas
		private void InitFTPHide()
		{
			configFTP = new ConfiguracionFTP(this);
			this.Hide();
			configFTP.Init();
		}

		//restaura
		private void InitPivoteFTPRestore()
		{
			this.Hide();
			pivotFTP.Show();
		}

		//inicializa
		private void InitPivoteFTP()
		{
			pivotFTP = new PivoteFTP(this);
			this.Hide();
			pivotFTP.Show();
		}

		//inicia a escondidas
		private void InitPivoteFTPHide()
		{
			pivotFTP = new PivoteFTP(this);
			this.Hide();
			pivotFTP.Init();
		}


		public void Minimize()
		{
			ShowTBI();
		}

		public void ConnectionDB()
		{			
			try
			{
				//db = new VoxContext();
				db = new VoxContext();
				root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(db.fileJsonDir));
				config = root.Config[0];
				gLog.SaveLog("--- Inicio VxGuardian --- ");
			}
			catch (Exception ex)
			{
				gLog.SaveLog("No se pudo conectar base de datos -- " + ex.Message);
				//new System.Threading.Thread(() =>
				//{
				//	globalLog.Logg("No se puedo conectar");
				//	//globalLog.Logger("\r\n \r\n No se pudo conectar con la base de datos local :" + DateTime.Now.ToString(), "error");
				//}).Start();
				//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
				//Environment.Exit(1);
				//throw;
			}
		}


		public void ConfigTbi()
		{
			tbi = new TaskbarIcon();
			tbi.Icon = Properties.Resources.favicon;
			tbi.TrayMouseDoubleClick += Tbi_TrayMouseDoubleClick;
			tbi.ToolTipText = "VxGuardian";

			HiddenTBI();
		}

		public void ShowTBI()
		{
			tbi.Visibility = Visibility.Visible;
		}

		public void HiddenTBI()
		{
			tbi.Visibility = Visibility.Hidden;
		}

		public void Restore()
		{
			this.Show();
			HiddenTBI();
		}

		private void Tbi_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			ShowSelected();
		}
	}
}
