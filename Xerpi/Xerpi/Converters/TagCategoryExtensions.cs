using Xamarin.Forms;
using Xerpi.Models.API;

namespace Xerpi.Converters
{
    public static class TagCategoryExtensions

    {
        private static readonly Color RatingBackgroundColor = Color.FromRgb(193, 215, 228);
        private static readonly Color RatingForegroundColor = Color.FromRgb(38, 126, 191);

        private static readonly Color OriginBackgroundColor = Color.FromRgb(185, 188, 225);
        private static readonly Color OriginForegroundColor = Color.FromRgb(57, 63, 133);

        private static readonly Color CharacterBackgroundColor = Color.FromRgb(181, 223, 216);
        private static readonly Color CharacterForegroundColor = Color.FromRgb(45, 134, 119);

        private static readonly Color OCBackgroundColor = Color.FromRgb(222, 197, 226);
        private static readonly Color OCForegroundColor = Color.FromRgb(152, 82, 163);

        private static readonly Color SpeciesBackgroundColor = Color.FromRgb(230, 201, 181);
        private static readonly Color SpeciesForegroundColor = Color.FromRgb(139, 85, 47);

        private static readonly Color NoneBackgroundColor = Color.FromRgb(208, 226, 156);
        private static readonly Color NoneForegroundColor = Color.FromRgb(111, 143, 14);

        private static readonly Color ContentOfficialBackgroundColor = Color.FromRgb(237, 230, 151);
        private static readonly Color ContentOfficialForegroundColor = Color.FromRgb(153, 152, 26);

        private static readonly Color SpoilersBackgroundColor = Color.FromRgb(244, 205, 194);
        private static readonly Color SpoilersForegroundColor = Color.FromRgb(194, 69, 35);

        private static readonly Color ContentFanmadeBackgroundColor = Color.FromHex("#efd7e7");
        private static readonly Color ContentFanmadeForegroundColor = Color.FromHex("#bb5496");



        public static Color BackgroundColor(this TagCategory category)
        {
            return category switch
            {
                TagCategory.Character => CharacterBackgroundColor,
                TagCategory.ContentFanmade => ContentFanmadeBackgroundColor,
                TagCategory.ContentOfficial => ContentOfficialBackgroundColor,
                TagCategory.None => NoneBackgroundColor,
                TagCategory.Rating => RatingBackgroundColor,
                TagCategory.Origin => OriginBackgroundColor,
                TagCategory.OC => OCBackgroundColor,
                TagCategory.Species => SpeciesBackgroundColor,
                TagCategory.Spoiler => SpoilersBackgroundColor,
                TagCategory.Unmapped => NoneBackgroundColor,
                _ => NoneBackgroundColor
            };
        }

        public static Color ForegroundColor(this TagCategory category)
        {
            return category switch
            {
                TagCategory.Character => CharacterForegroundColor,
                TagCategory.ContentFanmade => ContentFanmadeForegroundColor,
                TagCategory.ContentOfficial => ContentOfficialForegroundColor,
                TagCategory.None => NoneForegroundColor,
                TagCategory.Rating => RatingForegroundColor,
                TagCategory.Origin => OriginForegroundColor,
                TagCategory.OC => OCForegroundColor,
                TagCategory.Species => SpeciesForegroundColor,
                TagCategory.Spoiler => SpoilersForegroundColor,
                TagCategory.Unmapped => NoneForegroundColor,
                _ => NoneForegroundColor
            };
        }
    }
}
