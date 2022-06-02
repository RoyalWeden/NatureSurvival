using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrder : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private Transform bottomObj;

    void Start() {
        bottomObj = transform.Find("Bottom");
        if(gameObject.tag == "landscape" || gameObject.tag == "pond") {
            spriteRenderer = transform.GetComponent<SpriteRenderer>();
            float randScale = Random.Range(0.5f, 1.5f);
            transform.localScale = new Vector3(randScale, randScale, randScale);
        } else {
            spriteRenderer = transform.Find("Skin").GetComponent<SpriteRenderer>();
        }
    }


    void Update() {
        spriteRenderer.sortingOrder = (int)(-bottomObj.position.y * 10000);
    }
}
