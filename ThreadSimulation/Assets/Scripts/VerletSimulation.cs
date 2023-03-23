using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class VerletSimulation : MonoBehaviour
{
    public float pointRadius = .32f;
    public float lineThickness;
    public float gravity = 10f;
    public float friciton =.999f;
    public float bounceLoss = .95f;
    public bool running;

    public Color pointColor;
    public Color pinnedPointColor;

    protected List<Star> stars;
    protected List<Bar> bars;
    protected List<GameObject> drawn;

    Vector2 screenHalfSizeWorldUnits;

    protected virtual void Start()
    {
        if(stars == null)
        {
            stars = new List<Star>();
        }
        if (bars == null)
        {
            bars = new List<Bar>();
        }
        if (drawn == null)
        {
            drawn = new List<GameObject>();
        }

        running = false;
        screenHalfSizeWorldUnits = new Vector2(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);
    }

    void Update()
    {
        CleanUp();
        HandleInput(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if(running)
        {
            Simulate();
        }
    }

    void LateUpdate()
    {
        Draw();
    }

    void Simulate()
    {
        Debug.Log("running");
        foreach (Star s in stars)
        {
            //basic valet integration
            if (!s.pinned)
            {
                Vector2 positionBeforeUpdate = s.position;
                s.position += (s.position - s.prevPosition) * friciton; //velocity is the difference
                s.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
                s.prevPosition = positionBeforeUpdate;
            }

            //add bounce
            if(s.position.x > screenHalfSizeWorldUnits.x)
            {
                float offset = s.position.x - s.prevPosition.x;
                s.position.x = screenHalfSizeWorldUnits.x;
                s.prevPosition.x = s.position.x + offset * bounceLoss;
            }
            else if(s.position.x < -screenHalfSizeWorldUnits.x)
            {
                float offset = s.position.x - s.prevPosition.x;
                s.position.x = -screenHalfSizeWorldUnits.x;
                s.prevPosition.x = s.position.x + offset * bounceLoss;
            }
            if (s.position.y > screenHalfSizeWorldUnits.y)
            {
                float offset = s.position.y - s.prevPosition.y;
                s.position.y = screenHalfSizeWorldUnits.x;
                s.prevPosition.y = s.position.y + offset * bounceLoss;
            }
            else if (s.position.y < -screenHalfSizeWorldUnits.y)
            {
                float offset = s.position.y - s.prevPosition.y;
                s.position.y = -screenHalfSizeWorldUnits.y;
                s.prevPosition.y = s.position.y + offset * bounceLoss;
            }

        }
    }


    [System.Serializable]
    public class Star
    {
        public Vector2 position, prevPosition;
        public bool pinned;
    }

    [System.Serializable]
    public class Bar
    {
        public Star starA, starB;
        public float length;
        public bool dead;

        public Bar(Star starA, Star starB)
        {
            this.starA = starA;
            this.starB = starB;
            length = Vector2.Distance(starA.position, starB.position);
        }
    }

    protected virtual void HandleInput(Vector2 mousePosition)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            running = !running;
        }

        if(running)
        {
        }
        else
        {
            if(Input.GetMouseButtonDown(0))
            {
                //if (mouseOverPoint)
                //{
                //    drawingStick = true;
                //    stickStartIndex = i;
                //}
                //else
                //{
                    stars.Add(new Star() { position = mousePosition, prevPosition = mousePosition });
                    Debug.Log("click at point " + mousePosition.ToString());
                //}
            }
        }

    }

    void Draw()
    {
        foreach (Star s in stars)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer sphereRenderer = sphere.GetComponent<Renderer>();
            sphereRenderer.material.SetColor("_Color", s.pinned ? pinnedPointColor : pointColor);
            sphere.transform.position = s.position;
            sphere.transform.localScale = Vector3.one * pointRadius;
            drawn.Add(sphere);

        }
    }

    void CleanUp()
    {
        foreach(GameObject go in drawn)
        {
            Destroy(go);
        }
    }
}
