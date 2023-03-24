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
    public bool running;

    public GameObject starPrefab;
    public GameObject barPrefab;

    public Color pointColor;
    public Color pinnedPointColor;

    protected List<Star> stars; //dictionary or tuples
    protected List<Bar> bars;
    protected List<GameObject> drawn;

    Vector2 screenHalfSizeWorldUnits;

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
        screenHalfSizeWorldUnits = new Vector2(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);

        Pooler.instance.CreatePool(starPrefab, 500);
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

        if (running)
        {
        }
        else
        {
            int i = MouseOverPointIndex(mousePosition);
            bool mouseOverPoint = i != -1;

            if (Input.GetMouseButtonDown(1) && mouseOverPoint)
            {
                stars[i].pinned = !stars[i].pinned;
            }

            if (Input.GetMouseButtonDown(0))
            {
                //if (mouseOverPoint)
                //{
                //    drawingStick = true;
                //    stickStartIndex = i;
                //}
                //else
                //{
                stars.Add(new Star() { position = mousePosition, prevPosition = mousePosition });
                //}
            }
        }

    }

    int MouseOverPointIndex(Vector2 mousePosition)
    {
        for (int i = 0; i < stars.Count; i++)
        {
            float distance = (stars[i].position - mousePosition).magnitude;

            if (distance < pointRadius)
            {
                return i;
            }
        }
        return -1;
    }

    void Draw()
    {
        foreach (Star s in stars)
        {
            Pooler.instance.ReuseObject(starPrefab, s.position, Quaternion.identity, Vector3.one * pointRadius);
            GameObject drawnObject = Pooler.instance.lastInstance;
            drawn.Add(drawnObject);

        }
    }

    void CleanUp() // could check if a point has moved first?
    {
        foreach (GameObject gameObject in drawn)
        {
            gameObject.SetActive(false);
        }
    }
}
