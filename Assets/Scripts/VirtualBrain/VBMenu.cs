using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;
using WIDVE.Utilities;

namespace VBTesting
{
    public class VBMenu : MonoBehaviour
    {
        #region SERIALIZED FIELDS

        [SerializeField]
        VirtualBrain VirtualBrain;

        [SerializeField]
        VBSystemLoader SystemLoader;

		[SerializeField]
        GameObject MenuParent;

        [SerializeField]
        ButtonFloat MenuButton;

        [SerializeField]
        ButtonFloat OpenMenuButton;

        [SerializeField]
        ButtonFloat CloseMenuButton;

        [SerializeField]
        LessonSettings CurrentSettings;

        [SerializeField]
        LessonSettings DefaultSettings;

        [SerializeField]
        CursorSettings CursorSettings;

        [SerializeField]
        WindowSettings WindowSettings;

        [SerializeField]
        bool ActiveAtStart = false;

        [SerializeField]
        SceneObject VisualSystemObject;

        [SerializeField]
        SceneObject AuditorySystemObject;

        [SerializeField]
        SceneObject SomatosensorySystemObject;

        [Header("Buttons")]

        [SerializeField]
        VBButton StopAtStationsYes;

        [SerializeField]
        VBButton StopAtStationsNo;

        [SerializeField]
        VBButton ReplayStationsYes;

        [SerializeField]
        VBButton ReplayStationsNo;

        [SerializeField]
        VBButton LessonVisual;

        [SerializeField]
        VBButton LessonAuditory;

        [SerializeField]
        VBButton LessonSomatosensory;

        [Header("Panels")]

        [SerializeField]
        GameObject InstructionsPanel;

        [SerializeField]
        GameObject HomePanel;

        [SerializeField]
        GameObject LessonsPanel;

        [SerializeField]
        GameObject ControlsPanel;

        [SerializeField]
        GameObject SettingsPanel;

        [SerializeField]
        GameObject CreditsPanel;

        [SerializeField]
        VBButton HomePanelButton;

        [SerializeField]
        VBButton InstructionsPanelButton;

        [SerializeField]
        VBButton ControlsPanelButton;

        [SerializeField]
        VBButton SettingsPanelButton;

        [SerializeField]
        VBButton CreditsPanelButton;
	
		[SerializeField]
        VBButton LessonsPanelButton;

		[SerializeField]
		bool _isPC;
		
        //[SerializeField]
        // GameObject OpenMenuText; 7-12-21 commented out because there is no open menu text to reference (LC)

        //[SerializeField]
        // GameObject CloseMenuText; 7-12-21 commented out because there is no close menu text to reference (LC)

        #endregion

        #region STATES

        //Menu states
        FiniteStateMachine _menuSM;
        StateMachine MenuSM => _menuSM ?? (_menuSM = new FiniteStateMachine(MenuOpen));

        MenuStates.Open _menuOpen;
        MenuStates.Open MenuOpen => _menuOpen ?? (_menuOpen = new MenuStates.Open(this));

        MenuStates.Closed _menuClosed;
        MenuStates.Closed MenuClosed => _menuClosed ?? (_menuClosed = new MenuStates.Closed(this));

        //Panel states
        FiniteStateMachine _panelSM;
        StateMachine PanelSM => _panelSM ?? (_panelSM = new FiniteStateMachine(NoPanel));

        PanelStates.None _noPanel;
        PanelStates.None NoPanel => _noPanel ?? (_noPanel = new PanelStates.None(this, null, null));

		#endregion

		#region PROPERTIES AND FIELDS

		public bool IsActive => MenuSM.CurrentState is MenuStates.Open;

        GameObject MainPanel
		{
			get
			{
                if(VirtualBrain.InLesson) return HomePanel;
                else return LessonsPanel; // 7-10-21 Changing from InstructionsPanel to Lessons panel for new home VR Menu (LC)
			}
		}

        VBButton MainPanelButton
		{
			get
			{
                if(VirtualBrain.InLesson) return HomePanelButton;
                else return LessonsPanelButton; // 7-10-21 Changing from InstructionsPanel to Lessons panel for new home VR Menu (LC)
            }
		}

        #endregion

        #region STATE METHODS

        public void Open()
		{
            MenuSM.SetState(MenuOpen);
		}

        public void Close()
		{
            MenuSM.SetState(MenuClosed);
        }

        public void Toggle()
		{
            if(IsActive) Close();
            else Open();
		}

        public void RestartCurrentLesson()
		{
            Close();
            SystemLoader.LoadSystem(SystemLoader.CurrentSystem);
		}

