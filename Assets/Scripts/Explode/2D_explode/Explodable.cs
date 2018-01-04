using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

[RequireComponent(typeof(Rigidbody2D))]
public class Explodable : MonoBehaviour
{
    public System.Action<List<GameObject>> OnFragmentsGenerated;

    public bool allowRuntimeFragmentation = false;
    public int extraPoints = 0;
    public int subshatterSteps = 0;

    public string fragmentLayer = "Default2";
    public string sortingLayerName = "Default2";
    public int orderInLayer = 100;

    public enum ShatterType
    {
        Triangle,
        Voronoi
    };
    public ShatterType shatterType;
    public List<GameObject> fragments = new List<GameObject>();
    private List<List<Vector2>> polygons = new List<List<Vector2>>();


    /// <summary>
    /// Creates fragments if necessary and destroys original gameobject
    /// </summary>
    public void explode(TrashCleaner trashCleaner)
    {
        //if fragments were not created before runtime then create them now
        if (fragments.Count == 0 && allowRuntimeFragmentation)
        {
            GenerateFragments();
        }
        //otherwise unparent and activate them
        else
        {
            Vector2 vel = gameObject.GetComponent<Rigidbody2D>().velocity;

            foreach (GameObject frag in fragments)
            {
                if (trashCleaner != null)
                    frag.transform.parent = trashCleaner.transform;
                else
                    frag.transform.parent = null;
                SetFragmentSpeed(vel, frag.GetComponent<Rigidbody2D>());
            }

            if (trashCleaner != null)
                trashCleaner.IncTrash(fragments.Count);

            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// Creates fragments and then disables them
    /// </summary>
    public void fragmentInEditor()
    {
        if (fragments.Count > 0)
        {
            deleteFragments();
        }
        GenerateFragments();
        setPolygonsForDrawing();
        foreach (GameObject frag in fragments)
        {
            frag.transform.parent = transform;
            frag.SetActive(false);
        }
    }

    public void deleteFragments()
    {
        foreach (GameObject frag in fragments)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(frag);
            }
            else
            {
                Destroy(frag);
            }
        }
        fragments.Clear();
        polygons.Clear();
    }
    /// <summary>
    /// Turns Gameobject into multiple fragments
    /// </summary>
    /// 



    public void GenerateFragments()
    {
        fragments = new List<GameObject>();
        switch (shatterType)
        {
            case ShatterType.Triangle:
                fragments = SpriteExploder.GenerateTriangularPieces(gameObject, extraPoints, subshatterSteps);
                break;
            case ShatterType.Voronoi:
                fragments = SpriteExploder.GenerateVoronoiPieces(gameObject, extraPoints, subshatterSteps);
                break;
            default:
                Debug.Log("invalid choice");
                break;
        }
        //sets additional aspects of the fragments

        foreach (GameObject p in fragments)
        {
            if (p != null)
            {
                p.GetComponent<MeshRenderer>().sortingLayerName = sortingLayerName;
                p.layer = LayerMask.NameToLayer("Ignore Raycast");
                p.GetComponent<Renderer>().sortingOrder = orderInLayer;
                p.GetComponent<MeshRenderer>().sortingOrder = orderInLayer;

                p.transform.parent = gameObject.transform;
                Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.drag = 0.2f;
                rb.angularDrag = 0.2f;

                MeshRenderer mesh = p.GetComponent<MeshRenderer>();
                if (mesh != null)
                {
                    mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mesh.receiveShadows = false;
                }
            }
        }
    }

    private void SetFragmentSpeed(Vector2 vel, Rigidbody2D rb)
    {
        float xVel = Random.Range(-7, 8);
        float yVel = Random.Range(-7, 8);
        rb.velocity = vel + new Vector2(xVel, yVel);
    }


    private void setPolygonsForDrawing()
    {
        polygons.Clear();
        List<Vector2> polygon;

        foreach (GameObject frag in fragments)
        {
            polygon = new List<Vector2>();
            foreach (Vector2 point in frag.GetComponent<PolygonCollider2D>().points)
            {
                Vector2 offset = rotateAroundPivot((Vector2)frag.transform.position, (Vector2)transform.position, Quaternion.Inverse(transform.rotation)) - (Vector2)transform.position;
                offset.x /= transform.localScale.x;
                offset.y /= transform.localScale.y;
                polygon.Add(point + offset);
            }
            polygons.Add(polygon);
        }
    }
    private Vector2 rotateAroundPivot(Vector2 point, Vector2 pivot, Quaternion angle)
    {
        Vector2 dir = point - pivot;
        dir = angle * dir;
        point = dir + pivot;
        return point;
    }

    void OnDrawGizmos()
    {
        if (Application.isEditor)
        {
            if (polygons.Count == 0 && fragments.Count != 0)
            {
                setPolygonsForDrawing();
            }

            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector2 offset = (Vector2)transform.position * 0;
            foreach (List<Vector2> polygon in polygons)
            {
                for (int i = 0; i < polygon.Count; i++)
                {
                    if (i + 1 == polygon.Count)
                    {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[0] + offset);
                    }
                    else
                    {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[i + 1] + offset);
                    }
                }
            }
        }
    }
}
