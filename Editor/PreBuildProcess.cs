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
        int IOrderedCallback.callbackOrder => 0;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            Helper.LoadAll();
            Debug.Log("[Referencer] Loaded all because project is being built");
            AssetDatabase.SaveAssets();
        }
    }
}
