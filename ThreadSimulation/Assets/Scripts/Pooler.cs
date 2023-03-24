using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooler : MonoBehaviour
{
    Dictionary<int, Queue<PoolObjectInstance>> poolDict = new Dictionary<int, Queue<PoolObjectInstance>>();

    public GameObject lastInstance;

    //singleton pattern
    static Pooler _instance;

    public static Pooler instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<Pooler>();
            }
            return _instance;
        }
    }

    public void CreatePool(GameObject prefab, int poolSize)
    {
        int poolKey = prefab.GetInstanceID(); //unique int for every game object

        GameObject poolHolder = new GameObject(prefab.name + "Pool");
        poolHolder.transform.parent = transform;

        if(!poolDict.ContainsKey(poolKey))
        {
            poolDict.Add(poolKey, new Queue<PoolObjectInstance>());

            for(int i = 0; i < poolSize; i++)
            {
                PoolObjectInstance gameObject = new PoolObjectInstance(Instantiate(prefab) as GameObject);
                poolDict[poolKey].Enqueue(gameObject);
                gameObject.SetParent(poolHolder.transform);
            }
        }
    }

    public void ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDict.ContainsKey(poolKey))
        {
            PoolObjectInstance gameObjectToReuse = poolDict[poolKey].Dequeue();
            poolDict[poolKey].Enqueue(gameObjectToReuse);

            gameObjectToReuse.Resue(position, rotation, scale);

            lastInstance = gameObjectToReuse.gameObject;
        }
    }

    public class PoolObjectInstance
    {
        public GameObject gameObject;

        Transform transform;
        bool hasPoolObjectComponent;
        PoolObject poolObjectScript;

        public PoolObjectInstance(GameObject PoolObjectInstance)
        {
            gameObject = PoolObjectInstance;
            transform = PoolObjectInstance.transform;
            gameObject.SetActive(false);

            if (gameObject.GetComponent<PoolObject>())
            {
                hasPoolObjectComponent = true;
                poolObjectScript = gameObject.GetComponent<PoolObject>();
            }
        }

        public void Resue(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            gameObject.SetActive(true);

            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;

            if (hasPoolObjectComponent)
            {
                poolObjectScript.OnObjectReuse();
            }
        }

        public void SetParent(Transform parentTransform)
        {
            transform.parent = parentTransform;
        }
    }
}
 