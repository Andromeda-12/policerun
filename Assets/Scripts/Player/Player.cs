﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    [Header("Двигать")]
    public bool isRun;

    [Header("Имеет дополнительную жизнь")]
    public bool isHasSecondLife;

    [Header("Имеет бустер очков")]
    public bool isHasScoreBooster;

    [Header("Имеет бустер прыжка")]
    public bool isHasJumpBooster;

    [Header("Столкнулся с препятствием")]
    public bool isHitAnObstacle;

    [Header("Столкнулся с бустером")]
    public bool isHasCollisionWithBooster;

    [Header("Эффект бустера")]
    public string playerEffect;

    [Header("Variables")]
    [SerializeField] float      m_maxSpeed = 4.5f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] bool       m_hideSword = false;
    [Header("Effects")]
    [SerializeField] GameObject m_RunStopDust;
    [SerializeField] GameObject m_JumpDust;
    [SerializeField] GameObject m_LandingDust;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Prototype    m_groundSensor;
    private SensorObstacle      m_obstacleSensor;
    private AudioSource         m_audioSource;
    private AudioManager_PrototypeHero m_audioManager;
    private bool                m_grounded = false;
    private bool                m_moving = false;
    private int                 m_facingDirection = 1;
    private float               m_disableMovementTimer = 0.0f;


    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_audioManager = AudioManager_PrototypeHero.instance;
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Prototype>();
        m_obstacleSensor = transform.Find("ObstacleSensor").GetComponent<SensorObstacle>();
    }


    // Update is called once per frame
    void Update ()
    {
        // Decrease timer that disables input movement. Used when attacking
        m_disableMovementTimer -= Time.deltaTime;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = 0.0f;

        if (m_disableMovementTimer < 0.0f)
            inputX = Input.GetAxis("Horizontal");

        //GetAxisRaw returns either -1, 0 or 1
        float inputRaw = Input.GetAxisRaw("Horizontal");
        // Check if current move input is larger than 0 and the move direction is equal to the characters facing direction
        if (Mathf.Abs(inputRaw) > Mathf.Epsilon && Mathf.Sign(inputRaw) == m_facingDirection)
        {
            m_moving = true;
        }

        else
            m_moving = false;

        m_moving = isRun;

        if (m_obstacleSensor.isHasCollision)
        {
            isHitAnObstacle = true;
            m_obstacleSensor.isHasCollision = false;
        }

        if (m_obstacleSensor.isHasCollisionWithBooster)
        {
            isHasCollisionWithBooster = true;
            m_obstacleSensor.isHasCollisionWithBooster = false;
            playerEffect = m_obstacleSensor.playerEffect;
        }

        // Swap direction of sprite depending on move direction
        //if (inputRaw > 0)
        //{
        //    GetComponent<SpriteRenderer>().flipX = false;
        //    m_facingDirection = 1;

        //    GameObject[] environments = GameObject.FindGameObjectsWithTag("Environment");

        //    foreach (var env in environments)
        //    {
        //        Vector3 vector = new Vector3(env.transform.position.x, env.transform.position.y, env.transform.position.z);
        //        vector = new Vector3(vector.x - m_envSpeed, vector.y, vector.z);
        //        env.transform.position = Vector3.Lerp(env.transform.position, vector, m_envSpeed * Time.deltaTime);
        //    }
        //    GameObject obj = environments[environments.Length - 1];
        //    Instantiate(obj, new Vector3(obj.transform.position.x + 12, obj.transform.position.y, obj.transform.position.z), Quaternion.identity);
        //    Destroy(environments[0]);
        //}

        //else if (inputRaw < 0)
        //{
        //    GetComponent<SpriteRenderer>().flipX = true;
        //    m_facingDirection = -1;
        //}

        // SlowDownSpeed helps decelerate the characters when stopping
        float SlowDownSpeed = m_moving ? 1.0f : 0.5f;
        // Set movement
        m_body2d.velocity = new Vector2(inputX * m_maxSpeed * SlowDownSpeed, m_body2d.velocity.y);

        // Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // Set Animation layer for hiding sword
        int boolInt = m_hideSword ? 1 : 0;
        m_animator.SetLayerWeight(1, boolInt);

        // -- Handle Animations --
        //Jump
        if (Input.GetButtonDown("Jump") && m_grounded && m_disableMovementTimer < 0.0f)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && m_grounded && m_disableMovementTimer < 0.0f)
            {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
            }
        }

        //Run
        else if(m_moving)
            m_animator.SetInteger("AnimState", 1);

        //Idle
        else
            m_animator.SetInteger("AnimState", 0);
    }

    public void Stop()
    {
        m_animator.enabled = false;
        m_body2d.gravityScale = 0;
        m_body2d.angularVelocity = 0f;
        m_body2d.velocity = Vector3.zero;
    }

    public void SetSpeed(float value)
    {
        m_animator.speed = value;
    }

    public void setJumpForce(float value)
    {
        m_jumpForce = value;
    }

    // Function used to spawn a dust effect
    // All dust effects spawns on the floor
    // dustXoffset controls how far from the player the effects spawns.
    // Default dustXoffset is zero
    void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    {
        if (dust != null)
        {
            // Set dust spawn position
            Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * m_facingDirection, 0.0f, 0.0f);
            GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity) as GameObject;
            // Turn dust in correct X direction
            newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(m_facingDirection, 1, 1);
        }
    }

    // Animation Events
    // These functions are called inside the animation files
    void AE_runStop()
    {
        m_audioManager.PlaySound("RunStop");
        // Spawn Dust
        float dustXOffset = 0.6f;
        SpawnDustEffect(m_RunStopDust, dustXOffset);
    }

    void AE_footstep()
    {
        m_audioManager.PlaySound("Footstep");
    }

    void AE_Jump()
    {
        m_audioManager.PlaySound("Jump");
        // Spawn Dust
        SpawnDustEffect(m_JumpDust);
    }

    void AE_Landing()
    {
        m_audioManager.PlaySound("Landing");
        // Spawn Dust
        SpawnDustEffect(m_LandingDust);
    }
}
