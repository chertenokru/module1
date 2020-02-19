using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // кого спавним
    public GameObject[] enemy;

    // где
    public MeshRenderer planeBoundToSpawn;

    // сколько
    public int enemyCont = 2;
    public Transform transformLookAtThis;
    public float distanceBetWeenEnemy = 0.5f;


    void Awake()
    {
        Spawn();
    }


    private void Spawn()
    {
        List<Transform> enemyList =new List<Transform>();
        Vector3 newPosition = default;
        for (var i = 0; i < enemyCont; i++)
        {
            GameObject newEnemyPrefab = enemy[Random.Range(0, enemy.Length)];
            bool isTrueObject = false;
            // пытаемся расставить объекты не ближе заданного расстояния друг к другу
            float startTime = Time.realtimeSinceStartup;
            while (!isTrueObject)
            {
                newPosition = new Vector3(Random.Range(planeBoundToSpawn.bounds.min.x, planeBoundToSpawn.bounds.max.x),
                    planeBoundToSpawn.bounds.min.y,
                    Random.Range(planeBoundToSpawn.bounds.min.z, planeBoundToSpawn.bounds.max.z));
                isTrueObject = true;
                foreach (Transform transform in enemyList)
                {
                    if ((transform.position - newPosition).sqrMagnitude < distanceBetWeenEnemy * distanceBetWeenEnemy)
                        isTrueObject = false;
                }

                // чтобы не зависнуть при большом числе объектов, в поисках не существующего варианта расстановки
                // ограничеваем время 0.1 секундой, после чего ставим как есть
                if ((Time.realtimeSinceStartup - startTime) > 0.1f) isTrueObject = true;
            }
            
            GameObject newIns = Instantiate(newEnemyPrefab, newPosition, Quaternion.identity);
            newIns.gameObject.transform.LookAt(transformLookAtThis);
            enemyList.Add(newIns.transform);
        }
        enemyList.Clear();
    }
}