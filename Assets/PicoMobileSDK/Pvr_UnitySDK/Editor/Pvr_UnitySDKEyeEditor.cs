using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pvr_UnitySDKEye))]
public class Pvr_UnitySDKEyeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.changed = false;

        Pvr_UnitySDKEye sdkeye = (Pvr_UnitySDKEye)target;
        eFoveationLevel lastlevel = sdkeye.foveationLevel;
        eFoveationLevel newlevel = (eFoveationLevel)EditorGUILayout.EnumPopup("Foveation Level", sdkeye.foveationLevel);
        if (lastlevel != newlevel)
        {
            sdkeye.foveationLevel = newlevel;
            switch (sdkeye.foveationLevel)
            {
                case eFoveationLevel.None:
                    sdkeye.FoveationGainValue = Vector2.zero;
                    sdkeye.FoveationAreaValue = 0.0f;
                    sdkeye.FoveationMinimumValue = 0.0f;
                    break;
                case eFoveationLevel.Low:
                    sdkeye.FoveationGainValue = new Vector2(2.0f, 2.0f);
                    sdkeye.FoveationAreaValue = 0.0f;
                    sdkeye.FoveationMinimumValue = 0.125f;
                    break;
                case eFoveationLevel.Med:
                    sdkeye.FoveationGainValue = new Vector2(3.0f, 3.0f);
                    sdkeye.FoveationAreaValue = 1.0f;
                    sdkeye.FoveationMinimumValue = 0.125f;
                    break;
                case eFoveationLevel.High:
                    sdkeye.FoveationGainValue = new Vector2(4.0f, 4.0f);
                    sdkeye.FoveationAreaValue = 2.0f;
                    sdkeye.FoveationMinimumValue = 0.125f;
                    break;
            }
        }
        sdkeye.FoveationGainValue = EditorGUILayout.Vector2Field("Foveation Gain Value", sdkeye.FoveationGainValue);
        sdkeye.FoveationAreaValue = EditorGUILayout.FloatField("Foveation Area Value", sdkeye.FoveationAreaValue);
        sdkeye.FoveationMinimumValue = EditorGUILayout.FloatField("Foveation Minimum Value", sdkeye.FoveationMinimumValue);

        EditorUtility.SetDirty(sdkeye);
        if(GUI.changed)
        {
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager
                .GetActiveScene());
#endif
        }
    }

}
