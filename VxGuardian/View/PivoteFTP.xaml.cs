using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VxGuardian.Common;
using VxGuardian.Models;
using VxGuardian.EtcClass;
using System.Collections;

namespace VxGuardian.View
{
	/// <summary>
	/// Lógica de interacción para ConfiguracionFTP.xaml
	/// </summary>
	public partial class PivoteFTP : Window
	{
		private Inicio ini;
		public FtpClient ftpClient;
		public Timer time;
		public Log gLog;
		public BackgroundWorker worker;

		private VxCMSContext CMSdb;
		private RootCMS rootCMS;

		private string TemporalStorage;
		private BlackScreen bs;
		private Etc tools;
		bool Downloaded = false;
		bool initiated = false;

		public PivoteFTP()
		{
			InitializeComponent();
		}

		public PivoteFTP(Inicio _inicio)
		{
			ini = _inicio;
			InitializeComponent();

			gLog = new Log();
			tools = new Etc();
			//ConnectionDB()
			LoadInitialValuesINICIO();
		}

		public void ConnectionDBCMS(string _jsonFile)
		{
			try
			{
				//db = new VoxContext();
				CMSdb = new VxCMSContext();
				rootCMS = JsonConvert.DeserializeObject<RootCMS>(_jsonFile);
				gLog.SaveLog("Conexion DBCMS Correcta");

			}
			catch (Exception ex)
			{
				gLog.SaveLog("No se pudo conectar base de datos -- " + ex.Message);

				//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
				//Environment.Exit(1);
				//throw;
			}
		}

