using System.Collections.Generic;
using Lab2.Grid;
using UnityEngine;

namespace Lab2.Pathing
{
    public class Pathfinder
    {

        public List<Node> FindPath(Node startNode, Node goalNode, GridManager gridManager)
        {
            // 1. reset node costs
            startNode.gCost = 0;
            //startNode.hCost = 0;
            // 2 Initialize openSet and colosedSet
            HashSet<Node> openSet = new HashSet<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            // 3. set gCost and hCost for startNode
            startNode.hCost = HeuristicManhattan(startNode, goalNode);
            // 4. add startNode to openSet and loop until openset is empty
            openSet.Add(startNode);
            /* Checkpoint Questions:
             * Where do you actually compute g(n), h(n), and f(n)?
             * 
             * f(n) is computed in the foreach loop in the while loop
             * g(n) is computed as we loop through neighbors
             * h(n) is computed as we loop through neighbors after g(n)
             * 
             * Why do we use tentativeG < neighbour.gCost to decide whether we found a better path to a neighbour?
             * 
             * If we find a tentativeG that is less than the neighbour's gCost, we found a better path.
             * 
             * What would happen if your heuristic severely overestimates the real remaining distance?
             * 
             * A path will not be found or will be longer than it could be.
             * 
             * If you set h(n) = 0 for all nodes, what classic algorithm does A* become?
             * 
             * it becomes Dijkstra
             * 
             * If you ignored g(n) completely and only sorted by h(n), what behaviour would you expect to see?
             * 
             * You would get the manhattan path. Obsticles would be ignored.
             * 
             */

            while (openSet.Count > 0)
            {
                // pick node with lowest fCost
                Node currentNode = null;
                float lowestFCost = Mathf.Infinity;
                foreach (Node node in openSet)
                {
                    if (node.fCost < lowestFCost)
                    {
                        currentNode = node;
                        // f(n) computed here
                        lowestFCost = node.fCost;
                    }
                }
                // remove node from openSet
                openSet.Remove(currentNode);
                // add node to closedSet
                closedSet.Add(currentNode);
                // check if currentNode is goalNode
                if (currentNode == goalNode)
                {
                    // return path
                    List<Node> path = new List<Node>();
                    Node findPathNode = goalNode;

                    // Step backwards from Goal -> Start
                    while (findPathNode != startNode)
                    {
                        path.Add(findPathNode);
                        findPathNode = findPathNode.parent;
                    }

                    // Add the start node if you want the full path
                    // path.Add(startNode); 

                    // Reverse because we started from the Goal
                    path.Reverse();
                    return path;
                }

                // loop through neighbors
                foreach (Node neighbor in gridManager.GetNeighbours(currentNode))
                {
                    // check if neighbor is in closedSet
                    if (neighbor == null || closedSet.Contains(neighbor) || !neighbor.walkable)
                    {
                        continue;
                    }
                    // check if neighbor is in openSet
                    if (openSet.Contains(neighbor))
                    {
                        // check if neighbor gCost is lower than currentNode gCost
                        if (neighbor.gCost > currentNode.gCost + 1)
                        {
                            // g(n) computed here
                            neighbor.gCost = currentNode.gCost + 1;
                            // h(n) computed here
                            neighbor.hCost = HeuristicManhattan(neighbor, goalNode);
                            neighbor.parent = currentNode;
                        }
                    }
                    else
                    {
                        // g(n) computed here
                        neighbor.gCost = currentNode.gCost + 1;
                        // h(n) computed here
                        neighbor.hCost = HeuristicManhattan(neighbor, goalNode);
                        neighbor.parent = currentNode;
                        openSet.Add(neighbor);
                    }
                }
            }
            // return null if no path found
            return null;

        }

        // helper function (manhattan distance) to get hCost
        private float HeuristicManhattan(Node startNode, Node goalNode)
        {
            return Mathf.Abs(startNode.x - goalNode.x) + Mathf.Abs(startNode.y - goalNode.y);
        }
    }
}

