using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class GameManager {
    private static GameObject entitySpawner;

    // Player
    private static GameObject player;
    private static GenderType playerGender;

    // Animals
    private static List<GameObject> animals;

    // Score
    private static TextMeshProUGUI scoreText;
    private static int score;

    // Start new game
    public static void ResetGame() {
        playerGender = GenderType.Female;
        scoreText = GameObject.FindObjectOfType<Canvas>().transform.Find("Score").GetComponent<TextMeshProUGUI>();
        entitySpawner = GameObject.Find("Land Animals").gameObject;
        score = 0;
        scoreText.SetText(score.ToString());
        if(animals != null) {
            for(int i=0; i<animals.Count; i++) {
                try {
                    animals[0].GetComponent<AnimalController>().DestroySelf();
                } catch {

                }
                animals.RemoveAt(0);
            } 
        }
        animals = new List<GameObject>();
        if(player == null) {
            player = entitySpawner.GetComponent<EntitySpawner>().SpawnPlayer();
            player.GetComponent<PlayerController>().ResetPlayer(new Vector2(0, 0));
            player.GetComponent<PlayerController>().ResetSpear();
        } else {
            player.GetComponent<PlayerController>().ResetPlayer(new Vector2(0, 0));
            player.GetComponent<PlayerController>().ResetSpear();
        }
    }

    public static void SetPlayer(GameObject newPlayer) {
        player = newPlayer;
    }

    public static void AddAnimal(GameObject newAnimal) {
        animals.Add(newAnimal);
    }

    public static void IncreaseScore() {
        score++;
        scoreText.SetText(score.ToString());
    }

    public enum GenderType {
        Male,
        Female
    };
    public enum HitType {
        Bite,
        Claw,
        None
    };
    public enum AnimalType {
        Monkey,
        Tiger,
        Hippo
    }

    public static void SetPlayerGender() {

    }

    public static GenderType GetPlayerGender() {
        return playerGender;
    }
}
