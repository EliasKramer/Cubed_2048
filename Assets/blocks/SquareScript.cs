using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SquareScript : MonoBehaviour
{
    // Start is called before the first frame update
    private int squareValue;
    public GameObject combinationParticles;
    public GameObject dropIndicationPrefab;
    private GameObject dropIndicationObj;
    private PlayerManager player;
    //the minimal speed that has to be reached to trigger a gameover
    private const float deathSpeed = -0.1f;
    private DateTime lastTimeDeathSpeedWasReached = DateTime.UtcNow;
    private const double timeTillDeath = 500;
    private bool lastUpdateWasDeathSpeed = false;
    void Start()
    {
        player = transform.parent.GetComponent<PlayerManager>();
        squareValue = player.getRandomSpawnValue();
        
        dropIndicationPrefab.transform.localScale = new Vector2(1, 100);
        Vector2 prefabPos = new Vector2(transform.position.x, transform.position.y - (50*transform.localScale.y));
        dropIndicationObj = Instantiate(dropIndicationPrefab, prefabPos, Quaternion.identity, transform);
    }
    private void FixedUpdate()
    {
        if(GetComponent<BoxCollider2D>().enabled)
        {
            checkForDeath();
        }
    }

    private void checkForDeath()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        DateTime now = DateTime.UtcNow;
        bool isCurrentlyBelowDeathSpeed = rb.velocity.y > deathSpeed;

        if (!lastUpdateWasDeathSpeed && isCurrentlyBelowDeathSpeed)
        {
            lastTimeDeathSpeedWasReached = now;
        }
        lastUpdateWasDeathSpeed = isCurrentlyBelowDeathSpeed;

        double timeSinceStartDeathSpeed = (now - lastTimeDeathSpeedWasReached).TotalMilliseconds;

        bool enoughtTimeBelowDeathSpeed = timeSinceStartDeathSpeed > timeTillDeath && isCurrentlyBelowDeathSpeed;

        if (transform.position.y > player.getDeathHeight() &&
            enoughtTimeBelowDeathSpeed)
        {
            Debug.Log("game over");
            player.gameOver();
        }
    }

    private void Update()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb.velocity.magnitude > 1)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        else
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }
    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        collisionHandler(collision);
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        collisionHandler(collision);
    }
    private void collisionHandler(Collision2D collision)
    {
        if (collision.gameObject.tag == "Square" &&
            collision.gameObject.GetComponent<SquareScript>().getValue() == squareValue)
        {
            int newVal = squareValue * 2;
            setValue(newVal);
            addScore(newVal);
            player.addPossibleSpawnValue(newVal);
            
            GameObject particles = Instantiate(combinationParticles, transform.position, Quaternion.identity);
            particles.GetComponent<ParticleSystem>().startColor = GetComponentInChildren<textscript>().getColorForInt(squareValue);
            
            gameObject.GetComponent<Rigidbody2D>().velocity += (Vector2)(gameObject.transform.position - collision.transform.position).normalized * player.getExplosionForce();
            Destroy(collision.gameObject);
        }
    }

    private void setValue(int value)
    {
        squareValue = value;
    }

    public int getValue()
    {
        return squareValue;
    }

    private void addScore(int score)
    {
        Debug.Log("added score");
        player.addScore(score);
    }
    public void deleteDropIndication()
    {
        Destroy(dropIndicationObj);
    }
}
