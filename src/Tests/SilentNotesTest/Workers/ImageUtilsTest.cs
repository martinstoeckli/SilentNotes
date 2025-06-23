using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class ImageUtilsTest
    {
        [TestMethod]
        public void ShrinkSizeKeegRatio_WhenSmallEnough_KeepsSize()
        {
            var size = new BitmapDimension(6, 4);
            var res = ImageUtils.ShrinkDimensionKeepingRatio(size, 8);
            Assert.AreEqual(6, res.Width);
            Assert.AreEqual(4, res.Height);
        }

        [TestMethod]
        public void ShrinkSizeKeepRatio_WhenNull_Throws()
        {
            BitmapDimension size = null;
            Assert.ThrowsException<ArgumentNullException>(() => ImageUtils.ShrinkDimensionKeepingRatio(size, 8));
        }

        [TestMethod]
        public void ShrinkSizeKeepRatio_KeepsRatio()
        {
            var size = new BitmapDimension(27, 9);
            var res = ImageUtils.ShrinkDimensionKeepingRatio(size, 18);
            Assert.AreEqual(18, res.Width);
            Assert.AreEqual(6, res.Height);

            size = new BitmapDimension(9, 27);
            res = ImageUtils.ShrinkDimensionKeepingRatio(size, 18);
            Assert.AreEqual(6, res.Width);
            Assert.AreEqual(18, res.Height);
        }

        [TestMethod]
        [DeploymentItem(@"Resources/win_png.silentnotes_att")]
        [DeploymentItem(@"Resources/win_jpg.silentnotes_att")]
        [DeploymentItem(@"Resources/and_png.silentnotes_att")]
        [DeploymentItem(@"Resources/and_jpg.silentnotes_att")]
        public void Downsize_ChosesBestEncoding()
        {
            // Load ImagePicker examples from Windows/Android png/jpeg images.
            ImageInfo[] originalImages = new ImageInfo[]
            {
                new ImageInfo { ImageType = ImageType.Png, ImageContent = LoadSampleFile("win_png.silentnotes_att") }, // screenshot style image
                new ImageInfo { ImageType = ImageType.Jpeg, ImageContent = LoadSampleFile("win_jpg.silentnotes_att") }, // photo style image
                new ImageInfo { ImageType = ImageType.Png, ImageContent = LoadSampleFile("and_png.silentnotes_att") }, // screenshot style image
                new ImageInfo { ImageType = ImageType.Jpeg, ImageContent = LoadSampleFile("and_jpg.silentnotes_att") }, // photo style image
            };

            foreach (var originalImage in originalImages)
            {
                ImageInfo smallImage = ImageUtils.Downsize(originalImage.ImageContent, 1024, true);
                Assert.IsNotNull(smallImage);
                Assert.AreEqual(originalImage.ImageType, smallImage.ImageType);
            }
        }

        private static byte[] LoadSampleFile(string fileName)
        {
            return File.ReadAllBytes(fileName);
        }
    }
}
