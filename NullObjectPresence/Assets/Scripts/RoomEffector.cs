using System.Collections.Generic;
using UnityEngine;

namespace NullObjectPresence
{
    public class RoomEffector : MonoBehaviour
    {
        [SerializeField] private List<Material> materials;
        [SerializeField] private List<BaseSceneManager> sceneManagersList = new List<BaseSceneManager>();
        [SerializeField] private AnimationCurve distortCurve;
        [SerializeField] private Transform floor;
        [SerializeField] private Transform wall;
        [SerializeField] private Transform testObject;
        private Dictionary<SceneMode, Color> sceneManagers = new Dictionary<SceneMode, Color>();
        private float maxPlayTime = 5f;
        private float playTime = 0f;
        private float maxAdChangingTime = 5f;
        private float adChangingTime = 0f;
        private bool isA2D = true;
        private float maxDistiortionTime = 10f;
        [SerializeField] private float[] distortionTimes = new float[16];
        [SerializeField] private Vector4[] distortionCenter = new Vector4[16];
        private int nextDistortionIndex = 0;

        private static Vector2 WorldToHorizontalPlaneUV(Vector3 worldPos, Transform planeTransform)
        {
            //var local = planeTransform.InverseTransformPoint(worldPos);
            var origin = new Vector2(planeTransform.position.x, planeTransform.position.z);
            origin += new Vector2(- planeTransform.localScale.x * 5, - planeTransform.localScale.z * 5);
            var vec = new Vector2(worldPos.x, worldPos.z) - origin;
            vec = new Vector2(vec.x / (planeTransform.localScale.x * 10), 
                vec.y / (planeTransform.localScale.z * 20));
            Debug.Log(worldPos + " -> " + vec);
            return vec;
        }
        public static Vector2 WorldToVerticalPlaneUV(Vector3 worldPos, Transform planeTransform)
        {
            var origin = new Vector2(planeTransform.position.x, planeTransform.position.y);
            origin += new Vector2(- planeTransform.localScale.x * 5, - planeTransform.localScale.z);
            origin = new Vector2(origin.x, 0);  //計算がうまくいかないので対処
            Debug.Log(origin);
            var vec = new Vector2(worldPos.x, worldPos.y) - origin;
            vec = new Vector2(vec.x / (planeTransform.localScale.x * 10), 
                vec.y / (planeTransform.localScale.z * 20) + 0.5f);
            Debug.Log(worldPos + " -> " + vec);
            return vec;
        }
        
        public void StartDistortion(Vector3 position, bool isFloor)
        {
            var uv = Vector4.zero;
            if (isFloor)
            {
                uv = WorldToHorizontalPlaneUV(position, floor);
            }
            else
            {
                uv = WorldToVerticalPlaneUV(position, floor);
            }
            distortionTimes[nextDistortionIndex] = maxDistiortionTime;
            distortionCenter[nextDistortionIndex] = uv;
            foreach (var material in materials)
            {
                material.SetVectorArray("_DistortionCenter", distortionCenter);
            }
            
            nextDistortionIndex++;
            if (nextDistortionIndex >= distortionTimes.Length)
            {
                nextDistortionIndex = 0;
            }
        }

        public void StartAnalogToDigital()
        {
            isA2D = true;
            adChangingTime = maxAdChangingTime;
        }
        
        public void StartDigitalToAnalog()
        {
            isA2D = false;
            adChangingTime = maxAdChangingTime;
        }
        public void StartEffect(SceneMode mode, Vector3 position)
        {
            foreach (var material in materials)
            {
                material.SetVector("_Center", position);
                material.SetColor("_EffectColor", sceneManagers[mode]);
            }
            playTime = maxPlayTime;
        }
        void Start()
        {
            foreach (BaseSceneManager manager in sceneManagersList)
            {
                sceneManagers[manager.Mode] = manager.EffectColor;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartDistortion(testObject.position, testObject.position.y < 1);
            }
            var dt = Time.deltaTime;
            // 波紋エフェクト
            if (playTime > 0)
            {
                playTime -= dt;
                foreach (var material in materials)
                {
                    material.SetFloat("_Value", 1 - playTime / maxPlayTime);
                }
            }
            // AD変換
            if (adChangingTime > 0)
            {
                adChangingTime -= dt;
                var timeRatio = 1 - adChangingTime / maxAdChangingTime; // 0 -> 1
                foreach (var material in materials)
                {
                    material.SetFloat("_ADRatio", 
                        isA2D ? timeRatio : 1 - timeRatio);
                }
            }
            // Distortion
            var distortionIntensity = new float[distortionTimes.Length];
            var all0 = true;
            for (var i = 0; i < distortionTimes.Length; i++)
            {
                var dTime = distortionTimes[i];
                distortionIntensity[i] = 0;
                if (dTime > 0)
                {
                    all0 = false;
                    distortionTimes[i] -= dt;
                    var timeRatio = 1 - distortionTimes[i] / maxDistiortionTime; // 0 -> 1
                    distortionIntensity[i] = distortCurve.Evaluate(timeRatio);
                    Debug.Log(distortionIntensity[i]);
                }
            }

            if (!all0)
            {
                foreach (var material in materials)
                {
                    material.SetFloatArray("_DistortionIntensity", distortionIntensity);
                }
            }
        }
    }

    
}