using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    public class ColorUtilites 
    {

        public static float GetPerceptualDistance(Color c1, Color c2)
        {
            float rDim = (c1.r - c2.r) * 0.3f;
            float gDim = (c1.g - c2.g) * 0.59f;
            float bDim = (c1.b - c2.b) * 0.11f;

            return Mathf.Sqrt(rDim * rDim + gDim * gDim + bDim * bDim);
        }

        /// <summary>
        /// Compares the input colours for perceptual equality.
        /// </summary>
        /// <param name="c1">Color 1</param>
        /// <param name="c2">Color 2</param>
        /// <returns>true if c1 equals c2 else false.</returns>
        public static bool CheckEquality(Color c1, Color c2)
        {
            return GetPerceptualDistance(c1, c2) <= 0.01f;
        }
    }
}