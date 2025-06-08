// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SkiaSharp;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Image manipulation functions using SkiaSharp.
    /// </summary>
    internal static class ImageUtils
    {
        /// <summary>
        /// Downsizes an image, so it fits into the <paramref name="maxWidthOrHeight"/>
        /// sqare bounding box and keeps its aspect ration.
        /// </summary>
        /// <param name="imageContent">The content of the image, usually read from an image file.</param>
        /// <param name="maxWidthOrHeight">The width and height of the bounding box.</param>
        /// <param name="allowedEncodings">List of encodings which are accepted, if this parameter
        /// is null or empty, the original encoding of the image is kept.</param>
        /// <param name="quality">A value of the range 0-100, used for the compression. For encodings
        /// which accept a quality, this usually means that more quality results in a bigger file.
        /// A value of 100 means top quality.</param>
        /// <returns>An image container holding information about the downsized image.</returns>
        public static ImageContainer Downsize(
            byte[] imageContent,
            int maxWidthOrHeight,
            IEnumerable<ImageType> allowedEncodings,
            int quality = 94)
        {
            using (MemoryStream originalStream = new MemoryStream(imageContent))
            using (var originalData = SKData.Create(originalStream))
            using (var originalCodec = SKCodec.Create(originalData))
            using (var originalBitmap = SKBitmap.Decode(originalCodec))
            {
                BitmapSize originalSize = new BitmapSize(originalBitmap.Width, originalBitmap.Height);
                BitmapSize targetSize = ShrinkSizeKeepRatio(originalSize, maxWidthOrHeight);

                if (allowedEncodings == null)
                    allowedEncodings = Array.Empty<ImageType>();
                List<SKEncodedImageFormat> encodingsToTry = allowedEncodings
                    .Select(item => ToEncodedImageFormat(item, originalCodec.EncodedFormat))
                    .Distinct()
                    .ToList();
                if (encodingsToTry.Count == 0)
                    encodingsToTry.Add(originalCodec.EncodedFormat);

                using (SKBitmap resizedBitmap = originalBitmap.Resize(ToSKSize(targetSize), SKFilterQuality.High))
                {
                    // Try out which encoding is best and return the image in this encoding.
                    ImageContainer result = null;
                    foreach (SKEncodedImageFormat targetEncoding in encodingsToTry)
                    {
                        using (SKData resizedImageData = resizedBitmap.Encode(targetEncoding, quality))
                        {
                            ImageContainer targetImage = new ImageContainer
                            {
                                ImageType = ToImageType(targetEncoding).Value,
                                ImageContent = resizedImageData.ToArray()
                            };
                            result = ChooseBetterEncoding(targetImage, result);
                        }
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Calculates the size for an image, so it fits into the <paramref name="maxWidthOrHeight"/>
        /// sqare bounding box and keeps its aspect ration.
        /// </summary>
        /// <param name="size">The size of the bitmap.</param>
        /// <param name="maxWidthOrHeight">The size of the square bounding box.</param>
        /// <returns>Size which fits into the bounding box.</returns>
        public static BitmapSize ShrinkSizeKeepRatio(BitmapSize size, int maxWidthOrHeight)
        {
            ArgumentNullException.ThrowIfNull(size);

            // Keep size if it already fits to the max size
            if ((size.Width <= maxWidthOrHeight) && (size.Height <= maxWidthOrHeight))
                return size;

            bool isLandscape = size.Width > size.Height;
            double ratio = (double)size.Width / (double)size.Height;
            if (isLandscape)
                return new BitmapSize(maxWidthOrHeight, (int)(maxWidthOrHeight / ratio));
            else
                return new BitmapSize((int)(maxWidthOrHeight * ratio), maxWidthOrHeight);
        }

        private static ImageContainer ChooseBetterEncoding(ImageContainer image1, ImageContainer image2)
        {
            if ((image1 == null) || (image2 == null))
                return image1 ?? image2;

            double size1 = image1.ImageContent.Length * GetEncodingBonusFactor(image1.ImageType);
            double size2 = image2.ImageContent.Length * GetEncodingBonusFactor(image2.ImageType);
            return (size1 < size2) ? image1 : image2;
        }

        private static double GetEncodingBonusFactor(ImageType imageType)
        {
            switch (imageType)
            {
                case ImageType.Png:
                    return 0.8; // This encoding is slightly preferred and can be 20% bigger in size
                default:
                    return 1.0;
            }
        }

        private static SKEncodedImageFormat ToEncodedImageFormat(ImageType imageType, SKEncodedImageFormat originalImageFormat)
        {
            switch (imageType)
            {
                case ImageType.KeepOriginal:
                    return originalImageFormat;
                case ImageType.Jpeg:
                    return SKEncodedImageFormat.Jpeg;
                case ImageType.Png:
                    return SKEncodedImageFormat.Png;
                default:
                    throw new ArgumentOutOfRangeException(nameof(originalImageFormat));
            }
        }

        private static ImageType? ToImageType(SKEncodedImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case SKEncodedImageFormat.Jpeg:
                    return ImageType.Jpeg;
                case SKEncodedImageFormat.Png:
                    return ImageType.Png;
                default:
                    return null;
            }
        }

        private static SKSizeI ToSKSize(BitmapSize size)
        {
            return new SKSizeI(size.Width, size.Height);
        }
    }

    /// <summary>
    /// The Image container holds information about a loaded image.
    /// </summary>
    public class ImageContainer
    {
        /// <summary>
        /// Gets or sets the type of the image, for bitmaps this is the encoding.
        /// </summary>
        public ImageType ImageType { get; set; }

        /// <summary>
        /// Gets or sets the content of the image, which can be stored into a file.
        /// </summary>
        public byte[] ImageContent { get; set; }
    }

    /// <summary>
    /// Enumeration of all supported image types.
    /// </summary>
    public enum ImageType
    {
        /// <summary>Special state indicating that the images original image type shoudl be kept.</summary>
        KeepOriginal,

        /// <summary>The image is Jpeg encoded.</summary>
        Jpeg,

        /// <summary>The image is Png encoded.</summary>
        Png,
    }

    /// <summary>
    /// The size of a bitmap image.
    /// </summary>
    public class BitmapSize
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapSize"/> class.
        /// </summary>
        /// <param name="width">Sets the <see cref="Width"/> property.</param>
        /// <param name="height">Sets the <see cref="Height"/> property.</param>
        public BitmapSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>Gets the horizontal width in pixels of the image.</summary>
        public int Width { get; }

        /// <summary>Gets the vertical height in pixels of the image.</summary>
        public int Height { get; }
    }
}
