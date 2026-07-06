using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController player;

    public int fruitsCollected;
    private void Awake()
    {
       if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void AddFruit()
    {
        fruitsCollected++;
        Debug.Log("Fruits Collected: " + fruitsCollected);
    }

}
