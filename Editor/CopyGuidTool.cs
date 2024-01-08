using UnityEditor;

namespace Mew.Core
{
    public static class CopyGuidTool
    {
        [MenuItem("Assets/Mew/Copy GUID", true, 0)]
        public static bool CopyGuidValidate()
        {
            return Selection.activeObject != null;
        }

        [MenuItem("Assets/Mew/Copy GUID", false, 0)]
        public static void CopyGuid()
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));
            EditorGUIUtility.systemCopyBuffer = guid;
        }
    }
}