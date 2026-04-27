using System.Collections.Generic;
using Leap;
using NUnit.Framework;
using UnityEngine;

namespace NullObjectPresence
{
    public class HandManager : HandModelBase
    {
        private Hand hand_;
        //private List<LineObject> lines = new List<LineObject>();
        private LineObject currentLine = null;
        private List<LineRenderer> fingerViewer = new List<LineRenderer>();
        private GameObject[,] objects = new GameObject[5, 4];
        private float time = 0f;
        private float sampleRatio = 0.02f;
        
        [SerializeField] private float startWidth;
        [SerializeField] private float endWidth;
        [SerializeField] private Color startCol;
        [SerializeField] private Color endCol;
        [SerializeField] private Material handMaterial;
        [SerializeField] private Material material;
        
        /** The colors used for each bone. */
        protected Color[] colors = { Color.gray, Color.yellow, Color.cyan, Color.magenta };
        
        [Header("Thresholds")]
        [UnityEngine.Range(0f, 1f)] public float pinchOn = 0.75f;
        [UnityEngine.Range(0f, 1f)] public float pinchOff = 0.60f;
        [UnityEngine.Range(0f, 1f)] public float grabOn  = 0.85f;
        [UnityEngine.Range(0f, 1f)] public float grabOff = 0.70f;

        private bool pinching;
        private bool grabbing;
        private bool drawing;
        private float samplingInterval = 0.05f;
        private float samplingTime = 0f;
        public bool EnableTrail
        {
            set
            {
                enableTrail = value;
                if (!enableTrail)
                {
                    EndLine();
                }
            }
            get => enableTrail;
        }

        private bool enableTrail = false;

        public override ModelType HandModelType
        {
            get
            {
                return ModelType.Graphics;
            }
        }

        [SerializeField]
        private Chirality handedness;
        public override Chirality Handedness
        {
            get
            {
                return handedness;
            }
            set { }
        }

        public override Hand GetLeapHand()
        {
            return hand_;
        }

        public override void SetLeapHand(Hand hand)
        {
            hand_ = hand;
        }

        public override bool SupportsEditorPersistence()
        {
            return false;
        }

        public override void InitHand()
        {
            DrawDebugLines();
            InitColliders();
            InitViewer();
        }

        public override void UpdateHand()
        {
            DrawDebugLines();
            UpdateColliders();
            UpdateLine();
            JudgeGesture(GetLeapHand());
        }
        
        public override void FinishHand()
        {
            EndLine();
            FinishColliders();
            FinishViewer();
        }

        private void GenerateCollider(int finger, int index)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.tag = "Player";
            obj.GetComponent<Renderer>().enabled = false;
            obj.transform.localScale = 0.01f *  Vector3.one;
            obj.transform.parent = this.transform;
            obj.AddComponent<Rigidbody>().isKinematic = true;
            objects[finger, index] = obj;
        }

