﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_InventorySystem : MonoBehaviour
{
    //public Texture crosshairTexture;
    //public SC_CharacterController playerController;
    public SC_PickItem[] availableItems; //List with Prefabs of all the available items
    public LayerMask trash;

    //Available items slots
    int[] itemSlots = new int[12];
    bool showInventory = false;
    float windowAnimation = 1;
    float animationTimer = 0;

    //UI Drag & Drop
    int hoveringOverIndex = -1;
    int itemIndexToDrag = -1;
    Vector2 dragOffset = Vector2.zero;

    //Item Pick up
    SC_PickItem detectedItem;
    int detectedItemIndex;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //Initialize Item Slots
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animationTimer < 1)
        {
            animationTimer += Time.deltaTime;
        }

        if (showInventory)
        {
            windowAnimation = Mathf.Lerp(windowAnimation, 0, animationTimer);
            //playerController.canMove = false;
        }
        else
        {
            windowAnimation = Mathf.Lerp(windowAnimation, 1f, animationTimer);
            //playerController.canMove = true;
        }

        //Begin item drag
        if (Input.GetMouseButtonDown(0) && hoveringOverIndex > -1 && itemSlots[hoveringOverIndex] > -1)
        {
            itemIndexToDrag = hoveringOverIndex;
        }

        //Item pick up
        if (detectedItem && detectedItemIndex > -1)
        {
            //if (Input.GetKeyDown(KeyCode.F))
            //{
                //Add the item to inventory
                int slotToAddTo = -1;
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemSlots[i] == -1)
                    {
                        slotToAddTo = i;
                        break;
                    }
                }
                if (slotToAddTo > -1)
                {
                    itemSlots[slotToAddTo] = detectedItemIndex;
                    detectedItem.PickItem();
                }
           // }
        }
    }

    void FixedUpdate()
    {
        //Detect if the Player is looking at any item
        //RaycastHit hit;
        //Ray ray = playerController.playerCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

        //////////////////////////
        if (Input.touchCount > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;


            Touch touch = Input.GetTouch(0);
            Debug.Log(Input.GetTouch(0));

            if (touch.phase == TouchPhase.Began &&
                   Physics.Raycast(ray, out hit, Mathf.Infinity, trash))
            {
                Transform objectHit = hit.transform;

                //if (objectHit.CompareTag("Respawn"))
                //{
                    if ((detectedItem == null || detectedItem.transform != objectHit) && objectHit.GetComponent<SC_PickItem>() != null)
                    {
                        SC_PickItem itemTmp = objectHit.GetComponent<SC_PickItem>();

                        //Check if item is in availableItemsList
                        for (int i = 0; i < availableItems.Length; i++)
                        {
                            if (availableItems[i].itemName == itemTmp.itemName)
                            {
                                detectedItem = itemTmp;
                                detectedItemIndex = i;
                            }
                        }
                    }
            }
            else
            {
                detectedItem = null;
            }
        }
        ///////////////////////////

        //if (Physics.Raycast(ray, out hit, 2.5f))
        
    }

    void OnGUI()
    {
        //Inventory UI
        //GUI.Label(new Rect(5, 5, 200, 25), "Press 'Tab' to open Inventory");

        //Inventory window
        if (windowAnimation < 1)
        {
            GUILayout.BeginArea(new Rect(10 - (430 * windowAnimation), 100, 900, 1200), GUI.skin.GetStyle("box"));

            GUILayout.Label("Inventory", GUILayout.Height(25));

            GUILayout.BeginVertical();
            for (int i = 0; i < itemSlots.Length; i += 3)
            {
                GUILayout.BeginHorizontal();
                //Display 3 items in a row
                for (int a = 0; a < 3; a++)
                {
                    if (i + a < itemSlots.Length)
                    {
                        if (itemIndexToDrag == i + a || (itemIndexToDrag > -1 && hoveringOverIndex == i + a))
                        {
                            GUI.enabled = false;
                        }

                        if (itemSlots[i + a] > -1)
                        {
                            if (availableItems[itemSlots[i + a]].itemPreview)
                            {
                                GUILayout.Box(availableItems[itemSlots[i + a]].itemPreview, GUILayout.Width(300), GUILayout.Height(400));
                            }
                            else
                            {
                                GUILayout.Box(availableItems[itemSlots[i + a]].itemName, GUILayout.Width(300), GUILayout.Height(400));
                            }
                        }
                        else
                        {
                            //Empty slot
                            GUILayout.Box("", GUILayout.Width(300), GUILayout.Height(400));
                        }

                        //Detect if the mouse cursor is hovering over item
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        Vector2 eventMousePositon = Event.current.mousePosition;
                        if (Event.current.type == EventType.Repaint && lastRect.Contains(eventMousePositon))
                        {
                            hoveringOverIndex = i + a;
                            if (itemIndexToDrag < 0)
                            {
                                dragOffset = new Vector2(lastRect.x - eventMousePositon.x, lastRect.y - eventMousePositon.y);
                            }
                        }

                        GUI.enabled = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                hoveringOverIndex = -1;
            }

            GUILayout.EndArea();
        }

        //Item dragging
        if (itemIndexToDrag > -1)
        {
            if (availableItems[itemSlots[itemIndexToDrag]].itemPreview)
            {
                GUI.Box(new Rect(Input.mousePosition.x + dragOffset.x, Screen.height - Input.mousePosition.y + dragOffset.y, 95, 95), availableItems[itemSlots[itemIndexToDrag]].itemPreview);
            }
            else
            {
                GUI.Box(new Rect(Input.mousePosition.x + dragOffset.x, Screen.height - Input.mousePosition.y + dragOffset.y, 95, 95), availableItems[itemSlots[itemIndexToDrag]].itemName);
            }
        }

        //Display item name when hovering over it
        if (hoveringOverIndex > -1 && itemSlots[hoveringOverIndex] > -1 && itemIndexToDrag < 0)
        {
            GUI.Box(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 30, 100, 25), availableItems[itemSlots[hoveringOverIndex]].itemName);
        }

        if (!showInventory)
        {
            //Player crosshair
            GUI.color = detectedItem ? Color.green : Color.white;
            //GUI.DrawTexture(new Rect(Screen.width / 2 - 4, Screen.height / 2 - 4, 8, 8), crosshairTexture);
        }
    }

    public void myBag()
    {
        //Show/Hide inventory
            showInventory = !showInventory;
            animationTimer = 0;

            if (showInventory)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
    }
}