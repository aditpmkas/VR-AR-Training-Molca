using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager), typeof(ARAnchorManager))]
public class XRPlaceOnPlane : MonoBehaviour
{
    [Header("Prefab & UI")]
    public GameObject objectToPlace;
    public Button resetButton, rotateButton;

    ARRaycastManager raycastManager;
    ARAnchorManager anchorManager;
    static readonly List<ARRaycastHit> hits = new();
    GameObject spawnedObject;
    ARAnchor anchor;
    bool isHidden;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();

        resetButton?.onClick.AddListener(OnResetClicked);
        rotateButton?.onClick.AddListener(OnRotateClicked);
        SetButtons(false);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            TryPlace(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began &&
            !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            TryPlace(Input.GetTouch(0).position);
#endif
    }

    void TryPlace(Vector2 screenPos)
    {
        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return;

        var hit = hits[0];
        if (spawnedObject == null)
        {
            anchor = anchorManager.AttachAnchor(hit.trackable as ARPlane, hit.pose);
            spawnedObject = Instantiate(objectToPlace, anchor ? anchor.transform : null);
            spawnedObject.transform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);
        }
        else if (isHidden)
        {
            spawnedObject.transform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);
            spawnedObject.SetActive(true);
            isHidden = false;
        }

        SetButtons(true);
    }

    void OnResetClicked()
    {
        if (spawnedObject == null || isHidden) return;
        spawnedObject.SetActive(false);
        isHidden = true;
        SetButtons(false);
    }

    void OnRotateClicked()
    {
        if (spawnedObject == null || isHidden) return;
        spawnedObject.transform.Rotate(0, 90, 0);
    }

    void SetButtons(bool active)
    {
        if (resetButton) resetButton.interactable = active;
        if (rotateButton) rotateButton.interactable = active;
    }
}
