#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif

using Pvr_UnitySDKAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pvr_UnitySDKEyeOverlay : MonoBehaviour, IComparable<Pvr_UnitySDKEyeOverlay>
{
    public static List<Pvr_UnitySDKEyeOverlay> Instances = new List<Pvr_UnitySDKEyeOverlay>();

    public int layerIndex = 0;
    public ImageType layerType = ImageType.StandardTexture;
    public Transform layerTransform;

    public Texture[] layerTextures = new Texture[2];

    public int[] layerTextureIds = new int[2];
    public Matrix4x4[] MVMatrixs = new Matrix4x4[2];
    private Camera[] layerEyeCamera = new Camera[2];





    public int CompareTo(Pvr_UnitySDKEyeOverlay other)
    {
        return this.layerIndex.CompareTo(other.layerIndex);
    }

    #region Unity Methods
    private void Awake()
    {
        Instances.Add(this);

        this.layerEyeCamera[0] = Pvr_UnitySDKEyeManager.Instance.LeftEyeCamera;
        this.layerEyeCamera[1] = Pvr_UnitySDKEyeManager.Instance.RightEyeCamera;

        this.layerTransform = this.GetComponent<Transform>();

        this.InitializeBuffer();
    }

    private void LateUpdate()
    {
        this.UpdateCoords();
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }
    #endregion




    private void InitializeBuffer()
    {
        switch (this.layerType)
        {
            case ImageType.StandardTexture:              
            case ImageType.EquirectangularTexture:
                for (int i = 0; i < this.layerTextureIds.Length; i++)
                {
                    this.layerTextureIds[i] = this.layerTextures[i].GetNativeTexturePtr().ToInt32();
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Update MV Matrix
    /// </summary>
    private void UpdateCoords()
    {
        if (this.layerTransform == null || !this.layerTransform.gameObject.activeSelf)
        {
            return;
        }

        if (this.layerEyeCamera[0] == null || this.layerEyeCamera[1] == null)
        {
            return;
        }

        if (this.layerType == ImageType.StandardTexture)
        {
            // update MV matrix
            for (int i = 0; i < this.MVMatrixs.Length; i++)
            {
                this.MVMatrixs[i] = this.layerEyeCamera[i].worldToCameraMatrix * this.layerTransform.localToWorldMatrix;
            }
        }
    }

    #region Public Method
    /// <summary>
    /// Reset Layer Texture
    /// </summary>
    /// <param name="texture"></param>
    public void SetTexture(Texture texture)
    {
        for (int i = 0; i < this.layerTextures.Length; i++)
        {
            this.layerTextures[i] = texture;
        }
        this.InitializeBuffer();
    }

    #endregion

    public enum ImageType
    {
        StandardTexture = 0,
        //EglTexture = 1,
        EquirectangularTexture = 2
    }
}
