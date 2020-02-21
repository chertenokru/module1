using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private LineRenderer lineRenderer;
    private NavMeshPath pathNavMesh;
    private int layerMask;
    private GameController gameController;


    private Vector3 oldMousePosition;

    void Start()
    {
        pathNavMesh   = new NavMeshPath();
        navMeshAgent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();
        gameController = GetComponent<GameController>();
        layerMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouse = Input.mousePosition;
        if ((mouse - oldMousePosition).sqrMagnitude > .1f)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse);

            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                lineRenderer.positionCount = 0;
            else {
                if (navMeshAgent.CalculatePath(hit.point, pathNavMesh))
                {
                    lineRenderer.positionCount = pathNavMesh.corners.Length;
                    lineRenderer.SetPositions(pathNavMesh.corners);
                }
            }
            
            
            
        }

        if (Input.GetMouseButtonDown(0))
        {
          //  print("-----------------");
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            Vector3 prev = transform.position;
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
            {
                NavMeshPath path = new NavMeshPath();
                navMeshAgent.CalculatePath(hit.point, path);
                foreach (Vector3 pathCorner in path.corners)
                {
                    print((prev - pathCorner).magnitude);
                    prev = pathCorner;
                }

                //path.corners);
                navMeshAgent.SetDestination(hit.point);
            }
        }

        oldMousePosition = mouse;
    }
}