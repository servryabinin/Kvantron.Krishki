using System;
using System.Text.Json;

namespace KrishkiForms.CameraAndModbusClasses
{
	public class LocalSettings
	{
		public bool UseModule { get; set; } = true;
		public int DINumber { get; set; } = 1;
		public int DONumber { get; set; } = 1;
		public string ModuleIP { get; set; } = "169.254.192.10"; //was 10.16.1.24
        public bool UseVConcat { get; set; } = false;		
		public string Cam1SN { get; set; } = "DA0149100"; //was K05474844

		public int EndStreamDelay { get; set; } = 1000;

		private static LocalSettings _instance;

		/// <summary>
		/// Текущая версия настроек
		/// </summary>
		public static LocalSettings Instance
		{
			get
			{
				if (_instance == null)
				{
					try
					{

						_instance = JsonSerializer.Deserialize<LocalSettings>(File.ReadAllText("Settings.json"));
					}
					catch (Exception ex)
					{
						_instance = new LocalSettings();
					}
				}
				return _instance;
			}
		}		

		/// <summary>
		/// Сохранение текущих настроек в файл
		/// </summary>
		public void Save()
		{
			string data = JsonSerializer.Serialize(this);
			File.WriteAllText("Settings.json", data);
		}
	}
}
