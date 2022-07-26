using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
using WIDVE.Paths;
using PathCreation;

namespace VBTesting
{
    public class StationCreator : PrefabSpawner
    {
        static string TrackTag = "Track";

        public override Object Spawn()
        {
            GameObject spawned = base.Spawn() as GameObject;

            //try to place the new station on the current track
            if(spawned)
            {
                try
                {
                    GameObject track = GameObject.FindWithTag(TrackTag);
                    PathCreator trackPath = track.GetComponentInChildren<PathCreator>();
                    PathPosition pathPosition = spawned.GetComponentInChildren<PathPosition>();
                    pathPosition.SetPath(trackPath);
                    pathPosition.SetPosition(0);
                }
                catch(System.NullReferenceException)
                {
                    //in case the track or any other objects aren't present
                }
            }

            return spawned;
        }
    }
}