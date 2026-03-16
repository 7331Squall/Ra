using System;
using System.Collections.Generic;
using UnityEngine;

public static class GPTEarthCalc {

    //public static float ScaleTimesENeg9 = 1; // 1 = metros reais, 1e-9 recomendado

    // Constantes orbitais
    const double BiggerSemiAxis = 1.495978707e11; // semi-eixo maior (m)
    const double Eccentricity = 0.01671022;
    const double OrbitalInclination = 0.00005 * Mathf.Deg2Rad;
    const double Perihelion = 102.94719 * Mathf.Deg2Rad;
    const double AscendantNodeLongitude = -11.26064 * Mathf.Deg2Rad;
    const double MedianAnomaly = 100.46435 * Mathf.Deg2Rad;
    const double OrbitalPeriod = 365.256363004; // dias

    const double AxialTilt = 23.439281 * Mathf.Deg2Rad;
    // Duração do dia sideral (segundos)
    const double SiderealDay = 86164.0905;


    static readonly DateTime J2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static Vector3 CalculateEarthPosition(DateTime date, float scaleTimesENeg9) {
        double daysSinceJ2000 = (date.ToUniversalTime() - J2000).TotalDays;

        // Anomalia média
        double M = MedianAnomaly + 2.0 * Math.PI * (daysSinceJ2000 / OrbitalPeriod);

        // Resolver Kepler (Newton-Raphson)
        double E = SolveKepler(M);

        // Anomalia verdadeira
        double v = 2.0 * Math.Atan2(Math.Sqrt(1 + Eccentricity) * Math.Sin(E / 2), Math.Sqrt(1 - Eccentricity) * Math.Cos(E / 2));

        // Distância ao Sol
        double r = BiggerSemiAxis * (1 - Eccentricity * Math.Cos(E));

        // Posição no plano orbital
        double xOrb = r * Math.Cos(v);
        double yOrb = r * Math.Sin(v);

        // Rotação para espaço 3D
        double cosO = Math.Cos(AscendantNodeLongitude);
        double sinO = Math.Sin(AscendantNodeLongitude);
        double cosi = Math.Cos(OrbitalInclination);
        double sini = Math.Sin(OrbitalInclination);
        double cosw = Math.Cos(Perihelion);
        double sinw = Math.Sin(Perihelion);

        double x = (cosO * cosw - sinO * sinw * cosi) * xOrb + (-cosO * sinw - sinO * cosw * cosi) * yOrb;

        double y = (sinO * cosw + cosO * sinw * cosi) * xOrb + (-sinO * sinw + cosO * cosw * cosi) * yOrb;

        double z = (sinw * sini) * xOrb + (cosw * sini) * yOrb;

        return new Vector3((float) x, (float) z, (float) y) * (scaleTimesENeg9 * 1e-9f);
        // eixo Y da Unity = "up"
    }

    static double SolveKepler(double M) {
        double normalM = NormalizeAngle(M);
        double E = normalM;
        for (int x = 0; x < 6; x++) {
            E -= (E - Eccentricity * Math.Sin(E) - normalM) / (1 - Eccentricity * Math.Cos(E));
        }
        return E;
    }

    static double NormalizeAngle(double angle) {
        angle %= 2.0 * Math.PI;
        if (angle < 0)
            angle += 2.0 * Math.PI;
        return angle;
    }


    static double GetEarthSpinAngle(DateTime date) {
        double secondsSinceJ2000 = (date.ToUniversalTime() - J2000).TotalSeconds;

        double rotations = secondsSinceJ2000 / SiderealDay;

        return NormalizeAngle(-rotations * 2.0 * Math.PI);
    }

    public static Quaternion CalculateEarthRotation(DateTime date) {
        double spinAngle = GetEarthSpinAngle(date);

        Quaternion axialTilt = Quaternion.Euler(0f, 0f, (float) (AxialTilt * Mathf.Rad2Deg));

        Quaternion spin = Quaternion.AngleAxis((float) (spinAngle * Mathf.Rad2Deg), axialTilt * Vector3.up);

        return spin * axialTilt;
    }


    public static Mesh GenerateOrbitMesh(int segments = 512, int radialSegments = 8, float tubeRadius = 0.001f, float scaleTimesENeg9 = 1f) {
        List<Vector3> centers = new();

        // 1️⃣ Gerar pontos da órbita
        for (int i = 0; i <= segments; i++) {
            double t = (double) i / segments;
            double meanAnomaly = t * 2.0 * Mathf.PI;

            Vector3 pos = GPTEarthCalc.CalculateEarthPosition(
                GPTEarthCalc.J2000.AddDays(meanAnomaly / (2.0 * Mathf.PI) * 365.256363004), scaleTimesENeg9
            );

            centers.Add(pos);
        }

        // 2️⃣ Construir tubo
        List<Vector3> vertices = new();
        List<int> triangles = new();

        for (int i = 0; i < centers.Count; i++) {
            Vector3 forward = i < centers.Count - 1 ? (centers[i + 1] - centers[i]).normalized : (centers[i] - centers[i - 1]).normalized;

            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;

            for (int j = 0; j < radialSegments; j++) {
                float angle = j / (float) radialSegments * Mathf.PI * 2f;
                Vector3 offset = Mathf.Cos(angle) * right * tubeRadius + Mathf.Sin(angle) * up * tubeRadius;

                vertices.Add(centers[i] + offset);
            }
        }

        // 3️⃣ Triângulos
        for (int i = 0; i < centers.Count - 1; i++) {
            int baseIndex = i * radialSegments;
            int nextBase = (i + 1) * radialSegments;

            for (int j = 0; j < radialSegments; j++) {
                int a = baseIndex + j;
                int b = baseIndex + (j + 1) % radialSegments;
                int c = nextBase + j;
                int d = nextBase + (j + 1) % radialSegments;

                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);

                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}