using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;
using UnityEngine.SceneManagement;

namespace WIDVE.Utilities
{
    public class Menu : MonoBehaviour
    {
        protected enum OpenOrClosed { Open, Closed }

        [SerializeField]
        GameObject _menuObject;
        public GameObject MenuObject => _menuObject;

        [SerializeField]
        bool _startClosed = false;
        bool StartClosed => _startClosed;

       
        private string _sceneName;
        private Scene _currScene;
        private string _prevScene;

        FiniteStateMachine _fsm;
        protected StateMachine SM => _fsm ?? (_fsm =  new FiniteStateMachine());

        public bool IsOpen => SM.CurrentState is States.Open;

        protected void SetState(OpenOrClosed ooc)
		{
            if(ooc == OpenOrClosed.Closed) Close();
            else if(ooc == OpenOrClosed.Open) Open();
		}

        public void Close()
		{
            SM.SetState(new States.Closed(this));
		}

        public void Open()
		{
            SM.SetState(new States.Open(this));
		}

		void Start()
		{
            if(StartClosed) Close();
            else Open();
		}

        void Update()
        {
            _currScene = SceneManager.GetActiveScene();
            _sceneName = _currScene.name;

            if (_sceneName != _prevScene)
            {
                Close();
                AudioListener.pause = false;

            }
        }

        void OnEnable()
        {
            _currScene = SceneManager.GetActiveScene();
            _prevScene = _currScene.name;
        }

        protected class States
		{
            public class MenuState : State<Menu>
			{
                public MenuState(Menu menu) : base(menu)
                {

                }
			}

            public class Closed : MenuState
            {
                public Closed(Menu menu) : base(menu)
				{

				}

                public override void Enter()
				{
                    Target.MenuObject.SetActive(false);
                }
			}

            public class Open : MenuState
            {
                public Open(Menu menu) : base(menu)
                {

                }

                public override void Enter()
				{
                    Target.MenuObject.SetActive(true);
				}
            }
		}
    }
}