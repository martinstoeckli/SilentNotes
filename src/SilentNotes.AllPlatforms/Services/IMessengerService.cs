// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Messaging;

namespace SilentNotes.Services
{
    /// <summary>
    /// Abstraction interface to <see cref="CommunityToolkit.Mvvm.Messaging.IMessenger"/>, to get
    /// rid of the extension methods which makes it impossible to mock the <see cref="WeakReferenceMessenger"/>.
    /// </summary>
    public interface IMessengerService
    {
        /// <summary>
        /// Registers a recipient for a given type of message.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to receive.</typeparam>
        /// <param name="recipient">The recipient that will receive the messages.</param>
        /// <param name="handler">The <see cref="T:CommunityToolkit.Mvvm.Messaging.MessageHandler`2" /> to invoke when a message is received.</param>
        /// <exception cref="T:System.InvalidOperationException">Thrown when trying to register the same message twice.</exception>
        /// <remarks>This method will use the default channel to perform the requested registration.</remarks>
        void Register<TMessage>(object recipient, MessageHandler<object, TMessage> handler) where TMessage : class;

        /// <summary>
        /// Unregisters a recipient from messages of a given type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to stop receiving.</typeparam>
        /// <param name="recipient">The recipient to unregister.</param>
        /// <remarks>
        /// This method will unregister the target recipient only from the default channel.
        /// If the recipient has no registered handler, this method does nothing.
        /// </remarks>
        void Unregister<TMessage>(object recipient) where TMessage : class;
        
        /// <summary>
        /// Sends a message of the specified type to all registered recipients.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to send.</typeparam>
        /// <returns>The message that has been sent.</returns>
        /// <remarks>
        /// This method is a shorthand for <see cref="M:CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.Send``1(CommunityToolkit.Mvvm.Messaging.IMessenger,``0)" /> when the
        /// message type exposes a parameterless constructor: it will automatically create
        /// a new <typeparamref name="TMessage" /> instance and send that to its recipients.
        /// </remarks>
        TMessage Send<TMessage>() where TMessage : class, new();

        /// <summary>
        /// Sends a message of the specified type to all registered recipients.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>The message that was sent (ie. <paramref name="message" />).</returns>
        TMessage Send<TMessage>(TMessage message) where TMessage : class;
    }

    /// <summary>
    /// Implementation of the <see cref="IMessengerService"/> interface,
    /// based on the <see cref="WeakReferenceMessenger"/>.
    /// </summary>
    public class MessengerService : IMessengerService
    {
        /// <inheritdoc/>
        public void Register<TMessage>(object recipient, MessageHandler<object, TMessage> handler) where TMessage : class
        {
            WeakReferenceMessenger.Default.Register(recipient, default(MessengerToken), handler);
        }

        /// <inheritdoc/>
        public void Unregister<TMessage>(object recipient) where TMessage : class
        {
            WeakReferenceMessenger.Default.Unregister<TMessage, MessengerToken>(recipient, default(MessengerToken));
        }

        /// <inheritdoc/>
        public TMessage Send<TMessage>() where TMessage : class, new()
        {
            return WeakReferenceMessenger.Default.Send(new TMessage(), default(MessengerToken));
        }

        /// <inheritdoc/>
        public TMessage Send<TMessage>(TMessage message) where TMessage : class
        {
            return WeakReferenceMessenger.Default.Send(message, default(MessengerToken));
        }

        /// <summary>
        /// Use this token for all calls to <see cref="WeakReferenceMessenger"/>, so that we don't
        /// need to specify it everywhere.
        /// </summary>
        private readonly struct MessengerToken : IEquatable<MessengerToken>
        {
            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(MessengerToken other)
            {
                return true;
            }

            /// <inheritdoc />
            public override bool Equals(object? obj)
            {
                return obj is MessengerToken;
            }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
            {
                return 0;
            }
        }
    }
}
