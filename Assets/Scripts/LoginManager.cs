using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class LoginManager : MonoBehaviour
{
    public Toggle beerType;
    public Toggle wineType;
    public Toggle saveId;
    public Toggle autoLogin;
    public InputField userId;
    public InputField password;
    public GameObject err_popup;
    public Text err_str;

    // Start is called before the first frame update
    void Start()
    {
        if(Global.app_type == 0)
        {
            beerType.isOn = true;
        }
        else
        {
            wineType.isOn = true;
        }
        if(Global.setInfo.save_id == 1)
        {
            saveId.isOn = true;
            userId.text = Global.setInfo.userName;
        }
        else
        {
            saveId.isOn = false;
        }

        if(Global.setInfo.auto_login == 1)
        {
            autoLogin.isOn = true;
        }
        else
        {
            autoLogin.isOn = false;
        }

    }

    public void Login()
    {
        if (userId.text == "")
        {
            err_str.text = "Username을 입력하세요.";
            err_popup.SetActive(true);
        }
        else if (password.text == "")
        {
            err_str.text = "비밀번호를 입력하세요.";
            err_popup.SetActive(true);
        }
        else
        {
            WWWForm form = new WWWForm();
            int type = 0;
            if (wineType.isOn)
            {
                type = 1;
            }
            form.AddField("type", type);
            form.AddField("userID", userId.text);
            form.AddField("password", password.text);
            Debug.Log(Global.api_url + Global.login_api);
            WWW www = new WWW(Global.api_url + Global.login_api, form);
            StartCoroutine(ProcessLogin(www, saveId.isOn, userId.text, autoLogin.isOn, password.text, type));
        }
    }

    IEnumerator ProcessLogin(WWW www, bool is_idsave, string username, bool is_autosave, string password, int type)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            if (jsonNode["suc"].AsInt == 1)
            {
                Global.app_type = type;
                PlayerPrefs.SetInt("app_type", Global.app_type);
                if (is_idsave)
                {
                    PlayerPrefs.SetInt("saveId", 1);
                    PlayerPrefs.SetString("userId", username);
                    Global.setInfo.save_id = 1;
                }
                else
                {
                    PlayerPrefs.SetInt("saveId", 0);
                    Global.setInfo.save_id = 0;
                }
                if (is_autosave)
                {
                    PlayerPrefs.SetInt("auto_login", 1);
                    PlayerPrefs.SetString("userId", username);
                    PlayerPrefs.SetString("userPw", password);
                    Global.setInfo.auto_login = 1;
                }
                else
                {
                    PlayerPrefs.SetInt("auto_login", 0);
                    Global.setInfo.auto_login = 0;
                }
                Global.setInfo.userName = username;
                Global.setInfo.userPw = password;
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
                err_str.text = jsonNode["msg"];
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_str.text = "인터넷 연결을 확인하세요.";
            err_popup.SetActive(true);
        }
    }

    public void onConfirmErrPopup()
    {
        err_popup.SetActive(false);
    }

    float time = 0f;

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
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void onDBSet()
    {
        SceneManager.LoadScene("dbset");
    }
}
