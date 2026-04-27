using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NullObjectPresence.HyperObject
{
    public enum DrawMode
    {
        Line,
        Quad,
        Tube
    }
    public abstract class BaseHyperObject : MonoBehaviour
    {
        protected bool active = true;
        protected List<Vector4> points4d = new List<Vector4>();
        protected List<Vector3> points3d = new List<Vector3>();
        protected List<Color> colors = new List<Color>();
        protected List<Vector2Int> edges = new List<Vector2Int>();
        private static Material lineMaterial;
        [SerializeField, Range(0, 1)] protected float alpha = 1;
        [SerializeField, Range(0, 2)] protected float normalizeScaler = 1;
        [SerializeField] protected Camera targetCamera;
        [SerializeField] protected DrawMode drawMode = DrawMode.Line;
        [SerializeField] protected GameObject attractor;
        [SerializeField, Range(0, 1)] protected float attractForce = 0.1f;
        [SerializeField] protected UnityEvent<Vector3, bool> action;

        private int axis1 = 0;
        private int axis2 = 1;
        private int axis3 = 2;
        private int axis4 = 3;
        public bool Active { get => active; set => active = value; }
        public float Alpha { get => alpha; set => alpha = value; }
        public float Scaler { get => normalizeScaler; set => normalizeScaler = value; }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Plane"))
            {
                action.Invoke(collision.contacts[0].point, collision.gameObject.transform.position.y < 0.1f);
            }
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected virtual void Start()
        {
            SetPoints();
            SetEdges();
            SetColors();
            this.gameObject.SetActive(false);
            this.Active = false;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (!active)
            {
                return;
            }
            var dt = Time.deltaTime;
            var av = this.GetComponent<Rigidbody>().angularVelocity;
            Rotation4d(ref points4d, dt * av.x, axis1, axis4);
            Rotation4d(ref points4d, dt * av.y, axis2, axis4);
            Rotation4d(ref points4d, dt * av.z, axis3, axis4);
            this.points3d = this.ConvertTo3D(points4d, 3);
            this.ApplyForce();
        }
        protected virtual void OnRenderObject()
        {
            if (!active)
            {
                return;
            }
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                /*
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                */
                
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }

            lineMaterial.SetPass(0);

            switch (drawMode)
            {
                case DrawMode.Line:
                    GL.Begin(GL.LINES);
                    for (var i = 0; i < this.edges.Count; i++)
                    {
                        var e = this.edges[i];
                        var ca = this.colors[e.x];
                        var cb = this.colors[e.y];
                        ca.a *= alpha;
                        cb.a *= alpha;
                        if (e.x >= this.points3d.Count || e.x >= this.points3d.Count)
                        {
                            continue;
                        }
                        GL.Color(ca); 
                        GL.Vertex(this.transform.TransformPoint(this.points3d[e.x])); 
                        GL.Color(cb); 
                        GL.Vertex(this.transform.TransformPoint(this.points3d[e.y])); 
                    }
                    GL.End();
                    break;
                case DrawMode.Quad:
                    GL.Begin(GL.QUADS);
                    for (var i = 0; i < this.edges.Count; i++)
                    {
                        var e = this.edges[i];
                        var ca = this.colors[e.x];
                        var cb = this.colors[e.y];
                        ca.a *= alpha;
                        cb.a *= alpha;
                        if (e.x >= this.points3d.Count || e.x >= this.points3d.Count)
                        {
                            continue;
                        }
                        SetThickLineToGLQuad(
                            this.transform.TransformPoint(this.points3d[e.x]), ca, 
                            this.transform.TransformPoint(this.points3d[e.y]), cb, 0.03f);
                    }
                    GL.End();
                    break;
                case DrawMode.Tube:
                    GL.Begin(GL.TRIANGLES);
                    for (var i = 0; i < this.edges.Count; i++)
                    {
                        var e = this.edges[i];
                        var ca = this.colors[e.x];
                        var cb = this.colors[e.y];
                        ca.a *= alpha;
                        cb.a *= alpha;
                        if (e.x >= this.points3d.Count || e.x >= this.points3d.Count)
                        {
                            continue;
                        }
                        SetThickLineToGL(
                            this.transform.TransformPoint(this.points3d[e.x]), ca, 
                            this.transform.TransformPoint(this.points3d[e.y]), cb, 0.03f);
                    }
                    GL.End();
                    break;
                default:
                    break;
            }
        }
        private void SetThickLineToGLQuad(Vector3 a, Color ca, Vector3 b, Color cb, float width) 
        { 
            var dir = (b - a).normalized; // カメラに対して垂直な方向を作る
            var normal = Vector3.Cross(dir, targetCamera.transform.forward).normalized; 
            var offset = normal * (width * 0.5f); 
            var v0 = a - offset; 
            var v1 = a + offset; 
            var v2 = b + offset; 
            var v3 = b - offset; 
            GL.Color(ca); 
            GL.Vertex(v0); 
            GL.Color(ca); 
            GL.Vertex(v1); 
            GL.Color(cb); 
            GL.Vertex(v2); 
            GL.Color(cb); 
            GL.Vertex(v3); 
        }
        private void SetThickLineToGL(Vector3 a, Color ca, Vector3 b, Color cb, float width)
        {
            const int sides = 3; // 6〜12くらいが実用的。小さいほど軽い
            float radius = width * 0.5f;

            Vector3 axis = b - a;
            float length = axis.magnitude;

            if (length < 1e-6f || radius <= 0f)
                return;

            Vector3 dir = axis / length;

            // dir に垂直な2軸を作る
            Vector3 refAxis = Vector3.up;

            // dir と refAxis がほぼ平行だと外積が潰れるので回避
            if (Mathf.Abs(Vector3.Dot(dir, refAxis)) > 0.95f)
                refAxis = Vector3.right;

            Vector3 n1 = Vector3.Cross(dir, refAxis).normalized;
            Vector3 n2 = Vector3.Cross(dir, n1).normalized;

            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;

                float t0 = Mathf.PI * 2f * i / sides;
                float t1 = Mathf.PI * 2f * j / sides;

                Vector3 o0 = (Mathf.Cos(t0) * n1 + Mathf.Sin(t0) * n2) * radius;
                Vector3 o1 = (Mathf.Cos(t1) * n1 + Mathf.Sin(t1) * n2) * radius;

                Vector3 a0 = a + o0;
                Vector3 a1 = a + o1;
                Vector3 b0 = b + o0;
                Vector3 b1 = b + o1;

                // 側面 quad を三角形2枚で描く
                // Triangle 1: a0 -> b0 -> b1
                GL.Color(ca);
                GL.Vertex(a0);

                GL.Color(cb);
                GL.Vertex(b0);

                GL.Color(cb);
                GL.Vertex(b1);

                // Triangle 2: a0 -> b1 -> a1
                GL.Color(ca);
                GL.Vertex(a0);

                GL.Color(cb);
                GL.Vertex(b1);

                GL.Color(ca);
                GL.Vertex(a1);
            }
        }

        protected virtual void ApplyForce()
        {
            this.GetComponent<Rigidbody>().AddForce(
                (this.attractor.transform.position - this.transform.position) * this.attractForce
            );
            this.GetComponent<Rigidbody>().linearVelocity *= 0.95f;
        }

        protected abstract void SetPoints();

        protected abstract void SetEdges();

        protected virtual void SetColors()
        {
            colors.Clear();
            foreach (var p in points4d)
            {
                var u = Mathf.Atan2(p.y, p.x);
                var v = Mathf.Atan2(p.w, p.z);
                var hue = (u / (2 * Mathf.PI) + 1f) % 1f;
                var value = (v / (2 * Mathf.PI) + 1f) * 0.5f;
                var c = Color.HSVToRGB(hue, 1f, value);
                c.a = alpha;
                colors.Add(c);
            }
        }

        protected void Rotation4d(ref List<Vector4> points, float angle, int ax1, int ax2)
        {
            var matR = Matrix4x4.identity;
            matR[ax1, ax1] = Mathf.Cos(angle);
            matR[ax1, ax2] = - Mathf.Sin(angle);
            matR[ax2, ax1] = Mathf.Sin(angle);
            matR[ax2, ax2] = Mathf.Cos(angle);
            for (var i = 0; i < points.Count; i++)
            {
                points[i] = matR * points[i];
            }
        }
        protected List<Vector3> ConvertTo3D(List<Vector4> points, float d)
        {
            var reduced = new List<Vector3>();
            points.ForEach(p =>
            {
                var parse = 1f / (d - p.w);
                reduced.Add(new Vector3(p.x, p.y, p.z) * (parse * normalizeScaler));
            });
            return reduced;
        }
    }
}