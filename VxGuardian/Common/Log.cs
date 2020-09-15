using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VxGuardian.Common
{
	public class Log
	{

		private static string DateLog;

		public Log()
		{
			//Toma la fecha de la maquina al crear clase
			DateLog = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
		}


		public void CheckIfLogOnRemoteExist(string _dir, FtpClient _client)
		{
			if (!_client.DirectoryExists(_dir + "/" + "logs"))
			{
				_client.CreateDirectory(_dir + "/" + "logs");
			}
		}


		public void CleanLog(string dir)
		{
			if (File.Exists(dir + "\\" + "log.txt"))
			{
				File.WriteAllText(dir + "\\" + "log.txt", String.Empty);
			}
		}


		public void List(string ruta, string lines)
		{
			string path = ruta; // + "\\List\\";
			//VerifyDir(path);
			string fileName = "\\list.txt";

			try
			{
				var fileFull = path + fileName;

				if (!System.IO.File.Exists(fileFull))
				{
					System.IO.FileStream f = System.IO.File.Create(fileFull);
					f.Close();
				}

				FileStream file = WaitForFile(fileFull, FileMode.Append, FileAccess.Write, FileShare.Write);

				using (System.IO.StreamWriter sw = new StreamWriter(file))
				{
					sw.WriteLine(lines);
					sw.Close();
				}
			}
			catch (Exception) { }
		}







		public void Logg(string lines)
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\";
			VerifyDir(path);
			string fileName = "log.txt";

			try
			{
				var fileFull = path + fileName;

				if (!System.IO.File.Exists(fileFull))
				{
					System.IO.FileStream f = System.IO.File.Create(fileFull);
					f.Close();
				}

				FileStream file = WaitForFile(fileFull, FileMode.Append, FileAccess.Write, FileShare.Write);

				using (System.IO.StreamWriter sw = new StreamWriter(file))
				{
					sw.WriteLine(Environment.NewLine + GetDate() + " : " + lines);
					sw.Close();
				}
			}
			catch (Exception) { }
		}

		public void SaveLog(string lines)
		{
			new System.Threading.Thread(() =>
			{
				Logg(lines);
				//globalLog.Logger("\r\n \r\n No se pudo conectar con la base de datos local :" + DateTime.Now.ToString(), "error");
			}).Start();
		}

		public string GetDate()
		{
			return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
		}

		public void Logger(string lines, string file_name)
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\";
			VerifyDir(path);
			string fileName = file_name + " " + DateLog + ".txt";

			try
			{
				var fileFull = path + fileName;

				if (!System.IO.File.Exists(fileFull))
				{
					System.IO.FileStream f = System.IO.File.Create(fileFull);
					f.Close();
				}

				FileStream file = WaitForFile(fileFull, FileMode.Append, FileAccess.Write, FileShare.Write);

				using (System.IO.StreamWriter sw = new StreamWriter(file))
				{
					sw.WriteLine(lines);
					sw.Close();
				}
			}
			catch (Exception) { }
		}

		//Verificar directorio, si no existe lo crea.
		public static void VerifyDir(string path)
		{
			try
			{
				DirectoryInfo dir = new DirectoryInfo(path);
				if (!dir.Exists)
				{
					dir.Create();
				}
			}
			catch { }
		}

		public void checkIfLogExist(string dir)
		{
			if (!File.Exists(dir + "\\" + "log.txt"))
			{
				File.CreateText(dir + "\\" + "log.txt");
			}
		}

		//Funcion asincronica de escritura
		public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
		{
			for (int numTries = 0; numTries < 10; numTries++)
			{
				FileStream fs = null;
				try
				{
					fs = new FileStream(fullPath, mode, access, share);
					if (fs.CanWrite)
					{
						return fs;
					}
				}
				catch (IOException)
				{
					if (fs != null)
					{
						fs.Dispose();
					}
					Thread.Sleep(500);
				}
			}

			return null;
		}

		public void WriteOnLog(string dir, string text)
		{
			using (StreamWriter sw = new StreamWriter(dir + "\\" + "log.txt", true))
			{
				sw.WriteLine(text);
			}
		}
	}


}
