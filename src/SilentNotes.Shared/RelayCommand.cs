// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace SilentNotes
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// This is just a wrapper around the MvvmLight relay command, to allow extendability
    /// and to control access.
    /// </summary>
    public class RelayCommand : GalaSoft.MvvmLight.Command.RelayCommand, ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// This is just a wrapper around the MvvmLight relay command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : base(execute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// This is just a wrapper around the MvvmLight relay command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">Returns a value indicating whether the action is enabled.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
            : base(execute, canExecute)
        {
        }
    }

    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// This is just a wrapper around the MvvmLight relay command, to allow extendability
    /// and to control access.
    /// </summary>
    /// <typeparam name="T">Type of the parameter, the command can pass on to its action.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Generic variation.")]
    public class RelayCommand<T> : GalaSoft.MvvmLight.Command.RelayCommand<T>, ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// This is just a wrapper around the MvvmLight relay command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<T> execute)
            : base(execute)
        {
        }
    }
}
