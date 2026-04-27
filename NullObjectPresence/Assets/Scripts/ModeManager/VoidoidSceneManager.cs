using System.Collections.Generic;
using NullObjectPresence.HyperObject;
using UnityEngine;

namespace NullObjectPresence
{
    public class VoidoidSceneManager : BaseSceneManager
    {
        [SerializeField] private List<BaseHyperObject> hyperObjects;
        [SerializeField] private AnimationCurve alphaCurve;
        private int index = 0;
        private float tmpTime = 0;
        
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            // オブジェクトを有効にする
            hyperObjects[index].gameObject.SetActive(true);
            hyperObjects[index].Active = true;
            hyperObjects[index].Alpha = v * alphaCurve.Evaluate(0);
        }

        protected override void Start()
        {
            base.Start();
            return;
            // 配置生成用
            this.cubesArrangements.Clear();
            for (int i = 0; i < 10; i++)
            {
                var rad = 2.2f;
                var theta = 2 * Mathf.PI * i / 10;
                var pos = new Vector3(Mathf.Cos(theta)* rad, Mathf.Sin(theta) * rad, 0);
                pos = new Vector3(pos.x, pos.y * Mathf.Cos(Mathf.PI / 4), pos.y * Mathf.Sin(Mathf.PI / 4));
                pos += Vector3.up * 2.5f;
                var arr = new Arrangement()
                {
                    position = pos,
                    rotation = new Vector3(45, 0, i * 36)
                };
                this.cubesArrangements.Add(arr);
            }
        }
        
        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
        }
        
        public override void Exit(float v)
        {
            base.Exit(v);
            hyperObjects[index].Alpha = (1 - v) * alphaCurve.Evaluate(tmpTime);
        }

        // オブジェクトを無効にする
        public override void FinishExit()
        {
            foreach (var obj in hyperObjects)
            {
                obj.gameObject.SetActive(false);
                obj.Active = false;
                obj.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                obj.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }

        protected override void UpdateState(float v)
        {
            hyperObjects[index].Alpha = alphaCurve.Evaluate(v);
            tmpTime = v;
        }

        protected override void LoopProcess()
        {
            hyperObjects[index].gameObject.SetActive(false);
            hyperObjects[index].Active = false;
            index = (index + 1) % hyperObjects.Count;
            hyperObjects[index].gameObject.SetActive(true);
            hyperObjects[index].Active = true;
        }
    }
}