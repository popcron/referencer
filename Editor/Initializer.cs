using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Popcron.Referencer
{
    [InitializeOnLoad]
    public class Initializer
    {
        static Initializer()
        {
            //refresh if the references file is empty or doesnt exist
            List<References> references = Loader.FindAssetsByType<References>();
            if (references.Count == 0)
            {
                AssetProcessor.LoadAll();
            }

            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(local, "Unity", "Editor", "Editor.log");

            bool update = false;
            if (!File.Exists(path))
            {
                //if editor log doesnt exist, then update
                update = true;
            }
            else
            {
                DateTime now = DateTime.Now;

                string copy = path.Replace("Editor.log", "Editor.log.copy");
                File.Copy(path, copy, true);

                string text = File.ReadAllText(copy);
                string find = "LICENSE SYSTEM [";
                int start = text.IndexOf(find);
                string data = text.Substring(start + find.Length, 100).Split(']')[0];
                string year = data.Substring(0, 4);
                string month = "";
                string day = "";
                string hour = "";
                string minute = "";
                string seconds = "";
                string rest = data.Substring(4).Split(' ')[0];

                if (rest.Length == 2)
                {
                    month = int.Parse(rest[0].ToString()).ToString("00");
                    day = int.Parse(rest[1].ToString()).ToString("00");
                }
                else if (rest.Length == 3)
                {
                    //if the first 2 digits are less than or equal to 12
                    //then this is the month
                    //otherwise its just the first digit that is the month
                    if (int.Parse(rest.Substring(0, 2)) <= 12)
                    {
                        month = rest.Substring(0, 2);
                        day = int.Parse(rest[2].ToString()).ToString("00");
                    }
                    else
                    {
                        month = int.Parse(rest[0].ToString()).ToString("00");
                        day = rest.Substring(1, 2);
                    }
                }
                else if (rest.Length == 4)
                {
                    month = rest.Substring(0, 2);
                    day = rest.Substring(2, 2);
                }

                hour = int.Parse(data.Split(' ')[1].Split(':')[0]).ToString("00");
                minute = int.Parse(data.Split(' ')[1].Split(':')[1]).ToString("00");
                seconds = int.Parse(data.Split(' ')[1].Split(':')[2]).ToString("00");

                string key = Settings.UniqueIdentifier + ".InitializedLogTime";
                string logTimeString = year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + seconds;
                if (EditorPrefs.GetString(key) == logTimeString) return;

                DateTime logTime = DateTime.ParseExact(logTimeString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                TimeSpan diff = now - logTime;
                if (diff.Seconds < 120)
                {
                    EditorPrefs.SetString(key, logTimeString);
                    AssetProcessor.LoadAll();
                }
            }
        }
    }
}
