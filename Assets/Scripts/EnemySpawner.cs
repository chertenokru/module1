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
    
    
    void Awake()
    {
        Spawn();
    }

    

    [ContextMenu("Player Move")]
    private void Spawn()
    {
        
        for (var i = 0; i < enemyCont; i++)
        {
            var obj = enemy[Random.Range(0, enemy.Length )];
            Vector3 vector3 = new Vector3(Random.Range(planeBoundToSpawn.bounds.min.x, planeBoundToSpawn.bounds.max.x),planeBoundToSpawn.bounds.min.y,
                Random.Range(planeBoundToSpawn.bounds.min.z, planeBoundToSpawn.bounds.max.z));
            Instantiate(obj, vector3, Quaternion.identity);
        }
    }
}
