using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class CardObjectPool : MonoBehaviour
    {
        public static CardObjectPool Instance;

        private Button poolingObjectPrefab;

        Queue<Button> poolingObjectQueue = new Queue<Button>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            Initialize(10);
        }

        private void Initialize(int initCount)
        {
            poolingObjectPrefab = Instantiate(Resources.Load<Button>("Prefabs/UI/Button"));

            for (int i = 0; i < initCount; i++)
            {
                poolingObjectQueue.Enqueue(CreateNewObject());
            }
        }

        private Button CreateNewObject()
        {
            var newObj = Instantiate(poolingObjectPrefab);
            newObj.gameObject.SetActive(false);
            newObj.transform.SetParent(transform);
            return newObj;
        }

        public static Button GetObject()
        {
            if (Instance.poolingObjectQueue.Count > 0)
            {
                var obj = Instance.poolingObjectQueue.Dequeue();
                obj.transform.SetParent(null);
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                var newObj = Instance.CreateNewObject();
                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(null);
                return newObj;
            }
        }

        public static void ReturnObject(Button obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(Instance.transform);
            obj.transform.localScale = Vector3.one;
            Instance.poolingObjectQueue.Enqueue(obj);
        }
    }
}