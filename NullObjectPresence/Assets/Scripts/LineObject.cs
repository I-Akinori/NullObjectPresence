using System.Collections.Generic;
using UnityEngine;

public class LineObject : MonoBehaviour
{
    private LineRenderer line;
    private List<Vector3> points = new List<Vector3>();
    private List<Vector3> velocities = new List<Vector3>();
    private List<float> lifeTimes = new List<float>();
    private float lifeTime = 10f;
    private bool isDrawing;

    void Awake()
    {
        if (!TryGetComponent<LineRenderer>(out var lr))
        {
            this.line = this.gameObject.AddComponent<LineRenderer>();
        }
        isDrawing = true;
    }
    void Update()
    {
        UpdateLifeTimes();
        DrawLine();
        if (isDrawing)
        {
            return;
        }
        UpdatePosition();
    }

    public void Init(Color sc, Color ec, float sw, float ew, Material material)
    {
        if (line == null)
        {
            return;
        }
        line.material = material;
        line.startColor = sc;
        line.endColor = ec;
        line.startWidth = sw;
        line.endWidth = ew;
    }

    public void Release()
    {
        isDrawing = false;
    }

    public void AddPoint(Vector3 point)
    {
        this.points.Add(point);
        this.velocities.Add(Vector3.up * 1f);
        this.lifeTimes.Add(lifeTime);
    }

    public void UpdateHead(Vector3 point)
    {
        if (this.points.Count > 0)
        {
            this.points[^1] = point;
        }
        
    }

    private void UpdatePosition()
    {
        // whole
        var radius = 3;
        var center = new Vector3(0f, 3.5f, 0f);
        var wholeVel = Vector3.zero;
        for (var i = 0; i < points.Count; i++)
        {
            center += points[i];
            wholeVel += velocities[i];
        }
        center /= points.Count;
        wholeVel /= points.Count;
        {
            var localPos = center - Vector3.zero;
            var radForce = localPos * (radius - localPos.magnitude);
            var nor = localPos.normalized;
            var dir = wholeVel.normalized;
            var tan = Vector3.Cross(dir, nor);
            var angle = Mathf.PerlinNoise1D(Time.time * 1.0f) - 0.5f;
            angle *= Mathf.PI;
            var swayForce = dir * Mathf.Cos(angle) + tan * Mathf.Sin(angle);
            var waveForce = nor * Mathf.PerlinNoise1D(100 - Time.time * 1.0f);
            wholeVel += (radForce * 0.5f + swayForce * 1.0f 
                                         + waveForce * 0.1f) * Time.deltaTime;
        }
        for (var i = 0; i < velocities.Count; i++)
        {
            var localPos = points[i] - Vector3.zero;
            var radForce = localPos * (radius - localPos.magnitude);
            var nor = localPos.normalized;
            var dir = velocities[i].normalized;
            var tan = Vector3.Cross(dir, nor);
            var angle = Mathf.PerlinNoise1D(Time.time * 1.0f) - 0.5f;
            angle *= Mathf.PI;
            var swayForce = dir * Mathf.Cos(angle) + tan * Mathf.Sin(angle);
            var waveForce = nor * Mathf.PerlinNoise1D(100 - Time.time * 1.0f);
            velocities[i] += (radForce * 0.5f + swayForce * 1.0f 
                                              + waveForce * 0.1f) * Time.deltaTime;
            velocities[i] += wholeVel * 0.001f;
            velocities[i] /= velocities[i].magnitude;
            velocities[i] *= lifeTimes[i] / lifeTime;
        }
        for (var i = 0; i < velocities.Count; i++)
        {
            points[i] += velocities[i] * Time.deltaTime;
        }
    }
    
    private void UpdateLifeTimes()
    {
        var dt = Time.deltaTime;
        for (var i = this.lifeTimes.Count - 1; i >= 0; i--)
        {
            lifeTimes[i] -= dt;
            if (lifeTimes[i] > 0)
            {
                continue;
            }
            this.points.RemoveAt(i);
            this.velocities.RemoveAt(i);
            this.lifeTimes.RemoveAt(i);
            this.line.positionCount = this.points.Count;

            if (points.Count == 0)
            {
                Destroy(this.line.material);
                Destroy(this.gameObject);
            } 
        }
    }
    
    private void DrawLine()
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}
