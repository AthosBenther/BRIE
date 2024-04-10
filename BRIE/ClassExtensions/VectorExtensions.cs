using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BRIE.ClassExtensions
{
    public static class VectorExtensions
    {
        public static double Angle(this Vector vector)
        {
            // Calculate the angle in radians
            double angleRadians = Math.Atan2(vector.Y, vector.X);

            // Convert radians to degrees
            double angleDegrees = angleRadians * (180 / Math.PI);

            // Adjust for negative angles
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            return angleDegrees;
        }
    }
}
