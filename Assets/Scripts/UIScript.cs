using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public bool scaleWithDistance = false;
    public float scaleMultiplier = 1f;
    private Transform camTrans;
    private Transform trans;
    private float size;
    void Awake()
    {
        camTrans = Camera.main.transform;
        trans = transform;
    }
    void Update()
    {
        transform.LookAt(trans.position + camTrans.rotation * Vector3.forward,
            camTrans.rotation * Vector3.up);

        if (!scaleWithDistance)
            return;
        size = (camTrans.position - transform.position).magnitude;
        transform.localScale = Vector3.one * (size * (scaleMultiplier / 100f));
    }
}

