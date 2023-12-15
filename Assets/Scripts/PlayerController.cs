using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
   
    private NavMeshAgent agent;
    public float fireRate = 0.75f;
    private float nextFire;
    public GameObject bulletPref;
    public Transform bulletOrigin;
    public int hp = 100;
    public Slider hpBar;
    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        if(Input.GetKey(KeyCode.Space))
            Shoot();
    }

    void Move()
    {
        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horInput, 0f, verInput);
        agent.Move(movement * agent.speed * Time.deltaTime);
        
        if (movement.magnitude > 0.1f)
        {
            Quaternion newRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 10f * Time.deltaTime);
        }
    }
    
    void Shoot()
    {
        if (Time.time > nextFire + fireRate)
        {
            nextFire = Time.time + fireRate;
            GameObject bullet = Instantiate(bulletPref, bulletOrigin.position, Quaternion.identity);
            bullet.GetComponent<BulletScript>()?.InitializeBullet(transform.rotation * Vector3.forward);
        }
    }
    
    void TakeDamage(int damage)
    {
        hp -= damage;
        hpBar.value = hp;
        if (hp <= 0)
            PlayerDead();
    }
    
    void PlayerDead()
    {
        gameObject.SetActive(false);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            EnemyBulletScript bullet = collision.gameObject.GetComponent<EnemyBulletScript>();
            TakeDamage(bullet.damage);
            Debug.Log("PLAYER DAMAGED");
        }
    }
}
