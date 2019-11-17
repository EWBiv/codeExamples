/* Eugene Boehringer
 * NOTE:
 * -Missing code (around 400 lines in this class) is Partner's code
 * -AI was worked on but scrapped due to time
 * -Two other partners helped with small chunks of this remaining code, large majority of 
 * what is left is my own (Eugene Boehringer)
 * -Timeline of whole project was February-April 2019. Many more classes were included and some had
 * to be cut due to time constraint such as AI and A*. 
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
    class UnitManager
    {
        AI ai;

        public List<Unit> playerOneUnits;
        public List<Unit> playerTwoUnits;
        public List<Unit> aiUnits;

        List<Unit> currentUnits;
        List<Unit> enemyUnits;
        
        public bool twoPlayer; //Boolean regarding if it is a two player game or 1 vs. an AI

        public bool playerOneTurn; //Keeps track of turns in two player game
        bool aiTurn;    //Keeps track of AI turn (if playing with AI)

        SpriteBatch sb;

        MouseState msNew;
        MouseState msOld;

        public Random random;
        public TileManager tm;
        public FogOfWar fow;
        public List<Texture2D> unitTextures;
        public SpriteFont sf;

        // Setup for the movement.
        bool isMoving = false;
        Unit movingUnit;
        MouseState oldState = Mouse.GetState();
        MouseState newState = Mouse.GetState();

        // For rightClicking
        bool isRightClicking = false;
        Unit rightClickingUnit;

        public UnitManager(SpriteBatch sb, Random r, TileManager tm, bool twoPlayer, SpriteFont sf)
        {
            this.sb = sb;
            this.random = r;
            this.tm = tm;
            msNew = Mouse.GetState();
            this.sf = sf;

            playerOneUnits = new List<Unit>();
            playerTwoUnits = new List<Unit>();
            aiUnits = new List<Unit>();
            currentUnits = new List<Unit>();
            enemyUnits = new List<Unit>();

            unitTextures = new List<Texture2D>();
            this.twoPlayer = twoPlayer;

            //AI NOT IMPLEMENTED IN FINAL BUILD
            //If one player, make an instance of AI
            if (!twoPlayer)
            {
                ai = new AI(this, tm);
            }

            aiTurn = false;
            playerOneTurn = true;
        }

        /// <summary>
        /// used for units to access the textures they have
        /// </summary>
        /// <param name="image"></param>
        public void LoadTexture(Texture2D image)
        {
            unitTextures.Add(image);
        }

        public void LinkFogOfWar(FogOfWar fow)
        {
            this.fow = fow;
        }

        /// <summary>
        /// Puts unit in one of the lists depending on who controls it
        /// </summary>
        /// <param name="u">Unit to be added</param>
        public void LinkUnit(Unit u)
        {
            if (u.Player == Player.One)
            {
                playerOneUnits.Add(u);

                //Update the screen
                if (playerOneTurn)
                    currentUnits = playerOneUnits;

                else
                    enemyUnits = playerOneUnits;
            }

            else if (u.Player == Player.Two)
            {
                playerTwoUnits.Add(u);

                //Update the screen
                if (playerOneTurn)
                    enemyUnits = playerTwoUnits;

                else
                    currentUnits = playerTwoUnits;
            }

            else
            {
                aiUnits.Add(u);

                //Update the screen
                if (aiTurn == true)
                {
                    currentUnits = aiUnits;
                }
                else
                {
                    enemyUnits = aiUnits;
                }
            }
        }

        /// <summary>
        /// Removes unit from list to stop drawing and updating it
        /// </summary>
        /// <param name="u">Unit to be removed</param>
        public void UnlinkUnit(Unit u)
        {
            if (u.Player == Player.One)
            {
                playerOneUnits.Remove(u);

                //Update the screen
                if (playerOneTurn)
                    currentUnits = playerOneUnits;

                else
                    enemyUnits = playerOneUnits;
            }
            else if (u.Player == Player.Two)
            {
                playerTwoUnits.Remove(u);

                //Update the screen
                if (playerOneTurn)
                    enemyUnits = playerTwoUnits;

                else
                    currentUnits = playerTwoUnits;
            }
            else
            {
                aiUnits.Remove(u);
                if (aiTurn == true)
                    currentUnits = aiUnits;

                else
                    enemyUnits = aiUnits;
            }
        }

        /// <summary>
        /// Draws all units to the screen
        /// </summary>
        public void Draw()
        {
            for (int i = 0; i < currentUnits.Count; i++)
            {

                sb.Draw(currentUnits[i].Texture, tm.PointToPixel(currentUnits[i].Position) - tm.Offset, Color.White);
            }

            for (int i = 0; i < enemyUnits.Count; i++)
            {
                if (fow.UnitInFog(enemyUnits[i]) == false)
                    sb.Draw(enemyUnits[i].Texture, tm.PointToPixel(enemyUnits[i].Position) - tm.Offset, Color.White);
            }
            //Used for debugging
            //sb.DrawString(sf,Debug(),new Vector2(500,500),Color.White);
        }

        /// <summary>
        /// Updates the state of units and allows to move and attack
        /// </summary>
        public void Update(bool turnOver)
        {
            fow.Update(); //Updates fog of war


            //If its a two player game
            if (turnOver == true && twoPlayer == true)
            {
                //If it was playerOnesTurn
                if (playerOneTurn)
                {
                    ResetUnits(currentUnits);
                    //Updating the actual lists
                    playerOneUnits = currentUnits;
                    playerTwoUnits = enemyUnits;

                    //Changing the current lists
                    currentUnits = playerTwoUnits;
                    enemyUnits = playerOneUnits;


                    //Building capture and 'cash' implemented by project partners
                    BlueCaptureBuildings();
                    //CaptureBuildings(Player.Two);
                    ResetRedBuildings();
                    ResetBlueBuildings();
                    ResetNeutralBuildings();

                    // Give the blue player gold.
                    tm.GiveCash("blue");

                    playerOneTurn = false;
                }
                else
                {

                    ResetUnits(currentUnits);
                    //Updating the actual lists
                    playerTwoUnits = currentUnits;
                    playerOneUnits = enemyUnits;

                    //Changing the current lists
                    currentUnits = playerOneUnits;
                    enemyUnits = playerTwoUnits;

                    RedCaptureBuildings();
                    ResetRedBuildings();
                    ResetBlueBuildings();
                    ResetNeutralBuildings();

                    // Give the red player gold.
                    tm.GiveCash("red");

                    playerOneTurn = true;
                }
            }
            //If its the AI turn
            else if (turnOver == true)
            {
                ResetUnits(currentUnits);
                //Updating the actual lists
                playerOneUnits = currentUnits;
                aiUnits = enemyUnits;

                //Changing the current lists
                currentUnits = aiUnits;
                enemyUnits = playerOneUnits;

                CaptureBuildings(Player.AI);

                // Give the red player gold.
                tm.GiveCash("red");

                aiTurn = true;
            }

            //Thanks to Chris Hambacher (Project partner) for helping me with fixing this portion in February!
            if (aiTurn == false)
            {
                msNew = Mouse.GetState();
                for (int i = 0; i < currentUnits.Count; i++)
                {
                    if (msOld.LeftButton == ButtonState.Released && msNew.LeftButton == ButtonState.Pressed)
                    {
                        if (isMoving == false && isRightClicking == false && currentUnits[i].Moved == false)
                        {
                            //New vector to check if mouse is over the vector of the unit
                            if (tm.PixelToPoint(msNew.Position) == currentUnits[i].Position)
                            {
                                // Change the moving state to true, set a pointer to the unit that is moving.
                                isMoving = true;
                                movingUnit = currentUnits[i];
                            }
                        }
                    }

                    if (msOld.RightButton == ButtonState.Released && msNew.RightButton == ButtonState.Pressed)
                    {
                        if (isRightClicking == false && isMoving == false && currentUnits[i].NumRightClicks < currentUnits[i].RightClickLimit)
                        {
                            //New vector to check if mouse is over the vector of the unit
                            if (tm.PixelToPoint(msNew.Position) == currentUnits[i].Position)
                            {
                                // Change the moving state to true, set a pointer to the unit that is moving.
                                isRightClicking = true;
                                rightClickingUnit = currentUnits[i];
                            }
                        }
                    }
                }
                msOld = msNew;

                // If  a unit has been left-clicked.
                if (isMoving == true)
                {
                    newState = Mouse.GetState();

                    HighLightSquares(movingUnit.MaxMovement, movingUnit.Position, Color.DarkMagenta, false, false, false);

                    // If the mouse is clicked again, and the tile exists and is highlighted
                    if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                    {
                        Vector2 vect = tm.PixelToPoint(new Point(newState.X, newState.Y));

                        if (tm.TileExists(tm.PixelToPoint(newState.Position)) && tm.IsHighlighted(tm.PixelToPoint(newState.Position), Color.DarkMagenta) && !IsUnitPresent(currentUnits, vect) && !IsUnitPresent(enemyUnits, vect))
                        {
                            movingUnit.Moved = true;
                            movingUnit.Move(tm.PixelToPoint(newState.Position));
                            isMoving = false;
                            tm.ClearHighlight(Color.DarkMagenta);
                        }
                        else
                        {
                            isMoving = false;
                            tm.ClearHighlight(Color.DarkMagenta);
                        }
                    }
                    oldState = newState;
                }

                //If a unit has been right-clicked
                if (isRightClicking == true)
                {
                    newState = Mouse.GetState();

                    HighLightSquares(rightClickingUnit.RightClickRange, rightClickingUnit.Position, Color.Orchid, false, false, true);

                    if (oldState.RightButton == ButtonState.Released && newState.RightButton == ButtonState.Pressed)
                    {
                        Vector2 vect = tm.PixelToPoint(new Point(newState.X, newState.Y));

                        //If tile exists and is highlighted 
                        if (tm.TileExists(tm.PixelToPoint(newState.Position)) && tm.IsHighlighted(tm.PixelToPoint(newState.Position), Color.Orchid))
                        {
                            if (IsUnitPresent(enemyUnits, vect) && rightClickingUnit.CanAttack)
                            {
                                rightClickingUnit.NumRightClicks++;
                                rightClickingUnit.RightClick(this.UnitAt(enemyUnits, vect));
                                isRightClicking = false;
                                tm.ClearHighlight(Color.Orchid);
                            }
                            else if (IsUnitPresent(currentUnits, vect) && this.UnitAt(currentUnits, vect) != rightClickingUnit && rightClickingUnit.UC == UnitClass.Healer)
                            {
                                rightClickingUnit.NumRightClicks++;
                                rightClickingUnit.RightClick(this.UnitAt(currentUnits, vect));
                                isRightClicking = false;
                                tm.ClearHighlight(Color.Orchid);
                            }
                            //If nothing is present but the tile is highlighted
                            else
                            {
                                isRightClicking = false;
                                tm.ClearHighlight(Color.Orchid);
                            }
                            //If it's a cannon
                            //else if (rightClickingUnit.UC == UnitClass.MovableCannon)
                            //{
                            //    rightClickingUnit.NumRightClicks++;
                            //    rightClickingUnit.RightClick(this.UnitAt(enemyUnits, vect));
                            //    isRightClicking = false;
                            //    tm.ClearHighlight();
                            //}
                            //If it doesn't attack (probably only healers need this)
                        }
                        else
                        {
                            isRightClicking = false;
                            tm.ClearHighlight(Color.Orchid);
                        }
                    }
                    oldState = newState;
                }
            }
            else
            {
                ai.Turn(currentUnits, enemyUnits);
                

                //Updating the actual lists
                aiUnits = currentUnits;
                playerOneUnits = enemyUnits;

                //ResetBuildings();
                ResetUnits(currentUnits);

                //Changing the current lists
                currentUnits = playerOneUnits;
                enemyUnits = aiUnits;

                CaptureBuildings(Player.AI);

                // Give the blue player gold.
                tm.GiveCash("blue");

                //Reset the button, make it their turn again!
                aiTurn = false;
            }

        }

        /// <summary>
        /// To reset attacks and movement of units 
        /// </summary>
        /// <param name="units">List of units to be reset for a new turn</param>
        public void ResetUnits(List<Unit> units)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].Moved = false;
                units[i].NumRightClicks = 0;
            }
        }

        /// <summary>
        /// Returns true if a unit of units list given is present
        /// </summary>
        /// <param name="units">Units list</param>
        /// <param name="position">Position of possible unit</param>
        /// <returns>If a unit is present on the position given</returns>
        public bool IsUnitPresent(List<Unit> units, Vector2 position)
        {
            foreach (Unit u in units)
            {
                if (u.Position == position)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns unit assuming one is present.
        /// DO NOT USE UNLESS ABOVE METHOD RETURNS TRUE, WILL NOT CHECK
        /// </summary>
        /// <param name="units">List of units to check</param>
        /// <param name="position">X value of </param>
        /// <returns>The unit at a given position</returns>
        public Unit UnitAt(List<Unit> units, Vector2 position)
        {
            foreach (Unit u in units)
            {
                if (u.Position == position)
                    return u;
            }
            return null;
        }

        /// <summary>
        /// A method for AI that returns either the closest enemy unit in range,
        /// the closest enemy unit, and the closest of any unit
        /// </summary>
        /// <param name="attackingUnit">The starting unit</param>
        /// <param name="onlyInRange">If you want the closest unit in range or not</param>
        /// <param name="searchEnemies">If the AI wants to search just enemy units</param>
        /// <param name="searchFriendlies">If the AI wants to search just friendly units </param>
        /// <returns>The closest unit in attack range</returns>
        public Unit ClosestUnit(Unit unit, Boolean onlyInRange, Boolean searchEnemies, Boolean searchFriendlies)
        {
            Unit closestUnit = null;
            int closestDistance = 0; //Used to hold the smallest distance from given unit to any of the units
            int currentDistance = 0; //Distance of current loop unit to given unit
            if (searchEnemies)
            {
                foreach (Unit u in playerOneUnits)
                {
                    currentDistance = DistanceTo(unit, u);
                    //If searching for in range enemies only
                    if (onlyInRange == true && currentDistance <= unit.RightClickRange && currentDistance < closestDistance)
                    {
                        closestUnit = u;
                        closestDistance = currentDistance;
                    }
                    //If searching for out of range friendly
                    else if (currentDistance < closestDistance && !onlyInRange)
                    {
                        closestUnit = u;
                        closestDistance = currentDistance;
                    }
                }
            }
            if (searchFriendlies)
            {
                foreach (Unit u in aiUnits)
                {
                    currentDistance = DistanceTo(unit, u);
                    //If searching for in range friendlies only
                    if (onlyInRange == true && currentDistance <= unit.RightClickRange && currentDistance < closestDistance)
                    {
                        closestUnit = u;
                        closestDistance = currentDistance;
                    }
                    //If seraching for out of range friendly
                    else if (!onlyInRange && currentDistance < closestDistance)
                    {
                        closestUnit = u;
                        closestDistance = currentDistance;
                    }
                }
            }
            return closestUnit;
        }

        /// <summary>
        /// Returns closest structure according to the unit given according to the parameters given (For AI)
        /// </summary>
        /// <param name="unit"> Unit being used as 'origin' </param>
        /// <param name="onlyInRange">If building needs to be in move range of unit</param>
        /// <param name="searchEnemy">If searching for enemy buildings</param>
        /// <param name="searchNeutral">If searching for neutral buildings</param>
        /// <param name="searchFriendly">If searching for friendly buildings</param>
        /// <returns>The closest structure to the unit given</returns>
        public Tile ClosestStructure(Unit unit, Boolean onlyInRange, Boolean searchEnemy, Boolean searchNeutral, Boolean searchFriendly)
        {
            Tile closestBuilding = null;
            int closestDistance = int.MaxValue; //Used to hold the smallest distance from given unit to any of the units
            int currentDistance = 0; //Distance of current loop unit to given unit

            //If searching enemy buildings
            if (searchEnemy)
            {
                foreach (Tile b in tm.blueBuildings)
                {
                    currentDistance = DistanceTo(unit, b);
                    //If searching for in range enemy buildings and one is
                    if (onlyInRange == true && currentDistance <= unit.MaxMovement && currentDistance < closestDistance && IsUnitPresent(aiUnits,tm.PixelToPoint(b.Position)) == false && IsUnitPresent(playerOneUnits, tm.PixelToPoint(b.Position)) == false)
                    {
                        closestBuilding = b;
                        closestDistance = currentDistance;
                    }
                    //If searching for out of range enemy buildings and one is closest
                    else if (currentDistance < closestDistance && !onlyInRange && IsUnitPresent(aiUnits,tm.PixelToPoint(b.Position)) == false && IsUnitPresent(playerOneUnits, tm.PixelToPoint(b.Position)) == false)
                    {
                        closestBuilding = b;
                        closestDistance = currentDistance;
                    }
                }
            }
            //If searching neutral buildings
            if (searchNeutral)
            {
                foreach (Tile b in tm.neutralBuildings)
                {
                    currentDistance = DistanceTo(unit, b);
                    //If searching for in range neutral buildings and one is
                    if (onlyInRange == true && currentDistance <= unit.MaxMovement && currentDistance < closestDistance && IsUnitPresent(aiUnits, tm.PixelToPoint(b.Position)) == false && IsUnitPresent(playerOneUnits, tm.PixelToPoint(b.Position)) == false)
                    {
                        closestBuilding = b;
                        closestDistance = currentDistance;
                    }
                    //If searching for out of range neutral buildings and on is closest
                    else if (currentDistance < closestDistance && !onlyInRange && IsUnitPresent(aiUnits, tm.PixelToPoint(b.Position)) == false && IsUnitPresent(playerOneUnits, tm.PixelToPoint(b.Position)) == false)
                    {
                        closestBuilding = b;
                        closestDistance = currentDistance;
                    }
                }
            }
            //If searching friendly buildings
            if (searchFriendly)
            {
                foreach (Tile b in tm.redBuildings)
                {
                    currentDistance = DistanceTo(unit, b);
                    //If searching for in range friendly buildings and one is
                    if (onlyInRange == true && currentDistance <= unit.MaxMovement && currentDistance < closestDistance && IsUnitPresent(aiUnits, tm.PixelToPoint(b.Position)) == false && IsUnitPresent(playerOneUnits, tm.PixelToPoint(b.Position)) == false)
                    {
                        closestBuilding = b;
                        closestDistance = currentDistance;
                    }
                    //If searching for out of range friendly buildings and one is closest
                    else if (currentDistance < closestDistance && !onlyInRange && IsUnitPresent(aiUnits, tm.PixelToPoint(b.Position)) == false && IsUnitPresent(playerOneUnits, tm.PixelToPoint(b.Position)) == false)
                    {
                        closestBuilding = b;
                        closestDistance = currentDistance;
                    }
                }
            }
            return closestBuilding;
        }

        /// <summary>
        /// way w/out pathfinding to highlight squares
        /// </summary>
        /// <param name="maxMovement"> Max amount of tiles this unit can be moved </param>
        public void HighLightSquares(int movementPoints, Vector2 pos, Color c, Boolean ignoreForests, Boolean ignoreWater, Boolean attack)
        {
            //If it is a valid tile and isn't blocked by mountain or ocean tiles, and isn't already hi]ghlighted
            if (tm.TileExists(pos) == true && tm.TileAt(pos).TypeOfTile != TileType.Mountain)
            {
                //If you dont want to ignore forests and they are not there, or you are ignoring forests
                if ((tm.TileAt(pos).TypeOfTile != TileType.Forest && ignoreForests == false) || ignoreForests == true)
                {
                    //If you dont want to ignore water and it is not there, or you are ignoring water, and it isnt fog of war
                    if ((tm.TileAt(pos).TypeOfTile != TileType.Ocean && ignoreWater == false) || ignoreWater == true)
                    {
                        if ((tm.TileAt(pos).Col != Color.DarkOliveGreen && attack == true) || attack == false)
                        {
                            //If it isn't highlighted and needs to be
                            if ((tm.IsHighlighted(pos, c) == false))
                            {
                                tm.ToggleHighlight(pos, c);
                            }

                            //uses a movement point to go another tile in each direction (unless its out of movement points)
                            movementPoints--;
                            if (movementPoints >= 0)
                            {
                                HighLightSquares(movementPoints, new Vector2(pos.X, pos.Y + 1), c, ignoreForests, ignoreWater, attack);
                                HighLightSquares(movementPoints, new Vector2(pos.X, pos.Y - 1), c, ignoreForests, ignoreWater, attack);
                                HighLightSquares(movementPoints, new Vector2(pos.X + 1, pos.Y), c, ignoreForests, ignoreWater, attack);
                                HighLightSquares(movementPoints, new Vector2(pos.X - 1, pos.Y), c, ignoreForests, ignoreWater, attack);
                                HighLightSquares(movementPoints, new Vector2(pos.X - 1, pos.Y + 1), c, ignoreForests, ignoreWater, attack);
                                HighLightSquares(movementPoints, new Vector2(pos.X + 1, pos.Y - 1), c, ignoreForests, ignoreWater, attack);
                            }
                        }
                    }
                }
            }
        }
                
        /// <summary>
        /// Used for printing health to screen
        /// </summary>
        public String Debug()
        {
            string str = "";
            //for(int i = 0; i < list.Count; i++)
            //{
            //    str += (" " + list[i].Health);
            //}
            str += "P1 turn: " + playerOneTurn + Environment.NewLine;
            str += "Blue Count: " + tm.blueBuildings.Count + Environment.NewLine;
            str += "Red Count: " + tm.redBuildings.Count + Environment.NewLine;
            str += "Neutral Count: " + tm.neutralBuildings.Count + Environment.NewLine;
            str += "P1 Units Count: " + playerOneUnits.Count + Environment.NewLine;
            str += "P2 Units Count: " + playerTwoUnits.Count + Environment.NewLine;
            str += "CurrentUnits Count: " + currentUnits.Count + Environment.NewLine;
            str += "EnemyUnits Count " + enemyUnits.Count + Environment.NewLine;
            return str; 
        }
    }
}
