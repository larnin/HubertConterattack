using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Collider2DEx
{
    class LineIndexs
    {
        public LineIndexs(int _index1, int _index2)
        {
            index1 = _index1;
            index2 = _index2;
        }
        public int index1;
        public int index2;

        public static bool operator ==(LineIndexs a, LineIndexs b)
        {
            if (a.index1 == b.index1 && a.index2 == b.index2)
                return true;
            if (a.index1 == b.index2 && a.index2 == b.index1)
                return true;
            return false;
        }

        public static bool operator !=(LineIndexs a, LineIndexs b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var line = obj as LineIndexs;
            return line != null && line == this;
        }

        public override int GetHashCode()
        {
            var hashCode = -643491719;
            hashCode = hashCode * -1521134295 + index1.GetHashCode();
            hashCode = hashCode * -1521134295 + index2.GetHashCode();
            return hashCode;
        }
    }

    static void TryAdd(List<LineIndexs> lines, LineIndexs l)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] == l)
            {
                lines.RemoveAt(i);
                return;
            }
        }
        lines.Add(l);
    }

    public static Vector2[] GetOutlineVertices(Collider2D collider)
    {
        var vertexs = new List<int>();

        var mesh = collider.CreateMesh(true, true);

        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        var lines = new List<LineIndexs>();

        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            var line1 = new LineIndexs(triangles[i], triangles[i + 1]);
            var line2 = new LineIndexs(triangles[i + 1], triangles[i + 2]);
            var line3 = new LineIndexs(triangles[i + 2], triangles[i]);

            TryAdd(lines, line1);
            TryAdd(lines, line2);
            TryAdd(lines, line3);
        }

        if (lines.Count > 0)
        {
            vertexs.Add(lines[lines.Count - 1].index1);
            vertexs.Add(lines[lines.Count - 1].index2);

            lines.RemoveAt(lines.Count - 1);

            while (lines.Count > 0)
            {
                bool haveAddedALine = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].index1 == vertexs[0])
                    {
                        vertexs.Insert(0, lines[i].index2);
                        lines.RemoveAt(i);
                        i--;
                        haveAddedALine = true;
                    }
                    else if (lines[i].index2 == vertexs[0])
                    {
                        vertexs.Insert(0, lines[i].index1);
                        lines.RemoveAt(i);
                        i--;
                        haveAddedALine = true;
                    }
                    else if (lines[i].index1 == vertexs[vertexs.Count - 1])
                    {
                        vertexs.Add(lines[i].index2);
                        lines.RemoveAt(i);
                        i--;
                        haveAddedALine = true;
                    }
                    else if (lines[i].index2 == vertexs[vertexs.Count - 1])
                    {
                        vertexs.Add(lines[i].index1);
                        lines.RemoveAt(i);
                        i--;
                        haveAddedALine = true;
                    }
                    if (haveAddedALine)
                        break;
                }

                if (vertexs[0] == vertexs[vertexs.Count - 1])
                {
                    vertexs.RemoveAt(vertexs.Count - 1);
                    break;
                }

                if (!haveAddedALine)
                    break;
            }
        }

        var realVertexs = new Vector2[vertexs.Count];
        for (int i = 0; i < vertexs.Count; i++)
            realVertexs[i] = vertices[vertexs[i]];

        UnityEngine.Object.Destroy(mesh);

        return realVertexs;

    }
}
