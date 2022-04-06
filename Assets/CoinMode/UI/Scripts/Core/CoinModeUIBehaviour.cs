using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    public abstract class CoinModeUIBehaviour : MonoBehaviour
    {
        public CoinModeUIBehaviour parent { get; private set; } = null;
        private HashSet<CoinModeUIBehaviour> children { get; set; } = new HashSet<CoinModeUIBehaviour>();
        private HashSet<GameObject> childBehvaiourGameObjects { get; set; } = new HashSet<GameObject>();
        private List<Selectable> childSelectables { get; set; } = new List<Selectable>();

        private bool builtSelectables = false;

        protected virtual void OnValidate()
        {

        }

        protected virtual void Awake()
        {
            if (transform.parent != null)
            {
                parent = transform.parent.GetComponentInParent<CoinModeUIBehaviour>();
                if (parent != null)
                {
                    parent.AddChild(this);
                }
            }
        }

        protected virtual void Start()
        {

        }

        protected virtual void UpdateUI(float deltaTime)
        {
            foreach (CoinModeUIBehaviour child in children)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    child.UpdateUI(deltaTime);
                }
            }
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        protected virtual void OnDestroy()
        {
            if (parent != null)
            {
                parent.RemoveChild(this);
            }
        }

        protected virtual void AddChild(CoinModeUIBehaviour child)
        {
            if (!children.Contains(child))
            {
                children.Add(child);
                childBehvaiourGameObjects.Add(child.gameObject);
            }
        }

        protected virtual void RemoveChild(CoinModeUIBehaviour child)
        {
            if (children.Contains(child))
            {
                children.Remove(child);
                childBehvaiourGameObjects.Remove(child.gameObject);
            }
        }

        protected virtual void BuildSelectableTree()
        {
            FindSelectables(transform);
            builtSelectables = true;
        }

        private void FindSelectables(Transform t)
        {
            Selectable owned = t.GetComponent<Selectable>();
            if (owned != null)
            {
                childSelectables.Add(owned);
            }
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                if (!childBehvaiourGameObjects.Contains(c.gameObject))
                {
                    Selectable s = c.GetComponent<Selectable>();
                    if (s != null)
                    {
                        childSelectables.Add(s);
                    }
                    FindSelectables(c);
                }
                else
                {
                    // Not happy with this, I have it cached so could be faster
                    CoinModeUIBehaviour b = c.gameObject.GetComponent<CoinModeUIBehaviour>();
                    b.BuildSelectableTree();
                }
            }
        }

        protected virtual void GetAllChildSelectables(ref List<Selectable> children)
        {
            if(!builtSelectables)
            {
                BuildSelectableTree();
            }
            children.AddRange(childSelectables);
            foreach(CoinModeUIBehaviour b in this.children)
            {
                b.GetAllChildSelectables(ref children);
            }
        }
    }

    internal static class CoinModeUIExtensions
    {
        public static void FocusVertically(this ScrollRect instance, RectTransform target)
        {
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = target.localPosition;
            instance.content.localPosition = new Vector2(
                instance.content.localPosition.x,
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
        }
    }
}


