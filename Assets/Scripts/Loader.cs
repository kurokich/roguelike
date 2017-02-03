using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour
{
    //GameManagerのプレファブを指定
    public GameObject gameManager;

    void Awake()
    {
        //GameManagerが存在しない時、GameManagerを作成する
        if (GameManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }
}
