/*
 * Code by Eugene Boehringer
 * February-April 2019
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MostlyIrrelevant
{

    enum UnitClass
    {
        Soldier = 0, Archer = 1, HorsebackArcher = 2, MovableCannon = 3, Commander = 4, Healer = 5
    }

    enum Player
    {
        One = 0, Two = 1, AI = 2 
    }

    class Unit
    {
        private UnitManager um;

        private UnitClass uc;
        private Player player;
        private Vector2 position;
        private Random r;
        private Texture2D texture;

        private int baseHealth;
        private int health;
        private int baseDamage;

        private int maxMovement;   //Max units moved in one turn
        private int rightClickRange;
        private int fogOfWarRange;

        private int rightClickLimit; //Attack limit per turn (usually one)
        private int numRightClicks;

        private bool moved;
        private bool canAttack; //If the unit is a healer or not (healers cannot attack)

        public UnitClass UC
        {
            get
            {
                return uc;
            }
        }

        public Player Player
        {
            get
            {
                return player;
            }
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return texture;
            }
        }

        public int Health
        {
            get
            {
                return health;
            }
        }

        public int BaseHealth
        {
            get
            {
                return baseHealth;
            }
        }

        public int BaseDamage
        {
            get 
            {
                return baseDamage;
            }
        }

        public int MaxMovement
        {
            get
            {
                return maxMovement;
            }
        }

        public int RightClickRange
        {
            get
            {
                return rightClickRange;
            }
        }

        public int FogOfWarRange
        {
            get
            {
                return fogOfWarRange;
            }
        }


        public int RightClickLimit
        {
            get
            {
                return rightClickLimit;
            }
        }

        public int NumRightClicks
        {
            get
            {
                return numRightClicks;
            }
            set
            {
                numRightClicks = value;
            }
        }

        public bool Moved
        {
            get
            {
                return moved;
            }
            set
            {
                moved = value;
            }
        }

        public bool CanAttack
        {
            get
            {
                return canAttack;
            }
        }  

        public Unit(Vector2 position, UnitClass uc, UnitManager um, Player player)
        {
            this.um = um;
            this.uc = uc;
            this.r = um.random;
            this.player = player;
            this.position = position;
            moved = true;
            

            if (uc == UnitClass.Soldier)
            {
                if(player == Player.One)
                {
                    //Red texture
                    this.texture = um.unitTextures[0];
                }
                else
                {
                    //Blue texture
                    this.texture = um.unitTextures[1];
                }
                this.baseDamage = 3;
                this.baseHealth = 5;
                this.health = baseHealth;
                this.maxMovement = 2;
                this.rightClickRange = 1;
                this.rightClickLimit = 1;
                this.fogOfWarRange = 4;
                this.canAttack = true;
            }

            if (uc == UnitClass.Archer)
            {
                if (player == Player.One)
                {
                    //Red texture
                    this.texture = um.unitTextures[2];
                }
                else
                {
                    //Blue texture
                    this.texture = um.unitTextures[3];
                }
                this.baseDamage = 2;
                this.baseHealth = 3;
                this.health = baseHealth;
                this.maxMovement = 1;
                this.rightClickRange = 4;
                this.rightClickLimit = 1;
                this.fogOfWarRange = 2;
                this.canAttack = true;
            }

            if (uc == UnitClass.Healer)
            {
                if (player == Player.One)
                {
                    //Red texture
                    this.texture = um.unitTextures[4];
                }
                else
                {
                    //Blue texture
                    this.texture = um.unitTextures[5];
                }
                this.baseDamage = 0;
                this.baseHealth = 4;
                this.health = baseHealth;
                this.maxMovement = 2;
                this.rightClickRange = 2;
                this.rightClickLimit = 1;
                this.fogOfWarRange = 2;
                this.canAttack = false;
            }

            if (uc == UnitClass.HorsebackArcher)
            {
                //this.texture = texture;
                this.baseDamage = 40;
                this.baseHealth = 30;
                this.health = baseHealth;
                this.maxMovement = 1;
                this.rightClickRange = 6;
                this.rightClickLimit = 1;
                this.canAttack = true;
            }

            if (uc == UnitClass.MovableCannon)
            {
                //this.texture = texture;
                this.baseDamage = 50;
                this.baseHealth = 30;
                this.health = baseHealth;
                this.maxMovement = 1;
                this.rightClickRange = 12;
                this.rightClickLimit = 1;
                this.canAttack = true;
            }

            if (uc == UnitClass.Commander)
            {
                //this.texture = texture;
                this.baseDamage = 30;
                this.baseHealth = 90;
                this.health = baseHealth;
                this.maxMovement = 1;
                this.rightClickRange = 1;
                this.rightClickLimit = 1;
                this.canAttack = true;
            }

            numRightClicks = rightClickLimit;
        }

        public void Move(Vector2 position)
        {
            this.position = position;
        }

        /// <summary>
        /// Checks for
        /// </summary>
        /// <param name="u"></param>
        public void RightClick(Unit u)
        {
            if(this.uc == UnitClass.Healer)
            {
                this.HealUnit(u);
            }
            else
            {
                DealDamage(u);
            }    
        }

        /// <summary>
        /// Private method for healer that heals a unit
        /// </summary>
        /// <param name="u">The unit to be healed</param>
        private void HealUnit(Unit u)
        {
            if (3 + health > u.baseHealth)
                u.health = u.baseHealth;

            else
                u.health += 3;
        }

        /// <summary>
        /// Deals damage to a unit
        /// </summary>
        /// <param name="u">Unit being dealt the damage</param>
        private void DealDamage(Unit u)
        {
            u.TakeDamage(baseDamage);
        }

        /// <summary>
        /// The method for taking damage and checking state of unit subseqeuntly
        /// </summary>
        /// <param name="damageTaken">Integer representing combat damage</param>
        public void TakeDamage(int damageTaken)
        {
            if (damageTaken >= health)
            {
                health = 0;
                um.UnlinkUnit(this);
            }
            else
            {
                health -= damageTaken;
            }
        }

        public void Fortify()
        {
            //Commet needed, method used for taking buildings
            throw new NotImplementedException();
        }

        public Boolean isVisible()
        {
            if (um.tm.TileAt(position).Col == Color.DarkOliveGreen)
                return false;

            else
                return true;
        }
    }
}
