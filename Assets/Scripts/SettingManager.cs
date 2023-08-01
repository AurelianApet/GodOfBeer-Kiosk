using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LitJson;
using SimpleJSON;
using System;
using SocketIO;
using System.IO;

public class SettingManager : MonoBehaviour
{
    public GameObject select_popup;
    public Text select_str;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    float time = 0f;
    // Start is called before the first frame update
    void Start()
    {
        socketObj = Instantiate(socketPrefab);
        socket = socketObj.GetComponent<SocketIOComponent>();
        socket.On("open", socketOpen);
        socket.On("error", socketError);
        socket.On("close", socketClose);
    }

    IEnumerator GotoScene(string sceneName)
    {
        if (socket != null)
        {
            socket.OnApplicationQuit();
        }
        if (socketObj != null)
        {
            DestroyImmediate(socketObj);
        }
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }

    public void socketOpen(SocketIOEvent e)
    {
    }

    public void socketError(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
    }

    public void socketClose(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        if (!Input.anyKey)
        {
            time += Time.deltaTime;
        }
        else
        {
            if (time != 0f)
            {
                GameObject.Find("touch").GetComponent<AudioSource>().Play();
                time = 0f;
            }
        }
        if (time >= 30f)
        {
            Global.mycartlist.Clear();
            Global.cart_amount = 0;
            Global.cart_price = 0;
            SceneManager.LoadScene("start");
        }
    }

    public void onExit()
    {
        select_popup.SetActive(true);
        select_str.text = "앱을 종료하시겠습니까?";
    }

    public void onBack()
    {
        Global.is_start_loading = true;
        StartCoroutine(GotoScene("select"));
    }

    public void closePopup()
    {
        select_popup.SetActive(false);
    }

    public void onExitApp()
    {
        Application.Quit();
    }

    public void onTapSet()
    {
        SceneManager.LoadScene("tapset");
    }

    public void onAdminSet()
    {
        SceneManager.LoadScene("adminset");
    }

    public void onSizeSet()
    {
        SceneManager.LoadScene("sizeset");
    }

    public void onDeviceSet()
    {
        SceneManager.LoadScene("deviceset");
    }

    public void onOption()
    {
        SceneManager.LoadScene("option");
    }

    public void onDbSet()
    {
        SceneManager.LoadScene("dbset");
    }
}
