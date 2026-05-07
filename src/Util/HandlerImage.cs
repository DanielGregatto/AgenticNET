using SixLabors.ImageSharp;
using System;
using Util.Interfaces;

namespace Util
{
    public class HandlerImage : IHandlerImage
    {
        public HandlerImage()
        {
        }

        public Size ResizedDimensions(Image image, int maxWidth, int maxHeight, int v)
        {
            var response = new Size();

            if (image.Width > maxHeight || image.Height > maxHeight)
            {
                decimal percentual = 1.00M;
                while (percentual > 0)
                {
                    percentual = percentual - 0.01M;
                    response.Width = decimal.ToInt32(Math.Round(image.Width * percentual));
                    response.Height = decimal.ToInt32(Math.Round(image.Height * percentual));

                    if (response.Width <= maxWidth && response.Height <= maxHeight)
                        break;
                }
            }

            return response;
        }
    }
}