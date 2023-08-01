using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class splash : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Screen.orientation = ScreenOrientation.Landscape;
#if UNITY_IPHONE
		Global.imgPath = Application.persistentDataPath + "/kiosk_img/";
#elif UNITY_ANDROID
        Global.imgPath = Application.persistentDataPath + "/kiosk_img/";
#else
if( Application.isEditor == true ){ 
    	Global.imgPath = "/img/";
} 
#endif

#if UNITY_IPHONE
		Global.prePath = @"file://";
#elif UNITY_ANDROID
        Global.prePath = @"file:///";
#else
		Global.prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
#endif
        //delete all downloaded images
        try
        {
            if (Directory.Exists(Global.imgPath))
            {
                Directory.Delete(Global.imgPath, true);
            }
        }
        catch (Exception)
        {

        }
        Global.setInfo.admin_password = PlayerPrefs.GetString("admin_password");
        Global.setInfo.save_id = PlayerPrefs.GetInt("saveId");
        Global.setInfo.auto_login = PlayerPrefs.GetInt("auto_login");
        Global.setInfo.slide_option = PlayerPrefs.GetInt("slide_option");
        Global.app_type = PlayerPrefs.GetInt("app_type");
        if(PlayerPrefs.GetString("ip") == "")
        {
            SceneManager.LoadScene("dbset");
        }
        else
        {
            Global.server_address = PlayerPrefs.GetString("ip");
            Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
            Global.socket_server = "ws://" + Global.server_address + ":" + Global.api_server_port;
            WWWForm form = new WWWForm();
            WWW www = new WWW(Global.api_url + Global.checkDB_api, form);
            StartCoroutine(ProcessConnect(www));
        }
    }

    IEnumerator ProcessConnect(WWW www)
    {
        yield return www;
        if(www.error == null)
        {
            if (Global.setInfo.save_id == 1)
            {
                Global.setInfo.userName = PlayerPrefs.GetString("userId");
            }
            if (Global.setInfo.slide_option == 1)
            {
                string paths = PlayerPrefs.GetString("paths");
                Global.setInfo.paths = paths.Split(',');
            }
            if (Global.setInfo.auto_login == 1)
            {
                Global.setInfo.userName = PlayerPrefs.GetString("userId");
                Global.setInfo.userPw = PlayerPrefs.GetString("userPw");
                WWWForm form = new WWWForm();
                form.AddField("userID", Global.setInfo.userName);
                form.AddField("password", Global.setInfo.userPw);
                form.AddField("type", Global.app_type);
                WWW www1 = new WWW(Global.api_url + Global.login_api, form);
                StartCoroutine(ProcessLogin(www1));
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                SceneManager.LoadScene("login");
            }
        }
        else
        {
            SceneManager.LoadScene("dbset");
        }
    }

    IEnumerator ProcessLogin(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            if (jsonNode["suc"].AsInt == 1)
            {
                if(Global.setInfo.admin_password == "")
                {
                    SceneManager.LoadScene("option");
                }
                else
                {
                    Debug.Log(Global.setInfo.admin_password);
                    SceneManager.LoadScene("select");
                }
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                SceneManager.LoadScene("login");
            }
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene("login");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
