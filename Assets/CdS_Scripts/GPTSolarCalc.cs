using System;
using UnityEngine;

// ReSharper disable InconsistentNaming
public static class GPTSolarCalc {
    // [Header("Inputs")]
    // [Range(-90f, 90f)]
    // public float latitude;      // graus (positivo = norte)
    // [Range(-180f, 180f)]
    // public float longitude;     // graus (positivo = leste)
    // public int day;             // 1-31
    // public int month;           // 1-12
    // [Range(0,23)]
    // public int hour;            // 0-23
    // [Range(0,59)]
    // public int minute;          // 0-59
    // public float timeZone = 0;  // horas de UTC (ex: -3 para Brasil)
    //
    // [Header("Light")]
    // public Light sunLight;

    // public static Quaternion GetCelestialSphereRotation(float latitude, DateTime dateTime) {
    //     double tst = dateTime.Hour * 60.0 + dateTime.Minute;
    //     // A esfera celeste completa uma volta em 23h56min → ≈ 360° / 23.9344h
    //     float anguloSideral = (360f / 23.9344f) * (float) tst;
    //     //esferaCeleste.localRotation = 
    //     return Quaternion.Euler(0f, anguloSideral, 0f);
    // }

    public static (Vector3 position, Quaternion rotation) GetPositionNOAA(float latitude, DateTime dateTime) {
// 1) Dados do tempo
        int day = dateTime.Day;
        int month = dateTime.Month;
        int hour = dateTime.Hour;
        int minute = dateTime.Minute;

// 2) Dia do ano
        DateTime dt = new(2000, month, day, hour, minute, 0);
        int N = dt.DayOfYear;

// 3) Ângulo do ano (γ)
        double gamma = 2.0 * Math.PI / 365.0 * (N - 1 + (hour - 12.0) / 24.0 + minute / 1440.0);

// 4) Declinação solar δ
        double delta = 0.006918
          - 0.399912 * Math.Cos(gamma)
          + 0.070257 * Math.Sin(gamma)
          - 0.006758 * Math.Cos(2 * gamma)
          + 0.000907 * Math.Sin(2 * gamma)
          - 0.002697 * Math.Cos(3 * gamma)
          + 0.001480 * Math.Sin(3 * gamma);

// 5) Tempo solar verdadeiro simplificado (minutos)
        double tst = hour * 60.0 + minute;

// 6) Ângulo horário H (radianos)
        double hourAngle = (tst / 4.0 - 180.0) * Mathf.Deg2Rad;

// 7) Elevação e azimute (para t = agora)
        double latRad = latitude * Mathf.Deg2Rad;
        double cosZenith = Math.Sin(latRad) * Math.Sin(delta) + Math.Cos(latRad) * Math.Cos(delta) * Math.Cos(hourAngle);
        double zenith = Math.Acos(cosZenith);
        double elevation = Math.PI / 2.0 - zenith;
        double sinAz = -Math.Cos(delta) * Math.Sin(hourAngle) / Math.Sin(zenith);
        double cosAz = (Math.Sin(delta) - Math.Sin(latRad) * Math.Cos(zenith)) / (Math.Cos(latRad) * Math.Sin(zenith));
        double azimuth = Math.Atan2(sinAz, cosAz);
        if (azimuth < 0) azimuth += 2 * Math.PI;

// 8) Vetor direção Unity (X=leste, Y=cima, Z=norte)
        float x = (float) (Math.Cos(elevation) * Math.Sin(azimuth));
        float y = (float) Math.Sin(elevation);
        float z = (float) (Math.Cos(elevation) * Math.Cos(azimuth));
        Vector3 sunDir = new Vector3(x, y, z).normalized;

// 9) Inclinação da Terra
        const float tilt = 23.44f;
        Quaternion rotation;

// --- Mantém exatamente o comportamento do Sol (inalterado) ---
        Vector3 northCelestial = Quaternion.Euler(tilt, 0, 0) * Vector3.up;

// calcula up estável baseado no eixo do planeta (northCelestial)
        Vector3 right = Vector3.Cross(northCelestial, -sunDir);
        if (right.sqrMagnitude < 1e-6f) {
// fallback (evita degenerescência)
            right = Vector3.Cross(Vector3.forward, -sunDir);
        }
        right.Normalize();
        Vector3 up = Vector3.Cross(-sunDir, right).normalized;

        rotation = Quaternion.LookRotation(-sunDir, up);

        return (sunDir, rotation);
    }

