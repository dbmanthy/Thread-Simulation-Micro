using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Pooler;
using static VerletSimulation;

public class VerletSimulation : MonoBehaviour
{
    [Range(.1f,1f)]
    public float pointRadius = .32f;
    [Range(1f,4f)]
    public float lineThickness;
    [Range(-100,100)]
    public float gravity = 10f;
    [Range(0,1)]
    public float friciton = .999f;
    [Range(0,1)]
    public float bounceLoss = .95f;
    [Range(0,21)]
    public float regidity = 5f;
    [Range(1, 5)]
    public int meshLocking = 5;

    [Header("Cloth Settings")]
    public Vector2Int numPoints;
    public Vector2 boundsSize;

    public bool running;
    public bool constrainBarMinLength = true;

    public GameObject starPrefab;
    public GameObject barPrefab;

    public Color starColor;
    public Color pinnedStarColor;

    Color connectedBarColor;
    Color barColor;
    const float colorTint = .12f;

    protected List<Star> stars; //dictionary or tuples
    protected List<Bar> bars;
    protected List<GameObject> drawn;

    Vector2 screenHalfSizeWorldUnits;
    Vector2 breakPositionOld;
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

        connectedBarColor = new Color(starColor.r - colorTint, starColor.g - colorTint, starColor.b - colorTint);
        barColor = new Color(pinnedStarColor.r - colorTint, pinnedStarColor.g - colorTint, pinnedStarColor.b - colorTint);

        Pooler.instance.CreatePool(starPrefab, 10000);
        Pooler.instance.CreatePool(barPrefab, 10000);
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

        for (int i = 0; i < regidity; i++)
        {
            for (int b = 0; b < bars.Count; b++)
            {
                Bar bar = bars[barOrder[b]];
                if (bar.dead)
                {
                    continue;
                }

                Vector2 stickCentre = (bar.starHead.position + bar.starTail.position) / 2;
                Vector2 stickDir = (bar.starHead.position - bar.starTail.position).normalized;
                float length = (bar.starHead.position - bar.starTail.position).magnitude;

                if (length > bar.length || constrainBarMinLength)
                {
                    if (!bar.starHead.pinned)
                    {
                        bar.starHead.position = stickCentre + stickDir * bar.length / 2;
                    }
                    if (!bar.starTail.pinned)
                    {
                        bar.starTail.position = stickCentre - stickDir * bar.length / 2;
                    }
                }

            }
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

            //
            if (Input.GetMouseButtonDown(1))
            {
                breakPositionOld = mousePosition;
            }
            if (Input.GetMouseButton(1))
            {
                BreakBar(breakPositionOld, mousePosition);
                breakPositionOld = mousePosition;
            }
        }
        else
        {
            int i = MouseOverPointIndex(mousePosition);
            bool mouseOverPoint = i != -1;

            if (Input.GetKeyDown(KeyCode.G))
            {
                GenerateGrid(numPoints, boundsSize); 
            }

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
                        bars.Add(new Bar(stars[barStartIndex], stars[i]));
                        ShuffleArray();
                        barStartIndex = -1;
                    }
                }
                drawingBar = false;
            }
        }

    }

    void Draw()
    {
        foreach (Star star in stars)
        {
            Pooler.instance.ReuseObject(starPrefab, star.position, Quaternion.identity, Vector3.one * pointRadius, star.pinned ? pinnedStarColor : starColor);
            GameObject drawnObject = Pooler.instance.currentInstance;
            drawn.Add(drawnObject);
        }

        foreach (Bar bar in bars)
        {
            if (bar.dead)
            {
                continue;
            }
            Vector3 center = (bar.starHead.position + bar.starTail.position) / 2;
            var rotation = Quaternion.FromToRotation(Vector3.up, (bar.starHead.position - bar.starTail.position).normalized);
            Vector3 scale = new Vector3(lineThickness * lineThicknessFactor, (bar.starHead.position - bar.starTail.position).magnitude, lineThickness * lineThicknessFactor);
        
            Pooler.instance.ReuseObject(starPrefab, center, rotation, scale, connectedBarColor);
            GameObject drawnObject = Pooler.instance.currentInstance;
            drawn.Add(drawnObject);
        }

        if (drawingBar)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 center = (stars[barStartIndex].position + mousePosition) / 2;
            var rotation = Quaternion.FromToRotation(Vector3.up, (stars[barStartIndex].position - mousePosition).normalized);
            Vector3 scale = new Vector3(lineThickness * lineThicknessFactor, (stars[barStartIndex].position - mousePosition).magnitude, lineThickness * lineThicknessFactor);

            Pooler.instance.ReuseObject(starPrefab, center, rotation, scale, barColor);
            GameObject drawnObject = Pooler.instance.currentInstance;
            drawn.Add(drawnObject);
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

    void BreakBar(Vector2 start, Vector2 end)
    {
        for (int i = bars.Count - 1; i >= 0; i--)
        {
            if (Utility.LineSegmentsIntersect(start, end, bars[i].starHead.position, bars[i].starTail.position))
            {
                bars[i].dead = true;
            }
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

    void GenerateGrid(Vector2Int numStars, Vector2 spacing)
    {
        CleanUp();
        stars.Clear();
        bars.Clear();

        spacing.x = spacing.x < 2 ? 2 : spacing.x;
        spacing.y = spacing.y < 2 ? 2 : spacing.y;

        while (screenHalfSizeWorldUnits.x/pointRadius < numStars.x * (spacing.x/2))
        {
            if (spacing.x > 2)
            {
                spacing.x--;
            }
            else
            {
                numStars.x = Mathf.FloorToInt(screenHalfSizeWorldUnits.x/pointRadius);
            }
        }
        while (screenHalfSizeWorldUnits.y/pointRadius < numStars.y * (spacing.y/2))
        {
            if (spacing.y > 2)
            {
                spacing.y--;
            }
            else
            {
                numStars.y = Mathf.FloorToInt(screenHalfSizeWorldUnits.y/pointRadius);
            }
        }

        for (int y = 0; y < numStars.y; y++)
        {
            float posY = y / (numStars.y - 1f);//divide the space between 0-1 inot y parts
            for (int x = 0; x < numStars.x; x++)
            {
                bool locked = y == 0 && (x % meshLocking == 0 || x== numStars.x -1);
                float posX = x / (numStars.x - 1f);
                Vector2 position = new Vector2((posX -0.5f) * numStars.x * spacing.x * pointRadius, (0.5f - posY) * numStars.y * spacing.y * pointRadius); //center and scale by spacing
                stars.Add(new Star() { position = position, prevPosition = position, pinned = locked });
            }
        }

        for (int y = 0; y < numStars.y; y++)
        {
            for (int x = 0; x < numStars.x; x++)
            {
                if (x < numStars.x - 1)
                {
                    bars.Add(new Bar(stars[IndexFrom2DCoord(x, y, numStars)], stars[IndexFrom2DCoord(x + 1, y, numStars)]));
                }
                if (y < numStars.y - 1)
                {
                    bars.Add(new Bar(stars[IndexFrom2DCoord(x, y, numStars)], stars[IndexFrom2DCoord(x, y + 1, numStars)]));
                }
            }
        }

        ShuffleArray();
    }

    int IndexFrom2DCoord(int x, int y, Vector2Int numPoints)
    {
        return y * numPoints.x + x;
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
        public bool dead;

        public Bar(Star starA, Star starB)
        {
            starHead = starA;
            starTail = starB;
            length = Vector2.Distance(starA.position, starB.position);
        }
    }

}
