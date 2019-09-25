using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class Conway : MonoBehaviour {
	public bool lifeActive = true;
	[Header("Config")]
	public ComputeShader gameOfLife;
	public ComputeShader noise;
	public Texture2D initialWorld = null;
	public bool initNoise = false;

	public int worldSize = 1024;
	public float tickPeriod = 0.1f;
	
	
	[Header("Quantum particles in vacuum")]
	[Range(1,int.MaxValue/100)]
	public int quantumInstability = 1000000;

	[Header("Texture target")]
	public Material material;
	public string materialTexture = "_MainTex";

	private RenderTexture front;
	private RenderTexture back;

	RenderTexture newWorld(int size) {
		RenderTexture rt = new RenderTexture(size, size, 16, RenderTextureFormat.RFloat);
		rt.autoGenerateMips = false;
		rt.filterMode = FilterMode.Point;
		rt.enableRandomWrite = true;
		rt.wrapMode = TextureWrapMode.Repeat;
		rt.Create();
		return rt;
	}

	// Use this for initialization
	void Start() {
		front = newWorld(worldSize);
		back = newWorld(worldSize);

		if ( initNoise ) {
			noise.SetTexture(0, "Texture", front);
			noise.SetInt("worldSize", worldSize);
			noise.SetInt("seed", (int)System.DateTime.Now.Ticks);
			noise.Dispatch(0, worldSize / 8, worldSize / 8, 1);
		} else {
			Graphics.Blit(initialWorld, front);
		}

		material.SetTexture(materialTexture, front);

		StartCoroutine(LifeTick());
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
