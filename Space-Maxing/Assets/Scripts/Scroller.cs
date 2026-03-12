using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _x, _y;

    void Start()
    {
        // Ensure texture is set to Repeat mode
        if (_img != null && _img.texture != null)
        {
            _img.texture.wrapMode = TextureWrapMode.Repeat;
        }
    }

    void Update()
    {
        _img.uvRect = new Rect(_img.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _img.uvRect.size);
    }
}