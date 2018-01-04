using System.Collections.Generic;
using UnityEngine;

public class PathTrack : MonoBehaviour
{
    private List<Vector2> nodes = new List<Vector2>();
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private int SEGMENT_COUNT = 50;
    [SerializeField]
    private bool reverse = false;
    int curveCount = 0;

    LineRenderer line1;
    LineRenderer line2;
    GameObject line3;
    int startNode = -1;
    int triggerLimit = 5;
    float colWidth = 0.1f;
    float radius = 0.4f;

    #region EllipseLine

    /*   public float a = 5;
       public float b = 3;
       public float h = 1;
       public float k = 1;
       public float theta = 45;
       public int resolution = 1000;

       private Vector3[] positions;

        void Ellipse()
        {
            positions = CreateEllipse(a, b, h, k, theta, resolution);

            lineRenderer.positionCount=resolution + 1;
            for (int i = 0; i <= resolution; i++)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
        }

        Vector3[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution)
        {

            positions = new Vector3[resolution + 1];
            Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
            Vector3 center = new Vector3(h, k, 0.0f);

            for (int i = 0; i <= resolution; i++)
            {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
                positions[i] = q * positions[i] + center;
            }

            return positions;
        }*/
    #endregion

    void Update()
    {

        if (lineRenderer != null)
        {
            Transform[] pathTransforms = GetComponentsInChildren<Transform>();
            nodes = new List<Vector2>();
            for (int i = 0; i < pathTransforms.Length; i++)
            {
                if (pathTransforms[i] != transform)

                {
                    nodes.Add(pathTransforms[i].position);
                }
            }

            curveCount = (int)nodes.Count / 3;

            for (int j = 0; j < curveCount; j++)
            {
                for (int i = 1; i <= SEGMENT_COUNT; i++)
                {
                    float t = i / (float)SEGMENT_COUNT;
                    int nodeIndex = j * 3;

                    int ind0 = 0;
                    if (nodeIndex + 1 > nodes.Count - 1)
                    {
                        ind0 = nodeIndex + 1 - nodes.Count;
                    }
                    else
                        ind0 = nodeIndex + 1;

                    int ind1 = 0;
                    if (nodeIndex + 2 > nodes.Count - 1)
                    {
                        ind1 = nodeIndex + 2 - nodes.Count;
                    }
                    else
                        ind1 = nodeIndex + 2;

                    int ind2 = 0;
                    if (nodeIndex + 3 > nodes.Count - 1)
                    {
                        ind2 = nodeIndex + 3 - nodes.Count;
                    }
                    else
                        ind2 = nodeIndex + 3;

                    Vector3 pixel = CalculateCubicBezierPoint(t, nodes[nodeIndex], nodes[ind0], nodes[ind1], nodes[ind2]);
                    lineRenderer.positionCount = (j * SEGMENT_COUNT) + i;
                    lineRenderer.SetPosition((j * SEGMENT_COUNT) + (i - 1), pixel);
                }
            }
            AddColliderToLine();
        }
    }

