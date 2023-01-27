using Gist2.Extensions.SizeExt;
using nobnak.Gist.Compute.Blurring;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class Differentiate : MonoBehaviour {

    public Links link = new Links();
    public Tuner tuner = new Tuner();
    public ForDebug forDebug = new ForDebug();

    protected Blur blur;

    #region properties
    public Texture PatternTex { get; set; }
    #endregion

    #region unity
    private void OnEnable() {
        blur = new Blur(link.gaussianDownSampler);
    }
    private void OnDisable() {
        if (blur != null) {
            blur.Dispose();
            blur = null;
        }
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        var mat = link.mat;
        if (mat == null) {
            Graphics.Blit(source, destination);
            return;
        }

        var size = source.Size();
        var format = RenderTextureFormat.ARGBFloat;
        var tmp0 = RenderTexture.GetTemporary(size.x, size.y, 0, format);
        var tmp1 = RenderTexture.GetTemporary(size.x, size.y, 0, format);
        try {
            if (forDebug.output == Output.Pattern) {
                Graphics.Blit(PatternTex, tmp0);
                goto Output;
            }

            if (forDebug.output == Output.Source) {
                Graphics.Blit(source, tmp0);
                goto Output;
            }

            Graphics.Blit(source, tmp1, mat, (int)ShaderPass.SelectSource);
            Swap(ref tmp0, ref tmp1);
            if (forDebug.output == Output.Height) goto Output;

            if (tuner.blur_lod > 0) {
                blur.Render(tmp0, tmp1, tuner.blur_lod);
                Swap(ref tmp0, ref tmp1);
            }
            if (forDebug.output == Output.Height_blurred) goto Output;

            Graphics.Blit(tmp0, tmp1, mat, (int)ShaderPass.Differentiate);
            Swap(ref tmp0, ref tmp1);
            if (forDebug.output == Output.Differentiation) goto Output;

            mat.SetVector(P_Params0, new Vector4(0f, 0f, tuner.pert_uv_gain, 0f));
            mat.SetTexture(P_PatternTex, PatternTex);
            Graphics.Blit(tmp0, tmp1, mat, (int)ShaderPass.Perturbate);
            Swap(ref tmp0, ref tmp1);
            if (forDebug.output == Output.Perturbation) goto Output;

            Output:
            Graphics.Blit(tmp0, destination);
        } finally {
            RenderTexture.ReleaseTemporary(tmp0);
            RenderTexture.ReleaseTemporary(tmp1);
        }
    }
    #endregion

    #region static
    public static void Swap<T>(ref T a, ref T b) {
        var c = a;
        a = b;
        b = c;
    }
    #endregion

    #region declarations
    public static readonly int P_PatternTex = Shader.PropertyToID("_PatternTex");
    public static readonly int P_Params0 = Shader.PropertyToID("_Params0");
    public static readonly int P_Params1 = Shader.PropertyToID("_Params1");

    [System.Serializable]
    public enum Output {
        Result = 0,
        Source,
        Height,
        Height_blurred,
        Differentiation,
        Perturbation,
        Pattern,
    }
    [System.Serializable]
    public enum ShaderPass { 
        SelectSource = 0,
        Differentiate,
        Perturbate,
    }

    [System.Serializable]
    public class Links {
        public Material mat;
        public ComputeShader gaussianDownSampler;
    }
    [System.Serializable]
    public class ForDebug {
        public Output output;
    }
    [System.Serializable]
    public class Tuner {
        [Range(0, 100)]
        public int blur_lod = 0;
        [Range(0f, 10f)]
        public float pert_uv_gain = 1f;
    }
    #endregion
}
