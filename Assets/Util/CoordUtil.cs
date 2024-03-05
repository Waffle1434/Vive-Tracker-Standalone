using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Coord {
    [Serializable]
    public struct LatLonAlt {
        public double latitude, longitude;
        public float altitude_asl;

        public static readonly LatLonAlt zero = new LatLonAlt();

        public LatLonAlt(double latitude, double longitude, float altitude_asl = 0f) { this.latitude = latitude; this.longitude = longitude; this.altitude_asl = altitude_asl; }

        public static LatLonAlt operator -(LatLonAlt a, LatLonAlt b) => new LatLonAlt(a.latitude - b.latitude, a.longitude - b.longitude, a.altitude_asl - b.altitude_asl);

        public static bool operator ==(LatLonAlt a, LatLonAlt b) => a.latitude == b.latitude && a.longitude == b.longitude && a.altitude_asl == b.altitude_asl;
        public static bool operator !=(LatLonAlt a, LatLonAlt b) => !(a == b);

        public override bool Equals(object obj) => obj is LatLonAlt b ? this == b : false;
        public override int GetHashCode() => latitude.GetHashCode() ^ (longitude.GetHashCode() << 2) ^ (altitude_asl.GetHashCode() >> 2);
        public override string ToString() => $"{latitude:0.000}°,{longitude:0.000}°,{altitude_asl:0.0}m";

        public float Distance(LatLonAlt other) => CoordUtil.GetAproxOffsetXYZ(this, other).magnitude;
    }

    public class Ellipsoid {
        public double a;          // Semi-major earth axis
        public double f;          // Reciprocal of Flattening
        public double eccSq;      // Eccentricity squared
        public double eccPrimeSq; // Eccentricity Prime squared
        public float k0;          // Transverse Mercator Scale Factor
        public float molodensky_x;
        public float molodensky_y;
        public float molodensky_z;
    };

    public struct UTM {
        public enum Hemisphere : byte { N = (byte)'N', S = (byte)'S' }

        public Hemisphere hemisphere;
        public int zone;         // range 1-60
        public float easting;    // 166000 m to 834000 m
        public float northing;   // 0 m to 8260000 m
        public float altitude_asl;
        public Ellipsoid model;

        public override string ToString() => $"{(char)hemisphere} Z{zone} {easting}E {northing}N {altitude_asl}m";
    }

    #region Double Data Structures
    public struct Double3 {
        public double x, y, z;

        public static readonly Double3 zero = new Double3();
        public static readonly Double3 one = new Double3(1.0, 1.0, 1.0);
        public static readonly Double3 up = new Double3(0.0, 1.0, 0.0);
        public static readonly Double3 down = new Double3(0.0, -1.0, 0.0);
        public static readonly Double3 left = new Double3(-1.0, 0.0, 0.0);
        public static readonly Double3 right = new Double3(1.0, 0.0, 0.0);
        public static readonly Double3 forward = new Double3(0.0, 0.0, 1.0);
        public static readonly Double3 back = new Double3(0.0, 0.0, -1.0);

        public Double3(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }

        public static Double3 operator *(Double3 a, double b) => new Double3(a.x * b, a.y * b, a.z * b);

        public Double3 Normalized => this * (1.0 / Length);

        public double Length => Math.Sqrt(Dot(this, this));

        public static double Dot(Double3 a, Double3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

        public static Double3 Cross(Double3 a, Double3 b) => new Double3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    public struct DoubleMatrix3x4 {
        public double _11, _12, _13, _14;
        public double _21, _22, _23, _24;
        public double _31, _32, _33, _34;
        //public double _41, _42, _43, _44;

        public DoubleMatrix3x4(
            double _11, double _12, double _13, double _14,
            double _21, double _22, double _23, double _24,
            double _31, double _32, double _33, double _34
        //double _41, double _42, double _43, double _44
        ) {
            this._11 = _11; this._12 = _12; this._13 = _13; this._14 = _14;
            this._21 = _21; this._22 = _22; this._23 = _23; this._24 = _24;
            this._31 = _31; this._32 = _32; this._33 = _33; this._34 = _34;
            //this._41 = _41; this._42 = _42; this._43 = _43; this._44 = _44;
        }

        public DoubleMatrix3x4(Double3 forward, Double3 up, Double3 right, Double3 translation) {
            _11 = forward.x; _12 = forward.y; _13 = forward.z;
            _21 = up.x; _22 = up.y; _23 = up.z;
            _31 = right.x; _32 = right.y; _33 = right.z;
            _14 = translation.x; _24 = translation.y; _34 = translation.z;
            //_41 = 0.0; _42 = 0.0; _43 = 0.0; _44 = 1.0;
        }

        public Double3 TransformPoint(Double3 p) {
            return new Double3(
                _11 * p.x + _12 * p.y + _13 * p.z + _14,
                _21 * p.x + _22 * p.y + _23 * p.z + _24,
                _31 * p.x + _32 * p.y + _33 * p.z + _34
            );
        }
        public Double3 TransformDirection(Double3 d) {
            return new Double3(
                _11 * d.x + _12 * d.y + _13 * d.z,
                _21 * d.x + _22 * d.y + _23 * d.z,
                _31 * d.x + _32 * d.y + _33 * d.z
            );
        }


        public static DoubleMatrix3x4 operator *(DoubleMatrix3x4 a, DoubleMatrix3x4 b) {
            /*result._11 = a._11 * b._11 + a._12 * b._21 + a._13 * b._31 + a._14 * b._41;
            result._12 = a._11 * b._12 + a._12 * b._22 + a._13 * b._32 + a._14 * b._42;
            result._13 = a._11 * b._13 + a._12 * b._23 + a._13 * b._33 + a._14 * b._43;
            result._14 = a._11 * b._14 + a._12 * b._24 + a._13 * b._34 + a._14 * b._44;
            result._21 = a._21 * b._11 + a._22 * b._21 + a._23 * b._31 + a._24 * b._41;
            result._22 = a._21 * b._12 + a._22 * b._22 + a._23 * b._32 + a._24 * b._42;
            result._23 = a._21 * b._13 + a._22 * b._23 + a._23 * b._33 + a._24 * b._43;
            result._24 = a._21 * b._14 + a._22 * b._24 + a._23 * b._34 + a._24 * b._44;
            result._31 = a._31 * b._11 + a._32 * b._21 + a._33 * b._31 + a._34 * b._41;
            result._32 = a._31 * b._12 + a._32 * b._22 + a._33 * b._32 + a._34 * b._42;
            result._33 = a._31 * b._13 + a._32 * b._23 + a._33 * b._33 + a._34 * b._43;
            result._34 = a._31 * b._14 + a._32 * b._24 + a._33 * b._34 + a._34 * b._44;
            result._41 = a._41 * b._11 + a._42 * b._21 + a._43 * b._31 + a._44 * b._41;
            result._42 = a._41 * b._12 + a._42 * b._22 + a._43 * b._32 + a._44 * b._42;
            result._43 = a._41 * b._13 + a._42 * b._23 + a._43 * b._33 + a._44 * b._43;
            result._44 = a._41 * b._14 + a._42 * b._24 + a._43 * b._34 + a._44 * b._44;*/

            return new DoubleMatrix3x4(
                a._11 * b._11 + a._12 * b._21 + a._13 * b._31,
                a._11 * b._12 + a._12 * b._22 + a._13 * b._32,
                a._11 * b._13 + a._12 * b._23 + a._13 * b._33,
                a._11 * b._14 + a._12 * b._24 + a._13 * b._34 + a._14,
                a._21 * b._11 + a._22 * b._21 + a._23 * b._31,
                a._21 * b._12 + a._22 * b._22 + a._23 * b._32,
                a._21 * b._13 + a._22 * b._23 + a._23 * b._33,
                a._21 * b._14 + a._22 * b._24 + a._23 * b._34 + a._24,
                a._31 * b._11 + a._32 * b._21 + a._33 * b._31,
                a._31 * b._12 + a._32 * b._22 + a._33 * b._32,
                a._31 * b._13 + a._32 * b._23 + a._33 * b._33,
                a._31 * b._14 + a._32 * b._24 + a._33 * b._34 + a._34
            );
        }

        public static Double3 operator *(DoubleMatrix3x4 matrix, Double3 point) => matrix.TransformPoint(point);
    }
    #endregion

    public static class CoordUtil {
        public const double circum_equatorial = 40075016.6856;// Earth circumference around the equator (meters)
        public const double circum_meridonial = 40007863.0;// Earth circumference through the poles (meters)
        public const double earthEquatorialRadius = 6378137.0;
        public const double earthMeridonialRadius = 6356752.3;
        public const double avgEarthRadius = (2.0 * earthEquatorialRadius + earthMeridonialRadius) / 3.0;// Ellipse to avg circle


        /*public static Double3 LatLongAltToEarthXYZ(LatLongAlt position) {
            return new Double3();
        }

        public static void LatLongAltToRelativeXYZ(LatLongAlt origin, LatLongAlt position) {
            Double3 originXYZ = LatLongAltToEarthXYZ(origin);
            Double3 positionXYZ = LatLongAltToEarthXYZ(position);

            Double3 up = originXYZ.Normalized;
            Double3 right = Double3.Cross(Double3.up, up);
            Double3 forward = Double3.Cross(up, right);

            DoubleMatrix3x4 relXYZToEarthXYZ = new DoubleMatrix3x4(forward, up, right, originXYZ);

        }*/


#if UNITY_5_3_OR_NEWER
        /// <summary> Approximate XYZ meter difference between positions, not fully taking curvature into effect (decent for short distances). </summary>
        public static Vector3 GetAproxOffsetXYZ(this LatLonAlt position, LatLonAlt origin) {
            const double latToM = circum_meridonial / 360.0;
            const double lonToM = circum_equatorial / 360.0;

            LatLonAlt diff = position - origin;
            double n_offset = diff.latitude * latToM;
            // Adjust by latitude (circumference E-W gets smaller at higher latitudes)
            double e_offset = diff.longitude * lonToM * Math.Cos(position.latitude * deg2rad);

            return new Vector3((float)e_offset, diff.altitude_asl, (float)n_offset);
        }

        public static LatLonAlt GetLatLon(LatLonAlt origin, Vector3 originXYZ, Vector3 position) {
            const double mToLat = 360.0 / circum_meridonial;
            const double mToLon = 360.0 / circum_equatorial;

            Vector3 origin_offset = position - originXYZ;

            double lat = origin.latitude + origin_offset.z * mToLat;
            double lon = origin.longitude + origin_offset.x * (mToLon / Math.Cos(lat * deg2rad));
            return new LatLonAlt(lat, lon, origin.altitude_asl + origin_offset.y);
        }
#endif






        public static readonly Ellipsoid WGS84 = new Ellipsoid {
            a = 6378137.0,
            f = 298.257223563,
            eccSq = 6.69437999014e-3,
            eccPrimeSq = 6.73949674228e-3,
            k0 = 0.9996f
        };
        public static readonly Ellipsoid NAD27 = new Ellipsoid {
            a = 6378206.4,
            f = 294.9786982,
            eccSq = 6.69437999014e-3,
            eccPrimeSq = 6.73949674228e-3,
            k0 = 0.9996f,
            molodensky_x = 0f,
            molodensky_y = 125f,
            molodensky_z = 194f
        };
        public static readonly Ellipsoid AIRY = new Ellipsoid {
            a = 6377563.396,
            f = 299.3249646,
            eccSq = 6.670539761597e-3,
            eccPrimeSq = 6.715334668516e-3,
            k0 = 0.9996012717f,
            molodensky_x = 371f, // 375f
            molodensky_y = -112f, // -111f
            molodensky_z = 434f // 431f
        };

        const double pi = 3.14159265358979323846;
        const double deg2rad = pi / 180.0;
        const double rad2deg = 1.0 / deg2rad;

        public static UTM LatLonToUTM(LatLonAlt latLong, Ellipsoid model = null) {
            if (model == null) model = WGS84;
            UTM coord;
            double longitude = latLong.longitude;

            if (Math.Abs(longitude) >= 180) longitude = -180; // Enforce [-180,+180) interval.  Especially the value +180 is legal but it is a problem.
            coord.zone = (int)((longitude + 186) / 6);// Calculate grid zone "column". Range: 1-60
            int LongOrigin = coord.zone * 6 - 183;// Range: -177 to 177

            double r_latitude = latLong.latitude * deg2rad;
            double sin_latitude = Math.Sin(r_latitude);
            double cos_latitude = Math.Cos(r_latitude);
            double tan_latitude = sin_latitude / cos_latitude;

            // These calculations are taken from http://www.gmat.unsw.edu.au/cr/gmat2700/gda_techmanual.pdf  (page 15)
            double e_2 = model.eccSq;
            double e_4 = e_2 * e_2;
            double e_6 = e_4 * e_2;
            double A_0 = 1.0 - (e_2 / 4.0) - (3.0 * e_4 / 64.0) - (5.0 * e_6 / 256.0);
            double A_2 = (3.0 / 8.0) * (e_2 + e_4 / 4.0 + 15.0 * e_6 / 128.0);
            double A_4 = (15.0 / 256.0) * (e_4 + 3.0 * e_6 / 4.0);
            double A_6 = 35.0 * e_6 / 3072.0;
            double M = model.a * (A_0 * r_latitude -
                            A_2 * Math.Sin(2.0 * r_latitude) +
                            A_4 * Math.Sin(4.0 * r_latitude) -
                            A_6 * Math.Sin(6.0 * r_latitude));

            double testValue = 1.0 - model.eccSq * sin_latitude * sin_latitude;
            if (testValue < 0.0) testValue = 0.001;

            double N = model.a / Math.Sqrt(testValue);
            double T = tan_latitude * tan_latitude;
            double T2 = T * T;
            double C = model.eccPrimeSq * cos_latitude * cos_latitude;
            double A = cos_latitude * ((double)longitude - LongOrigin) * deg2rad;
            double A2 = A * A;
            double A4 = A2 * A2;

            coord.easting = (float)(model.k0 * N *
               (A +
                  (1.0 - T + C) * A * A2 / 6.0 +
                  (5.0 - 18.0 * T + T2 + 72.0 * C - 58.0 * model.eccPrimeSq) *
                  A * A4 / 120.0
               ) + 500000.0);

            coord.northing = (float)(model.k0 * (
                M + N * tan_latitude * (
                    A2 / 2.0 + (5.0 - T + 9.0 * C + 4.0 * C * C) * A4 / 24.0 +
                    (61.0 - 58.0 * T + T2 + 600.0 * C - 330.0 * model.eccPrimeSq) * A2 * A4 / 720.0
                )
            ));
            coord.hemisphere = UTM.Hemisphere.N;
            if (coord.northing < 0f) {
                coord.northing += 10000000f; // Add 10,000,000 to northing if negative (i.e., Southern Hemisphere)?
                coord.hemisphere = UTM.Hemisphere.S;
            }
            coord.altitude_asl = latLong.altitude_asl;
            coord.model = model;

            return coord;
        }

        public static LatLonAlt UTMtoLatLon(UTM utm, Ellipsoid datum = null) {
            // Reference: USGS Paper 1395 Nov 1993,  pp 61-64
            // Written by Mike Metcalf, ported to C#

            if (datum == null) datum = WGS84;

            double e = datum.eccSq;
            double northing = utm.northing;

            double testValue = e > 1.0 ? 0.001 : (1.0 - e);

            double e1 = (1.0 - Math.Sqrt(testValue)) / (1.0 + Math.Sqrt(testValue));   // 3-24
            double e1_2 = e1 * e1;
            double e1_3 = e1_2 * e1;
            double e1_4 = e1_3 * e1;
            double tmp3 = 151.0 / 96.0 * e1_3;                                         // 3-26
            double tmp4 = 1097.0 / 512.0 * e1_4;                                       // 3-26

            if (utm.hemisphere == UTM.Hemisphere.S) northing -= 10000000.0;// subtract false northing for southern hemisphere

            double mu = northing / datum.k0 / (datum.a * (1.0 - 0.25 * e - 3.0 / 64.0 * (e * e) - 5.0 / 256.0 * (e * e * e)));// 8-20 / 7-19
            double mu2 = 2.0 * mu;
            double cos2mu = Math.Cos(mu2);
            double sin2mu = Math.Sin(mu2);
            double phi_1 = mu + sin2mu * (1.5 * e1 - 27.0 / 32.0 * e1_3 - tmp3 + cos2mu * (
                2.0 * (21.0 / 16.0 * e1_2 - 55.0 / 32.0 * e1_4) - 4.0 * tmp4 + cos2mu * (4.0 * tmp3 + 8.0 * tmp4 * cos2mu)
            ));
            double sinphi = Math.Sin(phi_1);
            double cosphi = Math.Cos(phi_1);
            double tanphi = (Math.Abs(cosphi) > double.Epsilon) ? (sinphi / cosphi) : (1E38);
            double C1 = cosphi * cosphi * datum.eccPrimeSq;                            // 8-21
            double T1 = tanphi * tanphi;                                               // 8-22
            testValue = 1.0 - datum.eccSq * (sinphi * sinphi);
            if (testValue < 0.0) testValue = 0.001;
            double tmp = Math.Sqrt(testValue);
            double N1 = datum.a / tmp;                                                 // 8-23
            double R1 = datum.a * (1.0 - datum.eccPrimeSq) / (tmp * tmp * tmp);        // 8-24
            double D1 = (utm.easting - 500000.0) / (N1 * datum.k0);                    // 8-25
            double C2 = C1 * C1;
            double T2 = T1 * T1;
            double D2 = D1 * D1;
            double D4 = D2 * D2;

            // Latitude in radians
            double phi = phi_1 - (N1 * tanphi / R1) * (0.5 * D2 - (5.0 + 3.0 * T1 + 10.0 * C1 - 4.0 * C2 - datum.eccPrimeSq * 9.0) * D4 / 24.0 +
                (61.0 + 90.0 * T1 + 298.0 * C1 + 45.0 * T2 - 252.0 * datum.eccPrimeSq - 3.0 * C2) * D2 * D4 / 720.0);// 8-17

            // Longitude in radians
            double lambda = (D1 - (1.0 + 2.0 * T1 + C1) * D1 * D2 / 6.0 + (5.0 - 2.0 * C1 + 28.0 * T1 - 3.0 * C2 + datum.eccPrimeSq * 8.0 + 24.0 * T2) * D1 * D4 / 120.0) / cosphi;// 8-18

            return new LatLonAlt(phi * rad2deg, lambda * rad2deg + (utm.zone * 6 - 183), utm.altitude_asl);
        }
    }
}