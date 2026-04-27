using UnityEngine;

namespace NullObjectPresence
{
    public class SpringSceneManager : BaseSceneManager
    {
        [SerializeField] private ParticleSystem particleSystem;
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            // 桜を散らす
            particleSystem.Play();
            var module = particleSystem.emission;
            module.rateOverTime = v * 10;
        }
        
        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
        }

        // 桜を散らす
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