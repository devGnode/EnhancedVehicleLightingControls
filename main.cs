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

        /***
            IndicatorsManagment
        */
        private bool hasTurnedRight = false;
        private bool hasTurnedLeft = false;
        private const float TurnThreshold = 15.0f;
        private const float ResetThreshold = 5.0f;
        /***/

        /***
            QuickIndicatorManagment
        */
        private long indicatorSecondEllapsedTime = 5;
        private long currentTimeStamp = -1;

        private const int ANY_DIRECTION     = 0x00;
        private const int RIGHT_DIRECTION   = 0x01;
        private const int LEFT_DIRECTION    = 0x02;
        /***/

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

            if(ObjectIsNull(GetPlayer())) return;

            if (firstTime){

                string modName      = "Enhanced Vehicle Lighting Controls";
                string version      = "Release v1.0.0";
                string developer    = "MccDev260";
                Notification.PostMessageText($"{version} loaded !", new TextureAsset("CHAR_YOUTUBE", "CHAR_YOUTUBE"), false, FeedTextIcon.Message, developer, modName);
                firstTime = false;
            }

            if (Game.LastInputMethod == InputMethod.GamePad)GamePad();

            /**PROC*/
            IndicatorsManagment();
            QuickIndicatorManagment();
        }


        private void OnAborted(object sender, EventArgs e){
            this.Tick       -= OnTick;
            this.KeyDown    -= OnKeyDown;
            this.KeyUp      -= OnKeyUp;
        }

        #region Proc
        private void IndicatorsManagment(){

            if(!HasInVehicle()||!HasIndicatorOn()||IsQuickIndicator()) return;
    
                
            if(HasRightIndicatorOn()){

                    if(GetVehicle().SteeringAngle < -TurnThreshold ) hasTurnedRight = true;
                    else if(hasTurnedRight && Math.Abs(GetVehicle().SteeringAngle) < ResetThreshold){
                        ToggleRightIndicator();
                        hasTurnedRight = false;
                    }

            }else{
                hasTurnedRight = false;
            }
            if(HasLeftIndicatorOn()){

                if(GetVehicle().SteeringAngle > TurnThreshold ) hasTurnedLeft = true;
                else if(hasTurnedLeft && Math.Abs(GetVehicle().SteeringAngle) < ResetThreshold){
                    ToggleLeftIndicator();
                    hasTurnedLeft = false;
                }

            }else{
                hasTurnedLeft = false;
            }


        }

        private void QuickIndicatorManagment(){

            if(!HasInVehicle()||!HasIndicatorOn()||currentTimeStamp<0) return;
            if(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - currentTimeStamp >= indicatorSecondEllapsedTime ){
                QuickIndicator( HasRightIndicatorOn()? RIGHT_DIRECTION : LEFT_DIRECTION,false);
            }    

        }

        #endregion

        #region Input
        private void OnKeyDown(object sender, KeyEventArgs e)
        {   
            if (GetPlayer().IsInVehicle()){
              
                /*Toggle*/
                if(kbKeyActions.TryGetValue(e.KeyCode, out var action)) action();
                /*Others KbAction*/
                if(e.KeyCode==siren) ActiveSoundOfSiren(true);

                if(e.KeyCode==Keys.NumPad6) QuickIndicator(RIGHT_DIRECTION,true);
                if(e.KeyCode==Keys.NumPad5) QuickIndicator(ANY_DIRECTION,false);
                if(e.KeyCode==Keys.NumPad4) QuickIndicator(LEFT_DIRECTION,true);

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

                if(Game.IsControlJustPressed(GTA.Control.VehicleBrake)) BreakLights(true);
                else if(Game.IsControlJustReleased(GTA.Control.VehicleBrake)) BreakLights(false);
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
            ActiveSoundOfSiren(state);
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

        private bool HasIndicatorOn(){ return HasRightIndicatorOn()||HasLeftIndicatorOn(); }

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

        private bool IsQuickIndicator(){ return currentTimeStamp>0; }

        private void QuickIndicator(int direction, bool state){

            if(!HasInVehicle()||HasHazards()) return;
            if (IsQuickIndicator()&&state || direction == ANY_DIRECTION) SetIndicators(false,false);

            if (direction == RIGHT_DIRECTION) SetIndicators(false,state);
            if (direction == LEFT_DIRECTION) SetIndicators(state,false);
            currentTimeStamp = state ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : -1;
        }


        private void BreakLights(bool state){
            if(!HasInVehicle()||GetVehicle().IsSirenActive) return;
            GetVehicle().AreBrakeLightsOn = state;
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
