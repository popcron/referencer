using System;
using System.Linq;
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
            bool update = false;

            //find the .gitignore file by searching upwards 3 times
            string ignoreFilePath = FindIgnoreFile();
            if (ignoreFilePath != null)
            {
                //add the References file to the ignore file
                List<string> ignoreLines = File.ReadAllLines(ignoreFilePath).ToList();

                //add the entry
                string comment = "# Referencer asset file";
                string entry = "*References.asset*";
                if (!ignoreLines.Contains(entry))
                {
                    //check the comment
                    if (!ignoreLines.Contains(comment))
                    {
                        ignoreLines.Add(comment);
                    }
                    else
                    {
                    }

                    ignoreLines.Add(entry);
                    update = true;
                    Debug.Log("[Referencer] A .gitignore file was found. Added a new entry for the Referencer asset file.");

                    File.WriteAllLines(ignoreFilePath, ignoreLines.ToArray());
                }
            }

            string key = Settings.UniqueIdentifier + ".TimeSinceStartup";
            float lastTimeSinceStartup = EditorPrefs.GetFloat(key);
            EditorPrefs.SetFloat(key, (float)EditorApplication.timeSinceStartup);

            //if the last time is less than the current time, then it was just a recompile
            //otherwise, it was a startup
            if (lastTimeSinceStartup > EditorApplication.timeSinceStartup)
            {
                update = true;
                Debug.Log("[Referencer] Loading all assets on startup");
            }

            //check against license file time
            if (!update)
            {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(local, "Unity", "Editor", "Editor.log");

                if (!File.Exists(path))
                {
                    //if editor log doesnt exist, then update
                    update = true;
                    Debug.Log("[Referencer] An Editor log was not found, gonna load all assets.");
                }
                else
                {
                    DateTime now = DateTime.Now;
                    DateTime then = GetLicenseTime();

                    key = $"{Settings.UniqueIdentifier}.InitializedLogTime";
                    if (EditorPrefs.GetString(key) == then.ToLongDateString())
                    {
                        return;
                    }

                    //load on boot, 30 seconds should be enough
                    //longer boot times might happen because of first time importing
                    if ((now - then).TotalSeconds < 30)
                    {
                        EditorPrefs.SetString(key, then.ToLongDateString());
                        update = true;
                        Debug.Log("[Referencer] Loading all assets on startup.");
                    }
                }
            }

            if (!update)
            {
                //refresh if the references file is empty or doesnt exist
                List<References> references = Loader.FindAssetsByType<References>();
                if (references.Count == 0)
                {
                    update = true;
                    Debug.Log("[Referencer] No References asset file was found, creating new one.");
                }
            }

            if (update)
            {
                References references = Helper.LoadAll();
                Helper.PutInsideContainer(references);
            }
        }

        private static DateTime GetLicenseTime()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(local, "Unity", "Editor", "Editor.log");

            if (!File.Exists(path))
            {
                //if editor log doesnt exist, then update
                return DateTime.MinValue;
            }
            else
            {
                //copy the file to avoid sharing violations
                string copyPath = path.Replace("Editor.log", "Editor.log.copy");
                File.Copy(path, copyPath, true);

                string text = File.ReadAllText(copyPath);
                string find = "LICENSE SYSTEM [";
                int start = text.IndexOf(find);
                if (start != -1)
                {
                    string data = text.Substring(start + find.Length, 100).Split(']')[0];
                    string year = data.Substring(0, 4);
                    string month = "";
                    string day = "";
                    string rest = data.Substring(4).Split(' ')[0];

                    if (rest.Length == 2)
                    {
                        //only 2 digits, so first is month, and other is day
                        month = int.Parse(rest[0].ToString()).ToString("00");
                        day = int.Parse(rest[1].ToString()).ToString("00");
                    }
                    else if (rest.Length == 3)
                    {
                        //first digit is the month, and other 2 is the day
                        month = int.Parse(rest[0].ToString()).ToString("00");
                        day = rest.Substring(1, 2);
                    }
                    else if (rest.Length == 4)
                    {
                        //2 for month, 2 for day
                        month = rest.Substring(0, 2);
                        day = rest.Substring(2, 2);
                    }

                    string hour = int.Parse(data.Split(' ')[1].Split(':')[0]).ToString("00");
                    string minute = int.Parse(data.Split(' ')[1].Split(':')[1]).ToString("00");
                    string second = int.Parse(data.Split(' ')[1].Split(':')[2]).ToString("00");

                    string logTimeString = $"{year}-{month}-{day} {hour}:{minute}:{second}";
                    return DateTime.ParseExact(logTimeString, "yyyy-dd-MM HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }

        private static DateTime GetLicenseTime()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(local, "Unity", "Editor", "Editor.log");

            if (!File.Exists(path))
            {
                //if editor log doesnt exist, then update
                return DateTime.MinValue;
            }
            else
            {
                //copy the file to avoid sharing violations
                string copyPath = path.Replace("Editor.log", "Editor.log.copy");
                File.Copy(path, copyPath, true);

                string text = File.ReadAllText(copyPath);
                string find = "LICENSE SYSTEM [";
                int start = text.IndexOf(find);
                if (start != -1)
                {
                    string data = text.Substring(start + find.Length, 100).Split(']')[0];
                    string year = data.Substring(0, 4);
                    string month = "";
                    string day = "";
                    string rest = data.Substring(4).Split(' ')[0];

                    if (rest.Length == 2)
                    {
                        //only 2 digits, so first is month, and other is day
                        month = int.Parse(rest[0].ToString()).ToString("00");
                        day = int.Parse(rest[1].ToString()).ToString("00");
                    }
                    else if (rest.Length == 3)
                    {
                        //first digit is the month, and other 2 is the day
                        month = int.Parse(rest[0].ToString()).ToString("00");
                        day = rest.Substring(1, 2);
                    }
                    else if (rest.Length == 4)
                    {
                        //2 for month, 2 for day
                        month = rest.Substring(0, 2);
                        day = rest.Substring(2, 2);
                    }

                    string hour = int.Parse(data.Split(' ')[1].Split(':')[0]).ToString("00");
                    string minute = int.Parse(data.Split(' ')[1].Split(':')[1]).ToString("00");
                    string second = int.Parse(data.Split(' ')[1].Split(':')[2]).ToString("00");

                    string logTimeString = $"{year}-{month}-{day} {hour}:{minute}:{second}";
                    return DateTime.ParseExact(logTimeString, "yyyy-dd-MM HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }

        private static string FindIgnoreFile()
        {
            //beside Assets folder
            string folder = Directory.GetParent(Application.dataPath).FullName;
            string filePath = Path.Combine(folder, ".gitignore");
            if (File.Exists(filePath))
            {
                return filePath;
            }

            //above Assets folder
            folder = Directory.GetParent(folder).FullName;
            filePath = Path.Combine(folder, ".gitignore");
            if (File.Exists(filePath))
            {
                return filePath;
            }

            //above above Assets folder (why would anyone do this?)
            folder = Directory.GetParent(folder).FullName;
            filePath = Path.Combine(folder, ".gitignore");
            if (File.Exists(filePath))
            {
                return filePath;
            }

            return null;
        }
    }
}
