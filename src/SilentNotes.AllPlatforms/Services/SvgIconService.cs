// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISvgIconService"/> interface.
    /// </summary>
    internal class SvgIconService : ISvgIconService
    {
        private Dictionary<string, string> _svgResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgIconService"/> class.
        /// </summary>
        public SvgIconService()
        {
            // All icons are copied from https://materialdesignicons.com/ .
            // To add new icons, export the icon as ".SVG Optimized", remove the XML sugar and
            // replace " with ' characters.
            _svgResources = new Dictionary<string, string>
            {
                { IconNames.AlarmPlus, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M13,9H11V12H8V14H11V17H13V14H16V12H13M12,20A7,7 0 0,1 5,13A7,7 0 0,1 12,6A7,7 0 0,1 19,13A7,7 0 0,1 12,20M12,4A9,9 0 0,0 3,13A9,9 0 0,0 12,22A9,9 0 0,0 21,13A9,9 0 0,0 12,4M22,5.72L17.4,1.86L16.11,3.39L20.71,7.25M7.88,3.39L6.6,1.86L2,5.71L3.29,7.24L7.88,3.39Z' /></svg>" },
                { IconNames.Alert, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z' /></svg>" },
                { IconNames.ArrowAll, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M13,11H18L16.5,9.5L17.92,8.08L21.84,12L17.92,15.92L16.5,14.5L18,13H13V18L14.5,16.5L15.92,17.92L12,21.84L8.08,17.92L9.5,16.5L11,18V13H6L7.5,14.5L6.08,15.92L2.16,12L6.08,8.08L7.5,9.5L6,11H11V6L9.5,7.5L8.08,6.08L12,2.16L15.92,6.08L14.5,7.5L13,6V11Z' /></svg>" },
                { IconNames.ArrowCollapseDown, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19.92,12.08L12,20L4.08,12.08L5.5,10.67L11,16.17V2H13V16.17L18.5,10.66L19.92,12.08M12,20H2V22H22V20H12Z' /></svg>" },
                { IconNames.ArrowCollapseUp, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M4.08,11.92L12,4L19.92,11.92L18.5,13.33L13,7.83V22H11V7.83L5.5,13.33L4.08,11.92M12,4H22V2H2V4H12Z' /></svg>" },
                { IconNames.ArrowDown, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M11,4H13V16L18.5,10.5L19.92,11.92L12,19.84L4.08,11.92L5.5,10.5L11,16V4Z' /></svg>" },
                { IconNames.ArrowLeft, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20,11V13H8L13.5,18.5L12.08,19.92L4.16,12L12.08,4.08L13.5,5.5L8,11H20Z' /></svg>" },
                { IconNames.ArrowLeftBoldBoxOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M7,12L12,7V10H16V14H12V17L7,12M21,5V19A2,2 0 0,1 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3H19A2,2 0 0,1 21,5M19,5H5V19H19V5Z' /></svg>" },
                { IconNames.ArrowRightBold, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M4,15V9H12V4.16L19.84,12L12,19.84V15H4Z' /></svg>" },
                { IconNames.ArrowRightBoldBoxOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M17,12L12,17V14H8V10H12V7L17,12M3,19V5A2,2 0 0,1 5,3H19A2,2 0 0,1 21,5V19A2,2 0 0,1 19,21H5A2,2 0 0,1 3,19M5,19H19V5H5V19Z' /></svg>" },
                //{ IconNames.ArrowRightThinCircleOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20.03 12C20.03 7.59 16.41 3.97 12 3.97C7.59 3.97 3.97 7.59 3.97 12C3.97 16.41 7.59 20.03 12 20.03C16.41 20.03 20.03 16.41 20.03 12M22 12C22 17.54 17.54 22 12 22C6.46 22 2 17.54 2 12C2 6.46 6.46 2 12 2C17.54 2 22 6.46 22 12M13.54 13V16L17.5 12L13.54 8V11H6.5V13' /></svg>" },
                { IconNames.ArrowUp, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M13,20H11V8L5.5,13.5L4.08,12.08L12,4.16L19.92,12.08L18.5,13.5L13,8V20Z' /></svg>" },
                { IconNames.CheckboxBlankOffOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M22.11 21.46L2.39 1.73L1.11 3L3 4.9V19C3 20.11 3.9 21 5 21H19.1L20.84 22.73L22.11 21.46M5 19V6.89L17.11 19H5M8.2 5L6.2 3H19C20.1 3 21 3.89 21 5V17.8L19 15.8V5H8.2Z' /></svg>" },
                { IconNames.CheckboxBlankOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,3H5C3.89,3 3,3.89 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5C21,3.89 20.1,3 19,3M19,5V19H5V5H19Z' /></svg>" },
                { IconNames.CheckboxMultipleBlankOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20,16V4H8V16H20M22,16A2,2 0 0,1 20,18H8C6.89,18 6,17.1 6,16V4C6,2.89 6.89,2 8,2H20A2,2 0 0,1 22,4V16M16,20V22H4A2,2 0 0,1 2,20V7H4V20H16Z' /></svg>" },
                { IconNames.CheckboxMultipleOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20,2H8A2,2 0 0,0 6,4V16A2,2 0 0,0 8,18H20A2,2 0 0,0 22,16V4A2,2 0 0,0 20,2M20,16H8V4H20V16M16,20V22H4A2,2 0 0,1 2,20V7H4V20H16M18.53,8.06L17.47,7L12.59,11.88L10.47,9.76L9.41,10.82L12.59,14L18.53,8.06Z' /></svg>" },
                { IconNames.CheckboxOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M19,5V19H5V5H19M10,17L6,13L7.41,11.58L10,14.17L16.59,7.58L18,9' /></svg>" },
                { IconNames.ChevronDown, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z' /></svg>" },
                { IconNames.ChevronLeftBoxOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,3H5A2,2 0 0,0 3,5V19C3,20.11 3.9,21 5,21H19C20.11,21 21,20.11 21,19V5A2,2 0 0,0 19,3M19,19H5V5H19V19M15.71,7.41L11.12,12L15.71,16.59L14.29,18L8.29,12L14.29,6L15.71,7.41Z' /></svg>" },
                { IconNames.ChevronRightBoxOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,3H5A2,2 0 0,0 3,5V19C3,20.11 3.9,21 5,21H19C20.11,21 21,20.11 21,19V5A2,2 0 0,0 19,3M19,19H5V5H19V19M8.29,16.59L12.88,12L8.29,7.41L9.71,6L15.71,12L9.71,18L8.29,16.59Z' /></svg>" },
                { IconNames.ChevronUp, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M7.41,15.41L12,10.83L16.59,15.41L18,14L12,8L6,14L7.41,15.41Z' /></svg>" },
                { IconNames.Close, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z' /></svg>" },
                { IconNames.CloseCircleOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2C6.47,2 2,6.47 2,12C2,17.53 6.47,22 12,22C17.53,22 22,17.53 22,12C22,6.47 17.53,2 12,2M14.59,8L12,10.59L9.41,8L8,9.41L10.59,12L8,14.59L9.41,16L12,13.41L14.59,16L16,14.59L13.41,12L16,9.41L14.59,8Z' /></svg>" },
                { IconNames.CloseThick, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20 6.91L17.09 4L12 9.09L6.91 4L4 6.91L9.09 12L4 17.09L6.91 20L12 14.91L17.09 20L20 17.09L14.91 12L20 6.91Z' /></svg>" },
                { IconNames.CloudDownload, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M17,13L12,18L7,13H10V9H14V13M19.35,10.03C18.67,6.59 15.64,4 12,4C9.11,4 6.6,5.64 5.35,8.03C2.34,8.36 0,10.9 0,14A6,6 0 0,0 6,20H19A5,5 0 0,0 24,15C24,12.36 21.95,10.22 19.35,10.03Z' /></svg>" },
                { IconNames.CloudSync, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12,4C15.64,4 18.67,6.59 19.35,10.04C21.95,10.22 24,12.36 24,15A5,5 0 0,1 19,20H6A6,6 0 0,1 0,14C0,10.91 2.34,8.36 5.35,8.04C6.6,5.64 9.11,4 12,4M7.5,9.69C6.06,11.5 6.2,14.06 7.82,15.68C8.66,16.5 9.81,17 11,17V18.86L13.83,16.04L11,13.21V15C10.34,15 9.7,14.74 9.23,14.27C8.39,13.43 8.26,12.11 8.92,11.12L7.5,9.69M9.17,8.97L10.62,10.42L12,11.79V10C12.66,10 13.3,10.26 13.77,10.73C14.61,11.57 14.74,12.89 14.08,13.88L15.5,15.31C16.94,13.5 16.8,10.94 15.18,9.32C14.34,8.5 13.19,8 12,8V6.14L9.17,8.97Z' /></svg>" },
                { IconNames.CloudUpload, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M14,13V17H10V13H7L12,8L17,13M19.35,10.03C18.67,6.59 15.64,4 12,4C9.11,4 6.6,5.64 5.35,8.03C2.34,8.36 0,10.9 0,14A6,6 0 0,0 6,20H19A5,5 0 0,0 24,15C24,12.36 21.95,10.22 19.35,10.03Z' /></svg>" },
                { IconNames.CodeBraces, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M8,3A2,2 0 0,0 6,5V9A2,2 0 0,1 4,11H3V13H4A2,2 0 0,1 6,15V19A2,2 0 0,0 8,21H10V19H8V14A2,2 0 0,0 6,12A2,2 0 0,0 8,10V5H10V3M16,3A2,2 0 0,1 18,5V9A2,2 0 0,0 20,11H21V13H20A2,2 0 0,0 18,15V19A2,2 0 0,1 16,21H14V19H16V14A2,2 0 0,1 18,12A2,2 0 0,1 16,10V5H14V3H16Z' /></svg>" },
                { IconNames.Delete, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z' /></svg>" },
                { IconNames.DeleteEmpty, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20.37,8.91L19.37,10.64L7.24,3.64L8.24,1.91L11.28,3.66L12.64,3.29L16.97,5.79L17.34,7.16L20.37,8.91M6,19V7H11.07L18,11V19A2,2 0 0,1 16,21H8A2,2 0 0,1 6,19Z' /></svg>" },
                { IconNames.DeleteForever, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19M8.46,11.88L9.87,10.47L12,12.59L14.12,10.47L15.53,11.88L13.41,14L15.53,16.12L14.12,17.53L12,15.41L9.88,17.53L8.47,16.12L10.59,14L8.46,11.88M15.5,4L14.5,3H9.5L8.5,4H5V6H19V4H15.5Z' /></svg>" },
                { IconNames.DeleteRestore, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M14,14H16L12,10L8,14H10V18H14V14M6,7H18V19C18,19.5 17.8,20 17.39,20.39C17,20.8 16.5,21 16,21H8C7.5,21 7,20.8 6.61,20.39C6.2,20 6,19.5 6,19V7M19,4V6H5V4H8.5L9.5,3H14.5L15.5,4H19Z' /></svg>" },
                { IconNames.DotsVertical, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12,16A2,2 0 0,1 14,18A2,2 0 0,1 12,20A2,2 0 0,1 10,18A2,2 0 0,1 12,16M12,10A2,2 0 0,1 14,12A2,2 0 0,1 12,14A2,2 0 0,1 10,12A2,2 0 0,1 12,10M12,4A2,2 0 0,1 14,6A2,2 0 0,1 12,8A2,2 0 0,1 10,6A2,2 0 0,1 12,4Z' /></svg>" },
                { IconNames.Earth, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M17.9,17.39C17.64,16.59 16.89,16 16,16H15V13A1,1 0 0,0 14,12H8V10H10A1,1 0 0,0 11,9V7H13A2,2 0 0,0 15,5V4.59C17.93,5.77 20,8.64 20,12C20,14.08 19.2,15.97 17.9,17.39M11,19.93C7.05,19.44 4,16.08 4,12C4,11.38 4.08,10.78 4.21,10.21L9,15V16A2,2 0 0,0 11,18M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z' /></svg>" },
                { IconNames.EmoticonHappy, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M20,12A8,8 0 0,0 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12M22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2A10,10 0 0,1 22,12M10,9.5C10,10.3 9.3,11 8.5,11C7.7,11 7,10.3 7,9.5C7,8.7 7.7,8 8.5,8C9.3,8 10,8.7 10,9.5M17,9.5C17,10.3 16.3,11 15.5,11C14.7,11 14,10.3 14,9.5C14,8.7 14.7,8 15.5,8C16.3,8 17,8.7 17,9.5M12,17.23C10.25,17.23 8.71,16.5 7.81,15.42L9.23,14C9.68,14.72 10.75,15.23 12,15.23C13.25,15.23 14.32,14.72 14.77,14L16.19,15.42C15.29,16.5 13.75,17.23 12,17.23Z' /></svg>" },
                { IconNames.Export, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M23,12L19,8V11H10V13H19V16M1,18V6C1,4.89 1.9,4 3,4H15A2,2 0 0,1 17,6V9H15V6H3V18H15V15H17V18A2,2 0 0,1 15,20H3A2,2 0 0,1 1,18Z' /></svg>" },
                //{ IconNames.FileRestore, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M12,18C9.95,18 8.19,16.76 7.42,15H9.13C9.76,15.9 10.81,16.5 12,16.5A3.5,3.5 0 0,0 15.5,13A3.5,3.5 0 0,0 12,9.5C10.65,9.5 9.5,10.28 8.9,11.4L10.5,13H6.5V9L7.8,10.3C8.69,8.92 10.23,8 12,8A5,5 0 0,1 17,13A5,5 0 0,1 12,18Z' /></svg>" },
                { IconNames.FormatBold, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M13.5,15.5H10V12.5H13.5A1.5,1.5 0 0,1 15,14A1.5,1.5 0 0,1 13.5,15.5M10,6.5H13A1.5,1.5 0 0,1 14.5,8A1.5,1.5 0 0,1 13,9.5H10M15.6,10.79C16.57,10.11 17.25,9 17.25,8C17.25,5.74 15.5,4 13.25,4H7V18H14.04C16.14,18 17.75,16.3 17.75,14.21C17.75,12.69 16.89,11.39 15.6,10.79Z' /></svg>" },
                { IconNames.FormatHeader1, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M3,4H5V10H9V4H11V18H9V12H5V18H3V4M14,18V16H16V6.31L13.5,7.75V5.44L16,4H18V16H20V18H14Z' /></svg>" },
                { IconNames.FormatHeader2, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M3,4H5V10H9V4H11V18H9V12H5V18H3V4M21,18H15A2,2 0 0,1 13,16C13,15.47 13.2,15 13.54,14.64L18.41,9.41C18.78,9.05 19,8.55 19,8A2,2 0 0,0 17,6A2,2 0 0,0 15,8H13A4,4 0 0,1 17,4A4,4 0 0,1 21,8C21,9.1 20.55,10.1 19.83,10.83L15,16H21V18Z' /></svg>" },
                { IconNames.FormatHeader3, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M3,4H5V10H9V4H11V18H9V12H5V18H3V4M15,4H19A2,2 0 0,1 21,6V16A2,2 0 0,1 19,18H15A2,2 0 0,1 13,16V15H15V16H19V12H15V10H19V6H15V7H13V6A2,2 0 0,1 15,4Z' /></svg>" },
                { IconNames.FormatItalic, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M10,4V7H12.21L8.79,15H6V18H14V15H11.79L15.21,7H18V4H10Z' /></svg>" },
                { IconNames.FormatListBulleted, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M7,5H21V7H7V5M7,13V11H21V13H7M4,4.5A1.5,1.5 0 0,1 5.5,6A1.5,1.5 0 0,1 4,7.5A1.5,1.5 0 0,1 2.5,6A1.5,1.5 0 0,1 4,4.5M4,10.5A1.5,1.5 0 0,1 5.5,12A1.5,1.5 0 0,1 4,13.5A1.5,1.5 0 0,1 2.5,12A1.5,1.5 0 0,1 4,10.5M7,19V17H21V19H7M4,16.5A1.5,1.5 0 0,1 5.5,18A1.5,1.5 0 0,1 4,19.5A1.5,1.5 0 0,1 2.5,18A1.5,1.5 0 0,1 4,16.5Z' /></svg>" },
                { IconNames.FormatListNumbers, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M7,13H21V11H7M7,19H21V17H7M7,7H21V5H7M2,11H3.8L2,13.1V14H5V13H3.2L5,10.9V10H2M3,8H4V4H2V5H3M2,17H4V17.5H3V18.5H4V19H2V20H5V16H2V17Z' /></svg>" },
                { IconNames.FormatQuoteClose, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M14,17H17L19,13V7H13V13H16M6,17H9L11,13V7H5V13H8L6,17Z' /></svg>" },
                { IconNames.FormatStrikethrough, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M3,14H21V12H3M5,4V7H10V10H14V7H19V4M10,19H14V16H10V19Z' /></svg>" },
                { IconNames.FormatUnderline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M5,21H19V19H5V21M12,17A6,6 0 0,0 18,11V3H15.5V11A3.5,3.5 0 0,1 12,14.5A3.5,3.5 0 0,1 8.5,11V3H6V11A6,6 0 0,0 12,17Z' /></svg>" },
                { IconNames.Key, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M7,14A2,2 0 0,1 5,12A2,2 0 0,1 7,10A2,2 0 0,1 9,12A2,2 0 0,1 7,14M12.65,10C11.83,7.67 9.61,6 7,6A6,6 0 0,0 1,12A6,6 0 0,0 7,18C9.61,18 11.83,16.33 12.65,14H17V18H21V14H23V10H12.65Z' /></svg>" },
                { IconNames.KeyboardOffOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M1,4.27L2.47,5.74C2.18,6.08 2,6.5 2,7V17A2,2 0 0,0 4,19H15.73L18.73,22L20,20.72L2.28,3L1,4.27M4,17V7.27L5,8.27V10H6.73L8,11.27V13H9.73L10.73,14H8V16H12.73L13.73,17H4M5,11H7V13H5V11M17,11H19V13H17V11M19,10H17V8H19V10M14,11H16V13H14.83L14,12.17V11M13,10H11.83L11,9.17V8H13V10M22,7V17C22,17.86 21.45,18.58 20.7,18.87L18.83,17H20V7H8.83L6.83,5H20A2,2 0 0,1 22,7M16,10H14V8H16V10Z' /></svg>" },
                { IconNames.LinkVariant, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M10.59,13.41C11,13.8 11,14.44 10.59,14.83C10.2,15.22 9.56,15.22 9.17,14.83C7.22,12.88 7.22,9.71 9.17,7.76V7.76L12.71,4.22C14.66,2.27 17.83,2.27 19.78,4.22C21.73,6.17 21.73,9.34 19.78,11.29L18.29,12.78C18.3,11.96 18.17,11.14 17.89,10.36L18.36,9.88C19.54,8.71 19.54,6.81 18.36,5.64C17.19,4.46 15.29,4.46 14.12,5.64L10.59,9.17C9.41,10.34 9.41,12.24 10.59,13.41M13.41,9.17C13.8,8.78 14.44,8.78 14.83,9.17C16.78,11.12 16.78,14.29 14.83,16.24V16.24L11.29,19.78C9.34,21.73 6.17,21.73 4.22,19.78C2.27,17.83 2.27,14.66 4.22,12.71L5.71,11.22C5.7,12.04 5.83,12.86 6.11,13.65L5.64,14.12C4.46,15.29 4.46,17.19 5.64,18.36C6.81,19.54 8.71,19.54 9.88,18.36L13.41,14.83C14.59,13.66 14.59,11.76 13.41,10.59C13,10.2 13,9.56 13.41,9.17Z' /></svg>" },
                { IconNames.Lock, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12,17A2,2 0 0,0 14,15C14,13.89 13.1,13 12,13A2,2 0 0,0 10,15A2,2 0 0,0 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z' /></svg>" },
                { IconNames.LockOpenVariant, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M18 1C15.24 1 13 3.24 13 6V8H4C2.9 8 2 8.89 2 10V20C2 21.11 2.9 22 4 22H16C17.11 22 18 21.11 18 20V10C18 8.9 17.11 8 16 8H15V6C15 4.34 16.34 3 18 3C19.66 3 21 4.34 21 6V8H23V6C23 3.24 20.76 1 18 1M10 13C11.1 13 12 13.89 12 15C12 16.11 11.11 17 10 17C8.9 17 8 16.11 8 15C8 13.9 8.9 13 10 13Z' /></svg>" },
                { IconNames.LockOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12,17C10.89,17 10,16.1 10,15C10,13.89 10.89,13 12,13A2,2 0 0,1 14,15A2,2 0 0,1 12,17M18,20V10H6V20H18M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6C4.89,22 4,21.1 4,20V10C4,8.89 4.89,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z' /></svg>" },
                { IconNames.LockReset, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12.63,2C18.16,2 22.64,6.5 22.64,12C22.64,17.5 18.16,22 12.63,22C9.12,22 6.05,20.18 4.26,17.43L5.84,16.18C7.25,18.47 9.76,20 12.64,20A8,8 0 0,0 20.64,12A8,8 0 0,0 12.64,4C8.56,4 5.2,7.06 4.71,11H7.47L3.73,14.73L0,11H2.69C3.19,5.95 7.45,2 12.63,2M15.59,10.24C16.09,10.25 16.5,10.65 16.5,11.16V15.77C16.5,16.27 16.09,16.69 15.58,16.69H10.05C9.54,16.69 9.13,16.27 9.13,15.77V11.16C9.13,10.65 9.54,10.25 10.04,10.24V9.23C10.04,7.7 11.29,6.46 12.81,6.46C14.34,6.46 15.59,7.7 15.59,9.23V10.24M12.81,7.86C12.06,7.86 11.44,8.47 11.44,9.23V10.24H14.19V9.23C14.19,8.47 13.57,7.86 12.81,7.86Z' /></svg>" },
                { IconNames.Magnify, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z' /></svg>" },
                { IconNames.NoteTextOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M15 3H5A2 2 0 0 0 3 5V19A2 2 0 0 0 5 21H19A2 2 0 0 0 21 19V9L15 3M19 19H5V5H14V10H19M17 14H7V12H17M14 17H7V15H14' /></svg>" },
                { IconNames.OpenInNew, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M14,3V5H17.59L7.76,14.83L9.17,16.24L19,6.41V10H21V3M19,19H5V5H12V3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V12H19V19Z' /></svg>" },
                { IconNames.OrderBoolAscendingVariant, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M4 13C2.89 13 2 13.89 2 15V19C2 20.11 2.89 21 4 21H8C9.11 21 10 20.11 10 19V15C10 13.89 9.11 13 8 13M8.2 14.5L9.26 15.55L5.27 19.5L2.74 16.95L3.81 15.9L5.28 17.39M4 3C2.89 3 2 3.89 2 5V9C2 10.11 2.89 11 4 11H8C9.11 11 10 10.11 10 9V5C10 3.89 9.11 3 8 3M4 5H8V9H4M12 5H22V7H12M12 19V17H22V19M12 11H22V13H12Z' /></svg>" },
                //{ IconNames.Palette, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M17.5,12A1.5,1.5 0 0,1 16,10.5A1.5,1.5 0 0,1 17.5,9A1.5,1.5 0 0,1 19,10.5A1.5,1.5 0 0,1 17.5,12M14.5,8A1.5,1.5 0 0,1 13,6.5A1.5,1.5 0 0,1 14.5,5A1.5,1.5 0 0,1 16,6.5A1.5,1.5 0 0,1 14.5,8M9.5,8A1.5,1.5 0 0,1 8,6.5A1.5,1.5 0 0,1 9.5,5A1.5,1.5 0 0,1 11,6.5A1.5,1.5 0 0,1 9.5,8M6.5,12A1.5,1.5 0 0,1 5,10.5A1.5,1.5 0 0,1 6.5,9A1.5,1.5 0 0,1 8,10.5A1.5,1.5 0 0,1 6.5,12M12,3A9,9 0 0,0 3,12A9,9 0 0,0 12,21A1.5,1.5 0 0,0 13.5,19.5C13.5,19.11 13.35,18.76 13.11,18.5C12.88,18.23 12.73,17.88 12.73,17.5A1.5,1.5 0 0,1 14.23,16H16A5,5 0 0,0 21,11C21,6.58 16.97,3 12,3Z' /></svg>" },
                { IconNames.Pin, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M16,12V4H17V2H7V4H8V12L6,14V16H11.2V22H12.8V16H18V14L16,12Z' /></svg>" },
                { IconNames.Plus, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z' /></svg>" },
                { IconNames.Redo, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M18.4,10.6C16.55,9 14.15,8 11.5,8C6.85,8 2.92,11.03 1.54,15.22L3.9,16C4.95,12.81 7.95,10.5 11.5,10.5C13.45,10.5 15.23,11.22 16.62,12.38L13,16H22V7L18.4,10.6Z' /></svg>" },
                { IconNames.SafeSquareOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M10 12C10 10.9 10.9 10 12 10C13.1 10 14 10.9 14 12C14 13.1 13.1 14 12 14C10.9 14 10 13.1 10 12M7.8 17.7L6.3 16.3L7.8 14.8C7.3 14 7 13 7 12C7 11 7.3 10 7.8 9.3L6.3 7.8L7.8 6.3L9.2 7.8C10 7.3 11 7 12 7C13 7 14 7.3 14.8 7.8L16.3 6.3L17.7 7.7L16.2 9.2C16.7 10 17 11 17 12C17 13 16.7 14 16.2 14.8L17.7 16.3L16.3 17.7L14.8 16.2C14 16.7 13 17 12 17C11 17 10 16.7 9.3 16.2L7.8 17.7M12 9C10.3 9 9 10.3 9 12C9 13.7 10.3 15 12 15C13.7 15 15 13.7 15 12C15 10.3 13.7 9 12 9M20 2C21.1 2 22 2.9 22 4V20C22 21.1 21.1 22 20 22H19V23H15V22H9V23H5V22H4C2.9 22 2 21.1 2 20V4C2 2.9 2.9 2 4 2H20M20 20V4H4V20H20Z' /></svg>" },
                { IconNames.Settings, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.67 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z' /></svg>" },
                { IconNames.SortAlphabeticalAscending, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19 17H22L18 21L14 17H17V3H19M11 13V15L7.67 19H11V21H5V19L8.33 15H5V13M9 3H7C5.9 3 5 3.9 5 5V11H7V9H9V11H11V5C11 3.9 10.11 3 9 3M9 7H7V5H9Z' /></svg>" },
                { IconNames.SortBoolDescendingVariant, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M19 7H22L18 3L14 7H17V21H19M5 13C3.89 13 3 13.89 3 15V19C3 20.11 3.89 21 5 21H9C10.11 21 11 20.11 11 19V15C11 13.89 10.11 13 9 13M9.2 14.5L10.26 15.55L6.27 19.5L3.74 16.95L4.81 15.9L6.28 17.39M5 3C3.89 3 3 3.89 3 5V9C3 10.11 3.89 11 5 11H9C10.11 11 11 10.11 11 9V5C11 3.89 10.11 3 9 3M5 5H9V9H5Z' /></svg>" },
                { IconNames.TagMultiple, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M5.5,9A1.5,1.5 0 0,0 7,7.5A1.5,1.5 0 0,0 5.5,6A1.5,1.5 0 0,0 4,7.5A1.5,1.5 0 0,0 5.5,9M17.41,11.58C17.77,11.94 18,12.44 18,13C18,13.55 17.78,14.05 17.41,14.41L12.41,19.41C12.05,19.77 11.55,20 11,20C10.45,20 9.95,19.78 9.58,19.41L2.59,12.42C2.22,12.05 2,11.55 2,11V6C2,4.89 2.89,4 4,4H9C9.55,4 10.05,4.22 10.41,4.58L17.41,11.58M13.54,5.71L14.54,4.71L21.41,11.58C21.78,11.94 22,12.45 22,13C22,13.55 21.78,14.05 21.42,14.41L16.04,19.79L15.04,18.79L20.75,13L13.54,5.71Z' /></svg>" },
                { IconNames.TagOff, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M18.33 8.5L22.92 3.92L21.5 2.5L2.5 21.5L3.92 22.92L8.5 18.33L11.58 21.41A2 2 0 0 0 13 22A2 2 0 0 0 14.41 21.41L21.41 14.41A2 2 0 0 0 22 13A2 2 0 0 0 21.41 11.58M5.61 15.43L15.47 5.65L12.41 2.58A2 2 0 0 0 11 2H4A2 2 0 0 0 2 4V11A2 2 0 0 0 2.59 12.41M5.5 4A1.5 1.5 0 1 1 4 5.5A1.5 1.5 0 0 1 5.5 4Z' /></svg>" },
                { IconNames.TagOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M21.41 11.58L12.41 2.58A2 2 0 0 0 11 2H4A2 2 0 0 0 2 4V11A2 2 0 0 0 2.59 12.42L11.59 21.42A2 2 0 0 0 13 22A2 2 0 0 0 14.41 21.41L21.41 14.41A2 2 0 0 0 22 13A2 2 0 0 0 21.41 11.58M13 20L4 11V4H11L20 13M6.5 5A1.5 1.5 0 1 1 5 6.5A1.5 1.5 0 0 1 6.5 5Z' /></svg>" },
                { IconNames.TagPlusOutline, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M6.5 5A1.5 1.5 0 1 0 8 6.5A1.5 1.5 0 0 0 6.5 5M6.5 5A1.5 1.5 0 1 0 8 6.5A1.5 1.5 0 0 0 6.5 5M21.41 11.58L12.41 2.58A2 2 0 0 0 11 2H4A2 2 0 0 0 2 4V11A2 2 0 0 0 2.59 12.42L3 12.82A5.62 5.62 0 0 1 5.08 12.08L4 11V4H11L20 13L13 20L11.92 18.92A5.57 5.57 0 0 1 11.18 21L11.59 21.41A2 2 0 0 0 13 22A2 2 0 0 0 14.41 21.41L21.41 14.41A2 2 0 0 0 22 13A2 2 0 0 0 21.41 11.58M6.5 5A1.5 1.5 0 1 0 8 6.5A1.5 1.5 0 0 0 6.5 5M10 19H7V22H5V19H2V17H5V14H7V17H10Z' /></svg>" },
                { IconNames.Undo, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M12.5,8C9.85,8 7.45,9 5.6,10.6L2,7V16H11L7.38,12.38C8.77,11.22 10.54,10.5 12.5,10.5C16.04,10.5 19.05,12.81 20.1,16L22.47,15.22C21.08,11.03 17.15,8 12.5,8Z' /></svg>" },
                { IconNames.Wrap, "<svg width='24' height='24' viewBox='0 0 24 24'><path d='M21,5H3V7H21V5M3,19H10V17H3V19M3,13H18C19,13 20,13.43 20,15C20,16.57 19,17 18,17H16V15L12,18L16,21V19H18C20.95,19 22,17.73 22,15C22,12.28 21,11 18,11H3V13Z' /></svg>" },            };
        }

        /// <inheritdoc/>
        public string this[string id]
        {
            get { return _svgResources[id]; }
        }

        /// <inheritdoc/>
        public MarkupString EmbedLinkableSvgs(IEnumerable<string> ids)
        {
            StringBuilder result = new StringBuilder();
            foreach (string id in ids)
            {
                string svg = _svgResources[id];
                svg = svg.Insert(4, string.Format(" id='svg-{0}'", id));
                result.AppendLine(svg);
            }
            return (MarkupString)result.ToString();
        }

        /// <inheritdoc/>
        public MarkupString EmbedLinkableSvg(string id)
        {
            string result = _svgResources[id];
            result = result.Insert(4, string.Format(" id='svg-{0}'", id));
            return (MarkupString)result;
        }

        public string GetSvgLink(string id, int size = 24)
        {
            string result = string.Format("<svg width='{1}' height='{1}' viewBox='0 0 24 24'><use xlink:href='#svg-{0}' /></svg>", id, size);
            return result;
        }

        public MarkupString EmbedSvgLink(string id, int size = 24)
        {
            string result = GetSvgLink(id, size);
            return (MarkupString)result;
        }

        /// <inheritdoc/>
        public string LoadIconAsCssUrl(string id, IEnumerable<KeyValuePair<string, string>> attributes = null)
        {
            var enrichedAttributes = attributes != null
                ? new List<KeyValuePair<string, string>>(attributes)
                : new List<KeyValuePair<string, string>>();
            enrichedAttributes.Insert(0, new KeyValuePair<string, string>("xmlns", "http://www.w3.org/2000/svg"));

            string svg = LoadIcon(id, enrichedAttributes);

            // base64 encoding seems to be the only safe choice for cross browser compatibility.
            // Especially AndroidQ WebView hat problems with only Uri.EscapeUriString().
            string encodedSvg = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
            return string.Format(@"url(""data:image/svg+xml;base64,{0}"")", encodedSvg);
        }

        private string LoadIcon(string id, IEnumerable<KeyValuePair<string, string>> attributes = null)
        {
            string result = _svgResources[id];
            if (attributes != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> attribute in attributes)
                {
                    sb.Append(' ').Append(attribute.Key).Append("='").Append(attribute.Value).Append("'");
                }
                result = result.Insert(4, sb.ToString());
            }
            return result;
        }
    }

    /// <summary>
    /// List of available svg icon names, which can be retrieved by the <see cref="ISvgIconService"/>
    /// </summary>
    internal static class IconNames
    {
        public const string AlarmPlus = "alarm-plus";
        public const string Alert = "alert";
        public const string ArrowAll = "arrow-all";
        public const string ArrowCollapseDown = "arrow-collapse-down";
        public const string ArrowCollapseUp = "arrow-collapse-up";
        public const string ArrowDown = "arrow-down";
        public const string ArrowLeft = "arrow-left";
        public const string ArrowLeftBoldBoxOutline = "arrow-left-bold-box-outline";
        public const string ArrowRightBold = "arrow-right-bold";
        public const string ArrowRightBoldBoxOutline = "arrow-right-bold-box-outline";
        public const string ArrowUp = "arrow-up";
        public const string CheckboxBlankOffOutline = "checkbox-blank-off-outline";
        public const string CheckboxBlankOutline = "checkbox-blank-outline";
        public const string CheckboxMultipleBlankOutline = "checkbox-multiple-blank-outline";
        public const string CheckboxMultipleOutline = "check-box-multiple-outline";
        public const string CheckboxOutline = "check-box-outline";
        public const string ChevronDown = "chevron-down";
        public const string ChevronLeftBoxOutline = "chevron-left-box-outline";
        public const string ChevronRightBoxOutline = "chevron-right-box-outline";
        public const string ChevronUp = "chevron-up";
        public const string Close = "close";
        public const string CloseCircleOutline = "close-circle-outline";
        public const string CloseThick = "close-thick";
        public const string CloudDownload = "cloud-download";
        public const string CloudSync = "cloud-sync";
        public const string CloudUpload = "cloud-upload";
        public const string CodeBraces = "code-braces";
        public const string Delete = "delete";
        public const string DeleteEmpty = "delete-empty";
        public const string DeleteForever = "delete-forever";
        public const string DeleteRestore = "delete-restore";
        public const string DotsVertical = "dots-vertical";
        public const string Earth = "earth";
        public const string EmoticonHappy = "emoticon-happy";
        public const string Export = "export";
        public const string FormatBold = "format-bold";
        public const string FormatHeader1 = "format-header-1";
        public const string FormatHeader2 = "format-header-2";
        public const string FormatHeader3 = "format-header-3";
        public const string FormatItalic = "format-italic";
        public const string FormatListBulleted = "format-list-bulleted";
        public const string FormatListNumbers = "format-list-numbers";
        public const string FormatQuoteClose = "format-quote-close";
        public const string FormatStrikethrough = "format-strikethrough";
        public const string FormatUnderline = "format-underline";
        public const string Key = "key";
        public const string KeyboardOffOutline = "keyboard_off_outline";
        public const string LinkVariant = "link-variant";
        public const string Lock = "lock";
        public const string LockOpenVariant = "lock-open-variant";
        public const string LockOutline = "lock-outline";
        public const string LockReset = "lock-reset";
        public const string Magnify = "magnify";
        public const string NoteTextOutline = "note-text-outline";
        public const string OpenInNew = "open-in-new";
        public const string OrderBoolAscendingVariant = "order-bool-ascending-variant";
        public const string Pin = "pin";
        public const string Plus = "plus";
        public const string Redo = "redo";
        public const string SafeSquareOutline = "safe-square-outline";
        public const string Settings = "settings";
        public const string SortAlphabeticalAscending = "sort-alphabetical-ascending";
        public const string SortBoolDescendingVariant = "sort-bool-descending-variant";
        public const string TagMultiple = "tag-multiple";
        public const string TagOff = "tag-off";
        public const string TagOutline = "tag-outline";
        public const string TagPlusOutline = "tag-plus-outline";
        public const string Undo = "undo";
        public const string Wrap = "wrap";
    }
}
