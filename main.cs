using System;
using System.Windows.Forms;
using System.Collections.Generic;

using GTA;
using GTA.UI;
using GTA.Graphics;

namespace EnhancedVehicleLightingControls
{
    public class Main : Script
    {
        bool firstTime = true;
   
        Keys sirenToggleKey, beamToggleKey, interiorLightToggleKey, leftIndicatorKey, rightIndicatorKey, hazardsKey, siren;
        GTA.Control sirenToggleButton, beamToggleButton, interiorLightToggleButton, leftIndicatorButton, rightIndicatorButton, hazardsButton, modifierButton;

        private Dictionary<Keys,Action> kbKeyActions;

        public Main()
        {
            this.Tick       += OnTick;
            this.KeyDown    += OnKeyDown;
            this.KeyUp      += OnKeyUp;

            ScriptSettings config = ScriptSettings.Load("scripts\\EVLC_Settings.ini");

            #region Keys
            sirenToggleKey          = config.GetValue<Keys>("Emergency Vehicles", "Siren_Toggle_Key", Keys.Tab);
            siren                   = config.GetValue<Keys>("Emergency Vehicles", "Siren_hold_Key", Keys.J);
            beamToggleKey           = config.GetValue<Keys>("Headlights", "Beam_Toggle_Key", Keys.CapsLock);
            interiorLightToggleKey  = config.GetValue<Keys>("Interior", "Interior_Light_Toggle_Key", Keys.I);
            leftIndicatorKey        = config.GetValue<Keys>("Indicators", "Left_Indicator_key", Keys.Left);
            rightIndicatorKey       = config.GetValue<Keys>("Indicators", "Right_Indicator_Key", Keys.Right);
            hazardsKey              = config.GetValue<Keys>("Indicators", "Hazard_Lights_Key", Keys.Down);

            kbKeyActions = new Dictionary<Keys, Action>{
              {rightIndicatorKey, ToggleRightIndicator},
              {leftIndicatorKey,ToggleLeftIndicator},
              {hazardsKey, ToggleHazards},
              {interiorLightToggleKey, ToggleInteriorLights},
              {beamToggleKey, ToggleFullBeams},
              {sirenToggleKey, ToggleSiren}
            };
            #endregion

            #region Buttons
            sirenToggleButton           = config.GetValue<GTA.Control>("Emergency Vehicles", "Siren_Toggle_Button", GTA.Control.ScriptPadDown);
            beamToggleButton            = config.GetValue<GTA.Control>("Headlights", "Beam_Toggle_Button", GTA.Control.ScriptRLeft);
            leftIndicatorButton         = config.GetValue<GTA.Control>("Indicators", "Left_Indicator_Button", GTA.Control.ScriptPadLeft);
            rightIndicatorButton        = config.GetValue<GTA.Control>("Indicators", "Right_Indicator_Button", GTA.Control.ScriptPadRight);
            hazardsButton               = config.GetValue<GTA.Control>("Indicators", "Hazard_Lights_Button", GTA.Control.ScriptPadUp);
            modifierButton              = config.GetValue<GTA.Control>("Mod Settings", "Modifier_Button", GTA.Control.ScriptLB);
            interiorLightToggleButton   = config.GetValue<GTA.Control>("Interior", "Interior_Light_Toggle_Button", GTA.Control.ScriptRUp);
            #endregion
        }

        private void OnTick(object sender, EventArgs e)
        {

            if (firstTime)
            {
                string modName      = "Enhanced Vehicle Lighting Controls";
                string version      = "Release v1.0.0";
                string developer    = "MccDev260";
                Notification.PostMessageText($"{version} loaded !", new TextureAsset("CHAR_YOUTUBE", "CHAR_YOUTUBE"), false, FeedTextIcon.Message, developer, modName);
                firstTime = false;
            }

            if (Game.LastInputMethod == InputMethod.GamePad)GamePad();
        }

