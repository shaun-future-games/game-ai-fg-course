using UnityEngine;

public class GuardSensor : MonoBehaviour
{

    /// <summary>
    ///  Perception helper: detects a player target in rang/FOV and check line of sight via raycast
    ///  The BT uses this through a custom action node
    /// </summary>

    [Header("Target")]
    [SerializeField] private string targetTag = "Player";

    [Header("View")]
    [SerializeField] private float viewDistance = 10f;
    [Range(1f,180f)]
    [SerializeField] private float viewAngle = 90f;

    [Header("Line of Sight")]
    [SerializeField] private Transform eyes;
    [SerializeField] private LayerMask occlusionMask = ~0; // everything by default

    [SerializeField] private Transform cachedTarget;

    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;

    private Transform EyesTransform => eyes != null ? eyes : transform;

    private void Awake()
    {
        // cache once; good enough for now (improve later)
        GameObject go = GameObject.FindGameObjectWithTag(targetTag);
        if (go != null)
        {
            cachedTarget = go.transform;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool TrySenseTarget(out GameObject target, out Vector3 lastKnownPosition, out bool hasLineOfSight)
    {
        target = null;
        lastKnownPosition = default;
        hasLineOfSight = false;

        if (cachedTarget == null) return false;

        Vector3 eyePos = EyesTransform.position;
        Vector3 toTarget = cachedTarget.position - eyePos;

        float dist = toTarget.magnitude;
        if (dist > viewDistance) return false;

        Vector3 toTargetDir = toTarget / Mathf.Max(dist, 0.0001f);

        float halfAngle = viewAngle * 0.5f;
        float angle = Vector3.Angle(EyesTransform.forward, toTargetDir);

        if (angle > halfAngle) return false;

        // Raycast to check occulusion
        if (Physics.Raycast(eyePos, toTargetDir, out RaycastHit hit, viewDistance, occlusionMask))
        {
            if (hit.transform != cachedTarget) return false;
        }

        target = cachedTarget.gameObject;
        lastKnownPosition = cachedTarget.position;
        hasLineOfSight = true;
        return true;
    }
}
