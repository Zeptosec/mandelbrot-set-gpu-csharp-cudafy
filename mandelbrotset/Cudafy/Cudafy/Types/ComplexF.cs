/*
CUDAfy.NET - LGPL 2.1 License
Please consider purchasing a commerical license - it helps development, frees you from LGPL restrictions
and provides you with support.  Thank you!
Copyright (C) 2011 Hybrid DSP Systems
http://www.hybriddsp.com

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cudafy.Types
{
    /// <summary>
    /// Represents a complex single floating point number that is mapped to the native GPU equivalent.
    /// </summary>
    public struct ComplexF
    {
        /// <summary>
        /// Real part.
        /// </summary>
        public float x;

        /// <summary>
        /// Imaginary part.
        /// </summary>
        public float y;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexF"/> struct.
        /// </summary>
        /// <param name="real">The real part.</param>
        /// <param name="imaginary">The imaginary part.</param>
        public ComplexF(float real, float imaginary)
        {
            this.x = (float)real;
            this.y = (float)imaginary;
        }

        /// <summary>
        /// Conjugates the specified value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>Conjugated value.</returns>
        public static ComplexF Conj(ComplexF x)
        {
            return new ComplexF(x.x, -1 * x.y);
        }

        /// <summary>
        /// Adds value y to value x.
        /// </summary>
        /// <param name="x">Value one.</param>
        /// <param name="y">Value to be added.</param>
        /// <returns>New value.</returns>
        public static ComplexF Add(ComplexF x, ComplexF y)
        {
            x.x = x.x + y.x;
            x.y = x.y + y.y;
            return x;
        }

        /// <summary>
        /// Subtracts value y from value x.
        /// </summary>
        /// <param name="x">Value one.</param>
        /// <param name="y">Value to be subtracted.</param>
        /// <returns>New value.</returns>
        public static ComplexF Subtract(ComplexF x, ComplexF y)
        {
            x.x = x.x - y.x;
            x.y = x.y - y.y;
            return x;
        }

        /// <summary>
        /// Multiplies value x and y.
        /// </summary>
        /// <param name="x">Value one.</param>
        /// <param name="y">Value two.</param>
        /// <returns>New value.</returns>
        public static ComplexF Multiply(ComplexF x, ComplexF y)
        {
            ComplexF c = new ComplexF();
            c.x = x.x * y.x - x.y * y.y;
            c.y = x.x * y.y + x.y * y.x;
            return c;
        }

        /// <summary>
        /// Divides value x by y.
        /// </summary>
        /// <param name="x">Value one.</param>
        /// <param name="y">Value two.</param>
        /// <returns>New value.</returns>
        public static ComplexF Divide(ComplexF x, ComplexF y)
        {
            float s = Math.Abs(y.x) + Math.Abs(y.y); 
            float oos = 1.0f / s;
            float ars = x.x * oos;
            float ais = x.y * oos;
            float brs = y.x * oos;
            float bis = y.y * oos;
            s = (brs * brs) + (bis * bis);
            oos = 1.0f / s;
            ComplexF quot = new ComplexF(((ars * brs) + (ais * bis)) * oos,
                                        ((ais * brs) - (ars * bis)) * oos);
            return quot;
        }

        /// <summary>
        /// Gets the absolute of the specified value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>Absolute.</returns>
        public static float Abs(ComplexF x)
        {
            float a = x.x;
            float b = x.y;
            float v, w, t;
            a = Math.Abs(a);
            b = Math.Abs(b);
            if (a > b)
            {
                v = a;
                w = b;
            }
            else
            {
                v = b;
                w = a;
            }
            t = w / v;
            t = 1.0f + t * t;
            t = v * GMath.Sqrt(t);
            if ((v == 0.0f) || (v > 3.402823466e38f) || (w > 3.402823466e38f))
            {
                t = v + w;
            }
            return t;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("( {0}, {1}i )", x, y);
        }
    }
}
