using System.Collections.Generic;
using ASK.Core;
using Mechanics;
using Player;
using UnityEngine;
using VFX;

public class GrappleRenderer : MonoBehaviour {
    private LineRenderer _lr;
    private PlayerGrapplerStateMachine _parent;
    [SerializeField] private Transform anchor;
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private GameObject curvePointPrefab;
    [SerializeField] private float leafSpacing;
    [SerializeField] private float minLeafDistance;
    private List<Leaf> _leaves = new();
    private List<GrappleCurvePoint> _curvePoints = new();
    [SerializeField] private float curveSpeed;

    
    private void Awake() {
        _lr = GetComponent<LineRenderer>();
        _parent = transform.parent.GetComponent<PlayerGrapplerStateMachine>();
    }


    void Update() {
        if (_parent.IsGrappleExtending()) {
            Vector2 v = _parent.GetGrappleExtendPos();
            _lr.enabled = true;
            UpdatePoints(v);
        } else if (_parent.IsGrappling()) {
            _lr.enabled = true;
            UpdatePoints(_parent.GetGrapplePos());
        } else {
            RenderOff();
        }
    }

    private void RenderOff()
    {
        _lr.enabled = false;
        foreach (var leaf in _leaves)
        {
            leaf.SetEnabled(false);
        }
    }

    private void UpdatePoints(Vector2 p1) {
        Vector2 p0 = anchor.position;
        _lr.SetPosition(0, p0);
        _lr.SetPosition(1, p1);

        Vector2 vineVector = p1 - p0;
        
        
        int numLeaves = (int)Mathf.Max(0, (vineVector.magnitude-minLeafDistance)/ leafSpacing);
        float vectorAngle = Vector2.SignedAngle(Vector2.right, vineVector);
        
        for (int l = 0; l < numLeaves; ++l)
        {
            if (l >= _leaves.Count)
            {
                var newLeaf = Instantiate(leafPrefab, transform).GetComponent<Leaf>();
                _leaves.Add(newLeaf);

            }

            var curLeaf = _leaves[l];
            curLeaf.SetEnabled(true);
            curLeaf.transform.position = p0 + vineVector.normalized * (minLeafDistance + l * leafSpacing);
            curLeaf.SetRotation(vectorAngle + (l % 2 == 0 ? -90 : 90));
            //curLeaf.ScaleSize();
        }
        
        for (int l = numLeaves; l < _leaves.Count; ++l) _leaves[l].SetEnabled(false);
    }


}