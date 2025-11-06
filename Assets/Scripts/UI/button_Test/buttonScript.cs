using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class buttonScript : MonoBehaviour
{
    public bool isAct;
    public bool isZoom;
    public bool isBag;

    public GameObject toOpen = null;
    public GameObject toClose = null;
    public GameObject zoomMag = null;

    // 静态实例，用于其他脚本访问放大镜状态
    private static buttonScript instance;
    public static buttonScript Instance => instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (isZoom)
        {
            openZoom();
        }
        if (isZoom && Input.GetMouseButtonDown(0))
        {
            zoomMag.SetActive(false);
            isZoom = false;
            
            // 关闭放大镜时隐藏tooltip
            TooltipSystem.Hide();
        }
    }

    /// <summary>
    /// 打开物品背包（BagButton专用）
    /// </summary>
    public void OpenItemBag()
    {
        if (PackageManager.Instance != null)
        {
            PackageManager.Instance.OpenPanel();
            Debug.Log("[buttonScript] 已打开物品背包");
        }
        else
        {
            Debug.LogError("[buttonScript] 未找到 PackageManager！请确保场景中有 PackagePanel 并挂载了 PackageManager 脚本");
        }
    }
    
    /// <summary>
    /// 打开宝可梦菜单（PokemonBagButton专用）
    /// </summary>
    public void OpenPokemonMenu()
    {
        if (PokemonMenuManager.Instance != null)
        {
            PokemonMenuManager.Instance.OpenMenu();
            Debug.Log("[buttonScript] 已打开宝可梦背包");
        }
        else
        {
            Debug.LogError("[buttonScript] 未找到 PokemonMenuManager！请确保场景中有 PokemonMenuPanel 并挂载了 PokemonMenuManager 脚本");
        }
    }

    public void zoomTrue()
    {
        isZoom = true;
    }
    
    public void openZoom()
    {
        if (zoomMag != null)
        {
            zoomMag.SetActive(true);
            // For Screen Space - Overlay, the camera parameter should be null
            Vector2 localPoint;
            Canvas canvas = zoomMag.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas.transform as RectTransform;

            // Convert screen position to local position in the canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition,null, out localPoint);

            // Move the zoomMag GameObject to follow the cursor
            zoomMag.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
    }

    /// <summary>
    /// 静态方法 - 检查放大镜是否开启
    /// </summary>
    public static bool IsZoomActive()
    {
        return instance != null && instance.isZoom;
    }
}