		public bool CheckErrorResponse(string _link)
		{
			try
			{
				var webClient = new WebClient();
				var jsonPure = webClient.DownloadString(_link);
				JObject json = JObject.Parse(jsonPure);
				if((string) json["error"] == "Not Found"){
					return true;
				}
				return false;


			}
			catch (Exception ex)
			{
				return false;
				//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
				//Environment.Exit(1);
				//throw;
			}
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
				TemporalStorage = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxLine\\" + ini.config.CodePc;
				Etc.CreateDir(TemporalStorage);
				Sync();
				InitTime();
				initiated = true;
			}
			else
			{
				MessageBox.Show("Los campos obligatorios no se han llenado.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//ini.Close();
			//ini.HiddenTBI();
			//Environment.Exit(0);

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

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{

			if(ini.config.Syncing == 0)
			{
				Sync();
			}
			//Application.Current.Dispatcher.Invoke(delegate
			//{
			//	//btn_sync.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			//});
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


		private void BtnConfig_Click(object sender, RoutedEventArgs e)
		{
			ini.Show();
			this.Close();
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				tb.SelectAll();
			}
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				if (!tb.IsKeyboardFocusWithin)
				{
					e.Handled = true;

					tb.Focus();
				}
			}
		}

		public static int DownloadFile(String remoteFilename, String localFilename)
		{
			// Function will return the number of bytes processed
			// to the caller. Initialize to 0 here.
			int bytesProcessed = 0;

			// Assign values to these objects here so that they can
			// be referenced in the finally block
			Stream remoteStream = null;
			Stream localStream = null;
			WebResponse response = null;

			// Use a try/catch/finally block as both the WebRequest and Stream
			// classes throw exceptions upon error
			try
			{
				// Create a request for the specified remote file name
				WebRequest request = WebRequest.Create(remoteFilename);
				if (request != null)
				{
					// Send the request to the server and retrieve the
					// WebResponse object 
					response = request.GetResponse();
					if (response != null)
					{
						// Once the WebResponse object has been retrieved,
						// get the stream object associated with the response's data
						remoteStream = response.GetResponseStream();

						// Create the local file
						localStream = File.Create(localFilename);

						// Allocate a 1k buffer
						byte[] buffer = new byte[1024];
						int bytesRead;

						// Simple do/while loop to read from stream until
						// no bytes are returned
						do
						{
							// Read data (up to 1k) from the stream
							bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

							// Write the data to the local file
							localStream.Write(buffer, 0, bytesRead);

							// Increment total bytes processed
							bytesProcessed += bytesRead;
						} while (bytesRead > 0);
					}
				}
			}
			catch (Exception ex)
			{
				Log glog = new Log();
				glog.SaveLog("No se logro descargar archivos -- " + ex.Message);
			}
			finally
			{
				// Close the response and streams objects here 
				// to make sure they're closed even if an exception
				// is thrown at some point
				if (response != null) response.Close();
				if (remoteStream != null) remoteStream.Close();
				if (localStream != null) localStream.Close();
			}

			// Return total bytes processed to caller.
			return bytesProcessed;
		}


		private void Sync()
		{
			var dictionary = new Dictionary<int, int>();
			//descargar json
			using (var webClient = new WebClient())
			{
				//string download link
				string downloadLink = ini.config.ModePivote[0].Servidor + "pivot/" + ini.config.CodePc + "/get/" + ini.config.ModePivote[0].Contrasena;
				try 
				{
					//leer string json
					var jsonPure = webClient.DownloadString(downloadLink);
					
					//generar db
					ConnectionDBCMS(jsonPure);

					syncingOn();

					//raiz folder
					string rootFolder = ini.config.CarpetaRaiz + rootCMS.Code;
					//crear raiz
					Etc.CreateDir(rootFolder);
					//------------------------------------------------------------------------------------
					//Daniel
					//Crear archivo Json					
					string json = JsonConvert.SerializeObject(rootCMS);

					//string path2 = @"C:\\FTP\\Voxline\\Pivotes\\35000" +  "/temp6.json";
					string path = rootFolder + "/PlayList.json";

					//Gustavo
					if(File.Exists(path))
					{
						File.Delete(path);
						File.WriteAllText(path, json);
					}else
					{
						System.IO.File.WriteAllText(path, json);
					}


					//------------------------------------------------------------------------------------
					

					//recorrer computador
					 foreach (var computer in rootCMS.Computers)
					{
						//computador string
						string computerFolder = rootFolder + "/" + computer.Code;
						// crear string computador
						Etc.CreateDir(computerFolder);

						//auxiliar
						int aux = 0;
						foreach (Screens screen in computer.Screens)
						{
							//crear carpeta pantalla
							string screenFolder = computerFolder + "/p" + screen.Code;
							string screenFolder_TMP = computerFolder + "/p" + screen.Code +"_TMP";


							//chequear si existe archivo lock
							if (!Etc.CheckLock(screenFolder_TMP))
							{
								//crear carpeta pantalla
								Etc.CreateDir(screenFolder);
								Etc.CreateDir(screenFolder_TMP);

								//crear localmente la variable de la pantalla
								ScreensGuardian _screen = new ScreensGuardian();
								//primero?
								bool first = false;
								
								
								_screen = ini.config.Screens.Find(e => e.Code == screen.Code.ToString());

								if (_screen == null)
								{
								 //Esto no se ocupa ? 
									_screen = new ScreensGuardian();
									_screen.Alto = screen.Height.ToString();
									_screen.Ancho = screen.Width.ToString();
									_screen.Code = screen.Code.ToString();
									_screen.Nombre = screen.Name;
									_screen.VersionActual = "0";
									//_screen.VersionActual = (screen.Version-1).ToString();//cambio gonzalo
									//string versionValidate = screenFolder + "/" + "v" + screen.Version.ToString() + ".txt";//cambio gonzalo
									//cambio gonzalo
									//if (File.Exists(versionValidate))
									//{
									//	_screen.VersionActual = screen.Version.ToString();
									//}
									//cambio gonzalo
									//if (Etc.CheckEmptyFolder(screenFolder))
									//{
									//	first = true;
									//}

									first = true;
									ini.config.Screens.Add(_screen);
									ini.db.Save(ini.config);

									Etc.ClearDir(screenFolder);
								}


								//si se asigno recientemente
								if (first)
								{
									Etc.ClearDir(screenFolder_TMP);
									Etc.CreateLock(screenFolder_TMP);
									//Etc.CreateLock(screenFolder);
									int auxI = 0;
									if(screen.Playlist != null)
									{
										foreach (var content in screen.Playlist)
										{
											try
											{
												

												if (!CheckErrorResponse(content.Download.ToString()))
												{
													 
													  //  string contentName = content.OriginalID + "-"+ content.defOrder + content.Name + "-" + auxI + ".mp4";
													
												
													string contentName = content.OriginalID + "-" + content.Name +".mp4";

													//Daniel
													//Comparo si el contenName existe en la carpeta de descarga si es asi no lo descargo.
													if (Etc.CheckFile(screenFolder + "/" + contentName))
													{
														File.Copy(screenFolder + "/" + contentName, screenFolder_TMP + "/" + contentName);
														gLog.SaveLog("First: Se copio  " + contentName + " en " + screenFolder_TMP);
													}
													else
													{ 
														Etc.CreateDir(screenFolder_TMP);  // Daniel
														DownloadFile(content.Download.ToString(), screenFolder_TMP + "/" + contentName);// Daniel
														//DownloadFile(content.Download.ToString(), screenFolder + "/" + contentName);
														gLog.SaveLog("First: Se descargo " + contentName + " en " + screenFolder_TMP);
														auxI++;
													}
													

												}
											}
											catch (Exception ex)
											{
												gLog.SaveLog("First: No se logro descargar archivos -- " + ex.Message);
												//throw;
											}
										}// End foreach

										//GUSTAVO 
										//Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel									
										//Etc.DeleteLock(screenFolder_TMP); // Daniel
										//Etc.MoveDir(screenFolder_TMP, screenFolder);
										Directory.Delete(screenFolder, true); // Daniel

									}
									else
									{
										gLog.SaveLog("First: Sin Contenido asignado");
										//Etc.DeleteLock(screenFolder_TMP);
										Directory.Delete(screenFolder_TMP, true);
										if (Etc.CheckDir(screenFolder))
										{
											Etc.DeleteFiles(screenFolder);
										}
									}

									/* CAIDA
									Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel
									Directory.Delete(screenFolder,true); // Daniel
								    Etc.DeleteLock(screenFolder_TMP); // Daniel
									Etc.MoveDir(screenFolder_TMP, screenFolder); */


									//Etc.CreateVersion(screenFolder, screen.Version.ToString());
									//Etc.DeleteLock(screenFolder);

									//Etc.DeleteVersion(screenFolder, _screen.VersionActual);
									//Etc.CreateVersion(screenFolder, screen.Version.ToString());


									_screen.VersionActual = screen.Version.ToString();
									ini.config.Screens[aux] = _screen;
									ini.db.Save(ini.config);
									Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel								
									Etc.DeleteLock(screenFolder_TMP); // Daniel
									Etc.MoveDir(screenFolder_TMP, screenFolder);

									first = false;
								}
								//si la version
								else if(screen.Version > Int32.Parse(_screen.VersionActual))
								{
									//gustavo
									_screen.VersionActual = screen.Version.ToString();
									////------------------------------------------------------------------------------------
									////Daniel
									////Crear archivo Json
									//string json = JsonConvert.SerializeObject(rootCMS);
									//string path = computerFolder + "/PlayList.json";
									//System.IO.File.WriteAllText(path, json);
									////------------------------------------------------------------------------------------
									gLog.SaveLog("Version Actual " + _screen.VersionActual + " -- Version Remota " + screen.Version);
									//Etc.DeleteFiles(screenFolder);//cambio gonzalo
									//Etc.ClearDir(screenFolder);
									//Etc.CreateLock(screenFolder_TMP);
									// Daniel
									Etc.ClearDir(screenFolder_TMP);
									Etc.CreateLock(screenFolder_TMP);
									//

									int auxI = 0;
									if(screen.Playlist != null)
									{
										foreach (var content  in screen.Playlist)
										 {
											try
											 {
												 if (!CheckErrorResponse(content.Download.ToString()))
												 {
													//string contentName = content.defOrder + content.Name + "-" + auxI + ".mp4";
													//Daniel  
													string contentName = content.OriginalID + "-" + content.Name + ".mp4";
													//Daniel


													if (Etc.CheckFile(screenFolder + "/" + contentName))
													 {
														 File.Copy(screenFolder + "/" + contentName, screenFolder_TMP + "/" + contentName);
													}
													else
													{
														DownloadFile(content.Download.ToString(), screenFolder_TMP + "/" + contentName);
														gLog.SaveLog("Se descargo " + contentName + " en " + screenFolder);
														auxI++;
													}


												}
											}
											catch (Exception ex)
											{
												gLog.SaveLog("No se logro descargar archivos -- " + ex.Message);
												//throw;
											}
										}//end foreach
										Directory.Delete(screenFolder, true); // Daniel
									}
									else
									{
										gLog.SaveLog("Sin Contenido asignado");
										//Daniel

										Etc.DeleteLock(screenFolder_TMP);
										Directory.Delete(screenFolder_TMP, true);
										if(Etc.CheckDir(screenFolder))
                                        {
											Etc.DeleteFiles(screenFolder);
                                        }
										//-------------------
									}


									//Etc.DeleteVersion(screenFolder, _screen.VersionActual);
									//Etc.CreateVersion(screenFolder, screen.Version.ToString());
									_screen.VersionActual = screen.Version.ToString();
									ini.config.Screens[aux] = _screen;
									ini.db.Save(ini.config);

									//GUSTAVO
									Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel								
									Etc.DeleteLock(screenFolder_TMP); // Daniel
									Etc.MoveDir(screenFolder_TMP, screenFolder);
									///////////////////


									//Etc.DeleteLock(screenFolder);
								}

							}
							if (Etc.CheckDir(screenFolder_TMP))
							{
								Directory.Delete(screenFolder_TMP, true); // Daniel
							}
							aux++;
						}

						syncingOff();
					}
				}
				catch (Exception ex)
				{
					gLog.SaveLog("No se logro sincronizar archivos -- " + ex.Message);

					//throw;
				}



			}
		}



		public void LoadInitialValuesINICIO()
		{
			TxtCodigo.Text = ini.config.CodePc;
			TxtRaizSel.Text = ini.config.CarpetaRaiz;
			TxtChequeo.Text = ini.config.TiempoChequeo;
			TxtCodigoMaestro.Password = ini.config.ModePivote[0].Contrasena;
			TxtServidor.Text = ini.config.ModePivote[0].Servidor;
		}

		public Boolean CheckFieldsINICIAR()
		{
			if (
					!Etc.CheckFieldsTBOX(TxtCodigo) ||
					!Etc.CheckFieldsTBOX(TxtRaizSel) ||
					!Etc.CheckFieldsTBOX(TxtChequeo) ||
					!Etc.CheckFieldsTBOX(TxtServidor) ||
					!Etc.CheckFieldsPBOX(TxtCodigoMaestro)
				)
			{
				return false;
			}
			return true;
		}

		public void SaveNewConfig()
		{
			ini.config.CodePc = TxtCodigo.Text.Trim();
			ini.config.CarpetaRaiz = TxtRaizSel.Text.Trim();
			ini.config.TiempoChequeo = TxtChequeo.Text.Trim();
			ini.config.ModePivote[0].Servidor = TxtServidor.Text.Trim();
			ini.config.ModePivote[0].Contrasena = TxtCodigoMaestro.Password.Trim();
			ini.config.SelectedMode = "Pivote";
			ini.db.Save(ini.config);
		}

		// Event to track the progress
		void wc_progressBar(object sender, DownloadProgressChangedEventArgs e)
		{
			//progressBar.Value = e.ProgressPercentage;
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

		private void BtnCerrar_Click(object sender, RoutedEventArgs e)
		{
			ini.HiddenTBI();
			Environment.Exit(0);
		}
	}
}
