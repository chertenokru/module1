using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MouseController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private LineRenderer lineRenderer;
    private NavMeshPath pathNavMesh;
    private int layerMask;
    private int layerMaskObject;
    private GameController gameController;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private GraphicRaycaster graphicRaycaster;
    private RaycastHit raycastHit;
    private GameObject selectedObject;
    private bool isObjectSelected;
    private Character character;
    private Vector3 mousePosition;
    private Vector3 oldMousePosition;
    private bool isClickDone;

    void Start()
    {
        pathNavMesh = new NavMeshPath();
        navMeshAgent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();
        gameController = FindObjectOfType<GameController>();
        layerMask = LayerMask.GetMask("Ground");
        layerMaskObject = LayerMask.GetMask("InteractiveObject");
        eventSystem = FindObjectOfType<EventSystem>();
        pointerEventData = new PointerEventData(eventSystem);
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        isClickDone = false;
        mousePosition = Input.mousePosition;
        // не частим
        if (Input.GetMouseButtonDown(0) || (mousePosition - oldMousePosition).sqrMagnitude > 5f)
            // если над UI то ничего не делаем
            if (!isCursorInUI(mousePosition))
            {
                Ray castPoint = Camera.main.ScreenPointToRay(mousePosition);

                // есть что-нибудь интерактивное под крусором?
                if (Physics.Raycast(castPoint, out raycastHit, Mathf.Infinity, layerMaskObject))
                {
                    GameObject selGameObject = raycastHit.transform.gameObject;
                    if (isObjectSelected && selGameObject != selectedObject)
                    {
                        if (isObjectSelected) gameController.MoveOffSelectebleObject(selectedObject);
                        isObjectSelected = false;
                    }

                    // кликнули
                    if (Input.GetMouseButtonDown(0))
                    {
                        isClickDone = true;
                        gameController.ClickOnSelectebleCharacter(selGameObject);
                    }
                    // просто мышку подвели
                    else if (gameController.MoveOnSelectebleObject(selGameObject))
                    {
                        isObjectSelected = true;
                        selectedObject = selGameObject;
                    }
                }
                // ничего нет, а есть с чего выделение снять?
                else if (isObjectSelected)
                {
                    gameController.MoveOffSelectebleObject(selectedObject);
                    isObjectSelected = false;
                }

                // а тут мы будем путь за мышкой рисовать и перемещать игрока
                if (!isClickDone && gameController.isMovingMode(out character) && character.IsIdle())
                {
                    if (!Physics.Raycast(castPoint, out raycastHit, Mathf.Infinity, layerMask))
                        lineRenderer.positionCount = 0;
                    else
                    {
                        //перемешаем
                        if (Input.GetMouseButtonDown(0))
                        {
                            gameController.MovePlayer(raycastHit.point);
                            lineRenderer.positionCount = 0;
                        }
                        //рисуем возможный путь
                        else
                        {
                            if (character.GetAlowedPath(raycastHit.point, pathNavMesh) > 0)
                            {
                                lineRenderer.positionCount = pathNavMesh.corners.Length;
                                lineRenderer.SetPositions(pathNavMesh.corners);
                            }
                        }
                    }
                }
                else lineRenderer.positionCount = 0;


                oldMousePosition = mousePosition;
            }
    }

    private bool isCursorInUI(Vector3 mouse)
    {
        raycastResults.Clear();
        pointerEventData.position = mouse;
        graphicRaycaster.Raycast(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }
}