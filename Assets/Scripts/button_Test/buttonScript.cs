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

    // Start is called before the first frame update
    void Start()
    {
        
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
        }
    }

    public void openActBag()
    {
        toOpen.transform.SetAsLastSibling();
        toOpen.SetActive(true);
        toClose.SetActive(false);
        //toOpen.GetComponent<Animator>().SetBool("isOpen", true);

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



}
