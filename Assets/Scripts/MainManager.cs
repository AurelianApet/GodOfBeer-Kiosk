using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public GameObject cateItem;
    public GameObject cateParent;
    public GameObject menuItem;
    public GameObject menuParent;
    public GameObject cartItem;
    public GameObject cartParent;
    public GameObject pageItem;
    public GameObject pageParent;
    public Text cartPriceTxt;
    public Text cartAmountTxt;
    public Image languageBtn;
    public Text priceNotice;
    public Text amountNotice;
    public Image cartBtnImage;

    List<GameObject> mCartObj = new List<GameObject>();
    float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if(Global.LanguageType == 1)
        {
            priceNotice.text = "합계";
            amountNotice.text = "수량";
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            cartAmountTxt.text = Global.cart_amount.ToString() + " 개";
            cartBtnImage.sprite = Resources.Load<Sprite>("ordercart");
            languageBtn.sprite = Resources.Load<Sprite>("english");
        }
        else
        {
            priceNotice.text = "Total";
            amountNotice.text = "Quantity";
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price);
            cartAmountTxt.text = Global.cart_amount.ToString();
            cartBtnImage.sprite = Resources.Load<Sprite>("ordercart1");
            languageBtn.sprite = Resources.Load<Sprite>("korean");
        }
        if (Global.is_start_loading)
        {
            SendRequeset();
        }
        else
        {
            StartCoroutine(LoadCategoryList());
        }
        LoadCartList();
    }

    void LoadCartList()
    {
        //이미 잇는 주문카트내역을 조회해서 로드.
        Global.cart_price = 0;
        Global.cart_amount = 0;
        for (int i = 0; i < Global.mycartlist.Count; i++)
        {
            for (int j = 0; j < Global.mycartlist[i].amount; j++)
            {
                GameObject sItem = Instantiate(cartItem);
                sItem.transform.SetParent(cartParent.transform);
                try
                {
                    if (Global.mycartlist[i].image != "")
                    {
                        GameObject sobj = sItem.transform.Find("Image").gameObject;
                        StartCoroutine(downloadImage(Global.mycartlist[i].image, Global.imgPath + Path.GetFileName(Global.mycartlist[i].image), sobj));
                    }
                    sItem.transform.Find("id").GetComponent<Text>().text = Global.mycartlist[i].menu_id.ToString();
                    string mId = Global.mycartlist[i].menu_id;
                    sItem.transform.Find("minus").GetComponent<Button>().onClick.AddListener(delegate () { onRemoveItem(mId); });
                    mCartObj.Add(sItem);
                    Global.cart_price += Global.mycartlist[i].price;
                    Global.cart_amount++;
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
        }
    }

    void SendRequeset()
    {
        WWWForm form = new WWWForm();
        form.AddField("is_get_all", 1);
        form.AddField("order_type", Global.order_type);
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
        if (Global.cur_categoryId == "")
        {
            Global.cur_categoryId = Global.categorylist[0].id;
        }
        if (Global.cur_selectedId == "")
        {
            Global.cur_selectedId = Global.categorylist[0].id;
        }
        //UI에 추가
        int startCateIndex = 0;
        int selected_index = 0;
        for (int i = 0; i < Global.categorylist.Count; i++)
        {
            if (Global.cur_categoryId == Global.categorylist[i].id)
            {
                startCateIndex = i;
                break;
            }
        }
        for (int i = 0; i < Global.categorylist.Count; i++)
        { 
            if (Global.cur_selectedId == Global.categorylist[i].id)
            {
                selected_index = i;
                break;
            }
        }
        Debug.Log(Global.cur_selectedId + "," + Global.cur_categoryId);
        if (Global.categorylist.Count - startCateIndex < 4)
        {
            startCateIndex = Global.categorylist.Count - 4;
            if (startCateIndex < 0)
            {
                startCateIndex = 0;
            }
        }
        yield return new WaitForSeconds(0.1f);
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
                if(Global.LanguageType == 1)
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
                if(i == selected_index)
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
        for (int j = 0; j < Math.Ceiling(minfoList.Count / 8f); j++)
        {
            GameObject tmp = Instantiate(pageItem);
            tmp.transform.SetParent(pageParent.transform);
            try
            {
                if(j == 0)
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
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }
        onPage(Global.cur_pageIndex, minfoList);
    }

    void onChangePage(int index)
    {
        Global.cur_pageIndex = index;
        SceneManager.LoadScene("main");
    }

    void onPage(int index, List<MenuInfo> minfoList)
    {
        try
        {
            for(int i = 0; i < pageParent.transform.childCount; i ++)
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
                string img = "http://localhost:8011/img/" + UnityEngine.Random.Range(0, 30) + ".png";
                Debug.Log(img);
                if (minfoList[(index - 1) * 8 + i].image != "")
                {
                    StartCoroutine(downloadImage(minfoList[(index - 1) * 8 + i].image, Global.imgPath + Path.GetFileName(minfoList[(index - 1) * 8 + i].image), tmp.transform.Find("Image").gameObject));
                    //StartCoroutine(downloadImage(img, Global.imgPath + Path.GetFileName(img), tmp.transform.Find("Image").gameObject));
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

                if (minfoList[(index - 1) * 8 + i].is_soldout)
                {
                    tmp.transform.Find("mask").gameObject.SetActive(true);
                }
                else
                {
                    tmp.transform.Find("plus").GetComponent<Button>().onClick.AddListener(delegate () { addList(cinfo); });
                    tmp.GetComponent<Button>().onClick.AddListener(delegate () { onMenuDetail(cinfo); });
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    void LoadMenuList(string cateId)
    {
        //Global.cur_categoryId = cateId;
        Global.cur_selectedId = cateId;
        Global.cur_pageIndex = 1;
        SceneManager.LoadScene("main");
    }

    public void addList(CartInfo cinfo)
    {
        //UI에 추가
        GameObject sItem = Instantiate(cartItem);
        sItem.transform.SetParent(cartParent.transform);
        try
        {
            if (cinfo.image != "")
            {
                StartCoroutine(downloadImage(cinfo.image, Global.imgPath + Path.GetFileName(cinfo.image), sItem.transform.Find("Image").gameObject));
            }
            sItem.transform.Find("id").GetComponent<Text>().text = cinfo.menu_id.ToString();
            sItem.transform.Find("minus").GetComponent<Button>().onClick.AddListener(delegate () { onRemoveItem(cinfo.menu_id); });
        }
        catch (Exception ex)
        {

        }
        mCartObj.Add(sItem);
        //my cart list에 추가
        Global.addOneCartItem(cinfo);
        if(Global.LanguageType == 1)
        {
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            cartAmountTxt.text = Global.cart_amount.ToString() + " 개";
        }
        else
        {
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price);
            cartAmountTxt.text = Global.cart_amount.ToString();
        }
    }

    public void onMenuDetail(CartInfo cinfo)
    {
        Global.cur_selected_menu.image = cinfo.image;
        Global.cur_selected_menu.desc = cinfo.desc;
        Global.cur_selected_menu.menu_id = cinfo.menu_id;
        Global.cur_selected_menu.name = cinfo.name;
        Global.cur_selected_menu.engname = cinfo.engname;
        Global.cur_selected_menu.price = cinfo.price;
        SceneManager.LoadScene("menu_detail");
    }

    public void onRemoveItem(string mId)
    {
        //UI상에서 제거
        for (int i = 0; i < mCartObj.Count; i++)
        {
            try
            {
                if (mCartObj[i].transform.Find("id").GetComponent<Text>().text == mId.ToString())
                {
                    DestroyImmediate(mCartObj[i].gameObject);
                    mCartObj.Remove(mCartObj[i].gameObject);
                    break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //Global에서 제거
        Global.removeOneCartItem(mId);
        if(Global.LanguageType == 1)
        {
            cartAmountTxt.text = Global.cart_amount.ToString() + " 개";
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
        }
        else
        {
            cartAmountTxt.text = Global.cart_amount.ToString();
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price);
        }
    }

    IEnumerator downloadImage(string url, string pathToSaveImage, GameObject imgObj)
    {
        yield return new WaitForSeconds(0.001f);
        if (imgObj != null)
        {
            RawImage img = imgObj.GetComponent<RawImage>();
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

    private IEnumerator _downloadImage(WWW www, string savePath, RawImage img)
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

    void saveImage(string path, byte[] imageBytes, RawImage img)
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

    IEnumerator LoadPictureToTexture(string name, RawImage img)
    {
        WWW pictureWWW = new WWW(Global.prePath + name);
        yield return pictureWWW;

        if (img != null)
        {
            img.texture = pictureWWW.texture;
        }
    }

    public void onPrevious()
    {
        try
        {
            int start_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_categoryId)
                {
                    start_index = i; break;
                }
            }
            int select_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_selectedId)
                {
                    select_index = i; break;
                }
            }
            if (select_index != 0)
            {
                Global.cur_selectedId = Global.categorylist[select_index - 1].id;
                if (select_index - 1 < start_index)
                {
                    Global.cur_categoryId = Global.categorylist[start_index - 1].id;
                }
                Global.cur_pageIndex = 1;
                SceneManager.LoadScene("main");
            }
        }
        catch(Exception ex)
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
                if (Global.categorylist[i].id == Global.cur_categoryId)
                {
                    start_index = i; break;
                }
            }
            int select_index = -1;
            for (int i = 0; i < Global.categorylist.Count; i++)
            {
                if (Global.categorylist[i].id == Global.cur_selectedId)
                {
                    select_index = i; break;
                }
            }
            if(select_index != Global.categorylist.Count - 1)
            {
                Global.cur_selectedId = Global.categorylist[select_index + 1].id;
                if(select_index + 1 >= start_index + 4)
                {
                    Global.cur_categoryId = Global.categorylist[start_index + 1].id;
                }
                Global.cur_pageIndex = 1;
                SceneManager.LoadScene("main");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void onPrevMenuList()
    {
        if(Global.cur_pageIndex < 2)
        {
            return;
        }
        Global.cur_pageIndex--;
        SceneManager.LoadScene("main");
    }

    public void onNextMenuList()
    {
        if(Global.cur_pageIndex == pageParent.transform.childCount)
        {
            return;
        }
        Global.cur_pageIndex++;
        SceneManager.LoadScene("main");
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
            if(time != 0f)
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

    public void onOrderCart()
    {
        //주문카트
        SceneManager.LoadScene("ordercart");
    }

    void ChangeUI()
    {
        for(int i = 0; i < cateParent.transform.childCount; i++)
        {
            try
            {
                for (int j = 0; j < Global.categorylist.Count; j++)
                {
                    if (cateParent.transform.GetChild(i).transform.Find("id").GetComponent<Text>().text == Global.categorylist[j].id.ToString())
                    {
                        if (Global.LanguageType == 1)
                        {
                            cateParent.transform.GetChild(i).transform.Find("name").GetComponent<Text>().text = Global.categorylist[j].name;
                        }
                        else
                        {
                            cateParent.transform.GetChild(i).transform.Find("name").GetComponent<Text>().text = Global.categorylist[j].engname;
                        }
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }
        int cur_index = 0;
        for(int i = 0; i < Global.categorylist.Count; i ++)
        {
            if(Global.categorylist[i].id == Global.cur_categoryId)
            {
                cur_index = i;
                break;
            }
        }
        for(int i = 0; i < menuParent.transform.childCount; i++)
        {
            try
            {
                for (int j = 0; j < Global.categorylist[cur_index].menulist.Count; j++)
                {
                    if(menuParent.transform.GetChild(i).transform.Find("id").GetComponent<Text>().text == Global.categorylist[cur_index].menulist[j].menu_id.ToString())
                    {
                        if (Global.LanguageType == 1)
                        {
                            menuParent.transform.GetChild(i).transform.Find("name").GetComponent<Text>().text = Global.categorylist[cur_index].menulist[j].name;
                        }
                        else
                        {
                            menuParent.transform.GetChild(i).transform.Find("name").GetComponent<Text>().text = Global.categorylist[cur_index].menulist[j].engname;
                        }
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

    public void onChangeLanguage()
    {
        if (Global.LanguageType == 0)
        {
            //english
            priceNotice.text = "합계";
            amountNotice.text = "수량";
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            cartAmountTxt.text = Global.cart_amount.ToString() + " 개";
            cartBtnImage.sprite = Resources.Load<Sprite>("ordercart");
            languageBtn.sprite = Resources.Load<Sprite>("english");
            Global.LanguageType = 1;
            ChangeUI();
        }
        else
        {
            //korean
            priceNotice.text = "Total";
            amountNotice.text = "Quantity";
            cartPriceTxt.text = Global.GetPriceFormat(Global.cart_price);
            cartAmountTxt.text = Global.cart_amount.ToString();
            cartBtnImage.sprite = Resources.Load<Sprite>("ordercart1");
            languageBtn.sprite = Resources.Load<Sprite>("korean");
            Global.LanguageType = 0;
            ChangeUI();
        }
    }

    public void onHome()
    {
        Global.mycartlist.Clear();
        Global.cart_amount = 0;
        Global.cart_price = 0;
        SceneManager.LoadScene("start");
    }

    IEnumerator Destroy_Object(GameObject obj)
    {
        DestroyImmediate(obj);
        yield return null;
    }
}
