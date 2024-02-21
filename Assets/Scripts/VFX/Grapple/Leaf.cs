using UnityEngine;
using System.Collections;
using ASK.Core;

namespace VFX
{
    // private Vector3 scaleAmount = new Vector3(0.1f,0.1f,0.1f);
    public class Leaf : MonoBehaviour
    {
        private Vector3 targetScale = new Vector3(1f, 1f, 1f);
        private float growthDuration = 0.5f;
        [SerializeField] private float growthRate=3f;
        [SerializeField] private float fastGrowthRate=8f;
        



        public AnimationCurve animationCurve;
        void OnEnable(){
            ScaleSize();
        }
        public void SetEnabled(bool b) => gameObject.SetActive(b);

        public void SetRotation(float angle)
        {      
            transform.rotation = Quaternion.Euler(0, 0 ,angle);
        }

        public void ScaleSize()
        {
            float elapsedTime = 0f; 
            float graphValue = animationCurve.Evaluate(elapsedTime/growthDuration);
            StartCoroutine(IncreaseSizeCoroutine());
            
        }
        private IEnumerator IncreaseSizeCoroutine()
        {
            float elapsedTime = 0f; 
            float graphValue = animationCurve.Evaluate(elapsedTime/growthDuration);
            

            while (elapsedTime < growthDuration)
            {
                if(elapsedTime<0.2){
                    graphValue = animationCurve.Evaluate(elapsedTime/growthDuration);
                    transform.localScale = targetScale*graphValue;
                    elapsedTime += Game.TimeManager.DeltaTime*growthRate;
                }else{
                    graphValue = animationCurve.Evaluate(elapsedTime/growthDuration);
                    transform.localScale = targetScale*graphValue;
                    elapsedTime += Game.TimeManager.DeltaTime*fastGrowthRate;
                }

                yield return null;

            }

        }
    }
}