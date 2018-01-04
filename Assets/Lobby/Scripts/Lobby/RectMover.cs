using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectMover : MonoBehaviour
{
    private float speed = 90f;
    private RectTransform target1, target2;
    private Vector2 targetPos1, targetPos2;

    void FixedUpdate()
    {
        if (target1 != null)
        {
            Vector2 pos = Vector2.MoveTowards(target1.localPosition, targetPos1, speed);
            target1.localPosition = pos;

            if (target1.localPosition.AlmostEquals(targetPos1, 0.01f))
            {
                target1 = null;
            }
        }

        if (target2 != null)
        {
            Vector2 pos = Vector2.MoveTowards(target2.localPosition, targetPos2, speed);
            target2.localPosition = pos;

            if (target2.localPosition.AlmostEquals(targetPos2, 0.01f))
            {
                target2 = null;
            }
        }
    }

    public void SetTargets(RectTransform newTarget1, RectTransform newTarget2, Vector2 newTargetPos1, Vector2 newTargetPos2)
    {
        target1 = newTarget1;
        target2 = newTarget2;
        targetPos1 = newTargetPos1;
        targetPos2 = newTargetPos2;
    }

    public static void MoveTarget(RectTransform newTarget, Vector2 targetPos)
    {
        newTarget.localPosition = targetPos;
    }
}
