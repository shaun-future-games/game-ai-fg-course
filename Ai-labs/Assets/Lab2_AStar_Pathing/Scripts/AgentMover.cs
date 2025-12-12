using UnityEngine;
using Lab2.Grid;
using System.Collections.Generic;

public class AgentMover : MonoBehaviour
{
    [SerializeField]
    private GridManager gridManager;

    public float MoveSpeed = 3f;

    private List<Node> currentPath;
    private int currentPathIndex = 0;
    private bool started = false;

    private float distanceTolerance = 0.1f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gridManager.StartMoving && !started)
        {
            StartFollowingPath();
        }
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }
        if (currentPathIndex < currentPath.Count && started)
        {
            Node targetNode = currentPath[currentPathIndex];
            Vector3 targetPos = NodeToWorldPosition(targetNode);

            // move towards the target
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < distanceTolerance)
            {
                currentPathIndex++;
           
                if (currentPathIndex >= currentPath.Count)
                {
                    started = false;
                    gridManager.StartMoving = false;
                }
            }
        }
        
    }

    private void StartFollowingPath()
    {
            currentPath = gridManager.CurrentPath;
            currentPathIndex = 0;
            transform.position = NodeToWorldPosition(currentPath[currentPathIndex]);
            started = true;
    }

    public Vector3 NodeToWorldPosition(Node nodePos)
    {
        int x = Mathf.RoundToInt(nodePos.x * gridManager.cellSize);
        int y = Mathf.RoundToInt(nodePos.y * gridManager.cellSize);
        return new Vector3(x, 0f, y);
    }
}
