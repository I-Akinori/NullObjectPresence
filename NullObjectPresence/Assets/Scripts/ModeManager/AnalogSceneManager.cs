using UnityEngine;
using UnityEngine.Events;

namespace NullObjectPresence
{
    public class AnalogSceneManager : BaseSceneManager
    {
        [SerializeField] private GameObject backGround;
        [SerializeField] private UnityEvent action;
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            // 街の背景を出現
            backGround.GetComponent<Renderer>().material.SetFloat("_Alpha", v);
            action?.Invoke();
        }

        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
            backGround.GetComponent<Renderer>().material.SetFloat("_Alpha", 1);
            this.cubesManager.SetGravity(true);
        }

        // Exitは特に何もしない
    }
}