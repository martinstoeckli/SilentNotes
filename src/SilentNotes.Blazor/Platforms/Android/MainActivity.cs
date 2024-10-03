﻿using System.Diagnostics;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using SilentNotes.Platforms;
using SilentNotes.Services;
using static Android.Views.ViewTreeObserver;

namespace SilentNotes
{
    [Activity(
        Label = "SilentNotes",
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        Exported = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : MauiAppCompatActivity, IOnPreDrawListener
    {
        private readonly ApplicationEventHandler _applicationEventHandler = new ApplicationEventHandler();
        public Guid Id { get; } = Guid.NewGuid();
        private Android.Views.View _contentView;
        private bool _splashScreenCanBeClosed = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Delay closing of splash screen until app is ready:
            // https://developer.android.com/develop/ui/views/launch/splash-screen#suspend-drawing
            var messenger = Ioc.Instance.GetService<IMessengerService>();
            messenger.Register<MainLayoutReadyMessage>(
                this, (recipient, message) => OnMainLayoutReady());
            _contentView = FindViewById<Android.Views.View>(Android.Resource.Id.Content);
            _contentView.ViewTreeObserver.AddOnPreDrawListener(this);

            _applicationEventHandler.OnCreate(this);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            _applicationEventHandler.OnNewIntent(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _applicationEventHandler.OnResume(this);
        }

        protected override void OnPause()
        {
            _applicationEventHandler.OnPause(this);
            base.OnPause();
        }

        protected override void OnStop()
        {
            _applicationEventHandler.OnStop(this);
            base.OnStop();
        }
        protected override void OnDestroy()
        {
            _applicationEventHandler.OnDestroy(this);
            base.OnDestroy();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            _applicationEventHandler.OnActivityResult(this, requestCode, resultCode, data);
        }

        private void OnMainLayoutReady()
        {
            var messenger = Ioc.Instance.GetService<IMessengerService>();
            messenger.Unregister<MainLayoutReadyMessage>(this); // This is a one time event.

            _splashScreenCanBeClosed = true;
            _applicationEventHandler.OnMainLayoutReady();
        }

        /// <summary>
        /// By overwriting this method, we can keep the splashscreen open until the application is
        /// ready, avoiding unnecessary flickering.
        /// </summary>
        /// <returns>Returns true if the app is ready and the splash screen can be closed.</returns>
        public bool OnPreDraw()
        {
            if (_splashScreenCanBeClosed)
            {
                _contentView.ViewTreeObserver.RemoveOnPreDrawListener(this);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool DispatchKeyEvent(KeyEvent e)
        {
            // Workaround: Unfortunately the back button will automatically navigate back whenever
            // possible, there is no way to intercept it in MainPage.OnBackButtonPressed().
            if ((e.KeyCode == Keycode.Back) && (e.Action == KeyEventActions.Down))
            {
                // Ask the page to close currently open menus and dialogs.
                var message = new BackButtonPressedMessage { Handled = false };
                var messenger = Ioc.Instance.GetService<IMessengerService>();
                messenger.Send(message);
                if (message.Handled)
                    return true;

                // Navigate backwards to the page which is defined by the current page. If the
                // back route is null, the application will be closed.
                if (!string.IsNullOrEmpty(message.BackRoute))
                {
                    var navigation = Ioc.Instance.GetService<INavigationService>();
                    navigation.NavigateTo(message.BackRoute);
                    return true;
                }
            }
            return base.DispatchKeyEvent(e);
        }
    }
}
