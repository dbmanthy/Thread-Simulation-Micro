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
    public Slider meshLocking;
    public Text xDimensionText;
    public Text yDimensionText;
    public Text xSpacingText;
    public Text ySpacingText;
    public Text meshLockingText;

    [Header("Settings Settings")]
    public Slider pointRadius;
    public Slider lineThickness;
    public Slider gravity;
    public Slider friction;
    public Slider bounceLoss;
    public Slider regidity;
    public Text pointRadiusText;
    public Text lineThicknessText;
    public Text gravityText;
    public Text frictionText;
    public Text bounceLossText;
    public Text regidityText;


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
            meshLockingText.text = "mesh spacing" + meshLocking.value;
        }

        if(SettinMenu.activeSelf)
        {
            pointRadiusText.text = "point radius: " + pointRadius.value;
            lineThicknessText.text = "line thickness: " + lineThickness.value;
            gravityText.text = "gravity: " + gravity.value;
            frictionText.text = "friction: " + friction.value;
            bounceLossText.text = "bounce loss: " + bounceLoss.value;
            regidityText.text = "regidity: " + regidity.value;
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
        simulator.pointRadius = pointRadius.value;
        simulator.lineThickness = lineThickness.value;
        simulator.gravity = gravity.value;
        simulator.friciton = friction.value;
        simulator.bounceLoss = bounceLoss.value;
        simulator.regidity = regidity.value;
    }
}
