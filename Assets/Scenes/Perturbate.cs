using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class Perturbate : MonoBehaviour {

    public Material mat;

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (mat != null)
            Graphics.Blit(source, destination, mat);
        else
            Graphics.Blit(source, destination);
    }

    #region declarations
    [System.Serializable]
    public struct Shape {
        float2 center;
    };
    #endregion
}
