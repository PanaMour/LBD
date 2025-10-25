using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCardBack : MonoBehaviour
{
    public GameObject cardBack;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ThisMagic.staticCardBack == true)
        {
            cardBack.SetActive(true);
        }
        else
        {
            cardBack.SetActive(false);
        }

    }
}
