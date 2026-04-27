using UnityEngine;

namespace NullObjectPresence.HyperObject
{
    [RequireComponent(typeof(BaseHyperObject))]
    public class HyperObjectController : MonoBehaviour
    {
        private BaseHyperObject obj;
        [SerializeField] AnimationCurve alphaCurve;
        [SerializeField] AnimationCurve scaleCurve;
        public void Awake()
        {
            obj =  gameObject.GetComponent<BaseHyperObject>();
        }

        private void Update()
        {
            obj.Alpha = alphaCurve.Evaluate(Time.time);
            obj.Scaler = scaleCurve.Evaluate(Time.time);
            if (obj.Active)
            {
                if (obj.Scaler <= 0 || obj.Alpha <= 0)
                {
                    obj.Active = false;
                }
            }
            else
            {
                if (obj.Scaler > 0 && obj.Alpha > 0)
                {
                    obj.Active = true;
                }
            }
        }
    }
}