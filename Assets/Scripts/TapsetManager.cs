using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;
using System.IO;

public class TapsetManager : MonoBehaviour
{
    public GameObject tapItem;
    public GameObject tapParent;
    public GameObject pageItem;
    public GameObject pageParent;
    public GameObject detailPopup;
    public GameObject coffeeItem;
    public GameObject coffeeParent;
    public Text tapNoTxt;
    public InputField coffeeTxt;
    public InputField kegsizeTxt;
    public InputField soldoutTxt;
    public InputField sonsorTxt;
    public GameObject err_popup;
    public Text err_str;

    List<TapInfo> tapList = new List<TapInfo>();
    List<CoffeeInfo> coffeelist = new List<CoffeeInfo>();
    int old_page = -1;
    string cur_tapId = "";
    string cur_coffeeId = "";

    // Start is called before the first frame update
    void Start()
    {
        WWWForm form = new WWWForm();
        WWW www = new WWW(Global.api_url + Global.get_tapsetlist_api, form);
        StartCoroutine(LoadInfo(www));
    }

    IEnumerator Destroy_Object(GameObject obj)
    {
        DestroyImmediate(obj);
        yield return null;
    }

    IEnumerator LoadInfo(WWW www)
    {
        yield return www;
        tapList.Clear();
        coffeelist.Clear();
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            Debug.Log(jsonNode);
            JSONNode tlist = JSON.Parse(jsonNode["taplist"].ToString()/*.Replace("\"", "")*/);
            for (int i = 0; i < tlist.Count; i++)
            {
                TapInfo tinfo = new TapInfo();
                tinfo.no = tlist[i]["no"].AsInt;
                tinfo.id = tlist[i]["id"];
                tinfo.name = tlist[i]["name"];
                tinfo.product_id = tlist[i]["product_id"];
                tinfo.keg_capacity = tlist[i]["kegcharge"].AsInt;
                tinfo.soldout_capacity = tlist[i]["soldout"].AsInt;
                tinfo.sensor = tlist[i]["sensor"].AsInt;
                tapList.Add(tinfo);
            }
            JSONNode clist = JSON.Parse(jsonNode["coffeelist"].ToString());
            for(int i = 0; i < clist.Count; i++)
            {
                CoffeeInfo cinfo = new CoffeeInfo();
                cinfo.name = clist[i]["name"];
                cinfo.id = clist[i]["id"];
                coffeelist.Add(cinfo);
            }
            //UI에 추가
            for (int i = 0; i < Math.Ceiling(tapList.Count / 8f); i++)
            {
                GameObject tmp = Instantiate(pageItem);
                tmp.transform.SetParent(pageParent.transform);
                try
                {
                    tmp.transform.GetComponent<Button>().onClick.RemoveAllListeners();
                    int _i = i + 1;
                    tmp.transform.GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine(onPage(_i)); });
                }
                catch (Exception ex)
                {
                }
            }

            StartCoroutine(onPage(1));
            StartCoroutine(LoadCoffeelist());
        }
    }

    IEnumerator onPage(int index)
    {
        if (old_page == index)
        {
            yield return null;
        }
        while (tapParent.transform.childCount > 0)
        {
            StartCoroutine(Destroy_Object(tapParent.transform.GetChild(0).gameObject));
        }
        while (tapParent.transform.childCount > 0)
        {
            yield return new WaitForFixedUpdate();
        }
        if (old_page != -1)
        {
            try
            {
                pageParent.transform.GetChild(old_page - 1).Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("unsel");
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        try
        {
            pageParent.transform.GetChild(index - 1).Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("sel");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
        old_page = index;
        for (int i = 0; i < 8; i++)
        {
            if ((index - 1) * 8 + i >= tapList.Count)
                break;
            GameObject tmp = Instantiate(tapItem);
            tmp.transform.SetParent(tapParent.transform);
            try
            {
                tmp.transform.Find("id").GetComponent<Text>().text = tapList[(index - 1) * 8 + i].id.ToString();
                tmp.transform.Find("product_id").GetComponent<Text>().text = tapList[(index - 1) * 8 + i].product_id.ToString();
                tmp.transform.Find("no").GetComponent<Text>().text = tapList[(index - 1) * 8 + i].no.ToString();
                tmp.transform.Find("name").GetComponent<Text>().text = tapList[(index - 1) * 8 + i].name;
                tmp.transform.Find("soldout").GetComponent<Text>().text = Global.GetPriceFormat(tapList[(index - 1) * 8 + i].soldout_capacity) + " ml";
                tmp.transform.Find("keg").GetComponent<Text>().text = Global.GetPriceFormat(tapList[(index - 1) * 8 + i].keg_capacity) + " ml";
                TapInfo _tinfo = tapList[(index - 1) * 8 + i];
                tmp.GetComponent<Button>().onClick.AddListener(delegate () { onDetail(_tinfo); });
            }
            catch (Exception ex)
            {

            }
        }
    }

    void onDetail(TapInfo tinfo)
    {
        cur_tapId = tinfo.id;
        if(tinfo.product_id == "")
        {
            cur_coffeeId = "";
        }
        else
        {
            cur_coffeeId = tinfo.product_id;
        }
        tapNoTxt.text = tinfo.no.ToString();
        detailPopup.transform.Find("back/coffee").GetComponent<InputField>().text = tinfo.name;
        detailPopup.transform.Find("back/keg").GetComponent<InputField>().text = tinfo.keg_capacity.ToString();
        detailPopup.transform.Find("back/soldout").GetComponent<InputField>().text = tinfo.soldout_capacity.ToString();
        detailPopup.transform.Find("back/sensor").GetComponent<InputField>().text = tinfo.sensor.ToString();
        detailPopup.SetActive(true);
    }

    IEnumerator LoadCoffeelist()
    {
        while (coffeeParent.transform.childCount > 0)
        {
            StartCoroutine(Destroy_Object(coffeeParent.transform.GetChild(0).gameObject));
        }
        while (coffeeParent.transform.childCount > 0)
        {
            yield return new WaitForFixedUpdate();
        }
        for(int i = 0; i < coffeelist.Count; i++)
        {
            GameObject tmp = Instantiate(coffeeItem);
            tmp.transform.SetParent(coffeeParent.transform);
            tmp.GetComponent<Text>().text = coffeelist[i].name;
            tmp.transform.Find("id").GetComponent<Text>().text = coffeelist[i].id.ToString();
            int _i = i;
            string _cid = coffeelist[i].id;
            tmp.GetComponent<Button>().onClick.AddListener(delegate () { onSetCoffee(_i, _cid); });
        }
    }

    void onSetCoffee(int index, string id)
    {
        try
        {
            cur_coffeeId = coffeelist[index].id;
            coffeeTxt.text = coffeelist[index].name;
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void onSave()
    {
        //if(coffeeTxt.text == "" || kegsizeTxt.text == "" || soldoutTxt.text == "" || sonsorTxt.text == "")
        //{
        //    err_popup.SetActive(true);
        //    err_str.text = "설정값들을 정확히 입력하세요.";
        //    return;
        //}
        //Debug.Log(cur_tapId);
        //Debug.Log(cur_coffeeId);
        //if(cur_tapId == -1 || cur_coffeeId == -1)
        //{
        //    return;
        //}
        //WWWForm form = new WWWForm();
        //form.AddField("cur_tapid", cur_tapId);
        //form.AddField("cur_coffeeid", cur_coffeeId);
        //form.AddField("name", coffeeTxt.text);
        //form.AddField("keg", kegsizeTxt.text);
        //form.AddField("soldout", soldoutTxt.text);
        //form.AddField("sensor", sonsorTxt.text);
        //WWW www = new WWW(Global.api_url + Global.save_tapsetinfo_api, form);
        //StartCoroutine(processSaveInfo(www));
    }

    IEnumerator processSaveInfo(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            detailPopup.SetActive(false);
            SceneManager.LoadScene("tapset");
        }
        else
        {
            err_popup.SetActive(true);
            err_str.text = "서버와의 접속이 원활하지 않습니다.\n 잠시후에 다시 시도해주세요.";
        }
    }

    public void onBackBtn()
    {
        SceneManager.LoadScene("setting");
    }

    public void oncloseDetailPopup()
    {
        detailPopup.SetActive(false);
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
