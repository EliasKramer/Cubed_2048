using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour
{
    // Start is called before the first frame update
    private DateTime started;
    void Start()
    {
        started = DateTime.UtcNow;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = 0;
        if ((DateTime.UtcNow - started).TotalSeconds > 3)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Time.timeScale = 1;
        }
    }
}
