// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Reflection;
using SilentNotes.ViewModels;

namespace SilentNotes.Services
{
    /// <summary>
    /// Generic implementation of the <see cref="IRazorViewService"/> interface.
    /// </summary>
    /// <typeparam name="RazorViewClass">The type of the view can be used by this service.</typeparam>
    public class RazorViewService<RazorViewClass> : IRazorViewService where RazorViewClass : new()
    {
        /// <inheritdoc/>
        public string GenerateHtml(ViewModelBase viewmodel)
        {
            // Create new razor view
            RazorViewClass view = new RazorViewClass();

            // Set model of razor view
            PropertyInfo property = view.GetType().GetProperty("Model", BindingFlags.Public | BindingFlags.Instance);
            property.SetValue(view, viewmodel, null);

            // Generate html
            MethodInfo method = view.GetType().GetMethod("GenerateString");
            object html = method.Invoke(view, null);
            return html.ToString();
        }
    }
}
