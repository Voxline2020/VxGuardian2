using FluentFTP;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VxGuardian.Common;
using VxGuardian.Models;
using VxGuardian.EtcClass;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VxGuardian.View
{
	/// <summary>
	/// Lógica de interacción para ConfiguracionFTP.xaml
	/// </summary>
	public partial class ConfiguracionFTP : Window
	{
		private Inicio ini;
		public FtpClient ftpClient;
		public Timer time;
		public Log gLog;
		public BackgroundWorker worker;
		private string TemporalStorage;
		private string newVersion;
		private BlackScreen bs;
		private double ProgressValue;
		bool Downloaded = false;
		bool initiated = false;
		private Etc tools;


		public ConfiguracionFTP(Inicio _inicio)
		{
			ini = _inicio;
			gLog = new Log();
			time = new Timer();



			tools = new Etc();
			InitializeComponent();

			LoadInitialValuesINICIO();
			gLog.SaveLog("--- Modo FTP ---");

		}

		private void BtnIniciar_Click(object sender, RoutedEventArgs e)
		{
			Init();
		}

		public void Init()
		{
			if (CheckFieldsINICIAR())
			{
				this.Hide();
				ini.Minimize();
				SaveNewConfig();
				syncingOff();
				TemporalStorage = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxLine\\" + ini.config.CodePc;
				Etc.CreateDir(TemporalStorage);
				CreateFTP();
				initiated = true;
				if (CheckConexionFTP())
				{
					SyncAsync(ini.config.CodePc);
					if (!time.Enabled)
					{
						InitTime();
					}
				}

				else
				{
				}
			}
			else
			{
				MessageBox.Show("Los campos obligatorios no se han llenado.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void SaveNewConfig()
		{
			ini.config.CodePc = TxtCodigo.Text.Trim();
			ini.config.CarpetaRaiz = TxtRaizSel.Text.Trim();
			ini.config.TiempoChequeo = TxtChequeo.Text.Trim();
			ini.config.Reproductor = TxtReproductor.Text.Trim();
			ini.config.ModeFtp[0].IpFtp = TxtIPFTP.Text.Trim();
			ini.config.ModeFtp[0].Puerto = TxtPuerto.Text.Trim();
			ini.config.ModeFtp[0].Usuario = TxtUsuario.Text.Trim();
			ini.config.ModeFtp[0].Contrasena = TxtContrasena.Password.Trim();
			ini.config.SelectedMode = "FTP";
			ini.db.Save(ini.config);
		}

		//public void ConnectionDB_OLD()
		//{
		//	try
		//	{
		//		//db = new VoxContext();
		//		db = new VoxContext();
		//		root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(ini.db.fileJsonDir));
		//		config = root.Config[0];
		//		new System.Threading.Thread(() =>
		//		{
		//			globalLog.Logger("\r\n \r\n ------ Inicio MODO FTP", config.CodePc.ToString().Replace(" ", ""));
		//		}).Start();
		//	}
		//	catch (Exception e)
		//	{
		//		new System.Threading.Thread(() =>
		//		{
		//			globalLog.Logger("\r\n \r\n No se pudo conectar con la base de datos local", "DB");
		//		}).Start();
		//		//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
		//		Environment.Exit(1);
		//		//throw;
		//	}
		//}

		public void LoadInitialValuesINICIO()
		{
			TxtCodigo.Text = ini.config.CodePc;
			TxtRaizSel.Text = ini.config.CarpetaRaiz;
			TxtChequeo.Text = ini.config.TiempoChequeo;
			TxtReproductor.Text = ini.config.Reproductor;
			TxtIPFTP.Text = ini.config.ModeFtp[0].IpFtp;
			TxtPuerto.Text = ini.config.ModeFtp[0].Puerto;
			TxtUsuario.Text = ini.config.ModeFtp[0].Usuario;
			TxtContrasena.Password = ini.config.ModeFtp[0].Contrasena;
		}



		public Boolean CheckFieldsINICIAR()
		{
			if (
					!Etc.CheckFieldsTBOX(TxtCodigo) ||
					!Etc.CheckFieldsTBOX(TxtRaizSel) ||
					!Etc.CheckFieldsTBOX(TxtChequeo) ||
					!Etc.CheckFieldsTBOX(TxtIPFTP) ||
					!Etc.CheckFieldsTBOX(TxtPuerto) ||
					!Etc.CheckFieldsTBOX(TxtUsuario) ||
					!Etc.CheckFieldsPBOX(TxtContrasena)
				)
			{
				return false;
			}
			return true;
		}

		public void CreateFTP()
		{
			//Datos
			int port = Int32.Parse(ini.config.ModeFtp[0].Puerto);
			string ip = ini.config.ModeFtp[0].IpFtp;
			string user = ini.config.ModeFtp[0].Usuario;
			string password = ini.config.ModeFtp[0].Contrasena;

			// create an FTP

			try
			{
				ftpClient = new FtpClient(ip, user, password);
				ftpClient.Port = port;
				ftpClient.EncryptionMode = FtpEncryptionMode.Explicit;
				ftpClient.SslProtocols = SslProtocols.Default;
				ftpClient.ValidateCertificate += new FtpSslValidation(delegate (FtpClient c, FtpSslValidationEventArgs e)
				{
					e.Accept = true;
				});
				// cambio gonzalo
				ftpClient.DataConnectionType = FtpDataConnectionType.PASV;
				ftpClient.ReadTimeout = 5000;
				ftpClient.RetryAttempts = 3;
			}
			catch (Exception ex)
			{
				gLog.SaveLog(ex.Message);
			}

		}

		public void CloseConnection()
		{
			if (ftpClient.IsConnected)
			{
				ftpClient.Disconnect();
			}

		}

		public Boolean CheckConexionFTP()
		{
			if (!ftpClient.IsConnected)
			{
				try
				{
					ftpClient.Connect();
					//gLog.SaveLog("Conectado al servidor FTP");
				}
				catch (Exception ex)
				{
					gLog.SaveLog("No se logro conexion con el servidor ftp -- " + ex.Message);
					//Console.WriteLine(ex.ToString());
					return false;
				}
			}
			return true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//if (ftpClient.IsConnected){
			//	DisconnectFTP();
			//}
			//Environment.Exit(0);
		}

		private void BtnConfig_Click(object sender, RoutedEventArgs e)
		{
			ini.Show();
			this.Hide();
		}

		private void DisconnectFTP()
		{
			ftpClient.Disconnect();
		}


		public void createRemoteFoder(string dir, FtpClient _ftpclient)
		{
			if (!_ftpclient.DirectoryExists(dir))
			{
				_ftpclient.CreateDirectory(dir);
			}
		}

		public bool checkRemoteFolder(string dir, FtpClient _ftpclient)
		{
			if (!_ftpclient.DirectoryExists(dir))
			{
				return false;
			}
			return true;
		}


		private void InitTime()
		{
			double interval = Double.Parse(ini.config.TiempoChequeo);
			time = new Timer(Etc.minsToMS(interval));
			time.Elapsed += Timer_Elapsed;
			time.AutoReset = true;
			time.Start();
		}

		public void StopTime()
		{
			time.Stop();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if(ini.config.Syncing != 0)
			{
				SyncAsync(ini.config.CodePc);
			}
			//Application.Current.Dispatcher.Invoke(delegate
			//{
			//	//btn_sync.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			//});
		}



		//Verificar directorio, si no existe lo crea.


		public void SyncAsync(string _remotePath)
		{
			string Path = _remotePath;
			int Qty = 0;

			if (checkRemoteFolder(Path, ftpClient))
			{
				try
				{
					DownloadFilesAsync(ftpClient, Path);
					CloseConnection();
					gLog.SaveLog("Se sincronizo archivos");
				}
				catch (Exception ex)
				{
					gLog.SaveLog("No se logro sincronizar --" + ex.Message);
					//throw;
				}

				//si hubo descargas
				//Falta descargar JSON
				if (Downloaded)
				{
					try
					{
						//CreateBS();
						Etc.KillApp(ini.config.Reproductor); //Cierra el reproductor 
						System.Threading.Thread.Sleep(3000);
						//CopyTemporalToDirAsync(TemporalStorage, ini.config.CarpetaRaiz); //Copia el directorio temporal a la carpeta raiz


						//GUSTAVO
						CopyTemporalToDirAsync2(TemporalStorage, ini.config.CarpetaRaiz); //Copia el directorio temporal a la carpeta raiz 


						//OpenApp(ini.config.Reproductor);
						//CloseBS();
						Downloaded = false;
						syncingOff();
					}
					catch (Exception ex)
					{
						gLog.SaveLog(ex.Message);
						//Debug.WriteLine(ex.Message);
						//throw;
					}

					//gustavo copya el json a la carpeta final antes de borrar la temporal
					try
					{
						CopyJsonPlayList(TemporalStorage, ini.config.CarpetaRaiz);//Copia el json PLayList del directorio temporal a la carpeta raiz
					}
					catch (Exception ex)
					{

						gLog.SaveLog("ERROR COPY PlayList.json " + ex.Message);
					}
					//cambio gonzalo
					try
					{
						Etc.DeleteFiles(TemporalStorage);
					}
					catch (Exception ex)
					{

						gLog.SaveLog("ERROR CLEAR TEMPORAL " + ex.Message);
					}
				}

				Etc.OpenApp(ini.config.Reproductor);
			}
			else
			{
			}
		}

		private void CreateBS()
		{
			System.Threading.Thread _thread = new System.Threading.Thread(() =>
			{
				try
				{
					bs = new BlackScreen();
					bs.Show();
				}
				catch
				{ }
			});
			_thread.Start();

		}

		private void CloseBS()
		{
			System.Threading.Thread _thread = new System.Threading.Thread(() =>
			{
				try
				{
					bs = new BlackScreen();
					bs.Close();
				}
				catch
				{ }
			});
			_thread.Start();
		}

		void syncingOn()
		{
			ini.config.Syncing = 1;
			ini.db.Save(ini.config);
		}

		void syncingOff()
		{
			ini.config.Syncing = 0;
			ini.db.Save(ini.config);
		}

		//Gustavo
		private void CopyTemporalToDirAsync2(string _temporalFolder, string _destinyFolder)
		{
			//Borrar directiorio actual
			if(Directory.Exists(_destinyFolder))
			{
				Etc.DeleteFiles(_destinyFolder);
			}
			
			try
			{
				if (Directory.Exists(_temporalFolder))
				{
					//comprueba que exista el json 
					if (File.Exists(_temporalFolder + "\\" + "PlayList.json"))
					{
						//Crear directorio definitivo

						// leer json desde un archivo
						StreamReader file = File.OpenText(_temporalFolder + "\\" + "PlayList.json");
						JsonTextReader reader = new JsonTextReader(file);
						
							//json object para trabajar
							JObject jsondata = (JObject)JToken.ReadFrom(reader);
							
							var computers = jsondata["computers"];
							
							//recorre la lista de computadores en el JSON
							foreach (var computer in computers)
							{
								//No se crea una carpeta por computador SOLO PANTALLAS

								//Recorre la lista de pantallas por computador en el JSON
								foreach (var screen in computer["screens"])
								{
									//nombre de la carpetas
									string screen_folder_name = "p" + screen["code"];
									//crea el directorio definitivo
									Directory.CreateDirectory(_destinyFolder + "\\" + screen_folder_name);
									//extrae la version de la pantalla
									var screen_version = screen["version"];
									//url del archivo de version temporal
									string version_temp_path = _temporalFolder + "\\" + screen_folder_name + "\\" +"v"+ screen_version + ".txt";
									//url del archivo de version definitivo
									string version_destiny_path = _destinyFolder + "\\" + screen_folder_name + "\\" + "v" + screen_version + ".txt";
									//comprueba si existe el archivo de version temporal
									if (File.Exists(version_temp_path))
									{
										//copia el archivo de version temporal a la carpeta definitiva
										File.Copy(version_temp_path, version_destiny_path);
										
										//recorre los contenido de la playlist en el json 
										foreach (var content in screen["playlist"])
										{
											//nombre del contenido
											string content_name = content["name"].ToString();
											//id del contenido
											string content_original_id = content["originalID"].ToString();
											//posicion del contenido
											string content_position = content["defOrder"].ToString();

										//url del contenido temporal id-nombre-nombre.mp4
										string content_temp_path = _temporalFolder + "\\" + screen_folder_name + "\\" + content_original_id + "-" + content_name + ".mp4";
											//url del contenido definitivo orden-id-nombre.mp4
											string content_destiny_path = _destinyFolder + "\\" + screen_folder_name + "\\" + content_position + "-" + content_original_id + "-" + content_name + ".mp4";
											
											//Comprueba si existe el contenido temporal
											if (File.Exists(content_temp_path))
											{
												//Copia el contenido temporal a la carpeta definitiva
												File.Copy(content_temp_path, content_destiny_path);
											}
										}//end foreach playlist

									}else
									{
										gLog.SaveLog("No se encontro el archvio (Version).txt");
								}
									
								}//end foreach screens
							}//end foreach computers
						


					}else
					{
						gLog.SaveLog("PlayList.Json  no existe en la carpeta temporal");
					}

				}
				else
				{
					Console.WriteLine("Source path does not exist!");
				}
			}
			catch (Exception ex)
			{
				gLog.SaveLog("No se logro copiar archivos -- " + ex.Message);
				//throw;
			}
		}

		private void CopyTemporalToDirAsync(string _temporalFolder, string _destinyFolder)
		{
			try
			{
				if (Directory.Exists(_temporalFolder))
				{
					Copy(_temporalFolder, _destinyFolder, ini, ftpClient);
				}
				else
				{
					Console.WriteLine("Source path does not exist!");
				}
			}
			catch (Exception ex)
			{
				gLog.SaveLog("No se logro copiar archivos -- " + ex.Message);
				//throw;
			}
		}

		public static void Copy(string sourceDirectory, string targetDirectory, Inicio _ini, FtpClient ftpClient)
		{
			DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
			DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

			CopyAll(diSource, diTarget, _ini, ftpClient, null);
		}

		//GUSTAVO metodo para copiar el json de la carpeta temporal a la carpeta final
		public static void CopyJsonPlayList(string source, string target )
		{
			//Copiar PlayList
			//var exist_json = File.Exists(target.FullName + "\\PlayList.json");


			if (File.Exists(target + "\\PlayList.json"))
			{
				//SI el archivo existe
				//Borra el archivo 
				File.Delete(target + "\\PlayList.json");
				//Copia el archivo
				File.Copy(source + "\\PlayList.json", target + "\\PlayList.json");
			}
			else
			{
				//si no existe copia el
				File.Copy(source + "\\PlayList.json", target + "\\PlayList.json");
			}
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, Inicio _ini, FtpClient ftp, ScreensGuardian _screen = null)
		{

			ScreensGuardian screenAUX = _screen;

			var _screens = _ini.config.Screens.ToArray();

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				foreach (ScreensGuardian screen in _screens)
				{
					if ("p" + screen.Code == diSourceSubDir.Name)
					{
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
						CopyAll(diSourceSubDir, nextTargetSubDir, _ini, ftp, screen);
						Log TgLog = new Log();
						TgLog.SaveLog("DIRECTORIO COPIADO " + diSourceSubDir.Name);
					}
				}
			}

			{
				Directory.CreateDirectory(screenAUX.LocalPath);
				//cambio gonzalo
				try
				{
					Etc.DeleteFiles(screenAUX.LocalPath);
					System.Threading.Thread.Sleep(2000);
				}
				catch (Exception ex)
				{
					Log logaux = new Log();
					logaux.SaveLog("ERROR CLEAR " + ex.Message);
				}

				// Copy each file into the new directory.
				foreach (FileInfo fi in source.GetFiles())
				{
					//Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
					
					fi.CopyTo(Path.Combine(screenAUX.LocalPath, fi.Name), true);
					Log TgLog = new Log();
					TgLog.SaveLog("ARCHIVO COPIADO - " + Path.Combine(screenAUX.LocalPath, fi.Name));
				}

				int idx = _ini.config.Screens.FindIndex(s => s.Code == screenAUX.Code);
				//guardar version nueva
				screenAUX.VersionActual = screenAUX.VersionRemota.ToString();
				_ini.config.Screens[idx] = screenAUX;
				_ini.db.Save(_ini.config);
			}



		}

		private void OpenApp(string _dir)
		{
			Process proc = new Process();
			proc.StartInfo.FileName = @_dir;
			proc.Start();
			System.Threading.Thread.Sleep(5000);
			//Process.Start(@_dir);
		}

		//DESCARGA
		private void DownloadFilesAsync(FtpClient _ftpclient, string _remotePath)
		{
			string Path = _remotePath;
			string TemporalLocalFolder = TemporalStorage;
			Downloaded = false;



			//GUSTAVO 			
			//string root_temp_adress = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxLine\\PlayList.json";
			//Descargae el json a la carpeta raiz
			//JSON var directorio = _ftpclient.GetListing("PlayList.json");
			//var directorio = _ftpclient.GetListing(Path + "\\p859");
			//var pantalla = directorio[0];
			ftpClient.DownloadFile(TemporalLocalFolder+ "\\PLayList.json", "PlayList.json", FtpLocalExists.Overwrite, FtpVerify.Retry);

			//

				Etc.CreateDir(TemporalLocalFolder);

			int auxI = 0;

			//ini.config.Screens.Clear();
			//getScreens
			foreach (FtpListItem item in _ftpclient.GetListing(Path).OrderByDescending(item => item.Name))
			{
				//if the folder is the player folder, enter and download
				if (item.Type == FtpFileSystemObjectType.Directory && item.Name.Substring(0, 1) == "p")
				{
					string code = item.Name.Substring(1, item.Name.Length - 1);
					if(ini.config.Screens.Exists(x => x.Code == code))
					{
						int idx = ini.config.Screens.FindIndex(x => x.Code == code);
						ini.config.Screens[idx].Nombre = item.Name;
						ini.config.Screens[idx].Path = item.FullName;
						ini.config.Screens[idx].Code = code;
						ini.config.Screens[idx].LocalPath = ini.config.CarpetaRaiz + "\\p" + code;
					}
					else
					{
						ScreensGuardian _screen = new ScreensGuardian();
						 _screen.Nombre = item.Name;
						_screen.Path = item.FullName;
						_screen.Code = code;
						_screen.LocalPath = ini.config.CarpetaRaiz + "\\p" + code;
						_screen.VersionActual = "0";
						ini.config.Screens.Add(_screen);

					}

					ini.db.Save(ini.config);
					auxI++;
				}
			}

			var _screens = ini.config.Screens.ToArray();

			int aux = 0;
			foreach (ScreensGuardian screen in _screens)
			{
				int _versionRemota = GetRemoteVersion(_ftpclient, screen.Path);
				screen.VersionRemota = _versionRemota;
				ini.config.Screens[aux] = screen;
				ini.db.Save(ini.config);

				//----------------------------------------------------------
				//
				
				
				if (!Etc.CheckDir(screen.LocalPath) || screen.VersionRemota > Int32.Parse(screen.VersionActual))
				{
					string ScreenTemporal = TemporalLocalFolder + '\\' + screen.Nombre;
					Etc.CreateDir(ScreenTemporal);
					try
					{
						Etc.ClearDir(ScreenTemporal);
					}
					catch (Exception ex)
					{

						gLog.SaveLog("ERROR CLEAR DOWNLOAD " + ex.Message);
					}
					
					//Descarga los dentro de la carpeta correspondiendte a la pantalla en el ftp 
					foreach (FtpListItem item in _ftpclient.GetListing(screen.Path).OrderByDescending(item => item.Name))
					{
						 if (item.Type == FtpFileSystemObjectType.File)
						  {
							  string downloadFileName = ScreenTemporal + "\\" + item.Name;
							FileInfo f = new FileInfo(downloadFileName);
							try
							 {
								//GUSTAVO
								string rutaarchivoftp = screen.LocalPath + "\\" + item.Name; //Ruta del archivo en la carpeta definitiva		
								var directorio = Directory.Exists(rutaarchivoftp);

								if (Directory.Exists(screen.LocalPath))
								{
									if (File.Exists(rutaarchivoftp))
									{										
										Etc.CopyFile(rutaarchivoftp , ScreenTemporal + "\\" + item.Name);
										gLog.SaveLog("COPIADO " + item.Name);
										Downloaded = true;
									}
									else
									{
										if (ftpClient.DownloadFile(downloadFileName, item.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry))
										{
											Downloaded = true;
											gLog.SaveLog("DESCARGADO " + item.Name);
										}
										else
										{
											gLog.SaveLog("ERROR EN " + item.Name);
										}
										
									}

								}else
								{
									if (ftpClient.DownloadFile(downloadFileName, item.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry))
									{
										Downloaded = true;
										gLog.SaveLog("DESCARGADO " + item.Name);
									}
									else
									{
										gLog.SaveLog("ERROR EN " + item.Name);
									}
								}
								
								
								/* ANTES
								    if (ftpClient.DownloadFile(downloadFileName, item.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry))
								 {
									 Downloaded = true;
									 gLog.SaveLog("DESCARGADO " + item.Name);
								 }
								else
								{
									gLog.SaveLog("ERROR EN " + item.Name);
								}*/

							}
							catch (Exception ex)
							{
								gLog.SaveLog(ex.Message);
								//throw;
							}
						}
					  }


				}
				aux++;

			}

			CloseConnection();


		}

		private void SelRaiz_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			dialog.RootFolder = Environment.SpecialFolder.Desktop;
			if (dialog.ShowDialog(this).GetValueOrDefault())
			{
				TxtRaizSel.Text = dialog.SelectedPath;
			}
		}

		private void SelReproductor_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
			{
				AddExtension = true,
				Filter = "VxPlayer.exe|*.exe|VxPlayer.ink|*.ink",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
			};

			if (dialog.ShowDialog(this).GetValueOrDefault())
			{
				TxtReproductor.Text = dialog.FileName;
			}
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			Etc.SelectAddress(sender, e);
		}

		private void SelectPassword(object sender, RoutedEventArgs e)
		{
			Etc.SelectAddress(sender, e);
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			Etc.SelectivelyIgnoreMouseButton(sender, e);
		}




		public void DeleteAllVersions(string _folderScreen)
		{
			var filesOnScreenFolder = Directory.GetFiles(_folderScreen).OrderByDescending(f => f);

			foreach (var file in filesOnScreenFolder)
			{
				string fileNew = file.Split('\\').Last().ToString();
				if (fileNew.Substring(0, 1) == "v")
				{
					File.Delete(file);
				}
			}
		}

		public int GetVersion(string _folderScreen)
		{
			var filesOnScreenFolder = Directory.GetFiles(_folderScreen).OrderByDescending(f => f);

			try
			{
				foreach (var file in filesOnScreenFolder)
				{
					string fileNew = file.Split('\\').Last().ToString();
					if (fileNew.Substring(0, 1) == "v")
					{
						return Int32.Parse(fileNew.Substring(1, fileNew.Length - 1));
					}
				}
			}
			catch (Exception)
			{
				return 0;
				//throw;
			}

			return 0;


		}

		public static int GetRemoteVersion(FtpClient _ftpclient, string _path)
		{
			try
			{
				if (_ftpclient.DirectoryExists(_path))
				{
					foreach (FtpListItem item in _ftpclient.GetListing(_path).OrderByDescending(item => item.Name))
					{
						if (item.Type == FtpFileSystemObjectType.File && item.Name.Substring(0, 1) == "v")
						{
							string nombreFinal = item.Name.Split('.').First();
							int lengthFinal = nombreFinal.Length - 1;
							int NumeroFinal = Int32.Parse(nombreFinal.Substring(1, lengthFinal));
							return NumeroFinal;

						}
					}
					return 0;
				}
			}
			catch (Exception ex)
			{

				return 0;
			}

			return 0;
		}

		private void BtnCerrar_Click(object sender, RoutedEventArgs e)
		{
			ini.HiddenTBI();
			Environment.Exit(0);
		}

		private void TxtContrasena_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{

		}
	}
}