﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Popcron.Referencer
{
    /// <summary>
    /// This class is gonna load everything one last time before building
    /// </summary>
    public class PreBuildProcess : IPreprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder => int.MinValue + 1000;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[Referencer] Loaded all because project is being built");

            ReferencesLoader.LoadAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif