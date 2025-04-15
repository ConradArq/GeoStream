using MudBlazor;

namespace GeoStream.Themes
{
    public static class GeoStreamTheme
    {
        public static MudTheme DarkTheme = new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Black = "#121212",
                Background = "#1E1E2F",            // main background
                Surface = "#2A2A3D",               // card/form background
                DrawerBackground = "#1C1C28",      // sidebar background
                AppbarBackground = "#2D2D40",      // top bar
                DrawerText = "#ECEFF4",            // sidebar text
                TextPrimary = "#FFFFFF",           // main text
                TextSecondary = "#B0B8C0",         // subtitles, form labels
                ActionDefault = "#5EC4B8",         // icon buttons
                LinesDefault = "#3D3D5C",          // dividers, outlines
                Primary = "#5EC4B8",               // soft teal for buttons and accents
                Secondary = "#FF6B81"              // soft red-pink for highlights
            },
            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "Arial", "sans-serif" },
                    FontSize = "0.95rem",
                    FontWeight = 400
                },
                H5 = new H5
                {
                    FontSize = "1.4rem",
                    FontWeight = 600,
                    LineHeight = 1.4
                },
                H6 = new H6
                {
                    FontSize = "1.1rem",
                    FontWeight = 600,
                    LineHeight = 1.4
                },
                Button = new Button
                {
                    FontWeight = 500,
                    TextTransform = "none"
                }
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px"
            }
        };
    }
}
