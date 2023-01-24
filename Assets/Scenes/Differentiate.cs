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
        if (link.mat == null) {
            Graphics.Blit(source, destination);
            return;
        }

        var size = source.Size();
        var format = RenderTextureFormat.ARGBHalf;
        var tmp0 = RenderTexture.GetTemporary(size.x, size.y, 0, format);
        var tmp1 = RenderTexture.GetTemporary(size.x, size.y, 0, format);
        try {
            Graphics.Blit(source, tmp1, link.mat, (int)ShaderPass.SelectSource);
            Swap(ref tmp0, ref tmp1);
            if (forDebug.output == Output.Source) goto Output;

            if (tuner.blur_lod > 0f) {
                Blur.FindSize(tuner.blur_lod, out int iter, out int lod);
                blur.Render(tmp0, tmp1, iter, lod);
                Swap(ref tmp0, ref tmp1);
            }
            if (forDebug.output == Output.Source_blurred) goto Output;

            Graphics.Blit(tmp0, tmp1, link.mat, (int)ShaderPass.Differentiate);
            Swap(ref tmp0, ref tmp1);
            if (forDebug.output == Output.Diff) goto Output;

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
    [System.Serializable]
    public enum Output {
        Result = 0,
        Source,
        Source_blurred,
        Diff,
    }
    [System.Serializable]
    public enum ShaderPass { 
        SelectSource = 0,
        Differentiate,
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
        [Range(0f, 10f)]
        public float blur_lod = 0f;
    }
    #endregion
}
