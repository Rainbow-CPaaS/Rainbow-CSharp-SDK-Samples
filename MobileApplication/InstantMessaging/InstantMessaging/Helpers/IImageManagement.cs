using System;
using System.IO;

namespace Rainbow.Helpers
{
    public interface IImageManagement
    {
        /// <summary>
        /// Get a value representing the screen density
        /// </summary>
        /// <returns><see cref="double"/>Screen density</returns>
        double GetDensity();

        /// <summary>
        /// Draw an source image on destiantion image in (x, y)
        /// </summary>
        /// <param name="msSrc"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="msDst"><see cref="Stream"/>Image Destination in Stream format</param>
        /// <param name="xDest"><see cref="int"/>x value</param>
        /// <param name="yDest"><see cref="int"/>y value</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        Stream DrawImage(Stream msSrc, Stream msDst, float xDest, float yDest);

        /// <summary>
        /// Create a rounded image from the square image provided
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        Stream GetRoundedFromSquareImage(Stream ms);

        /// <summary>
        /// Get a new bitmap using a translation (x,y) from the image provided
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="x"><see cref="float"/>Translation value on X axis</param>
        /// <param name="y"><see cref="float"/>Translation value on Y axis</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        Stream GetTranslated(Stream ms, float x, float y);

        /// <summary>
        /// From image source, get an image part using a arc. 
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="partNumber"><see cref="int"/>Number of the part to get</param>
        /// <param name="nbParts"><see cref="int"/>Max. number of parts to have a circle if we add several part</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        Stream GetArcPartFromSquareImage(Stream ms, int partNumber, int nbParts);

        /// <summary>
        /// From image source, get a square and scaled image of the size specified
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="imgSize"><see cref="Stream"/>Size of the image to create</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        Stream GetSquareAndScaled(Stream ms, int imgSize);

        /// <summary>
        /// Create a filled circle with a centered text
        /// </summary>
        /// <param name="imgSize"><see cref="int"/>Size of the image to create</param>
        /// <param name="rgbCircleColor"><see cref="String"/>Color of the circle in RGB syntax (like "#00FF00")</param>
        /// <param name="txt"><see cref="String"/>Text</param>
        /// <param name="rgbTextColor"><see cref="String"/>Color of the text in RGB syntax (like "#00FF00")</param>
        /// <param name="fontFamilyName"><see cref="String"/>Font family</param>
        /// <param name="fontSize"><see cref="String"/>Font size</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        Stream GetFilledCircleWithCenteredText(int imgSize, String rgbCircleColor, String txt, String rgbTextColor, string fontFamilyName, int fontSize);

    }
}
