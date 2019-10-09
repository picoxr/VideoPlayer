using UnityEngine;
using System.Collections;
using UnityEngine.Video;

public class VideoPlayer3D : MonoBehaviour
{
    public GameObject LeftScreen;
    public GameObject RightScreen;

    void Start()
    {
        HandleScreenObjUV();
    }

    private void HandleScreenObjUV()
    {
        if (LeftScreen == null || RightScreen == null)
        {
            Debug.Log("(handleScreenObjUV) Either leftScreen or rightScreen is null!");
            return;
        }
        var uv = LeftScreen.GetComponent<MeshFilter>().mesh.uv;
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i].x *= 0.5f;// 3D left-right
        }
        LeftScreen.GetComponent<MeshFilter>().mesh.uv = uv;

        uv = RightScreen.GetComponent<MeshFilter>().mesh.uv;
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i].x = 0.5f + uv[i].x * 0.5f;// 3D left-right
        }
        RightScreen.GetComponent<MeshFilter>().mesh.uv = uv;
    }

    void Update()
    {
        if (RightScreen.GetComponent<VideoPlayer>().texture!=null)
        {
            LeftScreen.GetComponent<MeshRenderer>().material.mainTexture = RightScreen.GetComponent<VideoPlayer>().texture;
        }
       
    }
}
