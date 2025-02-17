using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject[] legTargetPoints;
    [SerializeField] GameObject[] legRestingPoints;
    [SerializeField] GameObject[] legOriginalPoints; 
    [SerializeField] GameObject bodyTargetPoint;
    //[SerializeField] GameObject bodyRestingPoint;

    public float lerpDistance = 2f;
    public float lerpDuration = 0.5f;

    public LayerMask GroundLayer; // Layer to detect ground
    private float GroundRaycastOffset = 0.3f;

    private List<GameObject> movingPair1 = new List<GameObject>();
    private List<GameObject> movingPair2 = new List<GameObject>();
    private int queuedPair = 0;

    public GameObject spiderPhysBody;
    public GameObject spiderBody;
    public float spiderHeight = 2f;

    public float legSpacingLeftRight;
    public float legSpacingFrontBack;
    public float bodyRotationSpeed = 10f;

    void Start()
    {
        InitializeSpiderStance();
        PairLegs();
    }

    // Update is called once per frame
    void Update()
    {
        GroundTargetPoints();

        GroundBodyTargetPoint();

        StartCoroutine(MoveSpiderLegsZigZag()); 

        StartCoroutine(AdjustBodyPosition());

        RotateSpiderBody();
    }

    IEnumerator MoveSpiderLegsZigZag()
    {
        if(queuedPair == 0)
        {
            StartCoroutine(MoveLeg(legRestingPoints[0], legRestingPoints[0].transform.position, legTargetPoints[0].transform.position, lerpDistance));
            StartCoroutine(MoveLeg(legRestingPoints[3], legRestingPoints[3].transform.position, legTargetPoints[3].transform.position, lerpDistance));
            yield return new WaitForSeconds(0.3f);
            queuedPair = 1;
        }
        else
        {
            StartCoroutine(MoveLeg(legRestingPoints[1], legRestingPoints[1].transform.position, legTargetPoints[1].transform.position, lerpDistance));
            StartCoroutine(MoveLeg(legRestingPoints[2], legRestingPoints[2].transform.position, legTargetPoints[2].transform.position, lerpDistance));
            yield return new WaitForSeconds(0.3f);
            queuedPair = 0;
        }
    }

    void MoveSpiderLegs()
    {
        for (int i = 0; i < legTargetPoints.Length; i++)
        {
            StartCoroutine(MoveLeg(legRestingPoints[i], legRestingPoints[i].transform.position, legTargetPoints[i].transform.position, lerpDistance));
        }
    }

    IEnumerator MoveLeg(GameObject leg, Vector3 restingPoint, Vector3 targetPoint, float lerpDistance)
    {
        float distance = Vector3.Distance(restingPoint, targetPoint);

        if (distance > lerpDistance)
        {
            float time = 0;
            Vector3 startPosition = restingPoint;
            float peakHeight = 0.3f; // Adjust this to control the height of the arc

            while (time < lerpDuration)
            {
                float t = time / lerpDuration;
                Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPoint, t);

                // Create an arc motion using a sine function
                float heightOffset = Mathf.Sin(t * Mathf.PI) * peakHeight;

                // Apply the height offset to the Y-axis
                leg.transform.position = new Vector3(horizontalPosition.x, horizontalPosition.y + heightOffset, horizontalPosition.z);

                time += Time.deltaTime;
                yield return null;
            }

            // Ensure the leg reaches the exact target position
            leg.transform.position = targetPoint;
        }
    }

    IEnumerator AdjustBodyPosition()
    {
        Vector3 spiderPhysPos = spiderPhysBody.transform.position;
        Vector3 targetPoint = bodyTargetPoint.transform.position;
        float distance = Vector3.Distance(spiderPhysPos, targetPoint);

        if (distance != spiderHeight) 
        {
            float time = 0;
            Vector3 startPosition = spiderPhysPos;
            var bodyRestingPoint = targetPoint;
            bodyRestingPoint.y = targetPoint.y + spiderHeight;

            while (time < lerpDuration)
            {
                float t = time / lerpDuration;
                Vector3 horizontalPosition = Vector3.Lerp(startPosition, bodyRestingPoint, t);

                // Apply the height offset to the Y-axis
                spiderPhysBody.transform.position = new Vector3(horizontalPosition.x, horizontalPosition.y, horizontalPosition.z);

                time += Time.deltaTime;
                yield return null;
            }
        }
    }

    void GroundTargetPoints()
    {
        for (int i = 0; i < legOriginalPoints.Length; i++)
        {
            Vector3 groundPos = GetGroundPosition2(legOriginalPoints[i].transform.position);
            legTargetPoints[i].transform.position = groundPos;
        }
    }

    void GroundBodyTargetPoint()
    {
        //Vector3 groundPos = GetGroundPosition2(transform.position);
        Vector3 groundPos = transform.position;

        float totalHeight = 0f;

        for (int i = 0; i < legRestingPoints.Length; i++)
        {
            totalHeight += legRestingPoints[i].transform.position.y;
        }

        float averageHeight = totalHeight / legRestingPoints.Length;
        groundPos.y = averageHeight;

        bodyTargetPoint.transform.position = groundPos;
    }

    // (!) perhaps change to legTargetPoints for sooner rotating motion?
    void RotateSpiderBody()
    {
        // Assuming legRestingPoints order is:
        // [0] = Front Left, [2] = Front Right, [3] = Back Right, [1] = Back Left

        /*float leftHeight = (legRestingPoints[0].transform.position.y + legRestingPoints[1].transform.position.y) / 2f;
        float rightHeight = (legRestingPoints[2].transform.position.y + legRestingPoints[3].transform.position.y) / 2f;

        float frontHeight = (legRestingPoints[0].transform.position.y + legRestingPoints[2].transform.position.y) / 2f;
        float backHeight = (legRestingPoints[3].transform.position.y + legRestingPoints[1].transform.position.y) / 2f;*/

        float leftHeight = (legTargetPoints[0].transform.position.y + legTargetPoints[1].transform.position.y) / 2f;
        float rightHeight = (legTargetPoints[2].transform.position.y + legTargetPoints[3].transform.position.y) / 2f;

        float frontHeight = (legTargetPoints[0].transform.position.y + legTargetPoints[2].transform.position.y) / 2f;
        float backHeight = (legTargetPoints[3].transform.position.y + legTargetPoints[1].transform.position.y) / 2f;

        // Roll (X-axis): Difference between left and right heights
        float rollAngle = Mathf.Atan2(leftHeight - rightHeight, legSpacingLeftRight) * Mathf.Rad2Deg;

        // Pitch (Z-axis): Difference between front and back heights
        float pitchAngle = Mathf.Atan2(frontHeight - backHeight, legSpacingFrontBack) * Mathf.Rad2Deg;

        // Apply rotation smoothly
        Quaternion targetRotation = Quaternion.Euler(pitchAngle, spiderPhysBody.transform.rotation.eulerAngles.y, rollAngle);
        spiderPhysBody.transform.rotation = Quaternion.Slerp(spiderPhysBody.transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
    }

    void InitializeSpiderStance()
    {
        for(int i = 0; i < legOriginalPoints.Length; i++)
        {
            Vector3 groundPos = GetGroundPosition(legOriginalPoints[i].transform.position);
            legRestingPoints[i].transform.position = groundPos;
        }
    }

    Vector3 GetGroundPosition(Vector3 origin)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, GroundLayer))
        {
            return hit.point;
        }
        return origin; // Default to the original position if no ground detected
    }

    // Get standable points not necessarily ground
    Vector3 GetGroundPosition2(Vector3 origin)
    {
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Spider");

        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            return hit.point;
        }
        return origin; // Default to the original position if no ground detected
    }

    void PairLegs()
    {
        movingPair1.Add(legRestingPoints[0]);
        movingPair1.Add(legRestingPoints[3]);
        movingPair2.Add(legRestingPoints[1]);
        movingPair2.Add(legRestingPoints[2]);
    }
}
