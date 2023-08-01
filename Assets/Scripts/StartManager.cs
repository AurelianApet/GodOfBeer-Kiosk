using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SocketIO;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class StartManager : MonoBehaviour
{
    public GameObject imgSlidePrefab;
    public GameObject imgslideParent;
    public GameObject slideObj;
    public GameObject popupObj;

    // Start is called before the first frame update
    void Start()
    {
        if (Global.setInfo.slide_option == 1 && Global.setInfo.paths != null)
        {
            slideObj.SetActive(true);
            //저장된 이미지
            if (Global.setInfo.paths == null || Global.setInfo.paths.Length == 0)
                return;
            try
            {
                for (int i = 0; i < Global.setInfo.paths.Length; i++)
                {
                    GameObject slideobj = Instantiate(imgSlidePrefab);
                    slideobj.transform.SetParent(imgslideParent.transform);
                    Texture2D tex = NativeGallery.LoadImageAtPath(Global.setInfo.paths[i], 512); // image will be downscaled if its width or height is larger than 1024px
                    if (tex != null)
                    {
                        slideobj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
                    }
                    slideobj.transform.localScale = Vector3.one;
                    slideobj.transform.localPosition = Vector3.zero;
                    if (Global.setInfo.paths.Length == 1)
                    {
                        slideobj.GetComponent<Image>().type = Image.Type.Simple;
                    }
                    else
                    {
                        slideobj.GetComponent<Image>().type = Image.Type.Sliced;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }
        else
        {
            //디폴트이미지
            slideObj.SetActive(false);
        }
    }

    public void onTouch()
    {
        Global.is_start_loading = true;
        SceneManager.LoadScene("select");
    }

    public void onSet()
    {
        popupObj.SetActive(true);
    }

    public void onConfirmAdminCode()
    {
        if(Global.setInfo.admin_password == popupObj.transform.Find("pw").GetComponent<InputField>().text)
        {
            SceneManager.LoadScene("setting");
        }
        else
        {
            Debug.Log("wrong password");
        }
    }

    public void onClosePopup()
    {
        popupObj.SetActive(false);
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
}
