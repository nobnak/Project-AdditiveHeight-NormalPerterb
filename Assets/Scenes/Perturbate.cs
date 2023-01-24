using Gist2.Adapter;
using Gist2.Extensions.ScreenExt;
using Gist2.Extensions.SizeExt;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[ExecuteAlways]
public class Perturbate : MonoBehaviour {

    public Material mat;

    public Tuner tuner = new Tuner();

    #region properties
    public Texture PatternTex { get; set; }
    #endregion

    #region unity
    private void OnEnable() {
    }
    private void OnDisable() {
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {

        if (mat != null) {
            mat.SetVector(P_Params0, new Vector4(tuner.size, 0f, tuner.uv_offset_gain, 0f));

            mat.SetTexture(P_PatternTex, PatternTex);

            Graphics.Blit(source, destination, mat);
        } else {
            Graphics.Blit(source, destination);
        }
    }
    #endregion

    #region declarations
    public static readonly int P_PatternTex = Shader.PropertyToID("_PatternTex");
    public static readonly int P_Params0 = Shader.PropertyToID("_Params0");
    public static readonly int P_Params1 = Shader.PropertyToID("_Params1");

    [System.Serializable]
    public class Tuner {
        public float size = 0.1f;
        public float speed = 0f;
        public float uv_offset_gain = 1f;
    }
    #endregion
}
