﻿using System.Collections;
using UnityEngine;

public class StoneSet : MonoBehaviour
{
    protected Transform[] children;
    protected int degree;
    protected int[,,] rotationData = new int[4, 4, 2];
    protected int[,,] wallKickData = new int[4, 5, 2]
        {
            {{0,0},{-1,0},{-1,1},{0,-2},{-1,-2}},
            {{0,0},{1,0},{1,-1},{0,2},{1,2}},
            {{0,0},{1,0},{1,1},{0,-2},{1,-2}},
            {{0,0},{-1,0},{-1,-1},{0,2},{-1,2}}
        };
    // Use this for initialization

    protected virtual void Awake()
    {
        children = gameObject.GetComponentsInChildren<Transform>();
        degree = 0;
    }

    void Start()
    {
        firstSet();
        StartCoroutine("fall");
    }

    public void clicked(string str)
    {
        switch (str)
        {
            case "left": Move("left"); break;
            case "right": Move("right"); break;
            case "down": Move("down"); break;
            case "up": rotate(); break;
            case "fulldown": fullDown(); break;
        }
    }

    public void fullDown()
    {
        while(!isStuckedSet())
        {
            Move("down");
        }
        stopMove();
    }

    public virtual void rotate()
    {
        bool isValid;
        for (int _case = 0; _case < 5; _case++)
        {
            isValid = true;
            for (int idx = 0; idx < 4; idx++)
            {
                Vector2 vec = new Vector2(rotationData[degree, idx, 0] + wallKickData[degree, _case, 0], rotationData[degree, idx, 1] + wallKickData[degree, _case, 1]);
                if (!transform.GetChild(idx).GetComponent<Stone>().isValidPos(vec)) isValid = false;
            }
            for (int idx = 0; idx < 4; idx++)
            {
                Vector2 vec = new Vector2(rotationData[degree, idx, 0] + wallKickData[degree, _case, 0], rotationData[degree, idx, 1] + wallKickData[degree, _case, 1]);
                if (isValid) transform.GetChild(idx).GetComponent<Stone>().Move(vec);
            }
            if (isValid)
            {
                degree = (degree + 1) % 4;
                return;
            }
        }
    }

    public void resetFall()
    {
        StopCoroutine("fall");
        StartCoroutine("fall");
    }

    public bool isValidPosSet(Vector2 vec)
    {
        foreach (Transform child in children)
        {
            if (child.gameObject == gameObject || !child.CompareTag("Stone"))
                continue;
            if (!(child.gameObject.GetComponent<Stone>().isValidPos(vec)))
                return false;
        }
        return true;
    }

    public bool isStuckedSet()
    {
        foreach (Transform child in children)
        {
            if (child.gameObject == gameObject || !child.CompareTag("Stone"))
                continue;
            if ((child.gameObject.GetComponent<Stone>().isStucked()))
                return true;
        }
        return false;
    }

    public void Move(string dir)
    {
        Vector2 vec = Vector2.zero;
        switch (dir)
        {
            case "left": vec = Vector2.left; break;
            case "right": vec = Vector2.right; break;
            case "down":
                if (isStuckedSet())
                {
                    stopMove();
                    return;
                }
                vec = Vector2.down;
                break;
        }
        if (!isValidPosSet(vec)) return;
        
        foreach (Transform child in children)
        {
            if (child.gameObject == gameObject || !child.CompareTag("Stone"))
                continue;
            child.gameObject.GetComponent<Stone>().Move(vec);
        }
        if(dir == "down") resetFall();
    }

    public void firstSet()
    {
        foreach (Transform child in children)
        {
            if (child.gameObject == gameObject || !child.CompareTag("Stone"))
                continue;
            child.transform.GetComponent<Stone>().mapPos = new Vector2(-1, -1);
            Map.Instance.updateStone(child.transform.GetComponent<Stone>(), Vec2Math.roundVec2(child.transform.position));
        }
    }
    public void stopMove()
    {
        StopCoroutine("fall");
        StageManager.Instance.doNext();
        foreach (Transform child in children)
        {
            if (child.gameObject == gameObject || !child.CompareTag("Stone"))
                continue;
            child.parent = null;
            child.GetComponent<Stone>().startMove();
        }
        Destroy(gameObject);
    }

    IEnumerator fall()
    {
        while (true)
        {
            yield return new WaitForSeconds(StageManager.Instance.fallDelay);
            Move("down");

        }
    }

}
