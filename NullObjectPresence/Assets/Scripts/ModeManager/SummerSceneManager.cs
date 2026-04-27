using UnityEngine;

namespace NullObjectPresence
{
    public class SummerSceneManager : BaseSceneManager
    {
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private AnimationCurve rainRatio;
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            // 雨を降らす
            particleSystem.Play();
            var module = particleSystem.emission;
            module.rateOverTime = v * rainRatio.Evaluate(0) * 10;
        }
        
        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
        }

        // 雨を止ます
        public override void Exit(float v)
        {
            base.Exit(v);
            var module = particleSystem.emission;
            module.rateOverTime = (1 - v) * rainRatio.Evaluate(0) * 10;
        }

        public override void FinishExit()
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        } 
        protected override void UpdateState(float v)
        {
            base.UpdateState(v);
            var module = particleSystem.emission;
            module.rateOverTime = rainRatio.Evaluate(v) * 10;
        }
    }
}