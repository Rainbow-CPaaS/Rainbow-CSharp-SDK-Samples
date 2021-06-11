using MultiPlatformApplication.Helpers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
		/// The Resource Id of the image.
		/// </summary>
		public string Id { get; set; }

		public ImageSource ProvideValue(IServiceProvider serviceProvider)
		{
			ImageSource result = null;
			if (Id != null)
				result = ImageSource.FromResource(Helper.GetEmbededResourceFullPath(Id), typeof(ImageResourceExtension).Assembly);
			return result;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
			=> ((IMarkupExtension<ImageSource>)this).ProvideValue(serviceProvider);
	}
}
