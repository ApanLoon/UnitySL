using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> A simple replacement to built-in ScrollBar that exposes some extra information </summary>
public class UIScrollBar : Scrollbar, IEndDragHandler
{
	public bool isDragging { get; private set; }

	public override void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;
		base.OnBeginDrag(eventData);
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
	}
}
