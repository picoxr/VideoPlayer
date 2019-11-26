using UnityEngine;
using UnityEditor;
using Pvr_UnitySDKAPI;

[CustomEditor(typeof(Pvr_UnitySDKManager))]
public class Pvr_UnitySDKManagerEditor : Editor
{
    public delegate void HeadDofChanged(string dof);
    public static event HeadDofChanged HeadDofChangedEvent;

    static int QulityRtMass = 0;
    public delegate void Change(int Msaa);
    public static event Change MSAAChange;
    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        Pvr_UnitySDKManager manager = (Pvr_UnitySDKManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Current Build Platform", firstLevelStyle);
        EditorGUILayout.LabelField(EditorUserBuildSettings.activeBuildTarget.ToString());
        GUILayout.Space(10);

        EditorGUILayout.LabelField("RenderTexture Setting", firstLevelStyle);
        manager.RtAntiAlising = (RenderTextureAntiAliasing)EditorGUILayout.EnumPopup("RenderTexture Anti-Aliasing", manager.RtAntiAlising);
#if UNITY_2018_3_OR_NEWER
        GUI.enabled = false;
#endif
        manager.RtBitDepth = (RenderTextureDepth)EditorGUILayout.EnumPopup("RenderTexture Bit Depth", manager.RtBitDepth);
        manager.RtFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup("RenderTexture Format", manager.RtFormat);
#if UNITY_2018_3_OR_NEWER
        GUI.enabled = true;
#endif
        manager.DefaultRenderTexture = EditorGUILayout.Toggle("Use Default RenderTexture", manager.DefaultRenderTexture);
        if (!manager.DefaultRenderTexture)
        {
            manager.RtSize = EditorGUILayout.Vector2Field("    RT Size", manager.RtSize);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("1.width & height must be larger than 0;");
            EditorGUILayout.LabelField("2.the size of RT has a great influence on performance;");
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Pose Settings", firstLevelStyle);
        manager.TrackingOrigin = (TrackingOrigin)EditorGUILayout.EnumPopup("Tracking Origin", manager.TrackingOrigin);
        manager.Rotfoldout = EditorGUILayout.Foldout(manager.Rotfoldout, "Only Rotation Tracking");
        if (manager.Rotfoldout)
        {
            manager.HmdOnlyrot = EditorGUILayout.Toggle("  Only HMD Rotation Tracking", manager.HmdOnlyrot);
            if (manager.HmdOnlyrot)
            {
                manager.PVRNeck = EditorGUILayout.Toggle("    Enable Neck Model", manager.PVRNeck);
                if (manager.PVRNeck)
                {
                    manager.UseCustomNeckPara = EditorGUILayout.Toggle("Use Custom Neck Parameters", manager.UseCustomNeckPara);
                    if (manager.UseCustomNeckPara)
                    {
                        manager.neckOffset = EditorGUILayout.Vector3Field("Neck Offset", manager.neckOffset);
                    }
                }
            }
            else
            {
                manager.PVRNeck = false;
            }
            manager.ControllerOnlyrot =
                EditorGUILayout.Toggle("  Only Controller Rotation Tracking", manager.ControllerOnlyrot);
        }
        else
        {
            manager.HmdOnlyrot = false;
            manager.ControllerOnlyrot = false;
        }
        
        manager.MovingRatios = EditorGUILayout.FloatField("Position ScaleFactor", manager.MovingRatios);
        manager.SixDofPosReset = EditorGUILayout.Toggle("Enable 6Dof Position Reset", manager.SixDofPosReset);

        manager.DefaultRange = EditorGUILayout.Toggle("Use Default Safe Radius", manager.DefaultRange);
        if (!manager.DefaultRange)
        {
            manager.CustomRange = EditorGUILayout.FloatField("    Safe Radius(meters)", manager.CustomRange);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Other Settings", firstLevelStyle);
        manager.ShowFPS = EditorGUILayout.Toggle("Show FPS", manager.ShowFPS);
        manager.ShowSafePanel = EditorGUILayout.Toggle("Show SafePanel", manager.ShowSafePanel);
        manager.ScreenFade = EditorGUILayout.Toggle("Open Screen Fade", manager.ScreenFade);
        manager.DefaultFPS = EditorGUILayout.Toggle("Use Default FPS", manager.DefaultFPS);
        if (!manager.DefaultFPS)
        {
            manager.CustomFPS = EditorGUILayout.IntField("    FPS", manager.CustomFPS);
        }
        manager.Monoscopic = EditorGUILayout.Toggle("Use Monoscopic", manager.Monoscopic);
        manager.Copyrightprotection = EditorGUILayout.Toggle("Copyright protection", manager.Copyrightprotection);
        if (GUI.changed)
        {
            QulityRtMass = (int)Pvr_UnitySDKManager.SDK.RtAntiAlising;
            if (QulityRtMass == 1)
            {
                QulityRtMass = 0;
            }
            if (MSAAChange != null)
            {
                MSAAChange(QulityRtMass);
            }
            var headDof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
            if (HeadDofChangedEvent != null)
            {
                if (headDof == 0)
                {
                    HeadDofChangedEvent("3dof");
                }
                else
                {
                    HeadDofChangedEvent("6dof");
                }

            }
            EditorUtility.SetDirty(manager);
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager
                .GetActiveScene());
#endif
        }

        serializedObject.ApplyModifiedProperties();
    }

}
