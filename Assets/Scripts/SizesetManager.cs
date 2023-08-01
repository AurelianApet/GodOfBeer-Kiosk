using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;
using System.IO;

public class SizesetManager : MonoBehaviour
{
    public GameObject cateItem;
    public GameObject cateParent;
    public GameObject menuItem;
    public GameObject menuParent;
    public GameObject pageItem;
    public GameObject pageParent;
    public InputField tapTxt;
    public InputField coffesizeTxt;
    public InputField watersizeTxt;
    public Text nameTxt;
    public GameObject err_popup;
    public Text err_str;

    // Start is called before the first frame update
    void Start()
    {
        if (Global.is_start_loading)
        {
            SendRequeset();
        }
        else
        {
            StartCoroutine(LoadCategoryList());
        }
    }

    void SendRequeset()
    {
        WWWForm form = new WWWForm();
        WWW www = new WWW(Global.api_url + Global.get_categorylist_api, form);
        StartCoroutine(GetCategorylistFromApi(www));
    }

    IEnumerator GetCategorylistFromApi(WWW www)
    {
        yield return www;
        Global.categorylist.Clear();
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            JSONNode c_list = JSON.Parse(jsonNode["categorylist"].ToString()/*.Replace("\"", "")*/);
            for (int i = 0; i < c_list.Count; i++)
            {
                CategoryInfo cateInfo = new CategoryInfo();
                cateInfo.name = c_list[i]["name"];
                cateInfo.engname = c_list[i]["engname"];
                cateInfo.id = c_list[i]["id"];
                cateInfo.menulist = new List<MenuInfo>();
                JSONNode m_list = JSON.Parse(c_list[i]["menulist"].ToString()/*.Replace("\"", "")*/);
                for (int j = 0; j < m_list.Count; j++)
                {
                    MenuInfo minfo = new MenuInfo();
                    minfo.name = m_list[j]["name"];
                    minfo.engname = m_list[j]["engname"];
                    minfo.image = m_list[j]["img_path"];
                    minfo.desc = m_list[j]["content"];
                    if (m_list[j]["content"] == null)
                    {
                        minfo.desc = "";
                    }
                    minfo.menu_id = m_list[j]["id"];
                    minfo.price = m_list[j]["price"].AsInt;
                    if (m_list[j]["is_soldout"].AsInt == 1)
                    {
                        minfo.is_soldout = true;
                    }
                    else
                    {
                        minfo.is_soldout = false;
                    }
                    minfo.tap_id = m_list[j]["tap_id"];
                    minfo.coffee = m_list[j]["coffee"];
                    minfo.water = m_list[j]["water"];
                    cateInfo.menulist.Add(minfo);
                }
                Global.categorylist.Add(cateInfo);
            }
            Global.is_start_loading = false;
        }
        StartCoroutine(LoadCategoryList());
    }

    IEnumerator LoadCategoryList()
    {
        if (Global.categorylist.Count == 0)
        {
            yield break;
        }
        if (Global.cur_setCategoryId == "")
        {
            Global.cur_setCategoryId = Global.categorylist[0].id;
        }
        if (Global.cur_setSlectedId == "")
        {
            Global.cur_setSlectedId = Global.categorylist[0].id;
        }
        //UI에 추가
        int startCateIndex = 0;
        int selected_index = 0;
        for (int i = 0; i < Global.categorylist.Count; i++)
        {
            if (Global.cur_setCategoryId == Global.categorylist[i].id)
            {
                startCateIndex = i;
                break;
            }
        }
        for (int i = 0; i < Global.categorylist.Count; i++)
        {
            if (Global.cur_setSlectedId == Global.categorylist[i].id)
            {
                selected_index = i;
                break;
            }
        }
        if (Global.categorylist.Count - startCateIndex < 4)
        {
            startCateIndex = Global.categorylist.Count - 4;
            if (startCateIndex < 0)
            {
                startCateIndex = 0;
            }
        }
        for (int i = startCateIndex; i < Global.categorylist.Count; i++)
        {
            if (i >= startCateIndex + 4)
            {
                break;
            }
            try
            {
                GameObject tmp = Instantiate(cateItem);
                tmp.transform.SetParent(cateParent.transform);
                if (Global.LanguageType == 1)
                {
                    tmp.transform.Find("name").GetComponent<Text>().text = Global.categorylist[i].name;
                }
                else
                {
                    tmp.transform.Find("name").GetComponent<Text>().text = Global.categorylist[i].engname;
                }
                tmp.transform.Find("id").GetComponent<Text>().text = Global.categorylist[i].id.ToString();
                if (i != selected_index)
                {
                    tmp.transform.Find("Image").gameObject.SetActive(false);
                }
                string t_cateNo = Global.categorylist[i].id;
                tmp.GetComponent<Button>().onClick.AddListener(delegate () { LoadMenuList(t_cateNo); });
                if (i == selected_index)
                {
                    List<MenuInfo> minfoList = Global.categorylist[i].menulist;
                    LoadMenuInfo(minfoList);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

    void LoadMenuInfo(List<MenuInfo> minfoList)
    {
        if (minfoList == null || minfoList.Count == 0)
            return;
        Debug.Log(minfoList.Count);
        for (int j = 0; j < Math.Ceiling(minfoList.Count / 8f); j++)
        {
            Debug.Log(j);
            GameObject tmp = Instantiate(pageItem);
            tmp.transform.SetParent(pageParent.transform);
            try
            {
                if (j == 0)
                {
                    tmp.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("sel");
                }
                else
                {
                    tmp.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("unsel");
                }
                tmp.transform.Find("index").GetComponent<Text>().text = (j + 1).ToString();
                //tmp.GetComponent<Button>().onClick.RemoveAllListeners();
                int _i = j + 1;
                tmp.GetComponent<Button>().onClick.AddListener(delegate () { onChangePage(_i); });
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        onPage(Global.cur_setpageIndex, minfoList);
    }

    void onChangePage(int index)
    {
        Global.cur_setpageIndex = index;
        SceneManager.LoadScene("sizeset");
    }

    void onPage(int index, List<MenuInfo> minfoList)
    {
        try
        {
            for (int i = 0; i < pageParent.transform.childCount; i++)
            {
                pageParent.transform.GetChild(i).Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("unsel");
            }
            pageParent.transform.GetChild(index - 1).Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("sel");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
        for (int i = 0; i < 8; i++)
        {
            if ((index - 1) * 8 + i >= minfoList.Count)
                break;
            GameObject tmp = Instantiate(menuItem);
            tmp.transform.SetParent(menuParent.transform);
            try
            {
                tmp.transform.Find("id").GetComponent<Text>().text = minfoList[(index - 1) * 8 + i].menu_id.ToString();
                if (minfoList[(index - 1) * 8 + i].image != "")
                {
                    StartCoroutine(downloadImage(minfoList[(index - 1) * 8 + i].image, Global.imgPath + Path.GetFileName(minfoList[(index - 1) * 8 + i].image), tmp.transform.Find("Image").gameObject));
                }
                if (Global.LanguageType == 1)
                {
                    tmp.transform.Find("name").GetComponent<Text>().text = minfoList[(index - 1) * 8 + i].name;
                    tmp.transform.Find("price").GetComponent<Text>().text = Global.GetPriceFormat(minfoList[(index - 1) * 8 + i].price) + " 원";
                }
                else
                {
                    tmp.transform.Find("name").GetComponent<Text>().text = minfoList[(index - 1) * 8 + i].engname;
                    tmp.transform.Find("price").GetComponent<Text>().text = Global.GetPriceFormat(minfoList[(index - 1) * 8 + i].price);
                }

                CartInfo cinfo = new CartInfo();
                cinfo.name = minfoList[(index - 1) * 8 + i].name;
                cinfo.engname = minfoList[(index - 1) * 8 + i].engname;
                cinfo.menu_id = minfoList[(index - 1) * 8 + i].menu_id;
                cinfo.image = minfoList[(index - 1) * 8 + i].image;
                cinfo.desc = minfoList[(index - 1) * 8 + i].desc;
                cinfo.price = minfoList[(index - 1) * 8 + i].price;
                cinfo.amount = 1;
                cinfo.tap_id = minfoList[(index - 1) * 8 + i].tap_id;
                cinfo.coffee = minfoList[(index - 1) * 8 + i].coffee;
                cinfo.water = minfoList[(index - 1) * 8 + i].water;

                if (minfoList[(index - 1) * 8 + i].is_soldout)
                {
                    tmp.transform.Find("mask").gameObject.SetActive(true);
                }
                else
                {
                    tmp.GetComponent<Button>().onClick.AddListener(delegate () { onSelMenu(cinfo); });
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    void LoadMenuList(string cateId)
    {
        Global.cur_setSlectedId = cateId;
        Global.cur_setpageIndex = 1;
        SceneManager.LoadScene("sizeset");
    }

    public void onBack()
    {
        SceneManager.LoadScene("setting");
    }

    string cur_menuId = "";
    void onSelMenu(CartInfo cinfo)
    {
        nameTxt.text = cinfo.name;
        cur_menuId = cinfo.menu_id;
        tapTxt.text = cinfo.tap_id.ToString();
        coffesizeTxt.text = cinfo.coffee.ToString();
        watersizeTxt.text = cinfo.water.ToString();
    }

    public void onSave()
    {
        if(tapTxt.text == "" || coffesizeTxt.text == "" || watersizeTxt.text == "")
        {
            err_popup.SetActive(true);
            err_str.text = "TAP 정보를 정확히 입력하세요.";
            return;
        }
        WWWForm form = new WWWForm();
        form.AddField("menu_id", cur_menuId);
        form.AddField("tap", tapTxt.text);
        form.AddField("coffee", coffesizeTxt.text);
        form.AddField("water", watersizeTxt.text);
        WWW www = new WWW(Global.api_url + Global.save_tapinfo_api, form);
        StartCoroutine(ChangeTapInfo(www));
    }

    IEnumerator ChangeTapInfo(WWW www)
    {
        yield return www;
        if(www.error == null)
        {
            //err_popup.SetActive(true);
            //err_str.text = "저장되었습니다.";
            Global.is_start_loading = true;
            SceneManager.LoadScene("sizeset");
        }
    }

    public void closeErrPopup()
    {
        err_popup.SetActive(false);
    }

    public void onPrevious()
    {
        try
        {
            int start_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_setCategoryId)
                {
                    start_index = i; break;
                }
            }
            int select_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_setSlectedId)
                {
                    select_index = i; break;
                }
            }
            if (select_index != 0)
            {
                Global.cur_setSlectedId = Global.categorylist[select_index - 1].id;
                if (select_index - 1 < start_index)
                {
                    Global.cur_setCategoryId = Global.categorylist[start_index - 1].id;
                }
                Global.cur_setpageIndex = 1;
                SceneManager.LoadScene("sizeset");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void onNext()
    {
        try
        {
            int start_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_setCategoryId)
                {
                    start_index = i; break;
                }
            }
            int select_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_setSlectedId)
                {
                    select_index = i; break;
                }
            }
            if (select_index != Global.categorylist.Count - 1)
            {
                Global.cur_setSlectedId = Global.categorylist[select_index + 1].id;
                if (select_index + 1 >= start_index + 4)
                {
                    Global.cur_setCategoryId = Global.categorylist[start_index + 1].id;
                }
                Global.cur_setpageIndex = 1;
                SceneManager.LoadScene("sizeset");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void onPrevMenuList()
    {
        if (Global.cur_setpageIndex < 2)
        {
            return;
        }
        Global.cur_setpageIndex--;
        SceneManager.LoadScene("sizeset");
    }

    public void onNextMenuList()
    {
        if (Global.cur_setpageIndex == pageParent.transform.childCount)
        {
            return;
        }
        Global.cur_setpageIndex++;
        SceneManager.LoadScene("sizeset");
    }

    IEnumerator downloadImage(string url, string pathToSaveImage, GameObject imgObj)
    {
        yield return new WaitForSeconds(0.001f);
        if (imgObj != null)
        {
            Image img = imgObj.GetComponent<Image>();
            if (File.Exists(pathToSaveImage))
            {
                StartCoroutine(LoadPictureToTexture(pathToSaveImage, img));
            }
            else
            {
                WWW www = new WWW(url);
                StartCoroutine(_downloadImage(www, pathToSaveImage, img));
            }
        }
    }

    private IEnumerator _downloadImage(WWW www, string savePath, Image img)
    {
        yield return www;
        if (img != null)
        {
            //Check if we failed to send
            if (string.IsNullOrEmpty(www.error))
            {
                saveImage(savePath, www.bytes, img);
            }
            else
            {
                UnityEngine.Debug.Log("Error: " + www.error);
            }
        }
    }

    void saveImage(string path, byte[] imageBytes, Image img)
    {
        try
        {
            //Create Directory if it does not exist
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllBytes(path, imageBytes);
            //Debug.Log("Download Image: " + path.Replace("/", "\\"));
            StartCoroutine(LoadPictureToTexture(path, img));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    IEnumerator LoadPictureToTexture(string name, Image img)
    {
        WWW pictureWWW = new WWW(Global.prePath + name);
        yield return pictureWWW;

        if (img != null)
        {
            img.sprite = Sprite.Create(pictureWWW.texture, new Rect(0, 0, pictureWWW.texture.width, pictureWWW.texture.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