        public void RestartVirtualBrain()
		{
            Close();
            SystemLoader.LoadSystem(VirtualBrain.DefaultSystem);

            //wait for scenes to load before opening the menu
            StopAllCoroutines();
            StartCoroutine(OpenAfterWait(2));
		}

        #endregion

        #region PANELS

        public void ShowPanel(GameObject panel, VBButton panelButton)
		{
            PanelSM.SetState(new PanelStates.Generic(this, panel, panelButton));
		}

        public void ShowWelcomePanel()
		{
            ShowPanel(InstructionsPanel, InstructionsPanelButton);
		}

        public void ShowMainPanel()
		{
            ShowPanel(MainPanel, MainPanelButton);
		}

        public void ShowLessonsPanel()
        {
            ShowPanel(LessonsPanel, LessonsPanelButton); // changed from null to LessonsPanelButton 7-12-21 to include a lessons button on new VR Menu (LC)
        }

        public void ShowControlsPanel()
		{
            ShowPanel(ControlsPanel, ControlsPanelButton);
		}

        public void ShowSettingsPanel()
        {
            ShowPanel(SettingsPanel, SettingsPanelButton);
        }

        public void ShowCreditsPanel()
		{
            ShowPanel(CreditsPanel, CreditsPanelButton);
		}

		#endregion

		#region SETTINGS

		public void SetDefaultSettings()
		{
            CurrentSettings.Clone(DefaultSettings);
		}

        public void SetDefaultPanels()
		{
            HomePanel.SetActive(false);
            SettingsPanel.SetActive(false);
            ControlsPanel.SetActive(false);
            InstructionsPanel.SetActive(false);
		}

        public void SetButtonOutlines()
		{
            //set these based on current settings
            StopAtStationsYes.SetSelected(CurrentSettings.StopAtStations);
            StopAtStationsNo.SetSelected(!CurrentSettings.StopAtStations);

            ReplayStationsYes.SetSelected(CurrentSettings.StationMode == LessonSettings.StationModes.Replayable);
            ReplayStationsNo.SetSelected(CurrentSettings.StationMode != LessonSettings.StationModes.Replayable);

            //set lesson buttons based on current lesson
            Player player = VirtualBrain.Player;
            VBSystem currentSystem = player != null ? VirtualBrain.Player.CurrentSystem : null;
            if(currentSystem != null)
            {
                //outline the current system
                LessonVisual.SetSelected(currentSystem.SceneObject == VisualSystemObject);
                LessonAuditory.SetSelected(currentSystem.SceneObject == AuditorySystemObject);
                LessonSomatosensory.SetSelected(currentSystem.SceneObject == SomatosensorySystemObject);
            }
            else
            {
                //no current system
                LessonVisual.SetSelected(false);
                LessonAuditory.SetSelected(false);
                LessonSomatosensory.SetSelected(false);
            }
        }

        //methods for use with Unity events:

        public void SetStationsLocked(bool locked)
		{
            CurrentSettings.StopAtStations = locked;

            SetButtonOutlines();
        }

        public void SetStationsReplayable(bool replayable)
		{
            CurrentSettings.StationMode = replayable ? LessonSettings.StationModes.Replayable : LessonSettings.StationModes.NotReplayable;

            SetButtonOutlines();
        }

        public void SetClosedCaptionsMode(int mode)
		{
            CurrentSettings.ClosedCaptionsMode = (LessonSettings.ClosedCaptionsModes)mode;

            SetButtonOutlines();
        }

        public void SetClosedCaptionsDisplayMode(int mode)
		{
            CurrentSettings.ClosedCaptionsDisplayMode = (LessonSettings.ClosedCaptionsDisplayModes)mode;

            SetButtonOutlines();
        }

        #endregion

        IEnumerator CloseAfterWait(int frames)
		{
            int framesWaited = 0;
            while(framesWaited < frames)
            {
                yield return 0; //waits for one frame
                framesWaited++;
            }

            Close();
		}

        IEnumerator OpenAfterWait(int frames)
        {
            int framesWaited = 0;
            while(framesWaited < frames)
            {
                yield return 0; //waits for one frame
                framesWaited++;
            }

            Open();
        }

        void Start()
		{
            //initialized everything with the default settings
            SetDefaultSettings();
            //SetDefaultPanels();
            SetButtonOutlines();

            //close the menu if it was left open in editor...
            if(IsActive && !ActiveAtStart) Close();
            //else if(!IsActive && ActiveAtStart) Open();
			if(_isPC)
			{
				ShowLessonsPanel();
			}
            //RestartVirtualBrain();
		}