        private void InitColliders()
        {
            for (var f = 0; f < 5; f++)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (objects[f, i] != null)
                    {
                        continue;
                    }
                    GenerateCollider(f, i);
                }
            }
        }

        private void InitViewer()
        {
            if (fingerViewer.Count == 5)
            {
                return;
            }
            for (var f = 0; f < 5; f++)
            {
                var obj = new GameObject();
                obj.transform.parent = this.transform;
                var line = obj.AddComponent<LineRenderer>();
                line.material = new Material(handMaterial);
                line.startColor = Color.white;
                line.endColor = new Color(1, 1, 1, 0);
                line.startWidth = 0.1f;
                line.endWidth = 0.1f;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                fingerViewer.Add(line);
            }
            Debug.Log("InitViewer: " + fingerViewer.Count);
        }

        private void UpdateColliders()
        {
            var hand = GetLeapHand();
            
            for (var f = 0; f < 5; f++)
            {
                var finger = hand.fingers[f];
                for (var i = 0; i < 4; i++)
                {
                    if (objects[f, i] == null)
                    {
                        GenerateCollider(f, i);
                        continue;
                    }
                    var bone = finger.GetBone((Bone.BoneType)i);
                    objects[f, i].transform.position = bone.Center;
                }
            }
        }
        private void FinishColliders()
        {
            for (var f = 0; f < 5; f++)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (objects[f, i] == null)
                    {
                        continue;
                    }
                    Destroy(objects[f, i]);
                }
            }
        }

        private void FinishViewer()
        {
            foreach (var f in fingerViewer)
            {
                f.positionCount = 0;
            }
        }

        private void JudgeGesture(Hand hand)
        {
            float p = hand.PinchStrength; // 0..1
            float g = hand.GrabStrength;  // 0..1

            // Pinch
            var lr = hand.IsLeft ? "L" : "R";
            if (!pinching && p >= pinchOn) { pinching = true; Debug.Log($"{lr} Pinch START  p={p:F2}"); }
            if ( pinching && p <= pinchOff){ pinching = false;Debug.Log($"{lr} Pinch END    p={p:F2}"); }

            // Grab
            if (!grabbing && g >= grabOn)
            {
                grabbing = true;
                EndLine();
                Debug.Log($"{lr} Grab  START  g={g:F2}");
            }

            if (grabbing && g <= grabOff)
            {
                grabbing = false;
                Debug.Log($"{lr} Grab  END    g={g:F2}");
            }

            if (enableTrail)
            {
                if (drawing && g >= grabOn)
                {
                    EndLine();
                }
                if (!drawing && g <= grabOff)
                {
                    StartLine();
                }
            }
        }

        private void StartLine()
        {
            drawing = true;
            samplingTime = 0f;
            var obj = new GameObject("Line");
            obj.transform.parent = this.transform;
            currentLine = obj.AddComponent<LineObject>();
            currentLine.Init(startCol, endCol, startWidth, endWidth, material);
        }

        private void EndLine()
        {
            drawing = false;
            if (currentLine != null)
            {
                currentLine.Release();
                currentLine = null;
            }
            samplingTime = 0f;
        }
        private void UpdateLine()
        {
            samplingTime += Time.deltaTime;
            if (!drawing || currentLine == null)
            {
                return;
            }

            // 次のサンプリングまで、先頭は更新する (断続的な延長を防ぐため)
            if (samplingTime < samplingInterval)
            {
                currentLine.UpdateHead(GetLeapHand().fingers[1].GetBone((Bone.BoneType)3).Center);
                return;
            }

            samplingTime = 0f;
            currentLine.AddPoint(
                GetLeapHand().fingers[1].GetBone((Bone.BoneType)3).Center);
        }
        private void DrawDebugLines()
        {
            Hand hand = GetLeapHand();
            //Debug.DrawLine(hand.Arm.ElbowPosition, hand.Arm.WristPosition, Color.red); //Arm
            //Debug.DrawLine(hand.WristPosition, hand.PalmPosition, Color.white); //Wrist to palm line
            //Debug.DrawLine(hand.PalmPosition, hand.PalmPosition + hand.PalmNormal * hand.PalmWidth / 2, Color.black); //Hand Normal

            for (var f = 0; f < 5; f++)
            { 
                var finger = hand.fingers[f];
                var points = new List<Vector3>();
                points.Add(hand.WristPosition);
                for (var i = 0; i < 4; ++i)
                {
                    var bone = finger.GetBone((Bone.BoneType)i);
                    points.Add(bone.Center);
                }
                DrawSpline(points, f);
            }
        }
        private void DrawSpline(List<Vector3> points, int index)
        {
            
            if (fingerViewer.Count != 5)
            {
                InitViewer();
            }
            var positions = new List<Vector3>();
            var resolution = 100;
            for (var i = 0; i < resolution; i++)
            {
                var t = (float)i / resolution;
                positions.Add(SplineInterpolation.Interpolation(points, t));
            }

            fingerViewer[index].positionCount = positions.Count;
            fingerViewer[index].SetPositions(positions.ToArray());
        }
        private void DrawDebugSpline(List<Vector3> points)
        {
            var resolution = 100f;
            var cp = points[0];
            var pp = cp;
            for (var i = 1; i < resolution; i++)
            {
                var t = i / resolution;
                cp = SplineInterpolation.Interpolation(points, t);
                Debug.DrawLine(pp, cp, Color.white);
                pp = cp;
            }
        }

    }
}