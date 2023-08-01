using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;

public class OptionManager : MonoBehaviour
{
    public Toggle autoLogin;
    public Toggle noauto;
    public InputField adminPw;
    public Toggle slideImageOption;//true-default, false-selection
    public Toggle selectImageOption;
    public GameObject slideParent;
    public GameObject slideImage;
    public GameObject err_popup;
    public Text err_str;

    string[] slideImgs;
    GameObject[] imgSlideItem;

    // Start is called before the first frame update
    void Start()
    {
        if(Global.setInfo.auto_login == 1)
        {
            autoLogin.isOn = true;
        }
        else
        {
            noauto.isOn = true;
        }
        adminPw.text = Global.setInfo.admin_password;
        if(Global.setInfo.slide_option == 1)
        {
            selectImageOption.isOn = true;
            if (Global.setInfo.paths == null || Global.setInfo.paths.Length == 0)
                return;
            for (int i = 0; i < Global.setInfo.paths.Length; i++)
            {
                try
                {
                    GameObject slideobj = Instantiate(slideImage);
                    slideobj.transform.SetParent(slideParent.transform);
                    Texture2D tex = NativeGallery.LoadImageAtPath(Global.setInfo.paths[i], 512); // image will be downscaled if its width or height is larger than 1024px
                    if (tex != null)
                    {
                        slideobj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
                    }
                }
                catch ( Exception ex)
                {
                    Debug.Log(ex);
                }
            }
        }
        else
        {
            slideImageOption.isOn = true;
        }
    }

    public void onBack()
    {
        SceneManager.LoadScene("setting");
    }

    public void onSave()
    {
        if(adminPw.text == "")
        {
            err_popup.SetActive(true);
            err_str.text = "관리자 비밀번호를 입력하세요.";
            return;
        }
        if (autoLogin.isOn)
        {
            Global.setInfo.auto_login = 1;
        }
        else
        {
            Global.setInfo.auto_login = 0;
        }
        PlayerPrefs.SetInt("auto_login", Global.setInfo.auto_login);
        if(slideImageOption.isOn)
        {
            Global.setInfo.slide_option = 0;
        }
        else
        {
            Global.setInfo.slide_option = 1;
        }
        PlayerPrefs.SetInt("slide_option", Global.setInfo.slide_option);
        Global.setInfo.admin_password = adminPw.text;
        PlayerPrefs.SetString("admin_password", Global.setInfo.admin_password);
        string sImgs = "";
        if (slideImgs != null)
        {
            int lg = slideImgs.Length;
            if(lg > 5)
            {
                lg = 5;
            }
            for (int i = 0; i < slideImgs.Length; i++)
            {
                if (i != slideImgs.Length - 1)
                {
                    sImgs += slideImgs[i] + ",";
                }
                else
                {
                    sImgs += slideImgs[i];
                }
            }
        }
        PlayerPrefs.SetString("paths", sImgs);
        err_popup.SetActive(true);
        err_str.text = "성공적으로 저장되였습니다.";
    }

    IEnumerator ClearSlides()
    {
        while (slideParent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(slideParent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    public void onSelectImage()
    {
        StartCoroutine(ClearSlides());
        if (NativeGallery.CanSelectMultipleFilesFromGallery())
        {
            NativeGallery.GetImagesFromGallery((paths) =>
            {
                if (paths != null)
                {
                    slideImgs = paths;
                    imgSlideItem = new GameObject[paths.Length];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        if (i >= 5)
                            break;
                        imgSlideItem[i] = Instantiate(slideImage);
                        imgSlideItem[i].transform.SetParent(slideParent.transform);
                        Texture2D tex = NativeGallery.LoadImageAtPath(paths[i], 512); // image will be downscaled if its width or height is larger than 1024px
                        if (tex != null)
                        {
                            imgSlideItem[i].GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
                        }
                    }
                }
            }, title: "슬라이드이미지 선택", mime: "image/*");
        }
    }

    public void onCloseErrPopup()
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
        if (time >= 30f)
        {
            Global.mycartlist.Clear();
            Global.cart_amount = 0;
            Global.cart_price = 0;
            SceneManager.LoadScene("start");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
