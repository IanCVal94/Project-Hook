using System;
using A2DK.Phys;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace VFX
{
    public class Scarf : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        private Actor _attachedActor;
        [SerializeField] private GameObject _scarfPointPrefab;
        [SerializeField] private float minSpacing;
        [SerializeField] private float maxSpacing;

        private ScarfPoint[] _scarfPoints;

        [SerializeField] private SpriteRenderer _sr;

        private void Awake()
        {
            _scarfPoints = GetComponentsInChildren<ScarfPoint>();
            _attachedActor = GetComponentInParent<Actor>();
            if (_attachedActor == null)
            {
                DestroyChildrenRuntime();
                Destroy(this);
            }
            transform.parent = _attachedActor.transform.parent;
        }

        public void DestroyChildrenEditor() => DestroyChildren(DestroyImmediate);
        
        public void DestroyChildrenRuntime() => DestroyChildren(Destroy);
        
        public void DestroyChildren(Action<GameObject> destroyFunc)
        {
            int n = transform.childCount;
            for (int i = n-1; i >= 0; --i)
            {
                destroyFunc?.Invoke(transform.GetChild(i).gameObject);
            }
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        
        
        public void InitScarfPoints(float numPoints)
        {
            for (int n = 0; n < numPoints; ++n)
            {
                var prevChild = n == 0 ? anchor : transform.GetChild(n-1);
                var curChild = Instantiate(_scarfPointPrefab, transform);

                curChild.name += $" ({n})";
                curChild.transform.localPosition = new Vector3(0, -n*minSpacing, 0);
                
                var scarfPoint = curChild.GetComponent<ScarfPoint>();
                scarfPoint.SetAttachedTo(prevChild.transform);
                #if UNITY_EDITOR
                EditorUtility.SetDirty(curChild);
                #endif
            }
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            #endif
        }

        void FixedUpdate()
        {
            // float workingMinSpacing = Mathf.Max(attachedActor.velocity.magnitude, minSpacing);
            float workingMaxSpacing = minSpacing + _attachedActor.velocity.magnitude;

            if (_sr != null)
            {
                _scarfPoints.ForEach(s => s.FlipGravityX = (int)_sr.transform.localScale.x);
            }
            
            for (int i = 0; i < _scarfPoints.Length; ++i)
            {
                _scarfPoints[i].CalcPos(minSpacing, maxSpacing);
            }
        }
    }
}