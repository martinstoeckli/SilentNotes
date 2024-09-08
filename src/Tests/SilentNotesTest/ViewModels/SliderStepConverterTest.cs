using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class SliderStepConverterTest
    {
        [TestMethod]
        public void SliderStepToModelFactor_CalculatesCorrectly()
        {
            SliderStepConverter converter = new SliderStepConverter(100, 10);
            double factor;

            // Step 0 is factor 1.0
            factor = converter.SliderStepToModelFactor(0);
            Assert.AreEqual(1.0, factor);

            factor = converter.SliderStepToModelFactor(1);
            Assert.AreEqual(110.0 / 100.0, factor);

            factor = converter.SliderStepToModelFactor(-2);
            Assert.AreEqual(80.0 / 100.0, factor);
        }

        [TestMethod]
        public void ModelFactorToSliderStep_CalculatesCorrectly()
        {
            SliderStepConverter converter = new SliderStepConverter(100, 10);
            int step;

            // Factor 1.0 is step 0
            step = converter.ModelFactorToSliderStep(1.0);
            Assert.AreEqual(0, step);

            step = converter.ModelFactorToSliderStep(1.1);
            Assert.AreEqual(1, step);

            step = converter.ModelFactorToSliderStep(0.8);
            Assert.AreEqual(-2, step);
        }

        [TestMethod]
        public void ModelFactorToSliderStep_RoundsToSteps()
        {
            SliderStepConverter converter = new SliderStepConverter(100, 10);
            int step;

            step = converter.ModelFactorToSliderStep(1.1 + 0.01);
            Assert.AreEqual(1, step);

            step = converter.ModelFactorToSliderStep(1.1 - 0.01);
            Assert.AreEqual(1, step);
        }

        [TestMethod]
        public void ModelFactorToValue_CalculatesCorrectly()
        {
            SliderStepConverter converter = new SliderStepConverter(100, 10);
            double value;

            value = converter.ModelFactorToValue(1.1);
            Assert.AreEqual(100.0 * 1.1, value);

            value = converter.ModelFactorToValue(0.9);
            Assert.AreEqual(100.0 * 0.9, value);
        }

        [TestMethod]
        public void ModelFactorToValueAsInt_RoundsCorrectly()
        {
            SliderStepConverter converter = new SliderStepConverter(100, 10);
            int value;

            value = converter.ModelFactorToValueAsInt(1.1);
            Assert.AreEqual(110, value);

            value = converter.ModelFactorToValueAsInt(1.004);
            Assert.AreEqual(100, value);

            value = converter.ModelFactorToValueAsInt(1.0051);
            Assert.AreEqual(101, value);

            value = converter.ModelFactorToValueAsInt(0.9951);
            Assert.AreEqual(100, value);

            value = converter.ModelFactorToValueAsInt(0.994);
            Assert.AreEqual(99, value);
        }
    }
}
