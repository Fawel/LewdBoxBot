namespace LBox.Shared
{
    public class NewHotImageRestrictions
    {
        public readonly IntNaturalRange ImageSizeKb;
        public readonly IntNaturalRange Width;
        public readonly IntNaturalRange Height;

        public bool CheckImageParameters(
            int? imageSize = default,
            int? width = default,
            int? height = default) 
        {
            return ImageSizeKb.CheckInRange(imageSize)
                && Width.CheckInRange(width)
                && Height.CheckInRange(height);
        }

        public static NewHotImageRestrictions NoRestrictions =
            new NewHotImageRestrictions(
                IntNaturalRange.CreateEmptyRange(),
                IntNaturalRange.CreateEmptyRange(),
                IntNaturalRange.CreateEmptyRange());

        public NewHotImageRestrictions(IntNaturalRange imageSizeKb, IntNaturalRange width, IntNaturalRange height)
        {
            ImageSizeKb = imageSizeKb;
            Width = width;
            Height = height;
        }
    }

    public class Restriction
    {
        
    }
}