        #region Input
        private void OnKeyDown(object sender, KeyEventArgs e)
        {   
            if (GetPlayer().IsInVehicle()){
              
                /*Toggle*/
                if(kbKeyActions.TryGetValue(e.KeyCode, out var action)) action();
                /*Others KbAction*/
                if(e.KeyCode==siren) ActiveSoundOfSiren(true);

            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e){

            if (GetPlayer().IsInVehicle()){
                if(e.KeyCode==siren) ActiveSoundOfSiren(false);
            }

        }
        
        private void GamePad()
        {
            if (Game.IsControlPressed(modifierButton) && HasInVehicle())
            {
        
                // Disable all player controls except for some driving functions.
                Game.DisableAllControlsThisFrame();
                Game.EnableControlThisFrame(GTA.Control.VehicleAccelerate);
                Game.EnableControlThisFrame(GTA.Control.VehicleBrake);
                Game.EnableControlThisFrame(GTA.Control.VehicleHorn);
                Game.EnableControlThisFrame(GTA.Control.VehicleLookBehind);

                if (Game.IsControlJustReleased(sirenToggleButton))
                    ToggleSiren();

                if (Game.IsControlJustReleased(beamToggleButton))
                    ToggleFullBeams();

                if (Game.IsControlJustPressed(interiorLightToggleButton))
                    ToggleInteriorLights();

                if (Game.IsControlJustPressed(leftIndicatorButton))
                    ToggleLeftIndicator();

                if (Game.IsControlJustPressed(rightIndicatorButton))
                    ToggleRightIndicator();

                if (Game.IsControlJustPressed(hazardsButton))
                    ToggleHazards();

            }else if (Game.IsControlJustReleased(modifierButton)){
                Game.EnableAllControlsThisFrame();
            }else{
                if(Game.IsControlJustPressed(GTA.Control.ScriptRDown)) ActiveSoundOfSiren(true);
                else if(Game.IsControlJustReleased(GTA.Control.ScriptRDown))ActiveSoundOfSiren(false);
            }
        }
        #endregion

        private void ToggleSiren(){
            if(!HasInVehicle()) return;
            ActiveSoundOfSiren(GetVehicle().IsSirenSilent);
        }

        private void ActiveSoundOfSiren(bool sirenState){
            if(!HasInVehicle()) return;
            if(GetVehicle().HasSiren){
                if(!GetVehicle().IsSirenActive&&sirenState) GetVehicle().IsSirenActive = true;
                if(GetVehicle().IsSirenSilent == !sirenState ) return;
                GetVehicle().IsSirenSilent = !sirenState;
            }
        }

        public void HoldSiren(bool state){
            if(!HasInVehicle()) return;
            if(!GetVehicle().IsSirenActive&&state){
                if(!HasHazards()) ToggleHazards();
                GetVehicle().IsSirenActive = true;
            }
        }
        
        private void ToggleFullBeams(){
            if(!HasInVehicle()) return;
            if (GetVehicle().AreLightsOn)GetVehicle().AreHighBeamsOn = !GetVehicle().AreHighBeamsOn;
        }

        private void ToggleInteriorLights(){
            if(!HasInVehicle()) return;
            GetVehicle().IsInteriorLightOn = !GetVehicle().IsInteriorLightOn;
        }

        #region Indicators

        private bool HasHazards(){return HasLeftIndicatorOn() && HasRightIndicatorOn();}

        private bool HasLeftIndicatorOn(){return GetVehicle().IsLeftIndicatorLightOn;}

        private bool HasRightIndicatorOn(){return GetVehicle().IsRightIndicatorLightOn;}


        private void ToggleHazards(){
            bool state = !HasHazards();
            SetIndicators(state, state);
        }

        private void ToggleRightIndicator(){
            if (HasHazards()) return;
            SetIndicators(false, !HasRightIndicatorOn());
        }

        private void ToggleLeftIndicator(){
            if (HasHazards()) return;
            SetIndicators(!HasLeftIndicatorOn(), false);
        }

        private void SetIndicators(bool leftIndicator = false, bool rightIndicator = false){
            if(!HasInVehicle()) return;
            Vehicle vehicle = GetVehicle();
            vehicle.IsLeftIndicatorLightOn  = leftIndicator;
            vehicle.IsRightIndicatorLightOn = rightIndicator;
        }
        #endregion

        /***/
        #region Helpers
        private Ped GetPlayer(){
            Ped currentPlayer = Game.Player.Character;

            if(currentPlayer!=null && currentPlayer.Exists()) return currentPlayer; 
            return null;
        }

        private Vehicle GetVehicle(Ped player){
            if(!ObjectIsNull(player)&&!ObjectIsNull(player.CurrentVehicle)&&player.CurrentVehicle.Exists()) return player.CurrentVehicle;
            return null;
        }

        private Vehicle GetVehicle(){ return GetVehicle(GetPlayer());}

        private bool HasInVehicle(){ return !ObjectIsNull(GetVehicle(GetPlayer())); }

        private bool ObjectIsNull(object o){ return o==null; }
        /***/
        #endregion

    }
}
