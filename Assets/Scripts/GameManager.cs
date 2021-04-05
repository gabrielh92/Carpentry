using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    static System.Random rng;

    private void Awake() {
        if(instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

        if(rng == null) {
            rng = new System.Random(Time.time.GetHashCode() * Input.mousePosition.GetHashCode());
        }
    }

    public static int GetRandomNumber(int _min, int _max) {
        return rng.Next(_min, _max);
    }
}
