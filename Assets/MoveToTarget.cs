using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            print("-----------------");
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            Vector3 prev = transform.position;
            if (Physics.Raycast(castPoint,out hit,Mathf.Infinity))
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
    }
}
