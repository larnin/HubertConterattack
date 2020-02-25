using UnityEngine;
using System.Collections;

public class SimpleRopeControler : MonoBehaviour
{
    [SerializeField] float m_speed = 1;

    RopeLogic m_rope;

    Vector2 m_pos;

    void Start()
    {
        m_rope = GetComponent<RopeLogic>();
        m_pos = transform.position;
    }
    
    void Update()
    {
        Vector2 dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        m_pos += dir * Time.deltaTime * m_speed;

        m_rope.SetEnd(m_pos);
    }
}
