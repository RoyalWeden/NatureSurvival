using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour {

    private float spawnIncreaseRate;
    private float lastSpawnTime = -1;
    private List<Transform> ponds;

    // Prefabs
    public GameObject playerPrefab;
    public GameObject monkeyPrefab;
    public GameObject tigerPrefab;
    public GameObject hippoPrefab;

    void Start() {
        spawnIncreaseRate = 15f;
        ponds = new List<Transform>();
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("pond")) {
            ponds.Add(g.transform);
        }
    }

    void Update() {
        if(Mathf.FloorToInt(Time.fixedTime / spawnIncreaseRate) > lastSpawnTime) {
            lastSpawnTime = Mathf.FloorToInt(Time.fixedTime / spawnIncreaseRate);
            for(int i=0; i<lastSpawnTime+1; i++) {
                SpawnAnimal();
            }
        }
    }

    public GameObject SpawnPlayer() {
        return Instantiate(playerPrefab);
    }

    private void SpawnAnimal() {
        int randAnimal = Random.Range(0, 6);
        GameObject newAnimal;
        if(randAnimal <= 2) { // monkey
            newAnimal = Instantiate(monkeyPrefab, new Vector2(Random.Range(-.8f, .8f), Random.Range(-.4f, .4f)), Quaternion.identity, transform);
        } else if(randAnimal <= 4) { // tiger
            newAnimal = Instantiate(tigerPrefab, new Vector2(Random.Range(-.8f, .8f), Random.Range(-.4f, .4f)), Quaternion.identity, transform);
        } else { // hippo
            newAnimal = Instantiate(hippoPrefab, new Vector2(Random.Range(-.8f, .8f), Random.Range(-.4f, .4f)), Quaternion.identity, ponds[Random.Range(0, ponds.Count)]);
        }
        GameManager.AddAnimal(newAnimal);
    }
}
