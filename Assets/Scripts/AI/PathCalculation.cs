using System.Collections.Generic;
using UnityEngine;

public class PathCalculation : MonoBehaviour
{
    public static float AngleDir(Vector2 A, Vector2 B)
    {
        return -A.x * B.y + A.y * B.x;
    }

    private static bool angle = false;

    public static List<Vector3> PathCalc(bool createMarker, int aiLevel)
    {
        GameObject t = GameObject.Find("Track");
        if (t != null)
        {
            LineRenderer line = t.GetComponent<LineRenderer>();

            List<Vector3> pos = new List<Vector3>();

            float maxAngle = 4;

            int startNode = 0;
            float nodeDist = 9999999;
            Transform start = FindObjectOfType<StartController>().gameObject.transform;
            bool reverse = start.gameObject.GetComponent<StartController>().Reverse;

            float sideOffSet = line.startWidth / 2;
            for (int n = 0; n < line.positionCount; n++)
            {
                pos.Add(line.GetPosition(n));
                if (Vector2.Distance(pos[n], start.position) < nodeDist)
                {
                    startNode = n;
                    nodeDist = Vector2.Distance(pos[n], start.position);
                }
            }

            int i = startNode;

            List<Vector3> Blocks = new List<Vector3>();

            int startPos = -1;
            int endPos = -1;
            int type = 0;
            int count = 0;

            while (count < pos.Count)
            {
                if (!reverse)
                {
                    if (i >= pos.Count)
                        i -= pos.Count;
                }
                else
                {
                    if (i < 0)
                        i += pos.Count;
                }

                int j = i;
                int k = i;
                int l = i;

                if (!reverse)
                {
                    j += 1;
                    k += 4;
                    l += 5;

                    if (j >= pos.Count)
                        j = 0;

                    if (k >= pos.Count)
                        k -= pos.Count;

                    if (l >= pos.Count)
                        l -= pos.Count;
                }
                else
                {
                    j -= 1;
                    k -= 4;
                    l -= 5;

                    if (j < 0)
                        j += pos.Count;

                    if (k < 0)
                        k += pos.Count;

                    if (l < 0)
                        l += pos.Count;
                }

                Vector3 vec1 = pos[j] - pos[i];
                Vector3 vec2 = pos[l] - pos[k];

                float currentAngle = Vector2.Angle(vec1, vec2);

                int minDistBlock = 5;

                if (currentAngle > maxAngle)
                {
                    float point = -1f;

                    float angle = AngleDir(vec1, vec2);
                    if (angle > 0)
                    {
                        point = 1f;
                    }

                    int currentType = 0;
                    if (point > 0)
                        currentType = 1;
                    else
                        currentType = -1;

                    if (startPos == -1)
                    {
                        startPos = i;
                        type = currentType;
                    }
                    else
                    {
                        int b = 0;
                        if (!reverse)
                        {
                            b = startPos + 1;
                            if (b >= pos.Count)
                                b = 0;
                        }
                        else
                        {
                            b = startPos - 1;
                            if (b < 0)
                                b = pos.Count - 1;
                        }

                        Vector2 startVec = pos[b] - pos[startPos];
                        float dot2 = Vector2.Dot(startVec.normalized, vec2.normalized);

                        if (type != currentType || dot2 < 0)
                        {
                            endPos = i;
                        }
                    }

                    if (startPos != -1 && endPos != -1)
                    {
                        if (Vector2.Distance(pos[startPos], pos[endPos]) > minDistBlock)
                        {
                            Blocks.Add(new Vector3(startPos, endPos, type));
                            //Debug.Log(type);
                        }
                        startPos = -1;
                        endPos = -1;
                    }
                }
                else
                {
                    if (startPos == -1)
                    {
                        startPos = i;
                        type = 0;
                    }
                    else if (i + 1 == pos.Count)
                    {
                        endPos = i;
                        Blocks.Add(new Vector3(startPos, endPos, type));
                        //Debug.Log(type);
                    }

                    if (type != 0)
                    {
                        endPos = i;
                        if (Vector2.Distance(pos[startPos], pos[endPos]) > minDistBlock)
                        {
                            Blocks.Add(new Vector3(startPos, endPos, type));
                        }
                        startPos = -1;
                        endPos = -1;
                    }

                    type = 0;
                }

                if (!reverse)
                    i += 2;
                else
                    i -= 2;
                count += 2;
            }

            GameObject marker = new GameObject("AiPathNew");
            if (createMarker)
            {
                marker.AddComponent<PathEditor>();
            }
            else
            {
                Destroy(marker);
            }

            int prevType = -2;
            List<Vector3> markers = new List<Vector3>();
            for (int n = 0; n < Blocks.Count; n++)
            {
                Vector3 finpos = Vector3.zero;
                int k = 0;
                if (n + 1 < Blocks.Count)
                    k = n + 1;

                int currentType = (int)Blocks[n].z;
                int nextType = (int)Blocks[k].z;

                int side = 0;
                float sideShift = 0f;
                bool nitro = false;
                float nitroDist = 8f;

                int path = 1;
                if (aiLevel > 0)
                {
                    if (aiLevel == 1)
                    {
                        path = Random.Range(0, 2);
                    }
                    else
                    {
                        path = 0;
                    }
                }

                if (path == 1)
                {
                    if (nextType == 0)
                    {
                        if (currentType == 1)
                        {
                            if (prevType == -1)
                            {
                                side = 1;
                                sideShift = 0.5f;
                            }
                            else
                            {
                                side = -1;
                                sideShift = 0.5f;
                            }
                        }
                        else if (currentType == -1)
                        {
                            if (prevType == 1)
                            {
                                side = -1;
                                sideShift = 0.5f;
                            }
                            else
                            {
                                side = 1;
                                sideShift = 0.5f;
                            }
                        }

                        if (Vector2.Distance(pos[(int)Blocks[k].x], pos[(int)Blocks[k].y]) >= nitroDist)
                        {
                            nitro = true;
                        }
                    }
                    else
                    if (currentType == nextType)
                    {
                        side = 0;
                        sideShift = 0;
                    }
                    else
                    if (currentType == 1)
                    {
                        side = -1;
                    }
                    else if (currentType == -1)
                    {
                        side = 1;
                    }
                }
                else
                {
                    side = Random.Range(-1, 2);

                    float randomNum = Random.Range(-10, 11);
                    sideShift = randomNum / 10;

                    int numNitro = 0;

                    numNitro = Random.Range(0, 5);
                    if (numNitro == 0)
                    {
                        nitro = true;
                    }
                }

                if (side != 0)
                {
                    int m = (int)Blocks[n].y;
                    m++;
                    if (m + 1 >= pos.Count)
                        m = 0;

                    Vector3 dir = pos[m] - pos[(int)Blocks[n].y];

                    dir = Quaternion.Euler(0, 0, 90 * side) * dir;
                    dir = dir.normalized;

                    finpos = pos[(int)Blocks[n].y] + dir * sideOffSet * sideShift;
                }
                else
                {
                    float shiftDist = 4f;

                    if (aiLevel != 0)
                    {
                        shiftDist = Random.Range(0, 12);
                    }

                    endPos = (int)Blocks[n].y;
                    if (!reverse)
                        startPos = endPos - 1;
                    else
                        startPos = endPos + 1;

                    while (true == true)
                    {
                        if (!reverse)
                        {
                            if (startPos < 0)
                                startPos = pos.Count - 1;
                            if (Vector2.Distance(pos[endPos], pos[startPos]) >= shiftDist)
                            {
                                break;
                            }
                            else
                            {
                                startPos--;
                            }
                        }
                        else
                        {
                            if (startPos >= pos.Count)
                                startPos = 0;
                            if (Vector2.Distance(pos[endPos], pos[startPos]) >= shiftDist)
                            {
                                break;
                            }
                            else
                            {
                                startPos++;
                            }
                        }
                    }

                    finpos = pos[startPos];
                }
                if (nitro)
                    finpos.z = 1;
                prevType = currentType;

                markers.Add(finpos);
            }

            List<Vector3> calcPath = new List<Vector3>();

            for (int n = 0; n < markers.Count; n++)
            {
                if (createMarker)
                {
                    GameObject node = new GameObject("Node");
                    node.AddComponent<PathNode>();
                    Vector2 markerPos = new Vector2();
                    markerPos.x = markers[n].x;
                    markerPos.y = markers[n].y;
                    node.transform.position = markerPos;
                    if (markers[n].z == 1)
                    {
                        node.GetComponent<PathNode>().Nitro = true;
                    }
                    node.transform.parent = marker.transform;
                }
                calcPath.Add(markers[n]);
            }
            return calcPath;
        }
        else
            return null;
    }
}
