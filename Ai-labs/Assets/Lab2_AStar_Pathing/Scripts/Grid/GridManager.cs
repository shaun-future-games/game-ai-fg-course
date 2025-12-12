using Lab2.Pathing;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Lab2.Grid
{

    // checkpoint Q's:
    /*
     * What's a node and what's an edge
     * 
     * A node is a tile and an edge is the side of the square so to speak
     * 
     * How does your grid coordinate (x,y) map to world position(x*cellSize, z*cellSize)
     * 
     * Cell size is set to 1 and the cube that makes the tile is 1x1 so every tile lines up perfectly to the next
     *   but if you look at the overall world the tile middle is at the world position and all tiles on on the 
     *   positive site of the axis.
     *   
     * What happens if you try to access nodes[x, y] with corrdinates outside the array bounds? How are you preventing it?
     * 
     * You get an index out of bounds, this is prevented but ensuring that your methods are within width and height.
     * 
     * What unity function is used to convert a screen position to a 3D ray?
     * 
     * ScreenPointToRay() is used to get the mouse pointer click.
     * 
     * Why is it usefull to visualize walkable vs non-walkable tiles clearly
     * 
     * So you debug your pathfinding
     * 
     * What does it mean in graph terms when you turn a tile into a wall
     * 
     * According to the graph you are removing those nodes and edges from the graph
     * 
     */
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] public float cellSize = 1f;
        [Header("Prefabs & Materials")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Material walkableMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material goalMaterial;
        [SerializeField] private Material startMaterial;
        [SerializeField] private Material pathMaterial;
        private Node[,] nodes;
        private Dictionary<GameObject, Node> tileToNode = new();
        // Input action for click
        private InputAction clickAction;
        private InputAction middleClickAction;
        private InputAction rightClickAction;
        private InputAction pathfindAction;

        public Pathfinder AStar = new Pathfinder();
        public List<Node> CurrentPath = null;
        public bool StartMoving = false;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public Node startNode;
        public Node goalNode;
        private void Awake()
        {
            GenerateGrid();
        }
        private void OnEnable()
        {
            clickAction = new InputAction(
            name: "Click",
            type: InputActionType.Button,
            binding: "<Mouse>/leftButton"
            );
            clickAction.performed += OnClickPerformed;
            clickAction.Enable();
            middleClickAction = new InputAction(
            name: "MiddleClick",
            type: InputActionType.Button,
            binding: "<Mouse>/middleButton"
            );
            middleClickAction.performed += OnMiddleClickPerformed;
            middleClickAction.Enable();
            rightClickAction = new InputAction(
            name: "RightClick",
            type: InputActionType.Button,
            binding: "<Mouse>/rightButton"
            );
            rightClickAction.performed += OnRightClickPerformed;
            rightClickAction.Enable();
            pathfindAction = new InputAction(
            name: "Pathfind",
            type: InputActionType.Button,
            binding: "<Keyboard>/space"
            );
            pathfindAction.performed += OnPathfindPerformed;
            pathfindAction.Enable();
        }
        private void OnDisable()
        {
            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }
            if (middleClickAction != null)
            {
                middleClickAction.performed -= OnMiddleClickPerformed;
                middleClickAction.Disable();
            }
            if (rightClickAction != null)
            {
                rightClickAction.performed -= OnRightClickPerformed;
                rightClickAction.Disable();
            }
            if (pathfindAction != null)
            {
                pathfindAction.performed -= OnPathfindPerformed;
                pathfindAction.Disable();
            }
        }
        private void GenerateGrid()
        {
            nodes = new Node[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPos = new Vector3(x * cellSize, 0f,
                    y * cellSize);
                    GameObject tileGO = Instantiate(tilePrefab,
                    worldPos, Quaternion.identity, transform);
                    tileGO.name = $"Tile_{x}_{y}";
                    Node node = new Node(x, y, true, tileGO);
                    nodes[x, y] = node;
                    tileToNode[tileGO] = node;
                    SetTileMaterial(node, walkableMaterial);
                }
            }
            // randomly set start and end tile
            int xStart = Random.Range(0, width);
            int yStart = Random.Range(0, height);
            startNode = GetNode(xStart, yStart);
            SetTileMaterial(GetNode(xStart, yStart), startMaterial);
            // make sure goal isn't start
            int xGoal, yGoal;
            do
            {
                xGoal = Random.Range(0, width);
                yGoal = Random.Range(0, height);
            } while (xGoal == xStart && yGoal == yStart);
            goalNode = GetNode(xGoal, yGoal);
            SetTileMaterial(GetNode(xGoal, yGoal), goalMaterial);


        }
        private void OnClickPerformed(InputAction.CallbackContext
        ctx)
        {
            HandleMouseClick();
        }
        private void HandleMouseClick()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Ray ray =
            cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                if (tileToNode.TryGetValue(clicked, out Node node))
                {
                    if (node == startNode || node == goalNode) return;
                    bool newWalkable = !node.walkable;
                    SetWalkable(node, newWalkable);
                }
            }
        }

        // middle click changes start node locat and old start node to walkable
        private void OnMiddleClickPerformed(InputAction.CallbackContext ctx)
        {
            HandleMiddleClick();
        }
        private void HandleMiddleClick()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Ray ray =
            cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                if (tileToNode.TryGetValue(clicked, out Node node))
                {
                    if (node == startNode || node == goalNode) return;
                    var oldStartNode = startNode;
                    startNode = node;
                    SetTileMaterial(startNode, startMaterial);
                    SetWalkable(oldStartNode, true);
                    StartMoving = false;
                }
            }
            
        }

        // right click changes goal node location and old goal node to walkable
        private void OnRightClickPerformed(InputAction.CallbackContext ctx)
        {
            HandleRightClick();
        }
        private void HandleRightClick()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Ray ray =
            cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                if (tileToNode.TryGetValue(clicked, out Node node))
                {
                    if (node == startNode || node == goalNode) return;
                    var oldGoalNode = goalNode;
                    goalNode = node;
                    SetTileMaterial(goalNode, goalMaterial);
                    SetWalkable(oldGoalNode, true);
                    StartMoving = false;
                }
            }
        }
        
        private void OnPathfindPerformed(InputAction.CallbackContext ctx)
        {
            ResetBoardColors();
            List<Node> path = AStar.FindPath(startNode, goalNode, this);
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    SetTileMaterial(path[i], pathMaterial);
                }
                 CurrentPath = path;
                StartMoving = true;
            }
        }


        public Node GetNode(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            return nodes[x, y];
        }
        public Node GetNodeFromWorldPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.z / cellSize);
            return GetNode(x, y);
        }
        public IEnumerable<Node> GetNeighbours(Node node, bool
        allowDiagonals = false)
        {
            int x = node.x;
            int y = node.y;
            // 4-neighbour
            yield return GetNode(x + 1, y);
            yield return GetNode(x - 1, y);
            yield return GetNode(x, y + 1);
            yield return GetNode(x, y - 1);
            if (allowDiagonals)
            {
                yield return GetNode(x + 1, y + 1);
                yield return GetNode(x - 1, y + 1);
                yield return GetNode(x + 1, y - 1);
                yield return GetNode(x - 1, y - 1);
            }
        }
        public void SetWalkable(Node node, bool walkable)
        {
            node.walkable = walkable;
            SetTileMaterial(node, walkable ? walkableMaterial :
            wallMaterial);
        }
        private void SetTileMaterial(Node node, Material mat)
        {
            var renderer = node.tile.GetComponent<MeshRenderer>();
            if (renderer != null && mat != null)
            {
                renderer.material = mat;
            }
        }

        private void ResetBoardColors()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (nodes[x, y] == startNode || nodes[x, y] == goalNode) { continue; }
                    var node = GetNode(x, y);
                    SetTileMaterial(node, node.walkable ? walkableMaterial : wallMaterial);
                }
            }
        }
    }
}