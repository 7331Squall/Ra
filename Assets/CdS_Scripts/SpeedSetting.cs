public enum SpeedSettings {
    // TwoMinutes,
    // FiveMinutes,
    FifteenMinutes,
    ThirtyMinutes,
    OneHour,
    TwoHours,
    ThreeHours,
    SixHours,
    TwelveHours,
    OneDay
}

public enum IntervalSettings {
    Continuous,
    OneWeek,
    TwoWeeks,
    OneMonth,
    ThreeMonths
}

public static class SpeedSetting {
    public static string ToDisplayString(SpeedSettings value) {
        return value switch {
            // SpeedSettings.TwoMinutes     => "2 min/s",
            // SpeedSettings.FiveMinutes    => "5 min/s",
            SpeedSettings.FifteenMinutes => "15 min/s",
            SpeedSettings.ThirtyMinutes  => "30 min/s",
            SpeedSettings.OneHour        => "1 hora/s",
            SpeedSettings.TwoHours       => "2 horas/s",
            SpeedSettings.ThreeHours     => "3 horas/s",
            SpeedSettings.SixHours       => "6 horas/s",
            SpeedSettings.TwelveHours    => "12 horas/s",
            SpeedSettings.OneDay         => "1 dia/s",
            var _                        => value.ToString()
        };
    }

    public static int SpeedInSeconds(int value) {
        return SpeedInSeconds((SpeedSettings) value);
    }

    public static int SpeedInSeconds(SpeedSettings value) {
        return value switch {
            // SpeedSettings.TwoMinutes     => 120,
            // SpeedSettings.FiveMinutes    => 300,
            SpeedSettings.FifteenMinutes => 900,
            SpeedSettings.ThirtyMinutes  => 1800,
            SpeedSettings.OneHour        => 3600,
            SpeedSettings.TwoHours       => 7200,
            SpeedSettings.ThreeHours     => 10800,
            SpeedSettings.SixHours       => 21600,
            SpeedSettings.TwelveHours    => 43200,
            SpeedSettings.OneDay         => 86400,
            // SpeedSettings.OneWeek      => 604800,
            // SpeedSettings.TwoWeeks       => 1209600,
            // SpeedSettings.OneMonth       => 2592000,
            // SpeedSettings.ThreeMonths    => 7776000,
            var _ => (int) value
        };
    }
}


public static class IntervalSetting {
    public static string ToDisplayString(IntervalSettings value) {
        return value switch {
            IntervalSettings.Continuous  => "Contínuo",
            IntervalSettings.OneWeek     => "7 dias",
            IntervalSettings.TwoWeeks    => "14 dias",
            IntervalSettings.OneMonth    => "1 mês",
            IntervalSettings.ThreeMonths => "3 meses",
            var _                        => value.ToString()
        };
    }

    public static int IntervalInDays(int value) {
        return IntervalInDays((IntervalSettings) value);
    }

    public static int IntervalInDays(IntervalSettings value) {
        return value switch {
            IntervalSettings.OneWeek     => 7,
            IntervalSettings.TwoWeeks    => 14,
            IntervalSettings.OneMonth    => 30,
            IntervalSettings.ThreeMonths => 90,
            var _                        => (int) value
        };
    }
}