using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HistoryItem : MonoBehaviour {

	public Text txt_date;
	public Text txt_location;

	private float latitude;
	private float longtitude;

	private void Start () {
		GetComponent<Button> ().onClick.AddListener (CopyText);
	}
	public void SetItem (DateTime date, float latidude, float longtitude) {
		this.latitude = latidude;
		this.longtitude = longtitude;
		txt_date.text = date.ToString ();
		txt_location.text = "Latitude: " + latidude.ToString () + "\nLongtitude: " + longtitude.ToString ();
	}

	public void CopyText () {
		TextEditor te = new TextEditor ();
		te.text = latitude.ToString () + "," + longtitude.ToString ();
		te.SelectAll ();
		te.Copy ();
	}

}