using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Pooler;

public class VerletSimulation : MonoBehaviour
{
    public float pointRadius = .32f;
    public float lineThickness;
    public float gravity = 10f;
    public float friciton = .999f;
    public float bounceLoss = .95f;
    public float regidity = 5f;
    public bool running;

    public GameObject starPrefab;
    public GameObject barPrefab;

    public Color pointColor;
    public Color pinnedPointColor;

    protected List<Star> stars; //dictionary or tuples
    protected List<Bar> bars;
    protected List<GameObject> drawn;

    Vector2 screenHalfSizeWorldUnits;
    Star capturedStar;
    bool starCaptured;
    bool drawingBar;
    int barStartIndex;
    int[] barOrder;
    const float lineThicknessFactor = 1 / 30f;

    protected virtual void Start()
    {
        if (stars == null)
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
        starCaptured = false;
        drawingBar = false;
        screenHalfSizeWorldUnits = new Vector2(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);

        Pooler.instance.CreatePool(starPrefab, 500);
        Pooler.instance.CreatePool(barPrefab, 1000);
    }

    void Update()
    {
        CleanUp();
        HandleInput(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (running)
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
            if (s.position.x > screenHalfSizeWorldUnits.x)
            {
                float offset = s.position.x - s.prevPosition.x;
                s.position.x = screenHalfSizeWorldUnits.x;
                s.prevPosition.x = s.position.x + offset * bounceLoss;
            }
            else if (s.position.x < -screenHalfSizeWorldUnits.x)
            {
                float offset = s.position.x - s.prevPosition.x;
                s.position.x = -screenHalfSizeWorldUnits.x;
                s.prevPosition.x = s.position.x + offset * bounceLoss;
            }
            if (s.position.y > screenHalfSizeWorldUnits.y)
            {
                float offset = s.position.y - s.prevPosition.y;
                s.position.y = screenHalfSizeWorldUnits.y;
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
        public Star starHead, starTail;
        public float length;
        public bool beingDrawn;
        public bool dead;

        public Bar(Star starA, Star starB)
        {
            starHead = starA;
            starTail = starB;
            length = Vector2.Distance(starA.position, starB.position);
        }
    }

    protected virtual void HandleInput(Vector2 mousePosition)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            running = !running;
        }


        if (running)
        {
            int i = MouseOverPointIndex(mousePosition);
            bool mouseOverPoint = i != -1;

            //handle mouse drag
            //TODO could add a max diff between prevPos and Pos so there is a max move speed
            if (Input.GetMouseButton(0) && mouseOverPoint && !starCaptured)
            {
                capturedStar = stars[i];
                starCaptured = true;
            }

            if(starCaptured)
            {
                capturedStar.position = mousePosition;
            }

            if(Input.GetMouseButtonUp(0))
            {
                capturedStar = null;
                starCaptured = false;
            }
        }
        else
        {
            int i = MouseOverPointIndex(mousePosition);
            bool mouseOverPoint = i != -1;
            Debug.Log(i);

            //pin star
            if (Input.GetMouseButtonDown(1) && mouseOverPoint)
            {
                stars[i].pinned = !stars[i].pinned;
            }

            //creat star or bar
            if (Input.GetMouseButtonDown(0))
            {
                if (mouseOverPoint)
                {
                    drawingBar = true;
                    barStartIndex = i;
                    Debug.Log("bar start " + i.ToString());
                }
                else
                {
                    stars.Add(new Star() { position = mousePosition, prevPosition = mousePosition });
                }
            }

            //connect bar
            if (Input.GetMouseButtonUp(0))
            {
                if (mouseOverPoint && drawingBar)
                {
                    if (i != barStartIndex)
                    {
                        Debug.Log("bar connected");
                        bars.Add(new Bar(stars[barStartIndex], stars[i]));
                        ShuffleArray();
                    }
                }
                drawingBar = false;
            }
        }

    }

    int MouseOverPointIndex(Vector2 mousePosition)
    {
        for (int i = 0; i < stars.Count; i++)
        {
            float distance = (stars[i].position - mousePosition).magnitude;

            if (distance < pointRadius * 1.2f)
            {
                return i;
            }
        }
        return -1;
    }

    void Draw()
    {
        foreach (Star star in stars)
        {
            Pooler.instance.ReuseObject(starPrefab, star.position, Quaternion.identity, Vector3.one * pointRadius, star.pinned ? pinnedPointColor : pointColor);
            GameObject drawnObject = Pooler.instance.currentInstance;
            drawn.Add(drawnObject);
        }

        foreach (Bar bar in bars)
        {
            Vector3 center = (bar.starHead.position + bar.starTail.position) / 2;
            var rotation = Quaternion.FromToRotation(Vector3.up, (bar.starHead.position - bar.starTail.position).normalized);
            Vector3 scale = new Vector3(lineThickness * lineThicknessFactor, (bar.starHead.position - bar.starTail.position).magnitude, lineThickness * lineThicknessFactor);
        
            Pooler.instance.ReuseObject(starPrefab, center, rotation, scale, bar.beingDrawn ? pinnedPointColor : pointColor);
            GameObject drawnObject = Pooler.instance.currentInstance;
            drawn.Add(drawnObject);
        }

        if (drawingBar)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }
    }

    void CleanUp() //TODO could check if a point has moved first?
    {
        foreach (GameObject gameObject in drawn)
        {
            gameObject.SetActive(false);
        }
    }

    protected void ShuffleArray()
    {
        barOrder = new int[bars.Count];
        for (int i = 0; i < barOrder.Length; i++)
        {
            barOrder[i] = i;
        }
        Utility.ShuffleArray(barOrder, new System.Random());
    }
}
