using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur
{
    internal static class ConvertUnits
    {
        static readonly float cmPerMeter = 100;

        /// <summary>
        /// Use this to convert from 1/100 Unity units (centimeters) to Unity units (meters)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float ToMeters(float val)
        {
            return val / cmPerMeter;
        }

        /// <summary>
        /// Use this to convert from Unity units (meters) to 1/100 Unity units (centimeters)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float ToCentimeters(float val)
        {
            return val * cmPerMeter;
        }
    }
}