		void Update()
		{
			if(MenuButton && MenuButton.GetUp())
			{
                Toggle();
			}

            if(OpenMenuButton && OpenMenuButton.GetUp())
			{
                Open();
			}

            if(CloseMenuButton && CloseMenuButton.GetUp())
            {
                Close();
            }
        }

		#region STATE CLASSES

		public class MenuStates
		{
            public class Open : State<VBMenu>
            {
                bool i_ShowCursor;
                CursorLockMode i_Constraints;

                public Open(VBMenu menu) : base(menu) { }

				public override void Enter()
				{
                    //show menu
                    Target.MenuParent.SetActive(true);
                    Target.ShowMainPanel();
					if(Target._isPC)
					{
						 Target.LessonsPanel.SetActive(true);
						//ShowLessonsPanel();
					}
					//Target.ShowLessonsPanel();
                    // Target.OpenMenuText.SetActive(false); 7-12-21 commented out because there is no open menu text to reference (LC)
                    // Target.CloseMenuText.SetActive(true); 7-12-21 commented out because there is no close menu text to reference (LC)

                    //only show home button if in a lesson
					if(Target.HomePanelButton != null)
					{
						Target.HomePanelButton.gameObject.SetActive(Target.VirtualBrain.InLesson);
					}

                    //update button outlines
                    Target.SetButtonOutlines();

                    //show and unlock cursor
                    i_ShowCursor = Target.CursorSettings.ShowCursor;
                    i_Constraints = Target.CursorSettings.Constraints;
                    Target.CursorSettings.ShowCursor = true;
                    Target.CursorSettings.Constraints = CursorLockMode.None;

                    //freeze player
                    Target.CurrentSettings.FreezePlayer(true);

                    //notify
                    Target.CurrentSettings.NotifyMenu(true);
					
					//UnityEngine.Time.timeScale = 0;
					//Physics.autoSimulation = false;
					if(Target._isPC)
					{
						AudioListener.pause = true;
						Player player = Target.VirtualBrain.Player;
						if(player != null)
						{
							if(player.PlayingStation != null)
							{
								if(player.PlayingStation.CurrentTimeline != null)
								{
									player.PlayingStation.CurrentTimeline.Pause();
								}
							}
						}
					}
				}

				public override void Exit()
				{
                    //restore cursor settings
                    Target.CursorSettings.ShowCursor = i_ShowCursor;
                    Target.CursorSettings.Constraints = i_Constraints;

                    //unfreeze player
                    Target.CurrentSettings.FreezePlayer(false);

                    //hide menu
                    Target.MenuParent.SetActive(false);
                    // Target.OpenMenuText.SetActive(true); 7-12-21 commented out because there is no open menu text to reference (LC)
                    // Target.CloseMenuText.SetActive(false); 7-12-21 commented out because there is no open menu text to reference (LC)

                    //notify
                    Target.CurrentSettings.NotifyMenu(false);
					
					if(Target._isPC)
					{
						//UnityEngine.Time.timeScale = 1;
						AudioListener.pause = false;
						//Physics.autoSimulation = true;
						Player player = Target.VirtualBrain.Player;
						if(player != null)
						{
							if(player.PlayingStation != null)
							{
								if(player.PlayingStation.CurrentTimeline != null)
								{
									player.PlayingStation.CurrentTimeline.Resume();
								}
							}
						}
					}
                }
			}

            public class Closed : State<VBMenu>
            {
                public Closed(VBMenu menu) : base(menu) { }
			}
        }

        public class PanelStates
		{
            public abstract class PanelState : State<VBMenu>
			{
                public readonly GameObject Panel;
                public readonly VBButton PanelButton;

                public PanelState(VBMenu menu, GameObject panel, VBButton panelButton) : base(menu)
				{
                    Panel = panel;
                    PanelButton = panelButton;
                }

                public override void Enter()
                {
                    if(Panel) Panel.SetActive(true);
                    if(PanelButton) PanelButton.SetSelected(true); // abbreviated null checks
                }

				public override void Exit()
				{
                    if(Panel) Panel.SetActive(false);
                    if(PanelButton) PanelButton.SetSelected(false);
				}
			}

            public class None : PanelState
            {
                public None(VBMenu menu, GameObject panel, VBButton panelButton) : base(menu, panel, panelButton) { }
			}

            public class Generic : PanelState
			{
                public Generic(VBMenu menu, GameObject panel, VBButton panelButton):base(menu, panel, panelButton) { }
			}
        }

		#endregion
	}
}