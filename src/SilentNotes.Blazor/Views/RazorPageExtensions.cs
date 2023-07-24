// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using MudBlazor;

namespace SilentNotes.Views
{
    /// <summary>
    /// Extension methods for components on a Razor page.
    /// </summary>
    internal static class RazorPageExtensions
    {
    }

    /// <summary>
    /// Helper class for razor pages, which can build css class attributes with conditions.
    /// </summary>
    public static class Css
    {
        /// <summary>
        /// Inserts the css class only if the condition is met.
        /// <example><code>
        /// class="@Css.BuildClass(new CssClassIf("toggled", IsToggled))"
        /// </code></example>
        /// </summary>
        /// <param name="classNameAndCondition">Class will be applied only if their condition is true.</param>
        /// <param name="constant">Rest of classes which should be applied in any case.</param>
        /// <returns>Space delimited list of class names.</returns>
        public static string BuildClass(CssClassIf classNameAndCondition, string constant = null)
        {
            return BuildClass(new[] { classNameAndCondition }, constant);
        }

        /// <summary>
        /// Inserts the css classes only if their condition is met.
        /// <example><code>
        /// class="@Css.BuildClass(new [] { new CssClassIf("toggled", IsToggled) })"
        /// </code></example>
        /// </summary>
        /// <param name="classNamesAndConditions">List of classes will be applied only if their
        /// conditions are true.</param>
        /// <param name="constant">Rest of css classes which should be applied in any case.</param>
        /// <returns>Space delimited list of class names.</returns>
        public static string BuildClass(IEnumerable<CssClassIf> classNamesAndConditions, string constant = null)
        {
            var classNamesWithTrueCondition = classNamesAndConditions.Where(item => item.Condition).Select(item => item.ClassName).Append(constant);
            return string.Join(" ", classNamesWithTrueCondition);
        }
    }

    /// <summary>
    /// Known css classes.
    /// </summary>
    public class CssClasses
    {
        public const string MenuToggled = "mnu-toggled";
        public const string ButtonToggled = "btn-toggled";
    }

    /// <summary>
    /// Helper class for <see cref="Css.BuildClass(CssClassIf, string)"/>.
    /// </summary>
    public class CssClassIf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CssClassIf"/> class.
        /// </summary>
        /// <param name="className">Sets the <see cref="ClassName"/> property.</param>
        /// <param name="condition">Sets the <see cref="Condition"/> property.</param>
        public CssClassIf(string className, bool condition)
        {
            ClassName = className;
            Condition = condition;
        }

        /// <summary>The css class name to apply.</summary>
        public string ClassName { get; }

        /// <summary>The condition determining whether the css class name shoudl be applied.</summary>
        public bool Condition { get; }
    }

    /// <summary>
    /// Deactivate validation because it removes the error information.
    /// See: https://github.com/MudBlazor/MudBlazor/issues/4593
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MudTextFieldEx<T> : MudTextField<T>
    {
        protected override Task ValidateValue()
        {
            if (Validation == null)
                return Task.CompletedTask;
            return base.ValidateValue();
        }
    }
}
