using System.Drawing;

namespace MyApps
{
    public static class Colors
    {
        // Primary Action Color
        public static Color Primary = Color.FromArgb(0, 122, 204);       // Azure Blue (VS Code style)
        public static Color PrimaryHover = Color.FromArgb(0, 100, 180);
        public static Color PrimaryClick = Color.FromArgb(0, 80, 160);
        
        // Background & Structure
        public static Color DarkPrimary = Color.FromArgb(28, 28, 28);    // Main Content Background (Very Dark)
        public static Color DarkSecondary = Color.FromArgb(37, 37, 38); // Header & Sidebar Background
        public static Color DarkTertiary = Color.FromArgb(51, 51, 55);  // Button/Input Background (Hover on DarkSecondary)
        public static Color DarkHover = Color.FromArgb(60, 60, 65);     // Button Hover on DarkTertiary
        
        // Borders & Separators
        public static Color Border = Color.FromArgb(60, 60, 60);        // Subtle Border Color
        
        // Text & Icons
        public static Color TextPrimary = Color.FromArgb(240, 240, 240); // Near White Text
        public static Color TextSecondary = Color.FromArgb(170, 170, 180); // Muted Text
        public static Color TextMuted = Color.FromArgb(100, 100, 110);   // Very Muted Text

        // Histogram Chart Colors (Modern Flat UI)
        public static Color RedChannel = Color.FromArgb(231, 76, 60);    
        public static Color GreenChannel = Color.FromArgb(46, 204, 113); 
        public static Color BlueChannel = Color.FromArgb(52, 152, 219);  
        public static Color GrayChannel = Color.FromArgb(149, 165, 166); 
        public static Color GridLine = Color.FromArgb(50, 50, 50);       // Darker Grid for contrast
    }
}