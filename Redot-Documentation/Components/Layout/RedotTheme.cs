using MudBlazor;

namespace Redot_Documentation.Components.Layout;

public static class RedotTheme
{
    public static MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#FF3B0A",
            PrimaryContrastText = "#090909",
            Secondary = "#D92F0B",
            Background = "#09090B",
            BackgroundGray = "#0E0E12",
            Surface = "#17171B",
            AppbarBackground = "#070708",
            DrawerBackground = "#111114",
            TextPrimary = "#F5F5F5",
            TextSecondary = "#B0B0B8",
            ActionDefault = "#B0B0B8",
            LinesDefault = "#2A2A30",
            Divider = "#26262B"
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "10px", DrawerWidthLeft = "300px" },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Helvetica Neue", "Helvetica", "Arial", "sans-serif"] }
        }
    };
}
