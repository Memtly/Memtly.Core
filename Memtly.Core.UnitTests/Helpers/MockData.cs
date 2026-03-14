using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Memtly.Core.Enums;
using Memtly.Core.Models.Database;

namespace Memtly.Core.UnitTests.Helpers
{
	internal class MockData
	{
        public static DefaultHttpContext MockHttpContext(Dictionary<string, StringValues>? form = null, IFormFileCollection? files = null, MockSession session = null)
        {
            var ctx = new DefaultHttpContext()
            {
                Session = session ?? new MockSession()
            };

            ctx.Request.Form = new FormCollection(form, files);

            return ctx;
        }

        public static List<GalleryItemModel> MockGalleryItems(int count = 10, int? galleryId = null, GalleryItemState state = GalleryItemState.All)
		{
			var result = new List<GalleryItemModel>();

			for (var i = 0; i < count; i++)
			{
				result.Add(MockGalleryItem(galleryId, state));
			}

			return result;
		}

		public static GalleryItemModel MockGalleryItem(int? galleryId = null, GalleryItemState state = GalleryItemState.All)
		{
			var rand = new Random();

			return new GalleryItemModel()
			{
				Id = rand.Next(),
				GalleryId = galleryId != null ? (int)galleryId : rand.Next(),
				Title = $"{Guid.NewGuid()}.{MockFileExtension()}",
				UploadedBy = rand.Next(2) % 2 == 0 ? Guid.NewGuid().ToString() : null,
				MediaType = (MediaType)rand.Next(3),
				State = state == GalleryItemState.All ? (GalleryItemState)rand.Next(2) : state,
				FileSize = (int)rand.Next(2),
            };
		}

		public static string MockFileExtension()
		{
			var rand = new Random();
			
			string extension;
			switch (rand.Next(4))
			{
				case 0:
					extension = "jpg";
					break;
				case 1:
					extension = "jpeg";
					break;
				case 2:
					extension = "png";
					break;
				default:
					extension = "ffff";
					break;
			}

			return rand.Next(2) % 2 == 0 ? extension.ToUpper() : extension.ToLower();
		}
	}
}