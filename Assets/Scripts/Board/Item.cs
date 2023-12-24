using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer itemImage;

    public int Id { get; private set; }

    public void Init(int itemId, Sprite image)
    {
        Id = itemId;
        itemImage.sprite = image;
        itemImage.color = Color.white;
    }

    public void Init(int itemId, Color color)
    {
        Id = itemId;
        itemImage.color = color;
    }
}
