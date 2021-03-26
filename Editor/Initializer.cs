using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Popcron.Referencer
{
    [InitializeOnLoad]
    public class Initializer
    {
        public const string InitializedScenesKey = "Popcron.Referencer.Editor.ScenesInitialized";
        private const char ListSplitter = '|';

        private static bool IsSceneInitialized(string sceneName)
        {
            string scenesInitialized = EditorPrefs.GetString(InitializedScenesKey);
            if (string.IsNullOrEmpty(scenesInitialized))
            {
                return false;
            }

            string[] names = scenesInitialized.Split(ListSplitter);
            return Array.IndexOf(names, sceneName) != -1;
        }

        private static void SetSceneInitialized(string sceneName, bool state)
        {
            string scenesInitialized = EditorPrefs.GetString(InitializedScenesKey);
            if (string.IsNullOrEmpty(scenesInitialized))
            {
                EditorPrefs.SetString(InitializedScenesKey, state ? sceneName : "");
            }

            List<string> names = scenesInitialized.Split(ListSplitter).ToList();
            if (names.Contains(sceneName) && !state)
            {
                names.Remove(sceneName);
            }
            else if (!names.Contains(sceneName) && state)
            {
                names.Add(sceneName);
            }

            string newPref = string.Join(ListSplitter.ToString(), names);
            EditorPrefs.SetString(InitializedScenesKey, newPref);
        }

        private static void ResetInitializedScenes()
        {
            EditorPrefs.SetString(InitializedScenesKey, "");
        }

        static Initializer()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnChangedScene;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

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
                ResetInitializedScenes();
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
                    ResetInitializedScenes();
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
                        ResetInitializedScenes();
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
                    ResetInitializedScenes();
                }
            }

            if (update)
            {
                Helper.LoadAll();
                Debug.Log("[Referencer] Loaded all because of first launch");
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (!IsSceneInitialized(scene.path))
                {
                    Helper.LoadAll();
                    Debug.Log($"[Referencer] Loaded all because were about to play the scene {scene.name} when its not initialized");
                    SetSceneInitialized(scene.path, true);
                }
            }
        }

        private static void OnChangedScene(Scene oldScene, Scene newScene)
        {
            if (!IsSceneInitialized(newScene.path))
            {
                Helper.LoadAll();
                Debug.Log($"[Referencer] Loaded all because we changed to scene {newScene.name} that isnt initialized");
                SetSceneInitialized(newScene.path, true);
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
                    if (DateTime.TryParseExact(logTimeString, "yyyy-dd-MM HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                    else
                    {
                        return DateTime.Now;
                    }
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
