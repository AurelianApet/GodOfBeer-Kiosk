using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void onMarket()
    {
        Global.order_type = 0;
        SceneManager.LoadScene("main");
    }

    public void onPack()
    {
        Global.order_type = 1;
        SceneManager.LoadScene("main");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
