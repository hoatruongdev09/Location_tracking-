using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using UnityEngine;
using UnityEngine.UI;
public class LocationTracking : MonoBehaviour {
	public float updateTime = 60;
	public bool isTracking;
	public Text txt_status;
	public Text txt_latitude;
	public Text txt_longtitude;
	public Text txt_date;
	public GameObject panelHistory;

	public HistoryItem historyItem;
	public RectTransform historyHolder;
	private Mongo db;
	[SerializeField] private List<LocationData> cached = new List<LocationData> ();
	private void Start () {
		db = new Mongo ();
		db.Init ();
		Application.runInBackground = true;
		StartCoroutine (Init ());
	}
	public void StartTracking () {
		if (!isTracking) {
			StartCoroutine (Updating ());
		} else {
			StopCoroutine (Updating ());
			txt_status.text = "Stopped ";
			isTracking = false;
		}
	}
	public void CopyCurrentPositionToClipboard () {
		TextEditor te = new TextEditor ();
		te.text = txt_latitude.text + "," + txt_longtitude.text;
		te.SelectAll ();
		te.Copy ();
	}
	public void OpenHistory () {
		// List<Model_Location> listModel = db.GetAll ();
		// foreach (Model_Location md in listModel) {
		// 	Debug.Log (md.latitude + " ; " + md.longitude + " ; " + md.date);
		// }
		StartCoroutine (OpenHistoryPanel ());
	}
	public void ButtonBack () {
		panelHistory.SetActive (false);
	}
	private IEnumerator OpenHistoryPanel () {
		if (!CheckInternet ()) {
			txt_status.text = " no internet ....";
			yield break;
		}
		MongoCursor<Model_Location> listModel = db.GetAll ();
		yield return null;
		CreateListHistory (listModel);
		yield return null;
		panelHistory.SetActive (true);
		yield return null;
	}
	private void CreateListHistory (MongoCursor<Model_Location> listModel) {
		foreach (Transform chil in historyHolder.transform) {
			Destroy (chil.gameObject);
		}
		VerticalLayoutGroup layoutGroup = historyHolder.GetComponent<VerticalLayoutGroup> ();
		float historyItemHeight = historyItem.GetComponent<RectTransform> ().sizeDelta.y;
		historyHolder.sizeDelta = new Vector2 (historyHolder.sizeDelta.x, (historyItemHeight + layoutGroup.spacing) * listModel.Count () + layoutGroup.padding.top);

		foreach (Model_Location model in listModel) {
			HistoryItem it = Instantiate (historyItem, historyHolder.transform);
			it.SetItem (model.date, model.latitude, model.longitude);
		}
	}
	private IEnumerator Updating () {
		Input.location.Start ();
		isTracking = true;
		txt_latitude.text = Input.location.lastData.latitude.ToString ();
		txt_longtitude.text = Input.location.lastData.longitude.ToString ();
		txt_date.text = System.DateTime.Now.ToString ();
		if (CheckInternet ()) {
			txt_status.text = "Updating ...";
			if (cached.Count != 0) {
				foreach (LocationData ld in cached) {
					db.InsertLocation (ld);
				}
			}
			db.InsertLocation (new LocationData (Input.location.lastData, System.DateTime.Now));
			cached = new List<LocationData> ();
		} else {
			txt_status.text = "Updating ... no internet";
			cached.Add (new LocationData (Input.location.lastData, System.DateTime.Now));
		}
		//statusText.text = ("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
		Input.location.Stop ();
		yield return new WaitForSecondsRealtime (updateTime);
		StartCoroutine (Updating ());
	}
	private IEnumerator Init () {
		if (!Input.location.isEnabledByUser) {
			yield break;
		}
		Input.location.Start ();

		int maxWait = 30;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSecondsRealtime (1);
			maxWait--;
		}
		if (maxWait < 1) {
			txt_status.text = ("Time out");
			yield break;
		}
		if (Input.location.status == LocationServiceStatus.Failed) {
			txt_status.text = ("Unable to determine device location");
		} else {
			txt_status.text = ("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

		}
		Input.location.Stop ();
	}
	private bool CheckInternet () {
		string HtmlText = GetHtmlFromUri ("http://google.com");
		if (HtmlText == "") {
			return false;
		} else if (!HtmlText.Contains ("schema.org/WebPage")) {
			return false;
		} else {
			return true;
		}
	}
	public string GetHtmlFromUri (string resource) {
		string html = string.Empty;
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create (resource);
		try {
			using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse ()) {
				bool isSuccess = (int) resp.StatusCode < 299 && (int) resp.StatusCode >= 200;
				if (isSuccess) {
					using (StreamReader reader = new StreamReader (resp.GetResponseStream ())) {
						//We are limiting the array to 80 so we don't have
						//to parse the entire html document feel free to 
						//adjust (probably stay under 300)
						char[] cs = new char[80];
						reader.Read (cs, 0, cs.Length);
						foreach (char ch in cs) {
							html += ch;
						}
					}
				}
			}
		} catch {
			return "";
		}
		return html;
	}

}

[Serializable]
public class LocationData {
	public float latitude;
	public float longitude;
	public float altitude;
	public float horizontalAccuracy;
	public float verticalAccuracy;
	public double timestamp;
	public DateTime date;

	public LocationData (LocationInfo info, DateTime date) {
		latitude = info.latitude;
		longitude = info.longitude;
		altitude = info.altitude;
		horizontalAccuracy = info.horizontalAccuracy;
		verticalAccuracy = info.verticalAccuracy;
		timestamp = info.timestamp;
		this.date = date;
	}

	public static LocationData CreateFromJson (string data) {
		return JsonUtility.FromJson<LocationData> (data);
	}
	public static string GenerateJson (LocationData data) {
		return JsonUtility.ToJson (data);
	}
}