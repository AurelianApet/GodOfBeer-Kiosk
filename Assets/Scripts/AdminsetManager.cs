using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;

public class AdminsetManager : MonoBehaviour
{
    public GameObject tapItem;
    public GameObject tapParent;
    public GameObject pageItem;
    public GameObject pageParent;
    public GameObject select_popup;
    public Button select_confirmBtn;
    public Text select_str;
    public GameObject notice_popup;
    public Text notice_str;

    List<TapInfo> tapList = new List<TapInfo>();
    int old_page = -1;
    // Start is called before the first frame update
    void Start()
    {
        WWWForm form = new WWWForm();
        WWW www = new WWW(Global.api_url + Global.get_taplist_api, form);
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
                tinfo.is_soldout = tlist[i]["is_soldout"].AsInt;
                tinfo.product_id = tlist[i]["product_id"];
                tapList.Add(tinfo);
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
                TapInfo _tinfo = tapList[(index - 1) * 8 + i];
                tmp.transform.Find("kegcharge").GetComponent<Button>().onClick.AddListener(delegate () { onChangeKeg(_tinfo); });
                if(tapList[(index - 1) * 8 + i].is_soldout == 1)
                {
                    tmp.transform.Find("soldout").GetComponent<Image>().sprite = Resources.Load<Sprite>("soldout");
                    int _i = i;
                    int _index = index;
                    tmp.transform.Find("soldout").GetComponent<Button>().onClick.RemoveAllListeners();
                    tmp.transform.Find("soldout").GetComponent<Button>().onClick.AddListener(delegate () { onSoldout(_tinfo, _i, _index, true); });
                }
                else
                {
                    tmp.transform.Find("soldout").GetComponent<Image>().sprite = Resources.Load<Sprite>("soldout1");
                    int _i = i;
                    int _index = index;
                    tmp.transform.Find("soldout").GetComponent<Button>().onClick.RemoveAllListeners();
                    tmp.transform.Find("soldout").GetComponent<Button>().onClick.AddListener(delegate () { onSoldout(_tinfo, _i, _index); });
                }
                tmp.transform.Find("water").GetComponent<Button>().onClick.AddListener(delegate () { onWater(_tinfo); });
                tmp.transform.Find("coffee").GetComponent<Button>().onClick.AddListener(delegate () { onCoffee(_tinfo); });
            }
            catch (Exception ex)
            {

            }
        }
    }

    void onChangeKeg(TapInfo tinfo)
    {
        select_popup.SetActive(true);
        select_str.text = "Keg를 교체하시겠습니까?";
        select_confirmBtn.onClick.RemoveAllListeners();
        select_confirmBtn.onClick.AddListener(delegate () { onConfirmChangeKeg(tinfo); });
    }

    public void closeSelectPopup()
    {
        select_popup.SetActive(false);
    }

    public void closeNoticePopup()
    {
        notice_popup.SetActive(false);
    }

    void onConfirmChangeKeg(TapInfo tinfo)
    {
        WWWForm form = new WWWForm();
        form.AddField("tap_id", tinfo.id);
        WWW www = new WWW(Global.api_url + Global.change_keg_api, form);
        StartCoroutine(processChangeKeg(www));
    }

    IEnumerator processChangeKeg(WWW www)
    {
        yield return www;
        if(www.error == null)
        {
            select_popup.SetActive(false);
        }
    }

    void onSoldout(TapInfo tinfo, int i , int index, bool status = false)
    {
        WWWForm form = new WWWForm();
        form.AddField("tap_id", tinfo.id);
        if (status)
        {
            form.AddField("status", 1); //reback
        }
        else
        {
            form.AddField("status", 0); //soldout
        }
        WWW www = new WWW(Global.api_url + Global.soldout_api, form);
        StartCoroutine(processSoldout(www, tinfo, i, index, status));
    }

    IEnumerator processSoldout(WWW www, TapInfo tinfo, int i, int index, bool status)
    {
        yield return www;
        if (www.error == null)
        {
            try
            {
                tinfo.is_soldout = 1;
                tapList[(index - 1) * 8 + i] = tinfo;
                tapParent.transform.GetChild(i).Find("soldout").GetComponent<Button>().onClick.RemoveAllListeners();
                if (status)
                {
                    tapParent.transform.GetChild(i).Find("soldout").GetComponent<Image>().sprite = Resources.Load<Sprite>("soldout1");
                    tapParent.transform.GetChild(i).Find("soldout").GetComponent<Button>().onClick.AddListener(delegate () { onSoldout(tinfo, i, index); });
                }
                else
                {
                    tapParent.transform.GetChild(i).Find("soldout").GetComponent<Image>().sprite = Resources.Load<Sprite>("soldout");
                    tapParent.transform.GetChild(i).Find("soldout").GetComponent<Button>().onClick.AddListener(delegate () { onSoldout(tinfo, i, index, true); });
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

    void onWater(TapInfo tinfo)
    {
        notice_popup.SetActive(true);
        notice_str.text = "TAP" + tinfo.no + " Water 추출을 시작합니다.";
    }

    void onCoffee(TapInfo tinfo)
    {
        notice_popup.SetActive(true);
        notice_str.text = "TAP" + tinfo.no + " Coffee 추출을 시작합니다.";
    }

    public void onBack()
    {
        SceneManager.LoadScene("setting");
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
