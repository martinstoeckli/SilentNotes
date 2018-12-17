using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace SilentNotes.UWP.Services.CloudStorageServices
{
    public class DropboxCloudStorageServiceUwp : DropboxCloudStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxCloudStorageServiceUwp"/> class.
        /// </summary>
        public DropboxCloudStorageServiceUwp(ICryptoRandomService randomSource)
            : base(new CloudStorageAccount { CloudType = CloudStorageType.Dropbox}, null, randomSource)
        {
        }

        public override async void ShowOauth2LoginPage()
        {
            string startURL = "https://<providerendpoint>?client_id=<clientid>&scope=<scopes>&response_type=token";
            //string endURL = "http://<appendpoint>";

            System.Uri startURI = new System.Uri("https://www.dropbox.com/oauth2/authorize?response_type=token&client_id=2drl5n333frqsc8&redirect_uri=ch.martinstoeckli.silentnotes%3A%2F%2Foauth2redirect%2F&state=zAM6l2WSOtcXEIHf");
            //System.Uri endURI = new System.Uri(endURL);
            Uri callbackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            callbackUri = new Uri("ch.martinstoeckli.silentnotes://oauth2redirect/");
            WebAuthenticationResult webAuthenticationResult =
                    await WebAuthenticationBroker.AuthenticateAsync(
                    WebAuthenticationOptions.None,
                    startURI,
                    callbackUri);

            string result;
            switch (webAuthenticationResult.ResponseStatus)
            {
                case Windows.Security.Authentication.Web.WebAuthenticationStatus.Success:
                    // Successful authentication. 
                    result = webAuthenticationResult.ResponseData.ToString();
                    break;
                case Windows.Security.Authentication.Web.WebAuthenticationStatus.ErrorHttp:
                    // HTTP error. 
                    result = webAuthenticationResult.ResponseErrorDetail.ToString();
                    break;
                default:
                    // Other error.
                    result = webAuthenticationResult.ResponseData.ToString();
                    break;
            }
        }
    }
}
