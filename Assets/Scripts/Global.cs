using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;

public struct SetInfo
{
    public string[] paths;
    public int slide_option;//0-기정, 1-선택
    public int auto_login;//1-자동
    public int save_id;
    public string admin_password;//관리자 비밀번호
    public string userName;
    public string userPw;
}

public struct CategoryInfo
{
    public string id;
    public string name;
    public string engname;
    public List<MenuInfo> menulist;
}

public struct MenuInfo
{
    public string menu_id;
    public string image;
    public string name;
    public string engname;
    public string desc;
    public int price;
    public bool is_soldout;

    public string tap_id;
    public string coffee;
    public string water;
}

public struct CartInfo
{
    public string menu_id;
    public string image;
    public string name;
    public string engname;
    public string desc;
    public int price;
    public int amount;
    public string order_time;

    public string tap_id;
    public string coffee;
    public string water;
}

public struct TapInfo
{
    public string id;
    public int no;
    public string name;
    public int keg_capacity;
    public int soldout_capacity;
    public int sensor;
    public string product_id;
    public int is_soldout;
}

public struct CoffeeInfo
{
    public string id;
    public string name;
}

public struct DeviceInfo
{
    public string id;
    public string name;
    public string ip;
}

public class Global
{
    //setting information
    public static SetInfo setInfo = new SetInfo();
    public static int order_type = 0;//0-market, 1-pack

    //image download path
    public static string imgPath = "";
    public static string prePath = "";

    public static int app_type = 0;//0-beer, 1-wine
    //main
    public static int cart_amount = 0;
    public static int cart_price = 0;
    public static int LanguageType = 1;//0-english, 1-korean
    public static string cur_categoryId = "";
    public static string cur_selectedId = "";
    public static int cur_pageIndex = 1;
    public static string cur_setCategoryId = "";
    public static string cur_setSlectedId = "";
    public static int cur_setpageIndex = 1;
    public static bool is_start_loading = true;
    public static MenuInfo cur_selected_menu = new MenuInfo();
    public static List<CartInfo> mycartlist = new List<CartInfo>();
    public static List<CategoryInfo> categorylist = new List<CategoryInfo>();

    //api
    public static string server_address = "";
    public static string api_server_port = "3006";
    public static string api_url = "http://" + server_address + ":" + api_server_port + "/";
    static string api_prefix = "m-api/kiosk/";
    public static string checkDB_api = api_prefix + "check-db";
    public static string login_api = api_prefix + "login";
    public static string get_categorylist_api = api_prefix + "get-categorylist";
    public static string order_api = api_prefix + "order";
    public static string get_taplist_api = api_prefix + "get-taplist";
    public static string change_keg_api = api_prefix + "change-keg";
    public static string soldout_api = api_prefix + "soldout";
    public static string save_tapinfo_api = api_prefix + "save-tapinfo";
    public static string get_tapsetlist_api = api_prefix + "get-tapset";
    public static string save_tapsetinfo_api = api_prefix + "save-tapsetinfo";
    public static string get_taginfo_api = api_prefix + "get-taginfo";

    //socket server
    public static string socket_server = "ws://" + server_address + ":" + api_server_port;
    public static void removeOneCartItem(string menuId)
    {
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == menuId)
            {
                CartInfo cinfo = mycartlist[i];
                if (cinfo.amount > 1)
                {
                    cinfo.amount--;
                    mycartlist[i] = cinfo;
                }
                else
                {
                    mycartlist.Remove(mycartlist[i]);
                }
                cart_amount -= cinfo.amount;
                cart_price -= cinfo.price;
                break;
            }
        }
    }

    public static void trashCartItem(string menuId)
    {
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == menuId)
            {
                cart_amount -= mycartlist[i].amount;
                cart_price -= mycartlist[i].price * mycartlist[i].amount;
                mycartlist.Remove(mycartlist[i]);
                break;
            }
        }
    }

    public static void addOneCartItem(CartInfo cinfo)
    {
        bool is_existing = false;
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == cinfo.menu_id)
            {
                is_existing = true;
                cinfo.amount = mycartlist[i].amount + 1;
                mycartlist[i] = cinfo; break;
            }
        }
        if (!is_existing)
        {
            mycartlist.Add(cinfo);
        }
        cart_price += cinfo.price;
        cart_amount ++;
    }

    public static void addCartItem(CartInfo cinfo, int amount)
    {
        bool is_existing = false;
        for (int i = 0; i < mycartlist.Count; i++)
        {
            if (mycartlist[i].menu_id == cinfo.menu_id)
            {
                is_existing = true;
                cinfo.amount = mycartlist[i].amount + amount;
                mycartlist[i] = cinfo; break;
            }
        }
        if (!is_existing)
        {
            cinfo.amount = amount;
            mycartlist.Add(cinfo);
        }
        cart_price += cinfo.price * amount;
        cart_amount += cinfo.amount;
    }

    public static string GetShowSubInfoFormat(string str)
    {
        if (str.Length > 11)
        {
            str = str.Substring(0, 10) + "...";
        }
        return str;
    }

    public static string GetPriceFormat(float price)
    {
        return string.Format("{0:N0}", price);
    }
}


