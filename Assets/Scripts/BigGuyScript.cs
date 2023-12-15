using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BigGuyScript : MonoBehaviour
{
    #region VARIABLES
    public enum NPCStates
    {
        Patrol,
        Chase,
        Attack,
        Scared,
    }

    [SerializeField] Transform Player;
    [SerializeField] GameObject PlayerObject;
    [SerializeField] Material PatrolMaterial;
    [SerializeField] Material ChaseMaterial;
    [SerializeField] Material AttackMaterial;
    [SerializeField] Material ScareMaterial;
    [SerializeField] float ChaseRange = 20f;
    [SerializeField] float AttackRange = 15f;
    [SerializeField] float FearRange = 20f;

    public float fireRate = 0.5f;
    private float nextFire;
    int nextPatrolPoint = 0;
    NPCStates currentState = NPCStates.Patrol;
    NavMeshAgent navMeshAgent;
    MeshRenderer meshRenderer;
    public GameObject bulletPref;
    public Transform bulletOrigin;
    public int hp = 100;
    public Slider hpBar;
    public Text stateText;
    public float range = 10f;
    #endregion
    
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    void Update()
    {
        SwitchState();
        UpdateUI();
        if (PlayerObject.activeSelf == false)
            currentState = NPCStates.Patrol;

    }
    
    private void OnCollisionEnter(Collision collision)
    {
        //CHECK IF COLLIDING OBJECT IS A BULLET
        if (collision.gameObject.CompareTag("Bullet"))
        {
            //GET THE BULLET SCRIPT COMPONENT FROM THE OBJECT
            BulletScript bullet = collision.gameObject.GetComponent<BulletScript>();
            //TAKE DAMAGE BASED ON BULLET'S DAMAGE VALUE
            TakeDamage(bullet.damage);
        }
    }

    void UpdateUI()
    {
        //SHOW CURRENT STATE IN UI
        stateText.text = currentState.ToString();
    }
    
    #region STATES
    private void SwitchState()
    {
        switch (currentState)
        {
            case NPCStates.Patrol:
                Patrol();
                break;
            case NPCStates.Chase:
                Chase();
                break;
            case NPCStates.Attack:
                Attack();
                break;
            default:
                Patrol();
                break;
        }
    }
    
    private void Attack()
    {
        //CHANGE MATERIAL
        if (meshRenderer.material != AttackMaterial)
            meshRenderer.material = AttackMaterial;
        //STOP MOVEMENT
        navMeshAgent.SetDestination(transform.position);
        //LOOK AT PLAYER
        LookAtPlayer();
        //SHOOT
        Shoot();
        //CHECK IF THE STATE IS RELEVANT
        if (Vector3.Distance(transform.position, Player.position) > AttackRange)
            currentState = NPCStates.Chase;
    }

    private void Chase()
    {
        //CHANGE MATERIAL
        if (meshRenderer.material != ChaseMaterial)
            meshRenderer.material = ChaseMaterial;
        //FOLLOW PLAYER
        navMeshAgent.SetDestination(Player.position);
        //CHECK IF STATE IS RELEVANT
        if (Vector3.Distance(transform.position, Player.position) <= AttackRange)
            currentState = NPCStates.Attack;
        if (Vector3.Distance(transform.position, Player.position) > ChaseRange)
            currentState = NPCStates.Patrol;

    }

    private void Patrol()
    {
        //CHANGE MATERIAL
        if (meshRenderer.material != PatrolMaterial)
            meshRenderer.material = PatrolMaterial;

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(transform.position, range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.red, 1.0f);
                navMeshAgent.SetDestination(point);
            }
        }
        //CHECK IF STATE IS RELEVANT
        if (Vector3.Distance(transform.position, Player.position) < ChaseRange)
            currentState = NPCStates.Chase;
    }
    #endregion
    
    private void LookAtPlayer()
    {
        //CALCULATE THE DIRECTION FROM THE NPC TO THE PLAYER
        Vector3 direction = Player.position - transform.position;
        //ROTATE THE NPC TOWARDS THE PLAYER
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    void TakeDamage(int damage)
    {
        //REDUCE HP BY DMG AMOUNT
        hp -= damage;
        //UPDATE THE UI VALUE
        hpBar.value = hp;
        if (hp <= 0)
            EnemyDead();
    }
    
    void EnemyDead()
    {
        gameObject.SetActive(false);
    }
    
    void Shoot()
    {
        //CHECK SHOOTING COOLDOWN
        if (Time.time > nextFire + fireRate)
        {
            //UPDATE COOLDOWN
            nextFire = Time.time + fireRate;
            //INSTANTIATE A BULLET AT THE BULLET ORIGIN POSITION
            GameObject bullet = Instantiate(bulletPref, bulletOrigin.position, Quaternion.identity);
            //INITIALIZE THE BULLET'S DIRECTION
            bullet.GetComponent<EnemyBulletScript>()?.InitializeBullet(transform.rotation * Vector3.forward);
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 ranPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(ranPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}