    /// <summary>
    /// Retorna a rotação da esfera para alinhar seu eixo Y com o pólo celeste
    /// e aplicar a rotação em torno desse eixo de acordo com o DateTime (LST).
    /// longitudeDeg: longitude do observador, latitudeDeg: latitude do observador.
    /// dateTimeUtc: data/hora UTC
    /// </summary>
    public static Quaternion OrientationForCelestialPole(float latitudeDeg, DateTime dateTimeUtc) {
        // 1) Julian Date
        double Y = dateTimeUtc.Year;
        double M = dateTimeUtc.Month;
        double D = dateTimeUtc.Day + (dateTimeUtc.Hour + dateTimeUtc.Minute / 60.0 + dateTimeUtc.Second / 3600.0) / 24.0;
        if (M <= 2) {
            Y -= 1;
            M += 12;
        }
        int A = (int) Math.Floor(Y / 100.0);
        int B = 2 - A + (int) Math.Floor(A / 4.0);
        double JD = Math.Floor(365.25 * (Y + 4716)) + Math.Floor(30.6001 * (M + 1)) + D + B - 1524.5;

        double T = (JD - 2451545.0) / 36525.0;

        // 2) GMST em graus
        double GMST = (280.46061837 + 360.98564736629 * (JD - 2451545.0) + 0.000387933 * T * T - (T * T * T) / 38710000.0) % 360.0;
        // GMST = GMST % 360.0;
        if (GMST < 0) GMST += 360.0;

        // 3) LST em graus
        double LST = GMST % 360.0;
        if (LST < 0) LST += 360.0;

        // 4) Direção do polo celeste
        float latRad = Mathf.Deg2Rad * latitudeDeg;
        Vector3 poleDir = new Vector3(0f, Mathf.Sin(latRad), Mathf.Cos(latRad)).normalized; // +Y da esfera

        // 5) Rotação que alinha o eixo Y da esfera com o polo
        Quaternion alignPole = Quaternion.FromToRotation(Vector3.up, poleDir);

        // 6) Rotação em torno do eixo Y da esfera (já inclinado) pelo LST
        Quaternion rotAroundY = Quaternion.AngleAxis((float) LST, Vector3.up);

        // 7) Combina: primeiro inclina, depois gira em torno do eixo da esfera
        Quaternion finalRot = alignPole * rotAroundY;

        return finalRot;
    }

    // Constantes básicas
    const float ORBITAL_PERIOD_DAYS = 365.25f; // Um ano
    const float AXIAL_TILT_DEGREES = 23.44f;   // Inclinação da Terra

    // Data de referência: 21 de março (equinócio), Terra em Z+
    static readonly DateTime epoch = new DateTime(2000, 3, 21, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Calcula a posição e rotação da Terra em sua órbita ao redor do Sol.
    /// </summary>
    public static (Vector3 position, Quaternion rotation) GetEarthTransform(DateTime dateTime) {
        // Converter para UTC e calcular dias desde o "epoch"
        double daysSinceEpoch = (dateTime.ToUniversalTime() - epoch).TotalDays;

        // Fração da órbita completada
        float orbitFraction = (float) ((daysSinceEpoch % ORBITAL_PERIOD_DAYS) / ORBITAL_PERIOD_DAYS);

        // Ângulo orbital (radianos)
        float angle = orbitFraction * Mathf.PI * 2f;

        // Direção da Terra (plano XZ)
        Vector3 dir = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

        // Aplicar inclinação do eixo orbital (em torno do eixo X)
        Quaternion tilt = Quaternion.Euler(AXIAL_TILT_DEGREES, 0f, 0f);
        Vector3 inclinedDir = tilt * dir;

        // Rotação do planeta (giro próprio da Terra)
        // ~360° por dia (um dia sideral ≈ 23h56min)
        float spinAngle = (float) ((dateTime.TimeOfDay.TotalHours / 24f) * 360f);
        Quaternion spin = Quaternion.Euler(0f, spinAngle, 0f);

        // Rotação total da Terra (inclinação + rotação própria)
        Quaternion rotation = tilt * spin;

        return (inclinedDir, rotation);
    }
}