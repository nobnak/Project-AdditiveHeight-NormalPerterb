using Gist2.Adapter;
using Gist2.Extensions.ScreenExt;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteAlways]
public class Perturbate : MonoBehaviour {

    public Material mat;

    public Tuner tuner = new Tuner();

    protected GraphicsList<Shape> shapes;

    #region unity
    private void OnEnable() {
        shapes = new GraphicsList<Shape>((len) => {
            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, len, Marshal.SizeOf<Shape>());
            return buf;
        });
    }
    private void OnDisable() {
        if (shapes != null) {
            shapes.Dispose();
            shapes = null;
        }
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {

        if (mat != null) {
            mat.SetVector(P_Params0, new Vector4(tuner.size, 0f, tuner.uv_offset_gain, 0f));

            mat.SetInt(P_Shapes_Len, shapes.Count);
            mat.SetBuffer(P_Shapes, shapes);

            Graphics.Blit(source, destination, mat);
        } else {
            Graphics.Blit(source, destination);
        }
    }
    private void Update() {
        var dt = Time.deltaTime;
        for (var i = 0; i < shapes.Count;) {
            var s = shapes[i];
            var uv = s.center;
            uv.x -= dt * tuner.speed;
            s.center = uv;
            shapes[i] = s;

            if (uv.x < 0f) {
                shapes.RemoveAt(i);
                continue;
            }

            i++;
        }

        var m = Mouse.current;
        if (m.leftButton.wasPressedThisFrame) {
            var screen = ScreenExtension.ScreenSize();
            var uv = (float2)m.position.ReadValue() / screen;
            uv.x = 1f;
            shapes.Add(new Shape() { center = uv });
        }
    }
    #endregion

    #region declarations
    public static readonly int P_Shapes_Len = Shader.PropertyToID("_Shapes_Len");
    public static readonly int P_Shapes = Shader.PropertyToID("_Shapes");

    public static readonly int P_Params0 = Shader.PropertyToID("_Params0");
    public static readonly int P_Params1 = Shader.PropertyToID("_Params1");

    [System.Serializable]
    public struct Shape {
        public float2 center;
    };
    [System.Serializable]
    public class Tuner {
        public float size = 0.1f;
        public float speed = 0f;
        public float uv_offset_gain = 1f;
    }
    #endregion
}
