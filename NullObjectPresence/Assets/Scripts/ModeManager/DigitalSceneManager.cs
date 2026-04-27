using UnityEngine;
using UnityEngine.Events;

namespace NullObjectPresence
{
    public class DigitalSceneManager : BaseSceneManager
    {
        [SerializeField] private GameObject backGround;
        [SerializeField] private UnityEvent action;
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            // 街の背景を隠す
            backGround.GetComponent<Renderer>().material.SetFloat("_Alpha", 1 - v);
            action?.Invoke();
        }
        
        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
            backGround.GetComponent<Renderer>().material.SetFloat("_Alpha", 0);
            this.cubesManager.SetGravity(false);
        }
        
        // 特に何もしない
        public override void Exit(float v)
        {
            base.Exit(v);
            
        }
    }
}