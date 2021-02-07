using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> A simple replacement to built-in ScrollRect that exposes some extra information and options </summary>
public class UIScrollRect : ScrollRect
{
	public bool isDragging { get; private set; }
	/// <summary> Set true to limit the scrollview to dragging only by the scrollbars </summary>
	public bool disableDirectDragging;

	public override void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;
		if (!disableDirectDragging) base.OnBeginDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (!disableDirectDragging) base.OnDrag(eventData);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
		if (!disableDirectDragging) base.OnEndDrag(eventData);
	}
}
