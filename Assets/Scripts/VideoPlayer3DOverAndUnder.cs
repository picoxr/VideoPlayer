using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayer3DOverAndUnder : MonoBehaviour {

    public GameObject OverScreen;
    public GameObject UnderScreen;
	// Use this for initialization
	void Start () {
        HandleScreenObjUV();

    }
    private void HandleScreenObjUV()
    {
        if (OverScreen == null || UnderScreen == null)
        {
            Debug.Log("(handleScreenObjUV) Either OverScreen or UnderScreen is null!");
            return;
        }
        var uv = OverScreen.GetComponent<MeshFilter>().mesh.uv;
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i].y *= 0.5f;// 3D left-right
        }
        OverScreen.GetComponent<MeshFilter>().mesh.uv = uv;

        uv = UnderScreen.GetComponent<MeshFilter>().mesh.uv;
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i].y = 0.5f + uv[i].y * 0.5f;// 3D left-right
        }
        UnderScreen.GetComponent<MeshFilter>().mesh.uv = uv;
    }

    // Update is called once per frame
    void Update ()
    {
        if (UnderScreen.GetComponent<VideoPlayer>().texture!=null)
        {
            OverScreen.GetComponent<MeshRenderer>().material.mainTexture = UnderScreen.GetComponent<VideoPlayer>().texture;
        }
    }
}
