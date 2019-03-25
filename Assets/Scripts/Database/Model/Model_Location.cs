using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using UnityEngine;
using UnityEngine.UI;

public class Model_Location {
    public ObjectId _id;

    public float latitude;
    public float longitude;
    public float altitude;
    public float horizontalAccuracy;
    public float verticalAccuracy;
    public double timestamp;
    public DateTime date;
    public Model_Location (LocationInfo info, DateTime date) {
        latitude = info.latitude;
        longitude = info.longitude;
        altitude = info.altitude;
        horizontalAccuracy = info.horizontalAccuracy;
        verticalAccuracy = info.verticalAccuracy;
        timestamp = info.timestamp;
        this.date = date;
    }
}