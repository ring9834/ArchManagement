using System;
using System.Collections.Generic;
using Spire.Pdf.Graphics;

namespace ArchiveFileManagementNs.Models
{
    public class WatermarkColor
    {
        public static List<KeyValuePair<string, string>> Colors {
            get {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                KeyValuePair<string, string> kp1 = new KeyValuePair<string, string>("��ɫ", "Y");
                list.Add(kp1);
                KeyValuePair<string, string> kp2 = new KeyValuePair<string, string>("��ɫ", "B");
                list.Add(kp2);
                KeyValuePair<string, string> kp3 = new KeyValuePair<string, string>("��ɫ", "R");
                list.Add(kp3);
                KeyValuePair<string, string> kp4 = new KeyValuePair<string, string>("��ɫ", "G");
                list.Add(kp4);
                KeyValuePair<string, string> kp5 = new KeyValuePair<string, string>("��ɫ", "BL");
                list.Add(kp5);
                KeyValuePair<string, string> kp6 = new KeyValuePair<string, string>("��ɫ", "Z");
                list.Add(kp6);
                return list;
            } 
        }

        public static PdfBrush ColorAt(string key)
        {
            if (key.ToLower().Equals("y"))
                return PdfBrushes.DarkOrange;

            if (key.ToLower().Equals("b"))
                return PdfBrushes.Blue;

            if (key.ToLower().Equals("r"))
                return PdfBrushes.Red;

            if (key.ToLower().Equals("g"))
                return PdfBrushes.DarkGreen;

            if (key.ToLower().Equals("bl"))
                return PdfBrushes.Black;

            if (key.ToLower().Equals("z"))
                return PdfBrushes.Purple;
            return PdfBrushes.DarkGray;
        }
    }
}