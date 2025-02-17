using UnityEngine;

public class SpiderLegIK : MonoBehaviour
{
    [Header("Rig Joints (Make sure these are parented correctly)")]
    public Transform upperLeg;  // Root bone
    public Transform lowerLeg;  // Middle bone
    public Transform foot;      // End bone (effector)
    public Transform target;    // Target position for the foot

    [Header("IK Settings")]
    public int iterations = 10;
    public float tolerance = 0.01f;

    private float upperLegLength, lowerLegLength, totalLegLength;
    private Vector3 footStartOffset;

    void Start()
    {
        // Calculate bone lengths
        upperLegLength = Vector3.Distance(upperLeg.position, lowerLeg.position);
        lowerLegLength = Vector3.Distance(lowerLeg.position, foot.position);
        totalLegLength = upperLegLength + lowerLegLength;

        // Store initial foot offset from the lower leg
        footStartOffset = foot.localPosition;
    }

    void LateUpdate()
    {
        SolveIK();
    }

    void SolveIK()
    {
        if (!target) return;

        Vector3 upperPos = upperLeg.position;
        Vector3 lowerPos = lowerLeg.position;
        Vector3 footPos = foot.position;
        Vector3 targetPos = target.position;

        // Check if target is reachable
        float targetDistance = Vector3.Distance(upperPos, targetPos);
        if (targetDistance > totalLegLength)
        {
            // Target is out of reach, fully extend leg
            Vector3 dir = (targetPos - upperPos).normalized;
            lowerPos = upperPos + dir * upperLegLength;
            footPos = lowerPos + dir * lowerLegLength;
        }
        else
        {
            // FABRIK Iteration: Adjust positions
            for (int i = 0; i < iterations; i++)
            {
                // Backward Pass
                footPos = targetPos;
                lowerPos = footPos + (lowerPos - footPos).normalized * lowerLegLength;
                upperPos = lowerPos + (upperPos - lowerPos).normalized * upperLegLength;

                // Forward Pass
                upperPos = upperLeg.position; // Lock upperLeg to its original position
                lowerPos = upperPos + (lowerPos - upperPos).normalized * upperLegLength;
                footPos = lowerPos + (footPos - lowerPos).normalized * lowerLegLength;

                // Check convergence
                if ((footPos - targetPos).sqrMagnitude < tolerance * tolerance)
                    break;
            }
        }

        // Apply rotations instead of modifying world positions
        ApplyRotation(upperLeg, lowerPos, footStartOffset);
        ApplyRotation(lowerLeg, footPos, foot.localPosition);
    }

    void ApplyRotation(Transform joint, Vector3 targetPos, Vector3 localOffset)
    {
        Vector3 direction = targetPos - joint.position;
        if (direction.sqrMagnitude > 0.0001f) // Avoid NaN errors
        {
            joint.rotation = Quaternion.LookRotation(direction, joint.up);
        }
    }

    private void OnDrawGizmos()
    {
        if (upperLeg && lowerLeg && foot)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(upperLeg.position, lowerLeg.position);
            Gizmos.DrawLine(lowerLeg.position, foot.position);
        }
    }
}
