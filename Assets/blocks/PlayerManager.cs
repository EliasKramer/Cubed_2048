using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public GameObject prefabSquare;
    public GameObject redLine;
    public GameObject score;
    public GameObject gameOverObj;
    public GameObject cam;
    public GameObject outerBound;
    public GameObject errorMsg;

    private float deathHeight;
    private float spawnHeight;
    private GameObject currentHoldingObj = null;
    private DateTime lastTimeSquareMade = DateTime.UtcNow;
    private bool mouseDown = false;
    private HashSet<int> possibleSpawnValues = new HashSet<int>();
    private Vector2 boundSize = Vector2.zero;

    private bool keepSpeedAfterRelease = false;
    private float distanceRedlineToSquare = .3f;
    private float textBlockDist = .4f;

    private float explosionForce = 0.5f;

    private Vector2 mousePos;
    private Vector2 touchPos;
    private Vector2 cursorPos;
    private void Start()
    {
        possibleSpawnValues.Add(2);
        possibleSpawnValues.Add(4);
        possibleSpawnValues.Add(8);
        manageSettings();
        setOuterBounds();
        setScorePosition();
        deathHeight = spawnPos().y -
            prefabSquare.transform.localScale.y -
            redLine.transform.localScale.y -
            distanceRedlineToSquare;
        setRedLine();
    }

    private void manageSettings()
    {
        float bounceValue = .5f;
        bool keepSpeed = false;
        float size = 2f;
        float gravityScale = 1.5f;
        float mass = 10f;
        float explosionForce = .5f;
        try
        {
            string url = Path.Combine(Application.absoluteURL, "settings.csv");
            string[] rawData = File.ReadAllLines(url);

            string bouncinessPercent = (rawData[1].Split(';')[0]).Trim(' ');
            Debug.Log("bouncinessPercent: " + bouncinessPercent);
            string keepSpeedAfterDropping = (rawData[2].Split(';')[0]).Trim(' ').ToLower();
            Debug.Log("keepSpeedAfterDropping: " + keepSpeedAfterDropping);
            string blockSize = (rawData[3].Split(';')[0]).Trim(' ');
            Debug.Log("blockSize: " + blockSize);
            string gravity = (rawData[4].Split(';')[0]).Trim(' ');
            Debug.Log("gravity: " + gravity);
            string massValue = (rawData[5].Split(';')[0]).Trim(' ');
            Debug.Log("massValue: " + massValue);
            string explosionForceValue = (rawData[6].Split(';')[0]).Trim(' ');
            Debug.Log("explosionForceValue: " + explosionForceValue);

            bounceValue = valueBetween(int.Parse(bouncinessPercent), 0, 100) / 100;
            Debug.Log("bounce value set");
            keepSpeed = bool.Parse(keepSpeedAfterDropping);
            Debug.Log("keepSpeed value set");
            size = valueBetween(int.Parse(blockSize), 100, 500) / 100;
            Debug.Log("size value set");
            gravityScale = valueBetween(float.Parse(gravity), 0.1f, 10);
            Debug.Log("gravityScale value set");
            mass = valueBetween(float.Parse(massValue), 1, 100);
            Debug.Log("mass value set");
            explosionForce = valueBetween(float.Parse(explosionForceValue), -100f, 100f);

        }
        catch (Exception e)
        {
            displayError("gpfuscht bei de settings");
            Debug.Log(e);
        }

        prefabSquare.GetComponent<Rigidbody2D>().sharedMaterial.bounciness = bounceValue;
        keepSpeedAfterRelease = keepSpeed;
        prefabSquare.transform.localScale = new Vector3(size, size, size);
        prefabSquare.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
        prefabSquare.GetComponent<Rigidbody2D>().mass = mass;
        this.explosionForce = explosionForce;
    }

    private void setScorePosition()
    {
        Vector2 sizeOfScore = score.GetComponent<RectTransform>().sizeDelta;
        score.GetComponent<RectTransform>().position = new Vector2(0, boundSize.y / 2 - sizeOfScore.y / 2 + textBlockDist);

        spawnHeight = boundSize.y / 2 - sizeOfScore.y - .2f;
    }

    private void setOuterBounds()
    {
        var camera = cam.GetComponent<Camera>();
        float height = camera.orthographicSize * 2;
        float width = height * camera.aspect;
        boundSize = new Vector2(width, height);
        Debug.Log("height: " + height + " width: " + width);
        Vector2 boundScale = outerBound.transform.localScale;

        Instantiate(outerBound, new Vector3(0, height / 2 + boundScale.y / 2, 0), Quaternion.identity);
        Instantiate(outerBound, new Vector3(0, -height / 2 - boundScale.y / 2, 0), Quaternion.identity);
        Instantiate(outerBound, new Vector3(width / 2 + boundScale.x / 2, 0, 0), Quaternion.identity);
        Instantiate(outerBound, new Vector3(-width / 2 - boundScale.x / 2, 0, 0), Quaternion.identity);
    }

    private void setRedLine()
    {
        float redLineHeight = redLine.transform.localScale.y;
        float squareHeight = prefabSquare.transform.localScale.y;
        float yPos = deathHeight + (redLineHeight / 2) + (squareHeight / 2);
        Instantiate(redLine, new Vector3(redLine.transform.position.x, yPos, redLine.transform.position.z), Quaternion.identity);
    }
    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
        updateMousePos();
        //mouse down
        DateTime now = DateTime.UtcNow;
        double timeDiff = (now - lastTimeSquareMade).TotalMilliseconds;
        if (timeDiff > 500 && currentHoldingObj == null)
        {
            currentHoldingObj = Instantiate(prefabSquare, spawnPos(), Quaternion.identity, gameObject.transform);
            currentHoldingObj.GetComponent<BoxCollider2D>().enabled = false;
            currentHoldingObj.GetComponent<Rigidbody2D>().gravityScale = 0;

        }
        else if (currentHoldingObj != null)
        {
            if (keepSpeedAfterRelease)
            {
                Vector3 offset = spawnPos() - currentHoldingObj.transform.position;
                float mult = MultiplierForRange(offset.magnitude, .01f, .02f, false);
                currentHoldingObj.GetComponent<Rigidbody2D>().velocity = offset * mult * 10;
            }
            else
            {
                currentHoldingObj.transform.position = spawnPos();
            }
        }
    }
    private void updateMousePos()
    {
        Vector2 currMousePos = Vector2.zero;
        Vector2 currTouchPos = Vector2.zero;

        try
        {
            currMousePos = Input.mousePosition;
        }
        catch (Exception e)
        { }
        try
        {
            currTouchPos = Input.GetTouch(0).position;
        }
        catch (Exception e)
        { }



        Vector2 mouseOffset = currMousePos - mousePos;
        Vector2 touchOffset = currTouchPos - touchPos;

        if (mouseOffset.magnitude > touchOffset.magnitude)
        {
            cursorPos = currMousePos;
        }
        else
        {
            cursorPos = currTouchPos;
        }

    }

    private Vector3 spawnPos()
    {
        Vector3 mousePos = cursorPos;
        //convert mouse position to world position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        //get camera pixel size

        worldPos.z = 5;
        worldPos.y = spawnHeight - prefabSquare.transform.localScale.y / 2 + .1f + prefabSquare.transform.localScale.y;
        //Debug.Log("border: " + border + " worldpos.x " + worldPos.x);
        float posBounds = boundSize.x / 2 - prefabSquare.transform.localScale.x / 2;
        if (worldPos.x > posBounds)
        {
            worldPos.x = posBounds;
        }
        else if (worldPos.x < -posBounds)
        {
            worldPos.x = -posBounds;
        }
        return worldPos;
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dropSquare();
        }
    }
    public void OnTouch(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            dropSquare();
        }
    }
    public void dropSquare()
    {
        currentHoldingObj.GetComponent<BoxCollider2D>().enabled = true;
        currentHoldingObj.GetComponent<Rigidbody2D>().gravityScale = 1;
        currentHoldingObj.GetComponent<SquareScript>().deleteDropIndication();

        currentHoldingObj = null;
        lastTimeSquareMade = DateTime.UtcNow;
    }
    public void addScore(int score)
    {
        this.score.GetComponent<ScoreScript>().addValue(score);
    }
    public void gameOver()
    {
        GameObject gameOver = Instantiate(gameOverObj, new Vector3(0, 0, 0), Quaternion.identity);
        gameOver.GetComponent<TextMeshPro>().text = "Game Over \r\n Score: " + score.GetComponent<ScoreScript>().getValue();
    }
    public void addPossibleSpawnValue(int value)
    {
        if (value <= 128)
        {
            possibleSpawnValues.Add(value);
        }
    }
    public int getRandomSpawnValue()
    {
        int index = UnityEngine.Random.Range(0, possibleSpawnValues.Count);
        return possibleSpawnValues.ToList()[index];
    }
    public float MultiplierForRange(float range, float innerBorder, float outerBorder, bool negativeValuesAllowed)
    {
        float mappedRange = outerBorder - innerBorder;

        if (mappedRange < 0)
        {
            Debug.LogError("Mapping failed. Inner border should be smaller than outer border");
            return float.MinValue;
        }

        //if you dont want negative speed
        if (range < innerBorder && !negativeValuesAllowed)
        {
            return 0;
        }
        if (range < outerBorder)
        {
            float rangeInBorderSystem = range - innerBorder;
            float retVal = rangeInBorderSystem / mappedRange;
            return retVal >= -1 ? retVal : -1;
        }
        return 1;
    }
    /// <summary>
    /// if the values is smaller than min it will return min
    /// if the value is bigger than max it will return max
    /// else return the value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float valueBetween(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }
        if (value > max)
        {
            return max;
        }
        return value;
    }
    public void displayError(string error, int time = 3000)
    {
        GameObject errorObj = Instantiate(errorMsg, new Vector3(0, 0, 0), Quaternion.identity);
        errorObj.GetComponent<TextMeshPro>().text = "error: " + error;
        //destroy object after time
        Destroy(errorObj, time / 1000);
    }
    public float getDeathHeight()
    {
        return deathHeight;
    }
    public float getExplosionForce()
    {
        return explosionForce;
    }
}