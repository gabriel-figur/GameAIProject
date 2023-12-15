using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class EnemyBulletScript : MonoBehaviour
{
    private Rigidbody rigidbody;
    public float bulletVelocity = 1f;

    public int damage = 5;
    // Start is called before the first frame update
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, 2.0f);
    }

    public void InitializeBullet(Vector3 originalDirection)
    {
        transform.forward = originalDirection;
        rigidbody.velocity = transform.forward * bulletVelocity;
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
