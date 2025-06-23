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
        /// <param name="chooseBestEncoding">If true, the best encoding for the type of image
        /// (photo style/vector style) is detected and used for the resulting imag. If false, the
        /// original encoding is kept if possible, or if unknown JPEG is choosen..</param>
        /// <param name="quality">A value of the range 0-100, used for the compression. For encodings
        /// which accept a quality, this usually means that more quality results in a bigger file.
        /// A value of 100 means top quality.</param>
        /// <returns>An image container holding information about the downsized image.</returns>
        public static ImageInfo Downsize(
            byte[] imageContent,
            int maxWidthOrHeight,
            bool chooseBestEncoding = true,
            int quality = 94)
        {
            using (MemoryStream originalStream = new MemoryStream(imageContent))
            using (var originalData = SKData.Create(originalStream))
            using (var originalCodec = SKCodec.Create(originalData))
            using (var originalBitmap = SKBitmap.Decode(originalCodec))
            {
                BitmapDimension originalDimension = new BitmapDimension(originalBitmap.Width, originalBitmap.Height);
                BitmapDimension targetDimension = ShrinkDimensionKeepingRatio(originalDimension, maxWidthOrHeight);

                List<SKEncodedImageFormat> encodingsToTry = new List<SKEncodedImageFormat>();
                if (chooseBestEncoding)
                {
                    encodingsToTry.AddRange(new[]
                    {
                        ToEncodedImageFormat(ImageType.Jpeg),
                        ToEncodedImageFormat(ImageType.Png)
                    });
                }
                else
                {
                    bool isKnownEncoding = ToImageType(originalCodec.EncodedFormat).HasValue;
                    if (isKnownEncoding)
                        encodingsToTry.Add(originalCodec.EncodedFormat);
                    else
                        encodingsToTry.Add(ToEncodedImageFormat(ImageType.Jpeg));
                }

                using (SKBitmap resizedBitmap = originalBitmap.Resize(ToSKSize(targetDimension), SKFilterQuality.High))
                {
                    // Try out which encoding is best and return the image in this encoding.
                    ImageInfo result = null;
                    foreach (SKEncodedImageFormat targetEncoding in encodingsToTry)
                    {
                        using (SKData resizedImageData = resizedBitmap.Encode(targetEncoding, quality))
                        {
                            ImageInfo targetImage = new ImageInfo
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
        public static BitmapDimension ShrinkDimensionKeepingRatio(BitmapDimension size, int maxWidthOrHeight)
        {
            ArgumentNullException.ThrowIfNull(size);

            // Keep size if it already fits to the max size
            if ((size.Width <= maxWidthOrHeight) && (size.Height <= maxWidthOrHeight))
                return size;

            bool isLandscape = size.Width > size.Height;
            double ratio = (double)size.Width / (double)size.Height;
            if (isLandscape)
                return new BitmapDimension(maxWidthOrHeight, (int)(maxWidthOrHeight / ratio));
            else
                return new BitmapDimension((int)(maxWidthOrHeight * ratio), maxWidthOrHeight);
        }

        private static ImageInfo ChooseBetterEncoding(ImageInfo image1, ImageInfo image2)
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
                    return 0.8; // This encoding is slightly preferred and is allowed to be 20% bigger in size
                default:
                    return 1.0;
            }
        }

        private static SKEncodedImageFormat ToEncodedImageFormat(ImageType imageType)
        {
            switch (imageType)
            {
                case ImageType.Jpeg:
                    return SKEncodedImageFormat.Jpeg;
                case ImageType.Png:
                    return SKEncodedImageFormat.Png;
                default:
                    throw new ArgumentOutOfRangeException(nameof(imageType));
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

        private static SKSizeI ToSKSize(BitmapDimension size)
        {
            return new SKSizeI(size.Width, size.Height);
        }
    }

    /// <summary>
    /// The <see cref="ImageInfo"/> holds information about a loaded image.
    /// </summary>
    public class ImageInfo
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
        /// <summary>The image is Jpeg encoded.</summary>
        Jpeg,

        /// <summary>The image is Png encoded.</summary>
        Png,
    }

    /// <summary>
    /// The size of a bitmap image in pixels.
    /// </summary>
    public class BitmapDimension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapDimension"/> class.
        /// </summary>
        /// <param name="width">Sets the <see cref="Width"/> property.</param>
        /// <param name="height">Sets the <see cref="Height"/> property.</param>
        public BitmapDimension(int width, int height)
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
