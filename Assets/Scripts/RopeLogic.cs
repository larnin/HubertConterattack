using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RopeLogic : MonoBehaviour
{
    [SerializeField] LayerMask m_collisionMask;
    [SerializeField] float m_outDistance = 0.01f;
    [SerializeField] float m_moveDistanceDetection = 0.1f;
    [DraggablePoint2D]
    [SerializeField] Vector2 m_start = Vector2.zero;
    [DraggablePoint2D]
    [SerializeField] Vector2 m_end = Vector2.zero;
    [SerializeField] float m_width = 0.1f;

    Mesh m_mesh;
    MeshFilter m_meshFilter;

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
   
    class CollisionBody
    {
        public CollisionBody(Collider2D c)
        {
            collider = c;
            vertices = Collider2DEx.GetOutlineVertices(c);
        }

        public Vector2[] vertices;
        public Collider2D collider;
        public int nodeNb;
    }

    List<NodeInfo> m_nodes = new List<NodeInfo>();
    List<CollisionBody> m_bodies = new List<CollisionBody>();

    public void SetEnd(Vector2 end)
    {
        m_end = end;
    }

    void Start()
    {
        m_mesh = new Mesh();

        m_meshFilter = GetComponent<MeshFilter>();

        ResetRope();
    }
    
    void Update()
    {
        float maxDistanceSqr = m_moveDistanceDetection * m_moveDistanceDetection;

        if ((m_start - m_nodes[0].pos).sqrMagnitude > maxDistanceSqr)
            ResetRope();
        if ((m_end - m_nodes[m_nodes.Count - 1].pos).sqrMagnitude > maxDistanceSqr)
        {
            m_nodes[m_nodes.Count - 1].pos = m_end;
            RemoveNodes();
            UpdateRope();
            UpdateMesh();
        }

        DrawColliders();
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

    void DrawColliders()
    {
        foreach(var g in GameObject.FindObjectsOfType<GameObject>())
        {
            var comp = g.GetComponent<Collider2D>();
            if (comp == null)
                continue;

            var vertices = Collider2DEx.GetOutlineVertices(comp);

            for(int i = 0; i < vertices.Length; i++)
            {
                if (i == 0)
                    Debug.DrawLine(vertices[vertices.Length - 1], vertices[i], Color.green);
                else Debug.DrawLine(vertices[i - 1], vertices[i], Color.green);
            }
        }
    }

    void ResetRope()
    {
        m_nodes.Clear();
        m_bodies.Clear();

        m_nodes.Add(new NodeInfo(m_start));
        m_nodes.Add(new NodeInfo(m_end));

        UpdateRope();
        UpdateMesh();
    }

    void RemoveNodes()
    {
        //need at least 3 nodes to remove one
        int index = m_nodes.Count - 3;
        if (index < 0)
            return;

        var dir = m_nodes[index + 2].pos - m_nodes[index].pos;
        var dist = dir.magnitude;
        dir /= dist;

        var cast = Physics2D.Raycast(m_nodes[index].pos, dir, dist, m_collisionMask.value);

        if (cast.collider != null)
            return;

        if(m_nodes[index + 1].colliderIndex != -1)
        {
            var body = m_bodies[m_nodes[index + 1].colliderIndex];
            body.nodeNb--;
            if (body.nodeNb <= 0)
                m_bodies.Remove(body);
        }

        m_nodes.RemoveAt(index + 1);

        RemoveNodes();
    }

    void UpdateRope()
    {
        int index = m_nodes.Count - 2;
        if (index < 0)
            return;

        var dir = m_nodes[index + 1].pos - m_nodes[index].pos;
        var dist = dir.magnitude;
        dir /= dist;

        var cast = Physics2D.Raycast(m_nodes[index].pos, dir, dist, m_collisionMask.value);

        if (cast.collider == null)
            return;

        var bodyIndex = GetExistingBodyIndex(cast.collider);
        if (bodyIndex == -1)
        {
            bodyIndex = m_bodies.Count;
            m_bodies.Add(new CollisionBody(cast.collider));
        }
        var body = m_bodies[bodyIndex];
        body.nodeNb++;

        int leftIndex = -1;
        int rightIndex = -1;
        bool farestLeft;

        GetFarestVertex(body, m_nodes[index].pos, m_nodes[index + 1].pos, out leftIndex, out rightIndex, out farestLeft);

        if (leftIndex == -1 || rightIndex == -1)
        {
            body.nodeNb--;
            if (body.nodeNb <= 0)
                m_bodies.Remove(body);
            return;
        }

        NodeInfo node = null;
        if (farestLeft)
            node = new NodeInfo(GetExtrudedIndex(body, rightIndex), bodyIndex, rightIndex, farestLeft);
        else node = new NodeInfo(GetExtrudedIndex(body, leftIndex), bodyIndex, leftIndex, farestLeft);

        m_nodes.Insert(m_nodes.Count - 1, node);

        UpdateRope();
    }

    void GetFarestVertex(CollisionBody body, Vector2 startPos, Vector2 endPos, out int leftIndex, out int rightIndex, out bool farestLeft)
    {
        leftIndex = -1;
        rightIndex = -1;

        var dir = endPos - startPos;

        float leftAngle = 0;
        float rightAngle = 0;

        float epsilonSqrMax = m_outDistance * m_outDistance * 2;

        for (int i = 0; i < body.vertices.Length; i++)
        {
            var dPoint = body.vertices[i] - startPos;
            if (dPoint.sqrMagnitude < epsilonSqrMax)
                continue;

            var angle = Vector2.SignedAngle(dir, dPoint);

            if(angle > leftAngle)
            {
                leftIndex = i;
                leftAngle = angle;
            }
            if(angle < rightAngle)
            {
                rightIndex = i;
                rightAngle = angle;
            }
        }

        farestLeft = Mathf.Abs(leftAngle) > Mathf.Abs(rightAngle);
    }

    int GetExistingBodyIndex(Collider2D collider)
    {
        return m_bodies.FindIndex(x => { return x.collider == collider; });
    }

    Vector2 GetExtrudedIndex(CollisionBody body, int index)
    {
        if (index < 0 || index >= body.vertices.Length)
            return Vector2.zero;
        var pos = body.vertices[index];
        var posP1 = index == body.vertices.Length - 1 ? body.vertices[0] : body.vertices[index + 1];
        var posM1 = index == 0 ? body.vertices[body.vertices.Length - 1] : body.vertices[index - 1];

        var dir = posP1 + posM1 - 2 * pos;
        return pos - dir.normalized * m_outDistance;
    }

    void UpdateMesh()
    {
        int nbVertices = m_nodes.Count * 2;
        int nbTriangles = m_nodes.Count * 2 - 2;

        Vector3[] vertices = new Vector3[nbVertices];
        int[] tris = new int[nbTriangles * 3];

        //vertices
        for (int i = 0; i < m_nodes.Count; i++)
        {
            Vector2 dir = new Vector2();
            if (i > 0)
                dir += (m_nodes[i].pos - m_nodes[i - 1].pos).normalized;
            if (i < m_nodes.Count - 1)
                dir += (m_nodes[i + 1].pos - m_nodes[i].pos).normalized;

            Vector2 orthoDir = new Vector2(dir.y, -dir.x).normalized;

            float angle = 0;
            if (i > 0 && i < m_nodes.Count - 1)
                angle = Vector2.SignedAngle(m_nodes[i].pos - m_nodes[i - 1].pos, m_nodes[i + 1].pos - m_nodes[i].pos) / 2.0f;
            orthoDir /= Mathf.Cos(Mathf.Deg2Rad * angle);
            orthoDir *= m_width / 2.0f;

            vertices[2 * i] = m_nodes[i].pos + orthoDir;
            vertices[2 * i + 1] = m_nodes[i].pos - orthoDir;
        }

        //triangles
        for (int i = 0; i < m_nodes.Count - 1; i++)
        {
            tris[6 * i] = 2 * i;
            tris[6 * i + 1] = 2 * i + 2;
            tris[6 * i + 2] = 2 * i + 1;

            tris[6 * i + 3] = 2 * i + 1;
            tris[6 * i + 4] = 2 * i + 2;
            tris[6 * i + 5] = 2 * i + 3;
        }

        if(vertices.Length > m_mesh.vertices.Length)
        {
            m_mesh.vertices = vertices;
            m_mesh.triangles = tris;
        }
        else
        {
            m_mesh.triangles = tris;
            m_mesh.vertices = vertices;
        }
        
        m_meshFilter.mesh = m_mesh;
    }
}
