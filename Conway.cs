using System;
using System.Collections;
using UnityEngine;

public class Conway : MonoBehaviour {
	public bool lifeActive = true;
	[Header("Config")]
	public ComputeShader gameOfLife;
	[Space]
	public FilterMode filterMode = FilterMode.Point;
	public int worldSize = 1024;
	public float tickPeriod = 0.1f;
	[Space]
	public bool initWithTexture = false;
	public Texture2D initTexture = null;
	[Space]
	public bool initWithShader = true;
	public ComputeShader initShader;

	[Header("Spontaneous particles in vacuum")]
	[Range(0f, 1f)]
	public float quantumInstabilityPct = 0.1f;
	public int quantumInstability = 1000000;

	[Header("Texture target")]
	public Material material;
	public string materialTexture = "_MainTex";

	private RenderTexture front;
	private RenderTexture back;

	RenderTexture newWorld(int size) {
		RenderTexture rt = new RenderTexture(size, size, 16, RenderTextureFormat.RFloat);
		rt.autoGenerateMips = false;
		rt.filterMode = filterMode;
		rt.enableRandomWrite = true;
		rt.wrapMode = TextureWrapMode.Repeat;
		rt.Create();
		return rt;
	}

	// Use this for initialization
	void Start() {
		front = newWorld(worldSize);
		back = newWorld(worldSize);

		if ( initWithShader ) {
			initShader.SetTexture(0, "Texture", front);
			initShader.SetInt("worldSize", worldSize);
			initShader.SetInt("seed", (int)System.DateTime.Now.Ticks);
			initShader.Dispatch(0, worldSize / 8, worldSize / 8, 1);
		} else if (initWithTexture) {
			Graphics.Blit(initTexture, front);
		}

		material.SetTexture(materialTexture, front);

		StartCoroutine(LifeTick());
	}

	private void OnValidate() {
		quantumInstability = 1+(int)(Mathf.Pow(quantumInstabilityPct,2) * (int.MaxValue / 100));
	}

	IEnumerator LifeTick() {
		WaitForSeconds waitForSeconds = new WaitForSeconds(tickPeriod);
		while ( true ) {
			if ( lifeActive ) {
				gameOfLife.SetTexture(0, "Texture", front);
				gameOfLife.SetTexture(0, "TextureOut", back);
				gameOfLife.SetInt("worldSize", worldSize);
				gameOfLife.SetInt("seed", (int)System.DateTime.Now.Ticks);
				gameOfLife.SetInt("quantumInstability", quantumInstability);
				gameOfLife.Dispatch(0, worldSize / 8, worldSize / 8, 1);

				RenderTexture temp = back;
				back = front;
				front = temp;

				material.SetTexture(materialTexture, front);
			}
			yield return waitForSeconds;
		}
	}

	void Update() {
		if ( Input.GetKeyUp(KeyCode.C) ) {
			lifeActive = !lifeActive;
		}
	}
}
