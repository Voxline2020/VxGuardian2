using FluentFTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VxGuardian.EtcClass
{
	public class Etc
	{
		public static void CreateLock(string _root)
		{

			if (!File.Exists(_root + "/" + "lock.txt"))
			{
				File.Create(_root + "/" + "lock.txt").Close();
			
			}
		}


	public static void DeleteLock(string _root)
		{
			if (File.Exists(_root + "/" + "lock.txt"))
			{
				File.Delete(_root + "/" + "lock.txt");
			}
		}

		public static bool CheckLock(string _root)
		{
			if (File.Exists(_root + "/" + "lock.txt"))
			{
				return true;
			}
			return false;
		}

		public static bool CheckRemoteLock(FtpClient _ftp ,string _path)
		{
			if (_ftp.FileExists(_path + "/lock.txt"))
				return true;
			return false;
		}


		public static Boolean CheckFieldsTBOX(TextBox _tbox)
		{
			if (_tbox.Text.Trim() == "")
			{
				return false;
			}
			
			return true;
		}

		public static Boolean CheckFieldsPBOX(PasswordBox _pbox)
		{
			if (_pbox.Password.Trim() == "")
			{
				return false;
			}
			return true;
		}

		public static void SelectAddress(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				tb.SelectAll();
			}
		}

		public static void SelectPassword(object sender, RoutedEventArgs e)
		{
			PasswordBox tb = (sender as PasswordBox);
			if (tb != null)
			{
				tb.SelectAll();
			}
		}

		public static void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
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

		public static bool CheckFileHasCopied(string FilePath)
		{
			try
			{
				if (File.Exists(FilePath))
					using (File.OpenRead(FilePath))
					{
						return true;
					}
				else
					return false;
			}
			catch (Exception)
			{
				System.Threading.Thread.Sleep(100);
				return CheckFileHasCopied(FilePath);
			}
		}

		public static bool CheckIfFileHasRemove(string FilePath)
		{
			try
			{
				if (File.Exists(FilePath))
					using (File.OpenRead(FilePath))
					{
						return false;
					}
				else
					return true;
			}
			catch (Exception)
			{
				System.Threading.Thread.Sleep(100);
				return CheckIfFileHasRemove(FilePath);
			}
		}


		public static void ClearDir(string path)
		{
			try
			{
				Directory.Delete(path, true);
				Directory.CreateDirectory(path);

				//DirectoryInfo directory = new DirectoryInfo(path);

				//foreach (FileInfo file in directory.EnumerateFiles())
				//{

				//	file.Delete();

				//}

				//foreach (DirectoryInfo dir in directory.EnumerateDirectories())
				//{
				//	dir.Delete(true);

				//}
			}
			catch(Exception ex) {

			}
		}

		public static void CreateVersion(string _folder, string _version)
		{

			if (!File.Exists(_folder + "/" + "v" + _version + ".txt"))
			{
				File.Create(_folder + "/" + "v" + _version + ".txt").Close();
			}
		}



		public static void DeleteVersion(string _folder, string _version)
		{
			if (File.Exists(_folder + "/" + "v" + _version + ".txt"))
			{
				File.Delete(_folder + "/" + "v" + _version + ".txt");
			}
		}

		public static bool IsVersion(string _fileName)
		{
			if (_fileName.Substring(0, 1) == "v")
			{
				return true;
			}
			return false;
		}

		public static void KillApp(string _dir)
		{
			string NameExe = _dir.Split('\\').Last().Split('.').First();
			Process[] pname = Process.GetProcessesByName(NameExe);
			if (pname.Length > 0)
			{
				foreach (Process process in pname)
				{
					process.Kill();
				}
				//&MessageBox.Show("Process Running");
			}
			else
			{
				//MessageBox.Show("Process Not running");
			}

		}

		public static void OpenApp(string _app)
		{
			string NameExe = _app.Split('\\').Last().Split('.').First();
			Process[] pname = Process.GetProcessesByName(NameExe);
			if (pname.Length <= 0)
			{
				Process.Start(@_app);
				//&MessageBox.Show("Process Running");
			}
			else
			{
				//MessageBox.Show("Process Not running");
			}

			//ClearDir(TemporalStorage);
		}


		public static void MoveDir(String path, string pathDes)
        {
            try
            {
				Directory.Move(path, pathDes); // Daniel
				Directory.Delete(path, true); // Daniel
			}
			catch
			{ }
        }


		//Verificar directorio, si no existe lo crea.
		public static void CreateDir(string path)
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

		public static bool isNumeric(string s)
		{
			return int.TryParse(s, out int n);
		}

		public static bool CheckDir(string path)
		{
			DirectoryInfo dir = new DirectoryInfo(path);
			if (dir.Exists)
			{
				return true;
			}
			return false;
		}


		public static bool CheckList(string fichero, int id)
        {
			//string fichero = "c:\prueba.txt";

			try
			{
				using (StreamReader lector = new StreamReader(fichero))
				{
					while (lector.Peek() > -1)
					{
						string linea = lector.ReadLine();
						if (!String.IsNullOrEmpty(linea))
						{
							Console.WriteLine(linea);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
			return false;
		}


		public static bool CheckEmptyFolder(string path)
		{
			if (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0)
				{
				return true;
				}
			return false;
		}


		public static double minsToMS(double num)
		{
			return num * 60000;
		}
		//cambio gonzalo
		public static void DeleteFiles(string path)
		{
			try
			{
				List<string> Files = Directory.GetFiles( path, "*", SearchOption.AllDirectories).ToList();

				foreach (string items in Files)
				{
					File.Delete(items);
				}
			}
			catch (Exception)
			{

				throw;
			}
			
		}

		public static bool CheckFile(string file)
        {
			if (File.Exists(file))
			{
				return true;
			}
			return false;

		}

	}
}
