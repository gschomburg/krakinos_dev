﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using System.Diagnostics;

public class GridPath: MonoBehaviour{
	public GridSystem grid;
	public Node startNode;
	public Node endNode;

	public List<Node> path;

	public int currentIndex = 0; //can be stepped\

    public PathRequestManager requestManager;

    Coroutine pathFinderCoroutine;

    //public void find(GridSystem _grid, Vector3 _startPos, Vector3 _endPos)
    //{
    //    //figure out the nodes then run the node path
    //}
    void Awake()
    {
        //requestManager = PathRequestManager.GetInstance();
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<GridSystem>();
    }
    public void StartFindPath(GridSystem _grid, Node _startNode, Node _endNode)
    {
        //Debug.Log("StartFindPath");
        if (pathFinderCoroutine != null) StopCoroutine(pathFinderCoroutine);
        pathFinderCoroutine = StartCoroutine(find(_grid, _startNode, _endNode));
    }

    IEnumerator find(GridSystem _grid, Node _startNode, Node _endNode){
        //Stopwatch sw = new Stopwatch ();
        //sw.Start ();
        //Debug.Log("FindingPath");

        grid = _grid;
		startNode = _startNode;
		endNode = _endNode;

        bool pathSuccess = false;
        if (startNode.walkable && endNode.walkable)
        {

            //		List<Node> openSet = new List<Node>();
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);

            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(_startNode);
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                if (currentNode == _endNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbor in grid.getNeighbors(currentNode))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, _endNode);
                        neighbor.parent = currentNode;
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            RetracePath(_startNode, _endNode);
        }
        requestManager.FinishedProcessingPath(this, pathSuccess);
	}
    

   public  Vector3[] getNodeVectorPositions()
    {
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i<path.Count; i++)
        {
            positions[i] = grid.getTransformPosition(path[i].position);
        }
        return positions;
    }
	void RetracePath(Node _startNode, Node _endNode){
		path = new List<Node> ();
		Node currentNode = _endNode;
		while (currentNode != _startNode) {
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}
		//add in the start node to the path
		path.Add (_startNode);
		//can call simplify path here to remove extra nodes, could be good for drawing but not for step movement
		//maybe save a simplified path as well?
		path.Reverse ();
		//UnityEngine.Debug.Log ("path complete, nodes:"+path.Count);
	}

	Node[] SimplifyPath(List<Node> fullPath){
		List<Node> waypointNodes = new List<Node>();
		Vector3 directionOld = Vector3.zero;
		for (int i = 1; i < fullPath.Count; i++) {
			//calculate the direction between this node and the last node
			//some hting like below
			//Vector2 directionNewV2 = new Vector2(fullPath[i-1].position.x - fullPath[i].position.x, fullPath[i-1].position.y - fullPath[i].position.y);
			Vector3 directionNew = new Vector3 ();
			if(directionNew != directionOld){
				waypointNodes.Add (fullPath [i]);
			}
			directionOld = directionNew;
		}
		return waypointNodes.ToArray ();
	}

	int GetDistance(Node nodeA, Node nodeB){

		//TODO add z dimension
//		int diagD = 14;
//		int linD = 10;

//		int distX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
//		int distY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
//		int distZ = Mathf.Abs(nodeA.position.z - nodeB.position.z);
//
//		if (distX > distY) {
//			return diagD * distY + linD * (distX - distY);
//		} else {
//			return diagD * distX + linD * (distY - distX);
//		}
		float d = Vector3.Distance(new Vector3(nodeA.position.x, nodeA.position.y, nodeA.position.z), new Vector3(nodeB.position.x, nodeB.position.y, nodeB.position.z));
		return (int)(d*10);
	}
}
