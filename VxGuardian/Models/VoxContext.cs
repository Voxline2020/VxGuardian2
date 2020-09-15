using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace VxGuardian.Models
{


	public partial class Root
	{
		[JsonProperty("config", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public List<Config> Config { get; set; }
	}

	public partial class Config
	{
		[JsonProperty("serialID", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string SerialId { get; set; }

		[JsonProperty("serialKey", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string SerialKey { get; set; }

		[JsonProperty("selectedMode", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string SelectedMode { get; set; }

		[JsonProperty("codePC", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string CodePc { get; set; }

		[JsonProperty("carpetaRaiz", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string CarpetaRaiz { get; set; }

		[JsonProperty("reproductor", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Reproductor { get; set; }

		[JsonProperty("syncing", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public int Syncing { get; set; }

		[JsonProperty("tiempoChequeo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string TiempoChequeo { get; set; }

		[JsonProperty("modeVxCMS", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public List<Mode> ModeVxCms { get; set; }

		[JsonProperty("modeLocal", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public List<ModeLocal> ModeLocal { get; set; }

		[JsonProperty("modeFTP", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public List<ModeFtp> ModeFtp { get; set; }

		[JsonProperty("modePivote", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public List<Mode> ModePivote { get; set; }

		[JsonProperty("screens", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public List<ScreensGuardian> Screens { get; set; }
	}

	public partial class ModeFtp
	{
		[JsonProperty("ipFTP", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string IpFtp { get; set; }

		[JsonProperty("puerto", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Puerto { get; set; }

		[JsonProperty("usuario", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Usuario { get; set; }

		[JsonProperty("contrasena", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Contrasena { get; set; }

	}

	public partial class ScreensGuardian
	{
		[JsonProperty("nombre", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Nombre { get; set; }

		[JsonProperty("path", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Path { get; set; }

		[JsonProperty("localPath", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string LocalPath { get; set; }

		[JsonProperty("versionActual", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string VersionActual { get; set; }

		[JsonProperty("versionRemota", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public int VersionRemota { get; set; }

		[JsonProperty("ancho", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Ancho { get; set; }

		[JsonProperty("alto", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Alto { get; set; }

		[JsonProperty("code", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Code { get; set; }
	}

	public partial class ModeLocal
	{
	}

	public partial class Mode
	{
		[JsonProperty("servidor", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Servidor { get; set; }

		[JsonProperty("contrasena", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public string Contrasena { get; set; }
	}

	public partial class Db
	{
		public static Db FromJson(string json) => JsonConvert.DeserializeObject<Db>(json, Converter.Settings);
	}

	public static class Serialize
	{
		public static string ToJson(this Db self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
						{
								new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
						},
		};
	}

	public partial class VoxContext
	{
		public static string path = AppDomain.CurrentDomain.BaseDirectory;
		public string fileJsonDir;

		public VoxContext()
		{
			fileJsonDir = path + "\\db.json";
			
		}

		public void Save(Config _config)
		{
			VoxContext db = new VoxContext();
			Root _root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(db.fileJsonDir));
			_root.Config[0] = _config;
			// serialize JSON directly to a file again
			using (StreamWriter file = File.CreateText(fileJsonDir))
			{
				JsonSerializer serializer = new JsonSerializer();
				serializer.Serialize(file, _root);
			}
		}

	}
}
