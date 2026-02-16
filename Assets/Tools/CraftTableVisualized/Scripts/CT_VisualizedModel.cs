using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace CraftTableVisualized
{
    public class CT_VisualizedModel
    {
        public List<CraftTableDB.Recipe> recipes;

        public void Init(CraftTableDB craftTableDB)
        {
            recipes = craftTableDB.recipeList;
        }
    }
}