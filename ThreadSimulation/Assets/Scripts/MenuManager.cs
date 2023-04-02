using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject GridMenu;
    public GameObject SettinMenu;
    public GameObject HelpMenu;

    [Header("Grid Settings")]
    public Slider xDimension;
    public Slider yDimension;
    public Slider xSpacing;
    public Slider ySpacing;
    public Text xDimensionText;
    public Text yDimensionText;
    public Text xSpacingText;
    public Text ySpacingText;


    VerletSimulation simulator;

    void Start()
    {
        simulator = FindObjectOfType<VerletSimulation>();
    }

    void Update()
    {
        if(GridMenu.activeSelf)
        {
            xDimensionText.text = "x dimension: " + xDimension.value;
            yDimensionText.text = "y dimension: " + yDimension.value;
            xSpacingText.text = "x spacing: " + xSpacing.value;
            ySpacingText.text = "y spacing: " + ySpacing.value;
        }
        
    }

    public void OnSettingsClick()
    {
        GridMenu.SetActive(false);
        HelpMenu.SetActive(false);
        SettinMenu.SetActive(!SettinMenu.activeSelf);
    }

    public void OnGridClick()
    {
        SettinMenu.SetActive(false);
        HelpMenu.SetActive(false);
        GridMenu.SetActive(!GridMenu.activeSelf);
    }

    public void OnHelpClick()
    {
        GridMenu.SetActive(false);
        SettinMenu.SetActive(false);
        HelpMenu.SetActive(!HelpMenu.activeSelf);
    }

    public void SetGridValues()
    {
        simulator.numPoints = new Vector2Int((int)xDimension.value, (int)yDimension.value);
        simulator.meshSpacing = new Vector2((int)xSpacing.value, (int)ySpacing.value);

    }

    public void SetSettings()
    {

    }
}
