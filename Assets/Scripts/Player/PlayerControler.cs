using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PlayerControler : MonoBehaviour
{
    string horizontalButton = "horizontal";
    string jumpButton = "jump";

    [SerializeField] float m_treshold = 0.1f;
    [SerializeField] float m_groundedAcceleration = 1;
    [SerializeField] float m_flyAcceleration = 1;
    [SerializeField] float m_overSpeedDeceleration = 1;
    [SerializeField] float m_MaxSpeed = 1;
    [SerializeField] float m_groundDetectionDistance = 1;
    [SerializeField] float m_groundDetectionRadius = 1;
    [SerializeField] LayerMask m_groundMask;
    [SerializeField] float m_jumpSpeed = 1;
    [SerializeField] float m_jumpPressDuration = 0.3f;
    [SerializeField] float m_jumpSaveDelay = 0.3f;

    float m_moveDir = 0;
    float m_speed = 0;
    bool m_grounded = false;
    float m_jumpDuration = 0;
    float m_jumpSaveDuration = 0;

    Rigidbody2D m_rigidbody = null;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        m_moveDir = Input.GetAxisRaw(horizontalButton);
        if (Mathf.Abs(m_moveDir) < m_treshold)
            m_moveDir = 0;
        else m_moveDir = Mathf.Sign(m_moveDir) * (Mathf.Abs(m_moveDir) - m_treshold) / (1 - m_treshold);

        var hit = Physics2D.CircleCast(transform.position, m_groundDetectionRadius, new Vector2(0, 1), m_groundDetectionDistance, m_groundMask.value);
        m_grounded = hit.transform != null;

        bool jumpingLastFrame = m_jumpDuration < m_jumpPressDuration;
        bool jumpSaveLastFrame = m_jumpSaveDuration < m_jumpSaveDelay;
        bool jumpThisFrame = Input.GetKey(jumpButton);

        m_jumpDuration += Time.deltaTime;
        m_jumpSaveDuration += Time.deltaTime;

        if(!m_grounded)
        {
            if(jumpSaveLastFrame && jumpThisFrame)
                m_jumpSaveDuration = 0;
            //reset jump velocity
            if(jumpingLastFrame && jumpThisFrame)
            {
                var velocity = m_rigidbody.velocity;
                velocity.y = -m_jumpSpeed;
                m_rigidbody.velocity = velocity;
            }
        }
        else
        {
            if(jumpSaveLastFrame || jumpThisFrame)
            {
                m_jumpSaveDuration = m_jumpSaveDelay + 1;

                var velocity = m_rigidbody.velocity;
                velocity.y = -m_jumpSpeed;
                m_rigidbody.velocity = velocity;
            }
        }

        if (!jumpThisFrame)
            m_jumpDuration = m_jumpPressDuration + 1;
    }

    private void FixedUpdate()
    {
         
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(5, 5, 100, 20), "Grounded : " + m_grounded);
    }
}