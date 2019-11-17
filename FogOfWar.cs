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
    class FogOfWar
    {
        UnitManager um;
        TileManager tm;
        //Dictionary<Tile, Texture2D> redDictionary;
        //Dictionary<Tile, Texture2D> blueDictionary;
        //Texture2D hill;
        //List<Tile> EnemyBuildings;
        //List<Tile> FriendlyBuildings;

        public FogOfWar(UnitManager um, TileManager tm, Texture2D hill)
        {
            this.um = um;
            this.tm = tm;
            //this.hill = hill;
            //redDictionary = new Dictionary<Tile, Texture2D>();
            //blueDictionary = new Dictionary<Tile, Texture2D>();
        }

        public void Update()
        {
            if (um.playerOneTurn)
            {
                //Makes the board gray
                tm.HighlightBoard(Color.DarkOliveGreen);

                //Making board fog cleared around player and buildings owned
                for (int i = 0; i < um.playerOneUnits.Count; i++)
                {
                    um.HighLightSquares(um.playerOneUnits[i].FogOfWarRange, um.playerOneUnits[i].Position, Color.White, false, false, false);
                }
                for(int i = 0; i < tm.redBuildings.Count; i++)
                {
                    um.HighLightSquares(4, tm.PixelToPoint(tm.redBuildings[i].Position - tm.Offset), Color.White, false, false, false);
                }
            }
            else
            {
                //Two player
                if (um.twoPlayer)
                {
                    //Makes the board gray
                    tm.HighlightBoard(Color.DarkOliveGreen);

                    //Making board fog cleared around player and buildings owned
                    for (int i = 0; i < um.playerTwoUnits.Count; i++)
                    {
                        um.HighLightSquares(um.playerTwoUnits[i].FogOfWarRange, um.playerTwoUnits[i].Position, Color.White, false, true, false);
                    }
                    for (int i = 0; i < tm.blueBuildings.Count; i++)
                    {
                        um.HighLightSquares(4, tm.PixelToPoint(tm.blueBuildings[i].Position - tm.Offset), Color.White, false, true, false);
                    }
                }

                //AI - NOT IMPLEMENTED
                else
                {
                    tm.HighlightBoard(Color.DarkOliveGreen);

                    for (int i = 0; i < um.aiUnits.Count; i++)
                    {
                        um.HighLightSquares(um.aiUnits[i].FogOfWarRange, um.aiUnits[i].Position, Color.White, false, true, false);
                    }
                    for (int i = 0; i < tm.blueBuildings.Count; i++)
                    {
                        um.HighLightSquares(4, tm.PixelToPoint(tm.blueBuildings[i].Position - tm.Offset), Color.White, false, true, false);
                    }
                }
            }
        }   
        
        /// <summary>
        /// A check for if unit is in fog of war or not
        /// </summary>
        /// <param name="u">Unit you are checking for in fog</param>
        /// <returns>True if in fog, false if not</returns>
        public Boolean UnitInFog(Unit u)
        {
            Vector2 position = u.Position;

            if(tm.TileAt(position).Col == Color.DarkOliveGreen)
                return true;

            else
                return false;
        }
    }
}
