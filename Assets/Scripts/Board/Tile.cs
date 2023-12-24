using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private const float ItemDragStartAmount = 0.3f;

    [SerializeField]
    private GameObject selectedBackground;

    public int Row { get; private set; }
    public int Column { get; private set; }
    public Item Item { get; private set; }

    private bool mouseDown;
    private Vector3 mouseWorldPosition;
    private float mouseDistance;

    private Action<Tile> onItemClick;
    private Action<Tile, Vector2> onItemMove;

    private void Awake()
    {
        Deselect();
    }

    public void Init(int row, int column, Action<Tile> onItemClick, Action<Tile, Vector2> onItemMove)
    {
        this.Row = row;
        this.Column = column;

        this.onItemClick = onItemClick;
        this.onItemMove = onItemMove;
    }

    public void SetItem(Item item)
    {
        this.Item = item;
    }

    public void DestroyItem()
    {
        Destroy(Item.gameObject);
        RemoveItem();
    }

    public void RemoveItem()
    {
        Item = null;
    }

    private void OnMouseDown()
    {
        if(!GameController.Instance.InteractionAllowed)
        {
            return;
        }
        mouseDown = true;
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
    }

    private void OnMouseUp()
    {
        if (mouseDown)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPosition.z = 0;
            Vector3 diff = newPosition - mouseWorldPosition;
            Vector3 direction;
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                diff.y = 0;
                direction = Vector3.right * Mathf.Sign(diff.x);
            }
            else
            {
                diff.x = 0;
                direction = Vector3.up * Mathf.Sign(diff.y);
            }
            mouseDistance = diff.magnitude;

            if (mouseDistance > ItemDragStartAmount)
            {
                onItemMove?.Invoke(this, direction);
            }
            else
            {
                onItemClick?.Invoke(this);
            }
        }
        mouseDown = false;
    }

    public void Select()
    {
        selectedBackground.SetActive(true);
    }

    public void Deselect()
    {
        selectedBackground.SetActive(false);
    }
}
