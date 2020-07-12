using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

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
            AssetDatabase.SaveAssets();
        }
    }
}
