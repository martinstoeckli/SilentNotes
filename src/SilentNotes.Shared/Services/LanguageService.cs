// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace SilentNotes.Services
{
    /// <summary>
    /// App specific implementation of the <see cref="ILanguageService"/> interface.
    /// </summary>
    public class LanguageService : LanguageServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageService"/> class.
        /// </summary>
        /// <param name="languageCode">Language code.</param>
        public LanguageService(string languageCode)
            : base(languageCode)
        {
        }

        private void LoadEnglishResources(Dictionary<string, string> resources)
        {
            resources["ok"] = "OK";
            resources["cancel"] = "Cancel";
            resources["back"] = "Back";
            resources["continue"] = "Continue";

            resources["error_loading_repository"] = "Could not read the notes. The application was stopped, to avoid further damage of already existing notes.";
            resources["welcome_note"] = "<h1>Welcome to SilentNotes</h1><ul><li>SilentNotes respects your privacy with end-to-end encryption.</li><li>Take your notes and synchronize them between your pc and mobile devices.</li></ul>";

            resources["note_create_new"] = "Create new note";
            resources["note_view_or_edit"] = "Edit selected note";
            resources["note_to_recyclebin"] = "Move selected notes to the recycle bin";
            resources["note_undelete"] = "Restore note";
            resources["note_undelete_selected"] = "Restore selected notes";
            resources["note_title"] = "Title";
            resources["note_content"] = "Text";
            resources["note_left_key"] = "Left arrow key";
            resources["note_right_key"] = "Right arrow key";
            resources["note_colors"] = "Choose note color";
            resources["note_bold"] = "Bold";
            resources["note_italic"] = "Italic";
            resources["note_underline"] = "Underline";
            resources["note_strike"] = "Strike";
            resources["note_list_ordered"] = "Numbered list";
            resources["note_list_unordered"] = "Bulleted list";
            resources["note_code"] = "Code block";
            resources["note_quotation"] = "Quotation";

            resources["show_recyclebin"] = "Open recycle bin";
            resources["show_settings"] = "Settings";
            resources["empty_recyclebin"] = "Empty recycle bin";

            resources["show_transfer_code"] = "Show transfer code";
            resources["transfer_not_existing"] = "The transfer code will be available after the first synchronization.";
            resources["transfer_code"] = "Transfer code";
            resources["transfer_code_usage"] = "With this transfer code you can decrypt the notes stored in the cloud.";
            resources["transfer_code_required"] = "Please enter the transfer code. You can get the code from an already synchronized device, by using its menu entry «Show transfer code»";
            resources["transfer_code_show_history"] = "Show old transfer codes";
            resources["transfer_code_created"] = "A new transfer code {0} was generated, it can be checked up anytime using the menu ‹Show transfer code›.";
            resources["transfer_code_writedown"] = "We recommend to write the transfercode down, you need it to synchronize the notes with other devices, or when you later have to access the online backup.";

            resources["show_info"] = "Information";
            resources["version"] = "Version";
            resources["copyright"] = "Copyright";
            resources["website"] = "Website";
            resources["opensource"] = "License";
            resources["opensource_desc"] = "SilentNotes is an open source project, published under the terms of the Mozilla Public License v. 2.0.";
            resources["license"] = "SilentNotes respects your privacy, it does not collect user information and requires no unnecessary privileges. The notes never leave the device unencrypted.";
            resources["newer_version"] = "A newer version {0} of SilentNotes available, please install the update.";

            resources["cloud_url"] = "Server address";
            resources["cloud_username"] = "User name";
            resources["cloud_password"] = "Password";
            resources["cloud_service"] = "Online storage";
            resources["cloud_oauth_code"] = "Access code";
            resources["cloud_oauth_code_desc"] = "SilentNotes now opens the web page of the online-storage, please allow access for SilentNotes. Waiting for authorization…";
            resources["cloud_oauth_code_back"] = "Please go back to SilentNotes to continue.";
            resources["cloud_clear_settings"] = "Clear";
            resources["cloud_clear_settings_desc"] = "Removes conection to the online storage";
            resources["cloud_change_settings"] = "Change...";
            resources["cloud_change_settings_desc"] = "Connects to another online storage";
            resources["cloud_first_synchronization_title"] = "Set up the online storage";
            resources["cloud_first_synchronization_text"] = "This is your first synchronization, the necessary information to connect to the online storage is now collected.\nYou can find this information anytime in the menu ‹Settings›";
            resources["cloud_service_choice"] = "Online storage selection";
            resources["cloud_service_credentials"] = "Online storage credentials";
            resources["cloud_service_undefined"] = "No online storage is defined yet";
            resources["cloud_ftp_example"] = "ftp://ftp.example.org/silentnote/";
            resources["cloud_webdav_example"] = "https://webdav.example.org/";

            resources["sync"] = "Synchronization";
            resources["sync_auto"] = "Automatic synchronization";
            resources["sync_auto_never"] = "Never, only manual synchronization";
            resources["sync_auto_costfree"] = "Only if internet is free of cost";
            resources["sync_auto_always"] = "Always";
            resources["sync_notes"] = "Synchronizes notes with the cloud";
            resources["sync_success"] = "Successfully synchronized notes.";
            resources["sync_reject"] = "The synchronization with the cloud was aborted.";
            resources["sync_error_generic"] = "Could not synchronize the notes with the cloud, please try again later.";
            resources["sync_error_transfercode"] = "Please check the transfer code for typos, and use the latest version of SilentNotes.";
            resources["sync_error_connection"] = "Could not connect to the server, please check the internet connection and the server address in the settings.";
            resources["sync_error_privileges"] = "Please check username and password, and make sure you have enough privileges on the server.";
            resources["sync_error_repository"] = "Could not read the notes from the server, because the file has an invalid format.";
            resources["sync_error_revision"] = "Please update SilentNotes, the online-notes are stored in a more recent format.";
            resources["sync_repository_merge"] = "Merge notes from local device and server [recommended].";
            resources["sync_repository_cloud"] = "Use notes from the server, delete notes on the local device.";
            resources["sync_repository_device"] = "Use notes from the local device, delete notes on the server.";

            resources["encryption"] = "Encryption";
            resources["encryption_algorithm"] = "Encryption mode";
            resources["encryption_algo_chacha20"] = "ChaCha20-Poly1305 - Modern encryption algorithm [recommended]";
            resources["encryption_algo_aesgcm"] = "AES256-GCM";
            resources["encryption_algo_twofishgcm"] = "Twofish256-GCM";
            resources["encryption_adopt_cloud_desc"] = "Adopt encryption mode from the online-storage?";

            resources["gui"] = "User interface";
            resources["gui_show_arrow_keys"] = "Show cursor arrow keys when editing";
            resources["gui_font_size"] = "Font size (smaller ‹ normal › larger)";
            resources["gui_texture"] = "Theme";
            resources["gui_default_color"] = "Default color for new notes";
            resources["gui_arrow_key"] = "Go one character to the left/right";
        }

        private void LoadGermanResources(Dictionary<string, string> resources)
        {
            resources["ok"] = "OK";
            resources["cancel"] = "Abbrechen";
            resources["back"] = "Zurück";
            resources["continue"] = "Weiter";

            resources["error_loading_repository"] = "Die Notizen konnten nicht gelesen werden. Die Anwendung wurde gestoppt um bereits existierende Notizen nicht weiter zu beschädigen.";
            resources["welcome_note"] = "<h1>Willkommen bei SilentNotes</h1><ul><li>SilentNotes respektiert Ihre Privatsphäre mit Ende-zu-Ende Verschlüsselung.</li><li>Schreiben Sie Ihre Notizen und synchronisieren Sie sie zwischen PC und Mobilgeräten.</li></ul>";

            resources["note_create_new"] = "Neue Notiz erstellen";
            resources["note_view_or_edit"] = "Bearbeiten der selektierten Notiz";
            resources["note_to_recyclebin"] = "Selektierte Notizen in den Papierkorb verschieben";
            resources["note_undelete"] = "Notiz wiederherstellen";
            resources["note_undelete_selected"] = "Selektierte Notizen wiederherstellen";
            resources["note_title"] = "Titel";
            resources["note_content"] = "Text";
            resources["note_left_key"] = "Linke Pfeiltaste";
            resources["note_right_key"] = "Rechte Pfeiltaste";
            resources["note_colors"] = "Notizfarbe auswählen";
            resources["note_bold"] = "Fett";
            resources["note_italic"] = "Kursiv";
            resources["note_underline"] = "Unterstrichen";
            resources["note_strike"] = "Durchgestrichen";
            resources["note_list_ordered"] = "Nummerierte Liste";
            resources["note_list_unordered"] = "Aufzählungsliste";
            resources["note_code"] = "Code Block";
            resources["note_quotation"] = "Zitat";

            resources["show_recyclebin"] = "Papierkorb öffnen";
            resources["show_settings"] = "Einstellungen";
            resources["empty_recyclebin"] = "Papierkorb leeren";

            resources["show_transfer_code"] = "Transfercode anzeigen";
            resources["transfer_not_existing"] = "Der Transfercode ist erst verfügbar nach der ersten Synchronisation.";
            resources["transfer_code"] = "Transfercode";
            resources["transfer_code_usage"] = "Mit diesem Transfercode können Sie die in der Cloud gespeicherten Notizen entschlüsseln.";
            resources["transfer_code_required"] = "Bitte geben Sie den Transfercode ein. Sie erhalten den Code von einem bereits synchronisierten Gerät, indem sie dort den Menupunkt «Transfercode anzeigen» benutzen.";
            resources["transfer_code_show_history"] = "Ältere Transfercodes anzeigen";
            resources["transfer_code_created"] = "Ein neuer Transfercode {0} wurde generiert, er kann jederzeit im Menu unter ‹Transfercode anzeigen› nachgeschaut werden.";
            resources["transfer_code_writedown"] = "Wir empfehlen den Transfercode aufzuschreiben, Sie benötigen ihn um die Notizen mit anderen Geräten zu synchronisieren, oder wenn Sie später auf das Online Backup zugreifen müssen.";

            resources["show_info"] = "Information";
            resources["version"] = "Version";
            resources["copyright"] = "Copyright";
            resources["website"] = "Webseite";
            resources["opensource"] = "Lizenzierung";
            resources["opensource_desc"] = "SilentNotes ist ein Open Source Projekt, veröffentlicht gemäss den Bedingungen der Mozilla Public License v. 2.0.";
            resources["license"] = "SilentNotes respektiert Ihre Privatsphäre, es sammelt keine Benutzerinformationen und benötigt keine unnötigen Rechte. Die Notizen verlassen das Gerät nie unverschlüsselt.";
            resources["newer_version"] = "Eine neuere Version {0} von SilentNotes ist verfügbar, bitten installieren Sie das Update.";

            resources["cloud_url"] = "Server Addresse";
            resources["cloud_username"] = "Benutzername";
            resources["cloud_password"] = "Passwort";
            resources["cloud_service"] = "Online-Speicher";
            resources["cloud_oauth_code"] = "Zugangscode";
            resources["cloud_oauth_code_desc"] = "SilentNotes öffnet nun die Webseite des Online-Speichers, bitte erlauben Sie dort den Zugriff für SilentNotes. Warte auf Erlaubnis…";
            resources["cloud_oauth_code_back"] = "Bitte wechseln Sie nun zurück zu SilentNotes um weiterzufahren.";
            resources["cloud_clear_settings"] = "Entfernen";
            resources["cloud_clear_settings_desc"] = "Entfernt die Verbindung zum Online-Speicher";
            resources["cloud_change_settings"] = "Ändern...";
            resources["cloud_change_settings_desc"] = "Verbindet zu einem anderen Online-Speicher";
            resources["cloud_first_synchronization_title"] = "Einrichten des Online-Speichers";
            resources["cloud_first_synchronization_text"] = "Dies ist die erste Synchronisation, darum werden nun die nötigen Informationen gesammelt, um sich mit dem Online-Speicher zu verbinden.\nDie Informationen finden Sie jederzeit im Menu ‹Einstellungen›.";
            resources["cloud_service_choice"] = "Online-Speicher wählen";
            resources["cloud_service_credentials"] = "Online-Speicher Zugansdaten";
            resources["cloud_service_undefined"] = "Es ist noch kein Online-Speicher bestimmt";
            resources["cloud_ftp_example"] = "ftp://ftp.example.org/silentnote/";
            resources["cloud_webdav_example"] = "https://webdav.example.org/";

            resources["sync"] = "Synchronisation";
            resources["sync_auto"] = "Automatische Synchronisation";
            resources["sync_auto_never"] = "Nie, nur manuell synchronisieren";
            resources["sync_auto_costfree"] = "Nur über kostenfreie Internetverbindung";
            resources["sync_auto_always"] = "Immer";
            resources["sync_notes"] = "Synchronisiert Notizen mit dem Online-Speicher";
            resources["sync_success"] = "Notizen wurden erfolgreich synchronisiert.";
            resources["sync_reject"] = "Die Synchronisation mit dem Online-Speicher wurde abgebrochen.";
            resources["sync_error_generic"] = "Konnte die Notizen nicht mit dem Online-Speicher synchronisieren, bitte versuchen Sie es später nochmals.";
            resources["sync_error_transfercode"] = "Bitten prüfen Sie den Transfercode auf Tippfehler, und benutzen Sie die neueste Version von SilentNotes.";
            resources["sync_error_connection"] = "Konnte nicht zum Online-Speicher verbinden, bitte überprüfen Sie die Internetverbindung und die Server Adresse in den Einstellungen.";
            resources["sync_error_privileges"] = "Bitte überprüfen Sie Benutzername und Passwort, und stellen Sie sicher, dass Sie genügend Rechte auf dem Server besitzen.";
            resources["sync_error_repository"] = "Konnte die Notizen vom Online-Speicher nicht lesen, da die Datei ein ungültiges Format aufweist.";
            resources["sync_error_revision"] = "Bitte aktualisieren Sie SilentNotes, die Online-Notizen sind in einem neueren Format gespeichert.";
            resources["sync_repository_merge"] = "Zusammenführen der Notizen vom Online-Speicher und vom lokalem Gerät [empfohlen].";
            resources["sync_repository_cloud"] = "Verwende nur Notizen vom Online-Speicher, lösche Notizen auf dem lokalen Gerät.";
            resources["sync_repository_device"] = "Verwende nur Notizen vom lokalen Gerät, lösche Notizen auf dem Online-Speicher.";

            resources["encryption"] = "Verschlüsselung";
            resources["encryption_algorithm"] = "Verschlüsselungsverfahren";
            resources["encryption_algo_chacha20"] = "ChaCha20-Poly1305 - Moderner Algorithmus [empfohlen]";
            resources["encryption_algo_aesgcm"] = "AES-256-GCM";
            resources["encryption_algo_twofishgcm"] = "Twofish-256-GCM";
            resources["encryption_adopt_cloud_desc"] = "Verschlüsselungsverfahren vom Online-Speicher übernehmen?";

            resources["gui"] = "Benutzeroberfläche";
            resources["gui_show_arrow_keys"] = "Cursor Pfeiltasten beim Bearbeiten anzeigen";
            resources["gui_font_size"] = "Schriftgrösse (kleiner ‹ normal › grösser)";
            resources["gui_texture"] = "Design";
            resources["gui_default_color"] = "Standardfarbe für neue Notizen";
            resources["gui_arrow_key"] = "Ein Zeichen nach links/rechts bewegen";
        }

        /// <inheritdoc/>
        protected override void LoadTextResources(Dictionary<string, string> textResources, string languageCode)
        {
#if (LANG_EN && DEBUG)
            languageCode = "en";
#elif (LANG_DE && DEBUG)
            languageCode = "de";
#endif

            switch (languageCode.ToLowerInvariant())
            {
                case "de":
                    LoadGermanResources(textResources);
                    break;
                default:
                    LoadEnglishResources(textResources);
                    break;
            }
        }
    }
}
