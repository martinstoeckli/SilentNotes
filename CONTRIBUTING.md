# Contributing to SilentNotes

Thank you for taking the time to read this lines, before making a pull request or opening an issue! 👍

We are glad to hear about your [ideas↘](#feature-requests), to be informed when you [found a problem↘](#bug-reports), to receive [code contributions↘](#pull-requests) or a [translation↘](#localization) to another language. Please don't be disappointed though, should a suggestion be rejected, we try to explain the reasons why the idea is not implemented. Following the points below, you will make it easier for us to react appropriate and probably will get a faster response.

## Feature requests

To know what users need is crucial for the development of a useful application, so tell us about your ideas! There are a lot of considerations, like trade offs between simplicity vs functionality or technical restrictions, but even if an idea is rejected it may become a feature of a future version. SilentNotes goal is to be simple in usage, so it will hardly become a competitor to professional services like OneNote.

## Bug reports

If you found a bug, you can file an [issue](https://github.com/martinstoeckli/SilentNotes/issues), please make sure that you updated to the newest version of SilentNotes and include a much information as you can about the problem (but leave out sensitive data like passwords!). If possible, add a description that helps to reproduce the problem. If the problem is related to online synchronization, then a test account on the online storage service can be a huge help.

## Localization

You want to help by translating SilentNotes to your native language? We gladly accept translations, have a look at the files [here](src/Localization). You can make a pull request or send us an email, the address is available on SilentNotes [homepage](https://www.martinstoeckli.ch/silentnotes). Please consider following requirements before investing your time:

- Every new version will need an updated version of the language resources, so we are looking for a long time engagement.
- Publishing a new release will be delayed if there are pending translations, but we do our best to inform our volunteers at an early stage.

Usually there are only small changes between releases.

## Pull requests

Working together on the same project is probably one of the biggest advantages of an open source project. So code contributions are welcome, just be aware that they can be rejected or that we may ask for improvement before merging. Please do not take this personally, there are a lot of considerations to make (technical as well as strategical).

But you want to contribute and that's great, so have a look at following points:

- Please contact us and tell us about your idea before writing the code, either by opening an [issue](https://github.com/martinstoeckli/SilentNotes/issues) or by writing an [email](https://www.martinstoeckli.ch/silentnotes) directly.
- Always create a branch to do your improvements and make a pull request when you are ready. Small corrections like typos do not need a branch.
- Try to write [clean code](https://clean-code-developer.com/), we are not religious about it, but we see the value in carefully written code.
- Write unit-tests whenever this is possible with a reasonable amount of work. This applies especially to the low level code like models and helper/worker classes. View and ViewModels can be tested manually with system tests.
- Comment all public functions, try to describe their limits and how they are meant to be used, instead of repeating the (hopefully well choosen) method name 😉. Non public function can be commented if you think it makes sense.
- The repository contains a StyleCop config file with somewhat relaxed rules. We appreciate if you do a quick test before submitting, StyleCop can be installed as an extension in VisualStudio. To check a single file, just right click on the open source file and select _"Run StyleCop"_.
- All functionallity needs to be cross-platform.

If the contribution affects the Android app, please test with the minimum version (currently 6.0) as well as with two other versions. It is a tedious work, but there are often unexpected differences in the WebView implementations.