using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class RopeLogic : MonoBehaviour
{
    [SerializeField] float m_moveDistanceDetection = 0.1f;
    [DraggablePoint2D]
    [SerializeField] Vector2 m_start = Vector2.zero;
    [DraggablePoint2D]
    [SerializeField] Vector2 m_end = Vector2.zero;

    class NodeInfo
    {
        public NodeInfo(Vector2 _pos)
        {
            pos = _pos;
            colliderIndex = -1;
            colliderVertexIndex = -1;
        }

        public NodeInfo(Vector2 _pos, int _colliderIndex, int _colliderVertexIndex, bool _colliderSide)
        {
            pos = _pos;
            colliderIndex = _colliderIndex;
            colliderVertexIndex = _colliderVertexIndex;
            colliderSide = _colliderSide;
        }

        public Vector2 pos;
        public int colliderIndex;
        public int colliderVertexIndex;
        public bool colliderSide;
    }
   

    List<NodeInfo> m_nodes = new List<NodeInfo>();

    void Start()
    {
        m_nodes.Add(new NodeInfo(m_start));
        m_nodes.Add(new NodeInfo(m_end));
    }
    
    void Update()
    {


        Display();
    }


    void Display()
    {
        Color c = Color.red;
        float l = 0.1f;

        for(int i = 0; i < m_nodes.Count; i++)
        {
            Debug.DrawLine(m_nodes[i].pos - new Vector2(l, l), m_nodes[i].pos + new Vector2(l, l), c);
            Debug.DrawLine(m_nodes[i].pos - new Vector2(l, -l), m_nodes[i].pos + new Vector2(l, -l), c);

            if (i < m_nodes.Count - 1)
                Debug.DrawLine(m_nodes[i].pos, m_nodes[i + 1].pos, c);
        }
    }
}
