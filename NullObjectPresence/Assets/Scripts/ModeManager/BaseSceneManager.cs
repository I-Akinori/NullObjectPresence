using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace NullObjectPresence
{
    public abstract class BaseSceneManager : MonoBehaviour
    {
        [SerializeField] protected SceneMode mode;
        [SerializeField] protected Color effectColor;
        [SerializeField] protected Gradient lightColor;
        [SerializeField] protected float loopTime = 20f;
        [SerializeField] private Light light;
        [SerializeField] protected CubesManager cubesManager;
        [SerializeField] protected List<Arrangement> cubesArrangements = new List<Arrangement>();
        protected float timeRatio = 0;
        protected bool isPlaying = false;
        protected bool startEntry = false;

        public SceneMode Mode => mode;
        public Color EffectColor => effectColor;
        public virtual void Entry(float v, BaseSceneManager prevMng)
        {
            if (!startEntry)
            {
                // 初回のみ実行
                cubesManager.StartMove(cubesArrangements);
                startEntry = true;
            }
            // ブレンドが必要なもの：ライトの色
            var col = Color.Lerp(prevMng.GetColor(), this.GetColor(), v);
            UpdateLight(col);
            prevMng.Exit(v);
        }
        public virtual void FinishEntry(BaseSceneManager prevMng)
        {
            prevMng.FinishExit();
            isPlaying = true;
            startEntry = false; // 初回でないフラグを下げておく
        }

        public virtual void Exit(float v)
        {
            isPlaying = false;
            
        }
        public virtual void FinishExit()
        {
        }

        protected virtual void Start()
        {
            Debug.Log("Start");
            // 修了処理だけやっておく
            FinishExit();
        }

        protected virtual void Update()
        {
            if (!isPlaying)
            {
                return;
            }
            timeRatio += Time.deltaTime / loopTime;
            if (timeRatio >= 1)
            {
                LoopProcess();
                timeRatio = 0;
            }
            
            UpdateLight(lightColor.Evaluate(timeRatio));
            UpdateState(timeRatio);
        }

        protected virtual void UpdateLight(Color color)
        {
            light.color = color;
        }
        protected virtual void UpdateState(float v)
        {
            
        }
        protected virtual void LoopProcess()
        {
            
        }

        protected Color GetColor()
        {
            return lightColor.Evaluate(timeRatio);
        }
    }
}