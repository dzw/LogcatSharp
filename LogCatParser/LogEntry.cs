using System.Collections.Generic;
using System.Windows.Media;

namespace LessShittyLogcat {
    public class LogEntry {
        public string level { get; set; }
        public string time { get; set; }
        public string PID { get; set; }
        public string TID { get; set; }
        public string app { get; set; } // not bound
        public string tag { get; set; }
        public string text { get; set; }
        public string unwrapped { get; set; } // text before wrapping, to allow resize
        public string raw { get; set; } // un-split text

        public Brush Color {
            get
            {
                Dictionary<Color, SolidColorBrush> dic = new Dictionary<Color, SolidColorBrush> {
                    { Colors.DarkRed, new SolidColorBrush(Colors.DarkRed) },
                    { Colors.Black, new SolidColorBrush(Colors.Black) },
                    { Colors.DarkOrange, new SolidColorBrush(Colors.DarkOrange) },
                    { Colors.DarkBlue, new SolidColorBrush(Colors.DarkBlue) },
                };
                return dic[color];
            }
        }

        public Color color { get; set; }


        public string TextWrapping { get; set; } //__TEST__

        public override string ToString() {
            if (!string.IsNullOrEmpty(raw))
                return raw;
            if (!string.IsNullOrEmpty(text))
                return text;
            return base.ToString();
        }
    }
}