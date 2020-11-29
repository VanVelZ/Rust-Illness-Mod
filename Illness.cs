using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Illness", "ProZach", "1.0.0")]
    [Description("Get sick and die")]
    public class Illness : RustPlugin
    {
        public List<PlayerImmunity> onlineplayers = new List<PlayerImmunity>();
        void OnPlayerConnected(BasePlayer player)
        {
            onlineplayers.Add(new PlayerImmunity(player));
        }
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            onlineplayers.Remove(GetPlayerImmunity(player));
        }


        void OnItemUse(Item item, int amount)
        {

            var player = item.parent?.playerOwner;
            if (item.info.itemid == 1367190888 && player != null)GetPlayerImmunity(player).Diseases.Add(new Disease());
        }
        private object OnRunPlayerMetabolism(PlayerMetabolism metabolism, BasePlayer player, float delta)
        {
            GetPlayerImmunity(player).RunMetabolism();
            return null;
        }



        private PlayerImmunity GetPlayerImmunity(BasePlayer player)
        {
            return onlineplayers.Find(p => p.Player == player);
        }



        //these should get saved to /data in a json file
        public class PlayerImmunity {
            public BasePlayer Player { get; set; }
            public float InternalTemperature { get; set; } = 37;
            public List<Disease> Diseases { get; set; } = new List<Disease>();

            public PlayerImmunity(BasePlayer player)
            {
                Player = player;
            }


            public void RunMetabolism()
            {
                HandleTemperature();
                Diseases?.ForEach(d => d.RunMetabolism(this));
                if (Math.Round(Player._health) == 69) Diseases.Clear();
            }
            public void CureDisease(Disease disease)
            {
                disease.PlayerIsCured(this);
                Diseases.Remove(disease);
            }
            public void HandleTemperature()
            {
                //variables i need to find out
                float TEMPATFIREORFURNACE;
                float TEMPCLOTHEDINDESERT;
                float TEMPNAKEDINARCTIC;
                float TEMPJACKETINDESERT;
                float TEMPCLOTHEDARCTIC;
                float TEMPNAKEDONBEACH;
                float TEMPWETINARCTIC;

                //Internal Temperature changes based on external temoerature
                if (Player.metabolism.temperature.value < 20) InternalTemperature -= .02f;
                if (Player.metabolism.temperature.value < 0) InternalTemperature -= .05f;
                if (Player.metabolism.temperature.value > 45) InternalTemperature += .02f;
                if (Player.metabolism.temperature.value > 60) InternalTemperature += .05f;
                //Internal temperature tries to get back to normal
                if (InternalTemperature < 37) InternalTemperature += .01f;
                if (InternalTemperature > 37) InternalTemperature -= .01f;
                //Player gets sick
                if (InternalTemperature > 39) Diseases.Add(new Hypothermia());
                if (InternalTemperature < 35) Diseases.Add(new Hyperthermia());

            }
        }
        public class Disease
        {
            public string Name { get; set; } = "CornHealth";
            public float PlayerMaxHealth { get; set; } = 100;
            public float PlayerMaxCalories { get; set; } = 500;
            public float PlayerMaxHydration { get; set; } = 250;
            public float HydrationPenaltyPerTick { get; set; } = 0;
            public float CaloriesPenaltyPerTick { get; set; } = 0;
            public float VomitChance { get; set; } = 0;
            public float InfectionChance { get; set; } = 1;

            public virtual void TryToTransmit(PlayerImmunity playerImmunity)
            {
                var rng = new Random();
                float chance = rng.Next(0, 100) / 100;
                if (chance < InfectionChance) IsTransmitted(playerImmunity);
            }
            public virtual void IsTransmitted(PlayerImmunity playerImmunity)
            {
                playerImmunity.Player._maxHealth = PlayerMaxHealth;
                playerImmunity.Player.metabolism.hydration.max = PlayerMaxHydration;
                playerImmunity.Player.metabolism.calories.max = PlayerMaxCalories;
            }
            public virtual void PlayerIsCured(PlayerImmunity playerImmunity)
            {
                //what happens to the player when they are cured
                playerImmunity.Player._maxHealth = 100; //default
                playerImmunity.Player.metabolism.hydration.max = 250; //default
                playerImmunity.Player.metabolism.calories.max = 500; //default
            }
            public virtual void RunMetabolism(PlayerImmunity playerImmunity)
            {
                //what happens to player stats when they are sick
                playerImmunity.Player._health = new Random().Next(1, 100);
                if (Math.Round(playerImmunity.Player._health) == 69) playerImmunity.CureDisease(this);
            }

        }
        public class Hypothermia : Disease
        {

            public Hypothermia()
            {
                Name = "Hypothermia";
                PlayerMaxHealth = 75;
                HydrationPenaltyPerTick = .5f;
                CaloriesPenaltyPerTick = 1;
            }

            public override void RunMetabolism(PlayerImmunity playerImmunity)
            {
                base.RunMetabolism(playerImmunity);
            }
        }
        public class Hyperthermia : Disease
        {

            public Hyperthermia()
            {

            }
        }
    }
}