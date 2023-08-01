using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;
using SocketIO;
using System.IO;

public class OrdercartManager : MonoBehaviour
{
    public GameObject cart_item;
    public GameObject cart_parent;
    public GameObject err_popup;
    public Text err_msg;
    public GameObject select_popup;
    public Text select_str;
    public GameObject paypopup;
    public GameObject orderPopup;
    public Image payBtnImage;
    public Image backBtnImage;
    public Image allBtnImage;
    public Text priceTxt;
    public Text amountTxt;
    public Text priceNotice;
    public Text amonutNotice;
    public Text paypriceNotice;
    public Text payNotice;
    public Image payCancelImage;
    public Image payRequestImage;
    public InputField payamount;
    public InputField orderamountTxt;
    public Image orderCancelBtnImg;
    public Image confirmBtnImg;
    //public Image orderBtnImg;
    public InputField tagInput;
    public Text orderNoticeTxt;
    public Text orderPriceNoticeTxt;

    GameObject[] m_CartList;
    bool is_done_order = false;
    float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadCartList());
        if(Global.LanguageType == 1)
        {
            amonutNotice.text = "수량";
            priceNotice.text = "합계";
            priceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            amountTxt.text = Global.cart_amount + " 개";
            backBtnImage.sprite = Resources.Load<Sprite>("back");
            allBtnImage.sprite = Resources.Load<Sprite>("alldelete");
            //payBtnImage.sprite = Resources.Load<Sprite>("pay");
            payBtnImage.sprite = Resources.Load<Sprite>("order");
        }
        else
        {
            amonutNotice.text = "Quantity";
            priceNotice.text = "Total";
            priceTxt.text = Global.GetPriceFormat(Global.cart_price);
            amountTxt.text = Global.cart_amount.ToString();
            backBtnImage.sprite = Resources.Load<Sprite>("back1");
            allBtnImage.sprite = Resources.Load<Sprite>("alldelete1");
            //payBtnImage.sprite = Resources.Load<Sprite>("pay1");
            payBtnImage.sprite = Resources.Load<Sprite>("order1");
        }
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

    public void onBack()
    {
        Global.is_start_loading = true;
        SceneManager.LoadScene("main");
    }

    public void allDelete()
    {
        Global.mycartlist.Clear();
        Global.cart_price = 0;
        Global.cart_amount = 0;
        priceTxt.text = "0";
        amountTxt.text = "0";
        if (Global.LanguageType == 1)
        {
            priceTxt.text += " 원";
            amountTxt.text += " 개";
        }
        StartCoroutine(ClearList());
    }

    IEnumerator ClearList()
    {
        while (cart_parent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(cart_parent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator LoadCartList()
    {
        while (cart_parent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(cart_parent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
        m_CartList = new GameObject[Global.mycartlist.Count];
        for (int i = 0; i < Global.mycartlist.Count; i++)
        {
            m_CartList[i] = Instantiate(cart_item);
            m_CartList[i].transform.SetParent(cart_parent.transform);
            try
            {
                if (Global.mycartlist[i].image != "")
                {
                    StartCoroutine(downloadImage(Global.mycartlist[i].image, Global.imgPath + Path.GetFileName(Global.mycartlist[i].image), m_CartList[i].transform.Find("Image").gameObject));
                }
                //m_CartList[i].transform.Find("Image").GetComponent<ImageWithRoundedCorners>().radius = 30;
                m_CartList[i].transform.Find("id").GetComponent<Text>().text = Global.mycartlist[i].menu_id.ToString();
                Text amount = m_CartList[i].transform.Find("amount").GetComponent<Text>();
                amount.text = Global.mycartlist[i].amount.ToString();
                if(Global.LanguageType == 1)
                {
                    m_CartList[i].transform.Find("title").GetComponent<Text>().text = Global.mycartlist[i].name;
                }
                else
                {
                    m_CartList[i].transform.Find("title").GetComponent<Text>().text = Global.mycartlist[i].engname;
                }
                Text price = m_CartList[i].transform.Find("price").GetComponent<Text>();
                price.text = Global.GetPriceFormat(Global.mycartlist[i].price * Global.mycartlist[i].amount);
                int unit_price = Global.mycartlist[i].price;
                CartInfo cinfo = Global.mycartlist[i];
                m_CartList[i].transform.Find("minus").GetComponent<Button>().onClick.AddListener(delegate () { onMinusBtn(unit_price, price, amount, cinfo); });
                m_CartList[i].transform.Find("plus").GetComponent<Button>().onClick.AddListener(delegate () { onPlusBtn(unit_price, price, amount, cinfo); });
                m_CartList[i].transform.Find("trush").GetComponent<Button>().onClick.AddListener(delegate () { onTrash(cinfo.menu_id); });
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        priceTxt.text = Global.GetPriceFormat(Global.cart_price);
    }

    IEnumerator downloadImage(string url, string pathToSaveImage, GameObject imgObj)
    {
        yield return new WaitForSeconds(0.1f);
        Image img = imgObj.GetComponent<Image>();
        if (File.Exists(pathToSaveImage))
        {
            Debug.Log(pathToSaveImage + " exists");
            StartCoroutine(LoadPictureToTexture(pathToSaveImage, img));
        }
        else
        {
            Debug.Log(pathToSaveImage + " downloading--");
            WWW www = new WWW(url);
            StartCoroutine(_downloadImage(www, pathToSaveImage, img));
        }
    }

    private IEnumerator _downloadImage(WWW www, string savePath, Image img)
    {
        yield return www;
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
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
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
        Debug.Log("load image = " + Global.prePath + name);
        WWW pictureWWW = new WWW(Global.prePath + name);
        yield return pictureWWW;

        if (img != null)
        {
            img.sprite = Sprite.Create(pictureWWW.texture, new Rect(0, 0, pictureWWW.texture.width, pictureWWW.texture.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
        }
    }


    public void onMinusBtn(int unit_price, Text price, Text amount, CartInfo cinfo)
    {
        int cnt = 1;
        try
        {
            cnt = int.Parse(amount.text);
        }
        catch (Exception ex)
        {
        }
        if (cnt == 1)
            return;
        cnt--;
        Global.removeOneCartItem(cinfo.menu_id);
        amount.text = cnt.ToString();
        price.text = Global.GetPriceFormat(unit_price * cnt);
        if(Global.LanguageType == 1)
        {
            priceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            amountTxt.text = Global.cart_amount.ToString() + " 개";
        }
        else
        {
            priceTxt.text = Global.GetPriceFormat(Global.cart_price);
            amountTxt.text = Global.cart_amount.ToString();
        }
    }

    public void onPlusBtn(int unit_price, Text price, Text amount, CartInfo cinfo)
    {
        int cnt = 1;
        try
        {
            cnt = int.Parse(amount.text);
        }
        catch (Exception ex)
        {
        }
        cnt++;
        amount.text = cnt.ToString();
        Global.addOneCartItem(cinfo);
        price.text = Global.GetPriceFormat(unit_price * cnt);
        if (Global.LanguageType == 1)
        {
            priceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            amountTxt.text = Global.cart_amount.ToString() + " 개";
        }
        else
        {
            priceTxt.text = Global.GetPriceFormat(Global.cart_price);
            amountTxt.text = Global.cart_amount.ToString();
        }
    }

    public void onTrash(string menuId)
    {
        //UI상에서 제거
        for (int i = 0; i < m_CartList.Length; i++)
        {
            try
            {
                if (m_CartList[i].transform.Find("id").GetComponent<Text>().text == menuId.ToString())
                {
                    DestroyImmediate(m_CartList[i].gameObject);
                    break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //Global에서 제거
        Global.trashCartItem(menuId);
        if (Global.LanguageType == 1)
        {
            priceTxt.text = Global.GetPriceFormat(Global.cart_price) + " 원";
            amountTxt.text = Global.cart_amount.ToString() + " 개";
        }
        else
        {
            priceTxt.text = Global.GetPriceFormat(Global.cart_price);
            amountTxt.text = Global.cart_amount.ToString();
        }
    }

    public void onOrderBtn()
    {
        orderPopup.SetActive(true);
        orderamountTxt.text = Global.GetPriceFormat(Global.cart_price);
        if (Global.LanguageType == 1)
        {
            orderPriceNoticeTxt.text = "주문금액";
            orderNoticeTxt.text = "TAG를 리더기에 읽혀주세요.";
            orderCancelBtnImg.sprite = Resources.Load<Sprite>("cancel");
            tagInput.GetComponent<Image>().sprite = Resources.Load<Sprite>("order");
        }
        else
        {
            orderPriceNoticeTxt.text = "Amount";
            orderNoticeTxt.text = "Touch your TAG to order";
            orderCancelBtnImg.sprite = Resources.Load<Sprite>("cancel1");
            tagInput.GetComponent<Image>().sprite = Resources.Load<Sprite>("order1");
        }
        tagInput.Select();
        tagInput.ActivateInputField();
        tagInput.text = "";
        tagInput.onValueChanged.AddListener((value) => {
            checkTag(value);
        }
        );

    }

    float send_time = 0f;
    void checkTag(string value)
    {
        if (value == "")
            return;
        send_time += Time.deltaTime;
        StartCoroutine(sendCheckTag(value, send_time));
    }

    IEnumerator sendCheckTag(string str, float stime)
    {
        yield return new WaitForSeconds(0.1f);
        if (send_time != stime)
        {
            yield break;
        }
        WWWForm form = new WWWForm();
        form.AddField("data", str);
        WWW www = new WWW(Global.api_url + Global.get_taginfo_api, form);
        StartCoroutine(onCheckTagProcess(www));
        tagInput.text = "";
        send_time = 0f;
    }

    IEnumerator onCheckTagProcess(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            Debug.Log(jsonNode);
            if (jsonNode["suc"].AsInt == 1)
            {
                int st = jsonNode["status"].AsInt;
                int tag_id = jsonNode["tag_id"].AsInt;
                switch (st)
                {
                    case 0:
                        {
                            if(Global.LanguageType == 1)
                            {
                                err_msg.text = "등록되지 않은 TAG입니다.";
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                            }
                            else
                            {
                                err_msg.text = "This TAG is not registered.";
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                            }
                            err_popup.SetActive(true);
                            break;
                        };
                    case 1:
                        {
                            if(Global.LanguageType == 1)
                            {
                                err_msg.text = "등록되지 않은 TAG입니다.";
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                            }
                            else
                            {
                                err_msg.text = "This TAG is not registered.";
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                            }
                            err_popup.SetActive(true);
                            break;
                        }
                    case 2:
                        {
                            int is_pay_after = jsonNode["is_pay_after"].AsInt;
                            int period = jsonNode["period"].AsInt;
                            int remain = jsonNode["remain"].AsInt;
                            if (is_pay_after == 0)
                            {
                                int expired = jsonNode["expired"].AsInt;
                                if(expired == 1)
                                {
                                    if(Global.LanguageType == 1)
                                    {
                                        err_msg.text = "사용기한이 지난 TAG입니다.";
                                        confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                                    }
                                    else
                                    {
                                        err_msg.text = "Your TAG has expired.";
                                        confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                                    }
                                    err_popup.SetActive(true);
                                }
                                else
                                {
                                    if (remain == 0 || remain < Global.cart_price)
                                    {
                                        if(Global.LanguageType == 1)
                                        {
                                            err_msg.text = "TAG의 잔액이 부족합니다.";
                                            confirmBtnImg.sprite = Resources.Load<Sprite>("ok");

                                        }
                                        else
                                        {
                                            err_msg.text = "TAG's balance is not enough.";
                                            confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                                        }
                                        err_popup.SetActive(true);
                                    }
                                    else
                                    {
                                        onOrder(tag_id, is_pay_after);
                                    }
                                }
                            }
                            else
                            {
                                onOrder(tag_id, is_pay_after);
                            }
                            break;
                        };
                    case 3:
                        {
                            if(Global.LanguageType == 1)
                            {
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                                err_msg.text = "분실된 TAG입니다.";
                            }
                            else
                            {
                                err_msg.text = "Your TAG is lost.";
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                            }
                            err_popup.SetActive(true);
                            break;
                        };
                    case 4:
                        {
                            if(Global.LanguageType == 1)
                            {
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                                err_msg.text = "현재 셀프 이용 중인 TAG 입니다.";
                            }
                            else
                            {
                                confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                                err_msg.text = "Your TAG is in use.";
                            }
                            err_popup.SetActive(true);
                            break;
                        }
                }
            }
            else
            {
                if(Global.LanguageType == 1)
                {
                    confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                    err_msg.text = "이용할 수 없는 태그입니다.";
                }
                else
                {
                    confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                    err_msg.text = "You can`t use this TAG.";
                }
                err_popup.SetActive(true);
            }
        }
        else
        {
            if(Global.LanguageType == 1)
            {
                confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                err_msg.text = "서버접속이 원활하지 않습니다.\n 후에 다시 시도해주세요.";
            }
            else
            {
                confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                err_msg.text = "Unknown Error in server connection.";
            }
            err_popup.SetActive(true);
        }
    }

    public void onOrder(int tag_id, int is_pay_after)
    {
        //주문 api
        WWWForm form = new WWWForm();
        form.AddField("tag_id", tag_id);
        form.AddField("is_pay_after", is_pay_after);
        string oinfo = "[";
        for (int i = 0; i < Global.mycartlist.Count; i++)
        {
            if (i == 0)
            {
                oinfo += "{";
            }
            else
            {
                oinfo += ",{";
            }
            oinfo += "\"menu_id\":\"" + Global.mycartlist[i].menu_id + "\","
                + "\"menu_name\":\"" + Global.mycartlist[i].name + "\","
                + "\"price\":" + Global.mycartlist[i].price.ToString() + ","
                + "\"quantity\":" + Global.mycartlist[i].amount.ToString() + "}";
        }
        oinfo += "]";
        Debug.Log(oinfo);
        form.AddField("order_info", oinfo);
        form.AddField("order_type", Global.order_type);
        WWW www = new WWW(Global.api_url + Global.order_api, form);
        StartCoroutine(ProcessOrder(www));
    }

    IEnumerator ProcessOrder(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                Global.mycartlist.Clear();
                Global.cart_amount = 0;
                Global.cart_price = 0;
                if(Global.LanguageType == 1)
                {
                    err_msg.text = "주문이 완료되었습니다.";
                }
                else
                {
                    err_msg.text = "Your order has been placed.";
                }
                is_done_order = true;
                err_popup.SetActive(true);
                StartCoroutine(ScanDelay());
            }
            else
            {
                if(Global.LanguageType == 1)
                {
                    confirmBtnImg.sprite = Resources.Load<Sprite>("ok");

                }
                else
                {
                    confirmBtnImg.sprite = Resources.Load<Sprite>("ok");

                }
                if(Global.LanguageType == 1)
                {
                    err_msg.text = jsonNode["msg"];
                    confirmBtnImg.sprite = Resources.Load<Sprite>("ok");
                }
                else
                {
                    err_msg.text = "Order Failed.";
                    confirmBtnImg.sprite = Resources.Load<Sprite>("ok1");
                }
                err_popup.SetActive(true);
            }
        }
    }

    public void onConfirmErrPopup()
    {
        err_popup.SetActive(false);
        if (is_done_order)
        {
            SceneManager.LoadScene("start");
        }
    }

    public void onCloseSelectPopup()
    {
        select_popup.SetActive(false);
    }

    public void onCloseOrderpopup()
    {
        orderPopup.SetActive(false);
    }

    IEnumerator ScanDelay()
    {
        yield return new WaitForSeconds(10f);
        SceneManager.LoadScene("start");
    }

    public void onClosePaypopup()
    {
        paypopup.SetActive(false);
    }

    public void onOutputInvoice()
    {
        select_popup.SetActive(false);
    }

    public void onConfirmPay()
    {
        paypopup.SetActive(false);
    }

    public void onPay()
    {
        paypopup.SetActive(true);
        payamount.text = Global.GetPriceFormat(Global.cart_price);
        if(Global.LanguageType == 1)
        {
            paypriceNotice.text = "결제금액";
            payNotice.text = "카드를 리더기에 넣어주세요.";
            payCancelImage.sprite = Resources.Load<Sprite>("cancel");
            payRequestImage.sprite = Resources.Load<Sprite>("request");
        }
        else
        {
            paypriceNotice.text = "Amount";
            payNotice.text = "Insert Debit/Credit card";
            payCancelImage.sprite = Resources.Load<Sprite>("cancel1");
            payRequestImage.sprite = Resources.Load<Sprite>("request1");
        }
    }
}
