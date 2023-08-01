using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DbSetManager : MonoBehaviour
{
    public InputField ip;
    public InputField id;
    public InputField pw;
    public GameObject err_popup;
    public Text err_str;

    bool ip_set = false;

    // Start is called before the first frame update
    void Start()
    {
        ip.text = Global.server_address;
        if(Global.server_address != null && Global.server_address != "")
        {
            ip_set = true;
        }
    }

    public void Save()
    {
        if(ip.text == "")
        {
            err_popup.SetActive(true);
            err_str.text = "ip를 정확히 입력하세요.";
            return;
        }
        Global.server_address = ip.text;
        PlayerPrefs.SetString("ip", Global.server_address);
        Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
        Global.socket_server = "ws://" + Global.server_address + ":" + Global.api_server_port;
        WWWForm form = new WWWForm();
        WWW www = new WWW(Global.api_url + Global.checkDB_api, form);
        StartCoroutine(ProcessConnect(www));
    }

    IEnumerator ProcessConnect(WWW www)
    {
        yield return www;
        if(www.error == null)
        {
            ip_set = true;
            err_popup.SetActive(true);
            err_str.text = "성공적으로 저장되었습니다.";
        }
        else
        {
            Global.server_address = "";
            Global.api_url = "";
            Global.socket_server = "";
            err_popup.SetActive(true);
            err_str.text = "ip를 확인하세요.";
        }
    }

    public void onBack()
    {
        if(ip.text == "")
        {
            err_popup.SetActive(true);
            err_str.text = "ip를 정확히 입력하세요.";
            return;
        }
        if(!ip_set)
        {
            err_popup.SetActive(true);
            err_str.text = "ip를 입력하고 저장하세요.";
            return;
        }
        if (Global.setInfo.userName == null || Global.setInfo.userName == "")
        {
            SceneManager.LoadScene("login");
        }
        else
        {
            SceneManager.LoadScene("setting");
        }
    }

    public void closePopup()
    {
        err_popup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
