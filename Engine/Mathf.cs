using R;
using System;
using System.Numerics;

namespace HI
{
    public static class Mathf
    {

        public static Matrix4x4 ModelMatrixFromTransfrom(Transform transform)
        {
            Matrix4x4 matrix4 = new Matrix4x4();

            matrix4 = Matrix4x4.CreateTranslation(transform.position) *
            Matrix4x4.CreateScale(transform.scale) *
            Matrix4x4.CreateFromQuaternion(transform.rotation);

            return matrix4;
        }

        public static Quaternion QuaternionFromEuler(double yaw, double pitch, double roll) // yaw (Z), pitch (Y), roll (X)
        {
            double pi = Math.PI / 360;

            yaw = yaw * pi;
            pitch = pitch * pi;
            roll = roll * pi;

            float cy = (float)Math.Cos(yaw);
            float sy = (float)Math.Sin(yaw);
            float cp = (float)Math.Cos(pitch);
            float sp = (float)Math.Sin(pitch);
            float cr = (float)Math.Cos(roll);
            float sr = (float)Math.Sin(roll);

            Quaternion q = new Quaternion();
            q.W = cr * cp * cy + sr * sp * sy;
            q.X = sr * cp * cy - cr * sp * sy;
            q.Y = cr * sp * cy + sr * cp * sy;
            q.Z = cr * cp * sy - sr * sp * cy;

            return q;
        }

        public static bool Inside(Vector2 bottom_left, Vector2 size, Vector2 point)
        {
            return bottom_left.X <= point.X && bottom_left.X + size.X >= point.X && bottom_left.Y <= point.Y && bottom_left.Y + size.Y >= point.Y;
        }

    }
}
