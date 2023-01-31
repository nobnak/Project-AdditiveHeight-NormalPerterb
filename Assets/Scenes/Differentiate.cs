using GaussianDownsampleSys;
using Gist2.Extensions.SizeExt;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class Differentiate : MonoBehaviour {

    public Links link = new Links();
    public Tuner tuner = new Tuner();
    public ForDebug forDebug = new ForDebug();

    protected Camera attCam;
    protected GaussianDownsample blur;

    #region properties
    public Texture PatternTex { get; set; }
    #endregion

    #region unity
    private void OnEnable() {
        attCam = GetComponent<Camera>();

        blur = new GaussianDownsample();

        attCam.depthTextureMode |= DepthTextureMode.DepthNormals;
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
            mat.shaderKeywords = null;
            if (tuner.source != default)
                mat.EnableKeyword(tuner.source.ToString());

            mat.SetVector(P_Params0, new Vector4(0f, 0f, tuner.pert_uv_gain * 1e-2f, tuner.diff_limit * 1e-2f));

            if (forDebug.output == Output.Pattern) {
                Graphics.Blit(PatternTex, tmp0);
                goto Output;
            }

            if (forDebug.output == Output.Input) {
                Graphics.Blit(source, tmp0);
                goto Output;
            }

            switch (tuner.source) {
                case SourceMode.SRC_NORMAL: {
                    break;
                }
                default: {
                    Graphics.Blit(source, tmp1, mat, (int)ShaderPass.SelectSource);
                    Swap(ref tmp0, ref tmp1);
                    if (forDebug.output == Output.Height) goto Output;

                    if (tuner.blur.iterations > 0) {
                        blur.Render(tmp0, tmp1, tuner.blur.iterations, tuner.blur.lod);
                        Swap(ref tmp0, ref tmp1);
                    }
                    if (forDebug.output == Output.Height_blurred) goto Output;

                    break;
                }
            }

            Graphics.Blit(tmp0, tmp1, mat, (int)ShaderPass.Differentiate);
            Swap(ref tmp0, ref tmp1);
            if (forDebug.output == Output.Differ) goto Output;

            if (tuner.blur.iterations > 0) {
                blur.Render(tmp0, tmp1, tuner.blur.iterations, tuner.blur.lod);
                Swap(ref tmp0, ref tmp1);
            }
            if (forDebug.output == Output.Differ_blurred) goto Output;

            mat.SetTexture(P_PatternTex, PatternTex);
            Graphics.Blit(tmp0, tmp1, mat, (int)ShaderPass.Perturbate);
            Swap(ref tmp0, ref tmp1);

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
    public enum SourceMode {
        SRC_LUMINANCE = 0,
        SRC_DEPTH,
        SRC_NORMAL,
    }
    [System.Serializable]
    public enum Output {
        Pattern = -1,
        Result = 0,
        Input,
        Height,
        Height_blurred,
        Differ,
        Differ_blurred,
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
    public class BlurTuner {
        [Range(0, 10)]
        public int iterations = 0;
        [Range(0, 10)]
        public int lod = 0;
    }
    [System.Serializable]
    public class Tuner {
        public SourceMode source = default;
        [Range(0f, 100f)]
        public float pert_uv_gain = 1f;
        [Range(0f, 100f)]
        public float diff_limit = 1f;
        public BlurTuner blur = new BlurTuner();
    }
    #endregion
}
