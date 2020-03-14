using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Popcron.Referencer
{
    /// <summary>
    /// This class is gonna load everything one last time before building
    /// </summary>
    public class PreBuildProcess : IPreprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder => 0;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[Referencer] Loading all before building.");
            if (!Helper.DoesReferenceInstanceExist)
            {
                builder.AppendLine("[Referencer] References asset doesnt exist prior to building, will create one now.");
            }

            References references = Helper.LoadAll();
            Helper.SetDirty(references);
            AssetDatabase.SaveAssets();

            //open all of the scenes in the build target
            Scene originalScene = SceneManager.GetActiveScene();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                string path = EditorBuildSettings.scenes[i].path;
                if (!string.IsNullOrEmpty(path))
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    builder.AppendLine($"[Referencer] Checking out scene at path {path}");
                    if (scene.IsValid())
                    {
                        //make changes here lol
                        Helper.PutInsideContainer(references);

                        //save and close
                        builder.AppendLine($"[Referencer] Saved scene {path}");
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);

                        //print out the root scene directory
                        builder.AppendLine($"[Referencer] Print out of the root for scene at {path}");
                        List<GameObject> rootObjects = new List<GameObject>();
                        scene.GetRootGameObjects(rootObjects);
                        foreach (GameObject gameObject in rootObjects)
                        {
                            builder.AppendLine($"    {gameObject.name}");
                        }
                    }
                }
            }

            builder.AppendLine("[Referencer] Donesies");
            Debug.Log(builder.ToString());

            //just in case
            EditorSceneManager.OpenScene(originalScene.path, OpenSceneMode.Single);
        }
    }
}