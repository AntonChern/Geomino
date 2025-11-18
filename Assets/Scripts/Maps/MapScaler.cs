using UnityEngine;

public class MapScaler : MonoBehaviour
{
    public bool scaleX = true;
    public bool scaleY = true;

    private Camera mainCamera;
    private Renderer mapRenderer;
    private Vector2 startTextureOffset;
    private Vector2 scaleTextureOffset;
    private Vector2 dragTextureOffset;

    private Vector3 scale;
    private Vector2 startTextureScale;

    private void Start()
    {
        mainCamera = Camera.main;
        mapRenderer = GetComponent<Renderer>();

        startTextureOffset = mapRenderer.material.mainTextureOffset;
        scaleTextureOffset = Vector3.zero;
        dragTextureOffset = Vector3.zero;

        scale = transform.localScale;
        startTextureScale = mapRenderer.material.mainTextureScale;
    }

    private void LateUpdate()
    {
        float unitsTall = mainCamera.orthographicSize * 2f;
        float unitsWide = unitsTall * mainCamera.aspect;
        transform.localScale = new Vector3(scaleX ? unitsWide : scale.x, scaleY ? unitsTall : scale.y, scale.z);
        Vector2 textureScale = new Vector3(scaleX ? unitsWide / scale.x : startTextureScale.x, scaleY ? unitsTall / scale.y : startTextureScale.y);
        mapRenderer.material.mainTextureScale = textureScale;
        scaleTextureOffset = new Vector2(scaleX ? (textureScale.x - 1f) / 2f : 0f, scaleY ? (textureScale.y - 1f) / 2f : 0f);
        mapRenderer.material.mainTextureOffset = startTextureOffset - scaleTextureOffset + dragTextureOffset;
    }

    public void SetDragTextureOffset(Vector2 offset)
    {
        dragTextureOffset -= new Vector2(offset.x / scale.x, offset.y / scale.y);
    }
}
