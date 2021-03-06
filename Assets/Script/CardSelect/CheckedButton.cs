﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NTUT.CSIE.GameDev.CardSelect
{
    public class CheckedButton : MonoBehaviour
    {

        public Button btn1, btn2;

        // Use this for initialization
        void Start()
        {

        }

        // ckeck button
        public void Checked()
        {
            GameObject card = GameObject.Find("Card");
            if (this.GetComponent<Button>().name == "EasyButton")
                card.GetComponent<CreateCard>().SetGameDifficult(1);
            else if (this.GetComponent<Button>().name == "NormalButton")
                card.GetComponent<CreateCard>().SetGameDifficult(2);
            else if (this.GetComponent<Button>().name == "HardButton")
                card.GetComponent<CreateCard>().SetGameDifficult(3);
            this.GetComponent<Image>().color = Color.white;
            btn1.GetComponent<Image>().color = Color.gray;
            btn2.GetComponent<Image>().color = Color.gray;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}

