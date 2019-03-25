using System;
using UnityEngine; //
using System.Collections;
using System.Collections.Generic; // Lists

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
public class Mongo {
    private const string MONGO_URL = "mongodb://victim:victim123@ds151963.mlab.com:51963/location_track";
    private const string DATABASE_NAME = "location_track";

    private MongoClient client;
    private MongoServer server;
    private MongoDatabase db;

    private MongoCollection locations;

    public void Init () {
        client = new MongoClient (MONGO_URL);
        server = client.GetServer ();
        db = server.GetDatabase (DATABASE_NAME);

        Debug.Log ("Database has been inititialized");
        locations = db.GetCollection<Model_Location> ("location");
    }
    public void Shutdown () {
        server.Shutdown ();
        client = null;
        db = null;
    }

    public bool InsertLocation (LocationData data) {
        var tmp = db.GetCollection<Model_Location> ("location");
        locations.Insert (data);
        return true;
    }

    public MongoCursor<Model_Location> GetAll () {
        return locations.FindAllAs<Model_Location> ();
    }
}