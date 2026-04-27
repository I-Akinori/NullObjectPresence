using UnityEngine;

namespace NullObjectPresence
{
    public class HollowGlobeManager : BaseSceneManager
    {
        [SerializeField] private HandManager leftHand;
        [SerializeField] private HandManager rightHand;
        [SerializeField] private GameObject core;
        
        public override void Entry(float v, BaseSceneManager prevMng)
        {
            base.Entry(v, prevMng);
            core.gameObject.SetActive(true);
            core.GetComponent<Renderer>().material.SetFloat("_Alpha", v);
        }
        
        public override void FinishEntry(BaseSceneManager prevMng)
        {
            base.FinishEntry(prevMng);
            leftHand.EnableTrail = true;
            rightHand.EnableTrail = true;
            core.GetComponent<Renderer>().material.SetFloat("_Alpha", 1);
        }

        public override void Exit(float v)
        {
            base.Exit(v);
            leftHand.EnableTrail = false;
            rightHand.EnableTrail = false;
            core.GetComponent<Renderer>().material.SetFloat("_Alpha", 1 - v);
        }

        public override void FinishExit()
        {
            core.GetComponent<Renderer>().material.SetFloat("_Alpha", 0);
            core.gameObject.SetActive(false);
        } 
    }
}