OxyPlot is a cross-platform plotting library for .NET

- [Web page](http://oxyplot.org)  
- [Documentation](http://docs.oxyplot.org/)
- [Announcements](http://oxyplot.org/announcements) / [atom](http://oxyplot.org/atom.xml)
- [Discussion forum](http://discussion.oxyplot.org)
- [Source repository](http://github.com/oxyplot/oxyplot)
- [Issue tracker](http://github.com/oxyplot/oxyplot/issues)
- [NuGet packages](http://www.nuget.org/packages?q=oxyplot)
- [Stack Overflow](http://stackoverflow.com/questions/tagged/oxyplot)
- [Twitter](https://twitter.com/hashtag/oxyplot)
- [Gitter](https://gitter.im/oxyplot/oxyplot) (chat)

![License](https://img.shields.io/badge/license-MIT-red.svg)
[![Build status](https://img.shields.io/appveyor/ci/objorke/oxyplot.svg)](https://ci.appveyor.com/project/objorke/oxyplot)

![Plot](http://oxyplot.org/public/images/normal-distributions.png)

#### Branches

`master` - the release branch (stable channel)  
`develop` -  the main branch with the latest development changes (pre-release channel)

See '[A successful git branching model](http://nvie.com/posts/a-successful-git-branching-model/)' for more information about the branching model in use.

#### Getting started

1. Use the NuGet package manager to add a reference to OxyPlot (see details below if you want to use pre-release packages)
2. Add a `PlotView` to your user interface
3. Create a `PlotModel` in your code
4. Bind the `PlotModel` to the `Model` property of your `PlotView`

#### Examples

You can find examples in the `/Source/Examples` folder in the code repository.

#### NuGet packages

The latest pre-release packages are pushed by AppVeyor CI to [myget.org](https://www.myget.org/)
To install these packages, set the myget.org package source `https://www.myget.org/F/oxyplot` and remember the "-pre" flag. 

The stable release packages will be pushed to [nuget.org](https://www.nuget.org/packages?q=oxyplot).
Note that we have currently have a lot of old (v2015.*) and pre-release packages on this feed, this will be cleaned up as soon as we release [v1.0](https://github.com/oxyplot/oxyplot/milestones/v1.0).

Package | Targets
--------|---------------
OxyPlot.Core | Portable class library
OxyPlot.Wpf | WPF (NET40, NET45)  
OxyPlot.WindowsForms | Windows Forms (NET40, NET45)
OxyPlot.Windows | Windows 8.1 and Windows Phone 8.1
OxyPlot.WP8 | Windows Phone Silverlight
OxyPlot.Silverlight | Silverlight 5 
OxyPlot.GtkSharp | GTK# 2 and 3 (NET40, NET45)
OxyPlot.Xamarin.Android | MonoAndroid
OxyPlot.Xamarin.iOS | MonoTouch and iOS10
OxyPlot.Xamarin.Mac | Mac20
OxyPlot.Xamarin.Forms | MonoTouch, iOS10, MonoAndroid, WP8
OxyPlot.Xwt | NET40, NET45
OxyPlot.OpenXML | NET40, NET45
OxyPlot.Pdf | PdfSharp (NET40, NET45, SL5)

#### Contribute

See [Contributing](.github/CONTRIBUTING.md) for information about how to contribute!
