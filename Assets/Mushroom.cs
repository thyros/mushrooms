using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour {
	public static int maxSize = 4;
	public int size;

	public void SetSize(int size) {
		this.size = size;

		var text = GetComponentInChildren<TextMesh>();
		text.text = "" + size;

        float scale = 1 - (float)(maxSize - size) / 5;
        transform.localScale = new Vector3(scale, scale, scale);		
    }

	public void Grow(int size) {
		SetSize(this.size + size);
	}
}
