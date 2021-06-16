using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Extensions
{
	// BASED ON THIS: https://github.com/xamarin/XamarinCommunityToolkit/blob/main/src/CommunityToolkit/Xamarin.CommunityToolkit/Extensions/ImageResourceExtension.shared.cs

	/// <summary>
	/// Provides ImageSource by Resource Id from the current app's assembly.
	/// </summary>
	[ContentProperty(nameof(Id))]
	public class ImageResourceExtension : IMarkupExtension<ImageSource>
	{
		/// <summary>
		/// The Resource Id of the image. Example: "icon_doc_blue.png"
		/// </summary>
		public string Id { get; set; }

		public ImageSource ProvideValue(IServiceProvider serviceProvider)
		{
			ImageSource result = null;

			if(!String.IsNullOrEmpty(Id))
            {
				String filePath = Helper.GetEmbededResourceFullPath(Id);
				if (!String.IsNullOrEmpty(filePath))
					result = ImageSource.FromResource(filePath, typeof(ImageResourceExtension).Assembly);
				else
					result = Helper.GetImageSourceFromResourceDictionaryById(App.Current.Resources, Id);
			}

			return result;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
			=> ((IMarkupExtension<ImageSource>)this).ProvideValue(serviceProvider);
	}
}
