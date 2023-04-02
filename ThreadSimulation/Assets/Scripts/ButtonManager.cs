using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject GridMenu;
    public GameObject SettinMenu;
    public GameObject HelpMenu;

    public void OnSettingsClick()
    {
        SettinMenu.SetActive(!SettinMenu.activeSelf);
    }

    public void OnGridClick()
    {
        GridMenu.SetActive(!GridMenu.activeSelf);
    }

    public void OnHelpClick()
    {
        HelpMenu.SetActive(!HelpMenu.activeSelf);
    }
}
