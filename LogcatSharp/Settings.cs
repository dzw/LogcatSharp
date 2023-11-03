using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LitJson;

namespace LogcatSharp {
    public class Settings {
        private static Settings _Instance = null;

        public static Settings Instance {
            get
            {
                if (_Instance == null)
                    return _Instance = new Settings();
                return _Instance;
            }
        }

        private static string ConfigFile() {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb-path.txt");
            return file;
        }

        private JsonData _jsonData;

        public Settings() {
            //
            var file = ConfigFile();

            if (File.Exists(file)) {
                var json = File.ReadAllText(file);
                try {
                    _jsonData = JsonMapper.ToObject(json);
                    return;
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            }

            _jsonData = new JsonData();
        }

        public string GetPath() {
            if (_jsonData.ContainsKey("adb")) {
                return _jsonData["adb"] + "";
            }

            return null;
        }


        public void SavePath(string adbPath) {
            _jsonData["adb"] = adbPath;
            var file = ConfigFile();

            try {
                File.WriteAllText(file, _jsonData.ToJson());
            }
            catch {
            }
        }
    }
}