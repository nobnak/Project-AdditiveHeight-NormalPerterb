using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class Dots : MonoBehaviour {
 
    public Material mat;

    public Tuner tuner = new Tuner();

    #region unity
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (mat != null) {
            mat.SetVector(P_Params0, new Vector4(
                math.min(tuner.dot_size, tuner.tile_size),
                tuner.tile_size, 
                0f, 
                0f));

            Graphics.Blit(source, destination, mat);
        } else {
            Graphics.Blit(source, destination);
        }
    }
    #endregion

    #region declarations
    public static readonly int P_Params0 = Shader.PropertyToID("_Params0");

    [System.Serializable]
    public class Tuner {
        public int dot_size = 1;
        public int tile_size = 3;
    }
    #endregion
}
