using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WIDVE.Utilities;
using WIDVE.Patterns;

namespace VBTesting
{
    [ExecuteAlways]
    public class VBSystemLoader : MonoBehaviour
    {
        public enum Modes { Exclusive, Multiple } //Multiple not supported yet

        [SerializeField]
        Modes _mode = Modes.Exclusive;
        public Modes Mode => _mode;

        [SerializeField]
        CommandHistory SystemLoadHistory;

        public VBSystem CurrentSystem => Player ? Player.CurrentSystem : null;

        Player _player;
        Player Player
		{
			get
			{
				if(!_player)
				{
                    GameObject player_go = GameObject.FindGameObjectWithTag("Player");
                    if(player_go) _player = player_go.GetComponentInChildren<Player>();
				}
                return _player;
			}
		}

        public void LoadSystem(VBSystem system)
		{
            if(system) LoadSystem(system.SceneObject);
		}

        public void LoadSystem(SceneObject system)
		{
			switch(Mode)
			{
                default:
                case Modes.Exclusive:
                    //first, unload all active systems
                    VBSystem.UnloadAllSystems();

                    //might need to wait a frame afterward?

                    //then, load the new system
                    ICommand c = system.Load(LoadSceneMode.Additive);
                    if(SystemLoadHistory) SystemLoadHistory.Record(c);
                    break;

                case Modes.Multiple:
                    //in this mode, multiple systems can be loaded at once
                    //need to do a lot of work to make this possible however
                    break;
			}

            //need to reset the player trigger position too
            if(Player) Player.PlayerTrigger.ReturnToStart();
		}

        void OnDisable()
		{
#if UNITY_EDITOR
            //unload any scenes that were loaded during playmode
            if(SystemLoadHistory) SystemLoadHistory.UndoAll();
#endif
        }
    }
}