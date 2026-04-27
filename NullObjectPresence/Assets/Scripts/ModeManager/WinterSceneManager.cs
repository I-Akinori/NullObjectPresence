using UnityEngine;

namespace NullObjectPresence
{
    public class WinterSceneManager : BaseSceneManager
    {
        [SerializeField] private ParticleSystem particleSystem;
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            // 雪を降らす
            particleSystem.Play();
            var module = particleSystem.emission;
            module.rateOverTime = v * 10;
        }
        
        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
        }

        // 雪を止ます
        public override void Exit(float v)
        {
            base.Exit(v);
            var module = particleSystem.emission;
            module.rateOverTime = (1 - v) * 10;
        }

        public override void FinishExit()
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        } 
    }
}