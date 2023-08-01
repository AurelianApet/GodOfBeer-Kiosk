using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SocketIO;
using SimpleJSON;
using System.IO;

public class MenuDetailManager : MonoBehaviour
{
    public Image menuImg;
    public Text price;
    public Text menuTitle;
    public Text menuDesc;
    public Text amount;
    public Image backBtnImage;
    public Image cartBtnImage;
    public Image orderBtnImage;

    // Start is called before the first frame update
    void Start()
    {
        if (Global.cur_selected_menu.image != "")
        {
            StartCoroutine(downloadImage(Global.cur_selected_menu.image, Global.imgPath + Path.GetFileName(Global.cur_selected_menu.image), menuImg.gameObject));
        }
        if(Global.LanguageType == 1)
        {
            menuTitle.text = Global.cur_selected_menu.name;
            price.text = Global.GetPriceFormat(Global.cur_selected_menu.price) + " 원";
            backBtnImage.sprite = Resources.Load<Sprite>("back");
            cartBtnImage.sprite = Resources.Load<Sprite>("cart");
            orderBtnImage.sprite = Resources.Load<Sprite>("justorder");
        }
        else
        {
            menuTitle.text = Global.cur_selected_menu.engname;
            price.text = Global.GetPriceFormat(Global.cur_selected_menu.price);
            backBtnImage.sprite = Resources.Load<Sprite>("back1");
            cartBtnImage.sprite = Resources.Load<Sprite>("cart1");
            orderBtnImage.sprite = Resources.Load<Sprite>("justorder1");
        }
        menuDesc.text = Global.cur_selected_menu.desc;
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
        WWW pictureWWW = new WWW(Global.prePath + name);
        yield return pictureWWW;

        if (img != null)
        {
            img.sprite = Sprite.Create(pictureWWW.texture, new Rect(0, 0, pictureWWW.texture.width, pictureWWW.texture.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
        }
    }

    public void onminusBtn()
    {
        try
        {
            int cnt = int.Parse(amount.text);
            if(cnt > 1)
            {
                amount.text = (cnt - 1).ToString();
            }

        }catch(Exception ex)
        {
            amount.text = "1";
        }
    }

    public void onplusBtn()
    {
        try
        {
            int cnt = int.Parse(amount.text);
            amount.text = (cnt + 1).ToString();
        }
        catch (Exception ex)
        {
            amount.text = "1";
        }
    }

    public void onBackBtn()
    {
        Global.is_start_loading = true;
        SceneManager.LoadScene("main");
    }

    public void justOrder()
    {
        onaddCart(1);
        SceneManager.LoadScene("ordercart");
    }

    public void onaddCart(int type = 0)
    {
        int cnt = 1;
        try
        {
            cnt = int.Parse(amount.text);
        }
        catch (Exception ex)
        {

        }
        CartInfo cinfo = new CartInfo();
        cinfo.menu_id = Global.cur_selected_menu.menu_id;
        cinfo.name = Global.cur_selected_menu.name;
        cinfo.engname = Global.cur_selected_menu.engname;
        cinfo.price = Global.cur_selected_menu.price;
        cinfo.desc = Global.cur_selected_menu.desc;
        cinfo.image = Global.cur_selected_menu.image;
        Global.addCartItem(cinfo, cnt);
        if(type == 0)
        {
            SceneManager.LoadScene("main");
        }
    }
}
