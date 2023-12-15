using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RamboScript : MonoBehaviour
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
    [SerializeField] Material PatrolMaterial;
    [SerializeField] Material ChaseMaterial;
    [SerializeField] Material AttackMaterial;
    [SerializeField] Material ScareMaterial; 
    [SerializeField] float ChaseRange = 20f;
    [SerializeField] float AttackRange = 15f;
    [SerializeField] public GameObject PlayerPrefab;
    [SerializeField] float FearRange = 20f;
    private float range = 10f;
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
        if (PlayerPrefab.activeSelf == false)
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
            case NPCStates.Scared:
                Scared();
                break;
            default:
                Patrol();
                break;
        }
    }
    
    void Scared()
    {
        //CHANGE MATERIAL
        if (meshRenderer.material != ScareMaterial)
            meshRenderer.material = ScareMaterial;
        //CALCULATE A DIRECTION AWAY FROM THE PLAYER
        Vector3 runDirection = transform.position - Player.position;
        //SET THE DESTINATION OF THE NPC AWAY FROM THE PLAYER
        Vector3 runDestination = transform.position + runDirection.normalized * FearRange;
        navMeshAgent.SetDestination(runDestination);
        //CHECK IF THE STATE IS RELEVANT
        if (Vector3.Distance(transform.position, Player.position) > FearRange)
            currentState = NPCStates.Patrol;
    }

    private void Attack()
    {
        //CHANGE MATERIAL
        if (meshRenderer.material != AttackMaterial)
            meshRenderer.material = AttackMaterial;
        //MOVE TOWARDS THE PLAYER
        navMeshAgent.SetDestination(Player.position);
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
    #endregion"

    void TakeDamage(int damage)
    {
        //REDUCE HP BY DMG AMOUNT
        hp -= damage;
        //UPDATE THE UI VALUE
        hpBar.value = hp;
        //CHECK RELEVANT STATE
        if (hp < 50)
            currentState = NPCStates.Scared;
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