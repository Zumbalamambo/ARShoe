﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelList : ViewController {

    private NavigationViewController navigationView;
    public Camera cam;
    public Vector2 minPos, maxPos;
    public ShoeRow newRow, bestRow;

	// Use this for initialization
	void Start () {

        navigationView = UIManager.Instance.navigationView;
        GetComponent<ScrollRect>().onValueChanged.AddListener(OnScrollChanged);
        AddShoeItemsIntoRow();

        // navigation view의 첫번째 뷰로 설정
        if (navigationView != null && UIManager.Instance.viewStack.Count == 0)
        {
            navigationView.Push(this);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnScrollChanged(Vector2 scrollPos){
        cam.transform.position = Vector2.Lerp(minPos, maxPos, scrollPos.y);
    }

    void AddShoeItemsIntoRow(){
        UIManager.Instance.itemList.ForEach((item) =>
        {
            if(item.isNew) newRow.AddNewShoe(item);
            if(item.isBest) bestRow.AddNewShoe(item);
        });
    }
}