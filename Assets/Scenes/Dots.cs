using Gist2.Deferred;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class Dots : MonoBehaviour {
 
    public Material mat;

    public Tuner tuner = new Tuner();
    public Events events = new Events();

    protected RenderTextureWrapper dotsTex;
    protected Camera attCam;
    protected int2 attCam_size;
    protected Validator changed = new Validator();

    #region unity
    private void OnEnable() {
        attCam = GetComponent<Camera>();

        dotsTex = new RenderTextureWrapper(size => {
            var tex = new RenderTexture(size.x, size.y, 0, DefaultFormat.LDR);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.useMipMap = false;
            tex.hideFlags = HideFlags.DontSave;
            
            events.DotsOnCreate?.Invoke(tex);
            return tex;
        });

        changed.Reset();
        changed.CheckValidity += () => math.all(attCam_size == attCam.Size());
        changed.OnValidate += () => {
            attCam_size = attCam.Size();
            if (mat != null) {
                mat.SetVector(P_Params0, new Vector4(
                    math.min(tuner.dot_size, tuner.tile_size),
                    tuner.tile_size,
                    0f,
                    0f));

                dotsTex.Size = attCam.Size();
                Graphics.Blit(null, dotsTex, mat);
            }

            Debug.Log($"{GetType().Name}: Changed");
        };
    }
    private void OnDisable() {
        if (dotsTex != null) {
            dotsTex.Dispose();
            dotsTex = null;
        }
    }
    private void OnValidate() {
        changed.Invalidate();
    }
    private void Update() {
        changed.Validate();
    }
    #endregion

    #region declarations
    public static readonly int P_Params0 = Shader.PropertyToID("_Params0");

    [System.Serializable]
    public class Events {
        public TextureEvent DotsOnCreate = new TextureEvent();

        [System.Serializable]
        public class TextureEvent : UnityEvent<Texture> { }
    }
    [System.Serializable]
    public class Tuner {
        public int dot_size = 1;
        public int tile_size = 3;
    }
    #endregion
}
