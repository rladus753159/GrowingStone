﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    
    public float spawnPointX;
    public float spawnPointY;
    public float fallDelay;
    public float visualPointX;
    public float visualPointY;
    public float visualBetweenDistanceY;
    public int MatchCount;
    public int successH; //쌓아야하는 칸(0부터 시작)
    public int stageNum;
    public GameObject alpaca;
    public List<Stone> matchQueue = new List<Stone>{ };
    protected static StageManager instance = null;
    protected Queue<string> spawnQueue = new Queue<string> { };
    protected Vector2 successPos;
    protected string next;
    protected bool isSuccess = false;
    protected bool isCamMove;
    protected GameObject stoneSet;
    protected GameObject loose;
    protected GameObject UI;
    protected GameObject nextStone1;
    protected GameObject nextStone2;
    protected GameObject nextPanel;

    protected virtual void Awake ()
    {
        if (instance == null)
        {
            instance = this;
        }
        loose = GameObject.Find("Canvas").transform.Find("Loose").gameObject;
        UI = GameObject.Find("Canvas").transform.Find("UI").gameObject;
        nextPanel = GameObject.Find("NextPanel").gameObject;
        this.enabled = false;
    }

    protected virtual void OnEnable()
    {
        SoundManager.get("main").Play();
        spawnQueue.Clear();
        foreach (KeyValuePair<string, string> dic in StageData.Instance.getStageData(stageNum.ToString()))
        {
            if (!dic.Value.Equals("-1") && !dic.Value.Equals("")) spawnQueue.Enqueue(dic.Value);
        }
        if (spawnQueue.Count != 0)
            next = spawnQueue.Dequeue();
        spawnNext();
        isCamMove = false;
    }
    
    void FixedUpdate ()
    {
        if (isCamMove)
        {
            if (gameObject.transform.position.y < 7.5f)
                gameObject.transform.Translate(Vector3.up * Time.fixedDeltaTime * (2.5f / 0.8f));
            else
            {
                isCamMove = false;
                alpaca.SetActive(true);
            }
        }
        
    }

    public static StageManager Instance
    {
        get
        {
            return instance;
        }
    }

    public bool checkSuccess()
    {
        for (int i = 0; i < Map.Instance.width; i++)
        {
            if (Map.Instance.getStone(new Vector2(i, successH)) == null) continue;
            else if (!Map.Instance.getStone(new Vector2(i, successH)).isMoving)
            {
                successPos = new Vector2(i, 0);
                return true;
            }
        }
        return false;
    }

    public bool checkLoose()
    {
        return spawnQueue.Count == 0 && next == null && !isSuccess;
    }
    

    public void spawnNext()
    {
        if (checkSuccess())
        {
            success(successPos);
            return;
        }
        if (checkLoose())
        {
            Loose();
            return;
        }
        string current = next;
        if(spawnQueue.Count != 0)
            next = spawnQueue.Dequeue();
        else
            next = null;
        visualizeNext();
        string name = current.Substring(0, 1);
        string colors = current.Substring(2);
        GameObject prefab = Resources.Load("Prefabs/StoneSet" + name) as GameObject;
        stoneSet = Instantiate(prefab, new Vector3(spawnPointX, spawnPointY, 0), Quaternion.identity);
        for (int i = 0; i < colors.Length; i++)
        {
            stoneSet.transform.GetChild(i).GetComponent<Stone>().changeAppear(colors[i]);
        }
    }

    public bool isAllStop() {
        for (int y = 0; y < Map.Instance.height; y++)
        {
            for (int x = 0; x < Map.Instance.width; x++)
            {
                if (Map.Instance.getStone(new Vector2(x, y)) == null) continue;
                else if (Map.Instance.getStone(new Vector2(x, y)).isMoving) return false;
            }
        }
        return true;
    }

    public void visualizeNext()
    {
        Destroy(nextStone1);
        Destroy(nextStone2);
        if (next == null)
        {
            GameObject.Find("Remain").GetComponent<Text>().text = "남은 돌 : " + (spawnQueue.Count);
            return;
        }
        else GameObject.Find("Remain").GetComponent<Text>().text = "남은 돌 : " + (spawnQueue.Count + 1);
        string name = next.Substring(0, 1);
        string colors = next.Substring(2);
        GameObject prefab = Resources.Load("Prefabs/StoneSet" + name) as GameObject;
        nextStone1 = Instantiate(prefab, new Vector3(visualPointX, visualPointY, -1), Quaternion.identity);
        nextStone1.transform.localScale = Vector3.one * 0.58f;
        for (int i = 0; i < colors.Length; i++)
        {
            nextStone1.transform.GetChild(i).GetComponent<Stone>().changeAppear(colors[i]);
            Destroy(nextStone1.transform.GetChild(i).GetComponent<Stone>());
        }
        Destroy(nextStone1.GetComponent<StoneSet>());

        if (spawnQueue.Count == 0) return;
        name = spawnQueue.Peek().Substring(0, 1);
        colors = spawnQueue.Peek().Substring(2);
        prefab = Resources.Load("Prefabs/StoneSet" + name) as GameObject;
        nextStone2 = Instantiate(prefab, new Vector3(visualPointX + visualBetweenDistanceY, visualPointY, -1), Quaternion.identity);
        nextStone2.transform.localScale = Vector3.one * 0.58f;
        for (int i = 0; i < colors.Length; i++)
        {
            nextStone2.transform.GetChild(i).GetComponent<Stone>().changeAppear(colors[i]);
            Destroy(nextStone2.transform.GetChild(i).GetComponent<Stone>());
        }
        Destroy(nextStone2.GetComponent<StoneSet>());
    }


    public void doNext()
    {
        StartCoroutine("checkMatchsRoutine");
    }

    public void clicked(string str) 
    {
        SoundManager.get("touch").Play();
        if (stoneSet == null) return;
        stoneSet.GetComponent<StoneSet>().clicked(str);
    }

    public void Loose()
    {
        loose.SetActive(true);
        SoundManager.get("main").Stop();
        SoundManager.get("Stage fail").Play();
        //StopCoroutine("checkRoutine");
    }

    public void success(Vector3 successPos)
    {
        isSuccess = true;
        Destroy(nextStone1);
        Destroy(nextStone2);
        isCamMove = true;
        alpaca.GetComponent<Alpaca>().successPos = successPos;
        UI.SetActive(false);
        nextPanel.SetActive(false);
    }

    public void checkMatchs() {
        for (int y = 0; y < successH; y++)
        {
            for (int x = 0; x < Map.Instance.width; x++)
            {
                matchQueue.Clear();
                if (Map.Instance.getStone(new Vector2(x, y)) == null) continue;
                else if (Map.Instance.getStone(new Vector2(x, y)).isMoving) continue;
                Map.Instance.getStone(new Vector2(x, y)).checkMatch();
                if (matchQueue.Count >= MatchCount)
                {
                    StartCoroutine("destroyRoutine", matchQueue);
                    matchQueue.Clear();
                    return;
                }
            }
        }
        spawnNext();
    }

    public void stageReset()
    {
        for (int y = 0; y < Map.Instance.height; ++y)
            for (int x = 0; x < Map.Instance.width; ++x)
                if (Map.Instance.getStone(new Vector2(x, y)) != null)
                {
                    Map.Instance.getStone(new Vector2(x, y)).destroy();
                }
        SoundManager.get("main").Stop();
        Destroy(stoneSet);
        Destroy(nextStone1);
        Destroy(nextStone2);
        matchQueue.Clear();
        isSuccess = false;
        transform.position = new Vector3(transform.position.x, 5.5f, transform.position.z);
        alpaca.GetComponent<Alpaca>().reset();
        UI.SetActive(true);
        nextPanel.SetActive(true);
        UI.transform.Find("Remain").GetComponent<Text>().text = "남은 돌 : ";
        this.enabled = false;
    }

    protected IEnumerator checkMatchsRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        while (!isAllStop())
        {
            yield return null;
        }
        checkMatchs();
    }


    protected IEnumerator destroyRoutine(List<Stone> matchQueue)
    {
        List<Stone> destroyQueue = new List<Stone>(matchQueue);
        foreach (Stone st in destroyQueue)
        {
            st.changeAppear('g');
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Stone stone in destroyQueue)
        {
            stone.destroy();
        }
        doNext();
    }
}
