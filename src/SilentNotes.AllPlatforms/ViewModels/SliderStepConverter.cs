// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Converts the integer steps of a range input from the view, to a factor which can be stored
    /// in the model. The slider control can then work with integer steps, e.g.
    /// slider steps  -2,    -1,     0,    +1,    +2
    /// real values   80,    90,   100,   110,   120
    /// stored factors 0.8,   0.9,   1.0,   1.1,   1.2.
    /// Storing a factor instead of the absolute value in the model, allows later to change the
    /// number of steps easily without the need of recalculation of the stored value.
    /// </summary>
    public class SliderStepConverter
    {
        private readonly double _valueOfStep0;
        private readonly double _valueDifferencePerStep;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderStepConverter"/> class.
        /// </summary>
        /// <param name="valueOfStep0">The value that will be represented with step 0 in the view.</param>
        /// <param name="valueDifferencePerStep">The difference of the value when the view slider is moved one step.</param>
        public SliderStepConverter(double valueOfStep0, double valueDifferencePerStep)
        {
            _valueOfStep0 = valueOfStep0;
            _valueDifferencePerStep = valueDifferencePerStep;
        }

        /// <summary>
        /// Converts the current step of the slider in the settings view, to the factor which can
        /// be stored in the model.
        /// </summary>
        /// <param name="sliderStep">Current step of the slider in the settings view.</param>
        /// <returns>Factor to store in the model.</returns>
        public double SliderStepToModelFactor(int sliderStep)
        {
            return (_valueOfStep0 + (sliderStep * _valueDifferencePerStep)) / _valueOfStep0;
        }

        /// <summary>
        /// Converts the factor stored in the model, to the step of the slider in the settings view.
        /// </summary>
        /// <param name="modelFactor">Factor stored in the model.</param>
        /// <returns>Step number of the slider in the settings view.</returns>
        public int ModelFactorToSliderStep(double modelFactor)
        {
            double value = ModelFactorToValue(modelFactor);
            double step = (value - _valueOfStep0) / _valueDifferencePerStep;
            return (int)Math.Round(step, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Converts the factor stored in the model, to the real value used in the view.
        /// </summary>
        /// <param name="modelFactor">Factor stored in the model.</param>
        /// <returns>Real value used by the view.</returns>
        public double ModelFactorToValue(double modelFactor)
        {
            return _valueOfStep0 * modelFactor;
        }

        /// <summary>
        /// Converts the factor stored in the model, to the real value used in the view.
        /// The value is rounded to the next int value.
        /// </summary>
        /// <param name="modelFactor">Factor stored in the model.</param>
        /// <returns>Real value used by the view.</returns>
        public int ModelFactorToValueAsInt(double modelFactor)
        {
            double value = ModelFactorToValue(modelFactor);
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}