    private void AddColliderToLine()
    {
        if (line1 == null)
        {
            line1 = new GameObject("Line1").AddComponent<LineRenderer>();
            line2 = new GameObject("Line2").AddComponent<LineRenderer>();
            line3 = new GameObject("Line3");
            line1.transform.parent = lineRenderer.transform;
            line2.transform.parent = lineRenderer.transform;
            line3.transform.parent = lineRenderer.transform;
  
        }

        line1.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        if (line1 != null)
        {
            line1.loop = true;
            line2.loop = true;
            CapsuleCollider2D[] c = line1.GetComponentsInChildren<CapsuleCollider2D>();
            for (int i = 0; i < c.Length; i++)
            {
                Destroy(c[i].gameObject);
            }

            c = line2.GetComponentsInChildren<CapsuleCollider2D>();
            for (int i = 0; i < c.Length; i++)
            {
                Destroy(c[i].gameObject);
            }

            BoxCollider2D[] d = line3.GetComponentsInChildren<BoxCollider2D>();
            for (int i = 0; i < d.Length; i++)
            {
                Destroy(d[i].gameObject);
            }

            line1.positionCount = 1;
            line2.positionCount = 1;

            GameObject start = GameObject.Find("Start");
            float minDist = 999999;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                Vector2 startPos = lineRenderer.GetPosition(i);

                float dist = Vector2.Distance(startPos, start.transform.position);

                if (dist < minDist)
                {
                    minDist = dist;
                    startNode = i;
                }

                Vector2 endPos;

                if (i + 1 != lineRenderer.positionCount)
                    endPos = lineRenderer.GetPosition(i + 1);
                else
                    endPos = lineRenderer.GetPosition(0);

                Vector2 dir = endPos - startPos;

                Vector2 newDir = Quaternion.Euler(0, 0, 90) * dir;
                newDir = newDir.normalized;

                Vector2 finpos = startPos + newDir * (lineRenderer.startWidth / 2 + colWidth / 2);

                line1.SetPosition(i, finpos);

                newDir = Quaternion.Euler(0, 0, -90) * dir;
                newDir = newDir.normalized;
                finpos = startPos + newDir * (lineRenderer.startWidth / 2 + colWidth / 2);

                line2.SetPosition(i, finpos);
                if (i + 1 != lineRenderer.positionCount)
                {
                    line1.positionCount += 1;
                    line2.positionCount += 1;
                }
            }

            if (reverse == false)
            {
                int j = triggerLimit;
                for (int i = startNode; i < lineRenderer.positionCount; i++)
                {
                    if (j != triggerLimit)
                    {
                        j++;
                    }
                    else
                    {
                        j = 0;
                        CreateDistanceNode(i);
                    }
                }

                j = triggerLimit;
                for (int i = 0; i < startNode; i++)
                {
                    if (j != triggerLimit)
                    {
                        j++;
                    }
                    else
                    {
                        j = 0;
                        CreateDistanceNode(i);
                    }
                }
            }
            else
            {
                int j = triggerLimit;
                for (int i = startNode; i > 0; i--)
                {
                    if (j != triggerLimit)
                    {
                        j++;
                    }
                    else
                    {
                        j = 0;
                        CreateDistanceNode(i);
                    }
                }

                j = triggerLimit;
                for (int i = lineRenderer.positionCount - 1; i > startNode; i--)
                {
                    if (j != triggerLimit)
                    {
                        j++;
                    }
                    else
                    {
                        j = 0;
                        CreateDistanceNode(i);
                    }
                }
            }

            for (int i = 0; i < line1.positionCount; i++)
            {
                CreateBorder(i, line1);
            }

            for (int i = 0; i < line2.positionCount; i++)
            {
                CreateBorder(i, line2);
            }
        }
    }

    Vector2 prevPos;
    private void CreateBorder(int i, LineRenderer line)
    {
        Vector2 startPos = line.GetPosition(i);
        Vector2 endPos;

        if (i + 1 != line.positionCount)
            endPos = line.GetPosition(i + 1);
        else
            endPos = line.GetPosition(0);

        Vector2 midPoint = (startPos + endPos) / 2;

        float lineLength = Vector2.Distance(startPos, endPos); // length of line

        float curDist = Vector2.Distance(prevPos, midPoint);
        float minDist = 0.4f;

        if (curDist > minDist)
        {
            CapsuleCollider2D col = new GameObject("Collider").AddComponent<CapsuleCollider2D>();
            col.gameObject.tag = "Barrier";

            col.transform.parent = line.transform;
            col.direction = CapsuleDirection2D.Horizontal;
            col.transform.position = midPoint;
            prevPos = midPoint;

            col.size = new Vector2(lineLength + radius, colWidth); // size of collider is set where X is length of line, Y is width of line, Z will be set as per requirement

            float angle = (Mathf.Abs(startPos.y - endPos.y) / Mathf.Abs(startPos.x - endPos.x));
            if ((startPos.y < endPos.y && startPos.x > endPos.x) || (endPos.y < startPos.y && endPos.x > startPos.x))
            {
                angle *= -1;
            }
            angle = Mathf.Rad2Deg * Mathf.Atan(angle);
            col.transform.Rotate(0, 0, angle);
        }
    }

    private void CreateDistanceNode(int i)
    {
        Vector3 startPos = lineRenderer.GetPosition(i);
        BoxCollider2D col = new GameObject("DistanceNode").AddComponent<BoxCollider2D>();
        col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        col.gameObject.AddComponent<DistanceNode>();
        col.isTrigger = true;
        col.transform.parent = line3.transform;
        col.transform.position = startPos;
        col.size = new Vector3(lineRenderer.startWidth, colWidth, 1f);
        Vector3 endPos;

        if (i + 1 != lineRenderer.positionCount)
            endPos = lineRenderer.GetPosition(i + 1);
        else
            endPos = lineRenderer.GetPosition(0);

        Vector2 dir = startPos - endPos;

        col.transform.rotation = Quaternion.FromToRotation(Vector2.up, dir);
    }

    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;
        return p;
    }
}


