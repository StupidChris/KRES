﻿using UnityEngine;

namespace KRES.Extensions
{
    public static class CelestialBodyExtensions
    {
        //Borrowed from https://github.com/MuMech/MechJeb2/blob/master/MechJeb2/CelestialBodyExtensions.cs#L16-L24
        public static double TerrainAltitude(this CelestialBody body, double latitude, double longitude)
        {
            if (body.pqsController == null) { return 0; }

            Vector3d pqsRadialVector = QuaternionD.AngleAxis(longitude, Vector3d.down) * QuaternionD.AngleAxis(latitude, Vector3d.forward) * Vector3d.right;
            return body.pqsController.GetSurfaceHeight(pqsRadialVector) - body.pqsController.radius;
        }

        public static string GetBiome(this CelestialBody body, double latitude, double longitude)
        {
            if (body.BiomeMap == null) { return string.Empty; }
            if (body.BiomeMap.Map == null) { return body.BiomeMap.defaultAttribute.name; }
            return body.BiomeMap.GetAtt(latitude * KRESUtils.degToRad, longitude * KRESUtils.degToRad).name;
        }
    }
}
