using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SwordFighterServer
{
    public class Utility
    {
        public static float FlatAngleBetweenVectors(Vector3 a, Vector3 b)
        {
            var flatA = new Vector3(a.X, 0f, a.Z);
            var flatB = new Vector3(b.X, 0f, b.Z);

            float dot = Vector3.Dot(flatA, flatB);
            float magA = flatA.Length();
            float magB = flatB.Length();

            if (magA == 0f || magB == 0f)
                return 0f;

            double cosTheta = dot / (magA * magB);
            cosTheta = Math.Clamp(cosTheta, -1f, 1f);

            return (float) (Math.Acos(cosTheta) * (180f / Math.PI));
        }

        public static Vector3 ClampPosition(Vector3 position)
        {
            if (position.X < -50f)
            {
                position.X = -50f;
            }
            else if (position.X > 50f)
            {
                position.X = 50f;
            }
            if (position.Z < -50f)
            {
                position.Z = -50f;
            }
            else if (position.Z > 50f)
            {
                position.Z = 50f;
            }
            return position;
        }
    }
}
