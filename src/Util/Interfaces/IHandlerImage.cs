using SixLabors.ImageSharp;

namespace Util.Interfaces
{
    public interface IHandlerImage
    {
        /// <summary>
        /// Calculates the new dimensions for an image to fit within the specified maximum width and height while
        /// preserving the aspect ratio.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/> to be resized. Cannot be <see langword="null"/>.</param>
        /// <param name="maxWidth">The maximum allowed width, in pixels, for the resized image. Must be greater than 0.</param>
        /// <param name="maxHeight">The maximum allowed height, in pixels, for the resized image. Must be greater than 0.</param>
        /// <param name="v">An additional parameter that may influence the resizing behavior. Refer to the method's documentation for
        /// details on its usage.</param>
        /// <returns>A <see cref="Size"/> structure representing the width and height the image should be resized to in order to
        /// fit within the specified bounds while maintaining its aspect ratio.</returns>
        Size ResizedDimensions(Image image, int maxWidth, int maxHeight, int v);
    }
}